namespace Rochas.PDFTests
{
    using System;
    using Rochas.PDFGenerator.Enumerators;
    using Rochas.PDFGenerator.Helpers;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using Xunit;

    public class PDFComposerTests
    {
        private readonly PDFComposer _composer = new PDFComposer();

        private static byte[] DummyImage()
        {
            return File.ReadAllBytes("Resources/Images/dummy.png");
        }

        private PdfConfig BaseConfig() =>
            new PdfConfig
            {
                MarginBottom = 20,
                MarginTop = 20,
                MarginLeft = 20,
                MarginRight = 20,
                FontFamily = PdfFontFamily.LiberationSans,
                Header = new PdfHeaderConfig(),
                WatermarkOpacity = 50
            };

        
        [Fact]
        public void GeneratePdf_WithRichText_ShouldGenerateValidPdf()
        {
            var template = @"
            Relatório Completo

            Nome: {{Nome}}
            Categoria: {{Categoria}}
            Observações:
            - Teste 1
            - Teste 2
            - Teste 3

            Texto adicional grande para validar múltiplas linhas e quebra automática.
            Lorem ipsum dolor sit amet, consectetur adipiscing elit.
        ";

            var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
            {
                { new PdfBodyPlaceHolder { Key = "{{Nome}}", Style = new PdfPlaceHolderStyle { Bold = true } }, "Renato Rocha" },
                { new PdfBodyPlaceHolder { Key = "{{Categoria}}", Style = new PdfPlaceHolderStyle { Italic = true } }, "Administrador" }
            };

            byte[] pdfData = _composer.GeneratePdf(template, placeholders, BaseConfig());
            File.WriteAllBytes("Test_RT.pdf", pdfData);

            Assert.NotNull(pdfData);
            Assert.True(pdfData.Length > 300);
            Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(pdfData)[..4]);
        }

        // --------------------------------------------------------------------
        [Fact]
        public void GeneratePdf_LongBody_ShouldPaginateCorrectly()
        {
            var longText = new string('A', 8000);  // força várias páginas
            var template = "Conteúdo:\n" + longText;

            var placeholders = new Dictionary<PdfBodyPlaceHolder, string>();

            var pdfData = _composer.GeneratePdf(template, placeholders, BaseConfig());
            File.WriteAllBytes("Test_LB.pdf", pdfData);

            Assert.NotNull(pdfData);
            Assert.True(pdfData.Length > 1000);
        }

        // --------------------------------------------------------------------
        [Fact]
        public void GeneratePdf_WithHeaderAndWatermark_ShouldWorkTogether()
        {
            var config = BaseConfig();
            config.Header = new PdfHeaderConfig()
            {
                Title = "Relatório Integrado",
                LogoBytes = DummyImage(),
                TitleStyle = new PdfPlaceHolderStyle { Bold = true, FontSizePx = 22 }
            };

            config.WatermarkBytes = DummyImage();
            config.WatermarkOpacity = 30;

            var pdfData = _composer.GeneratePdf("Corpo do documento...", new Dictionary<PdfBodyPlaceHolder, string>(), config);
            File.WriteAllBytes("Test_LWM.pdf", pdfData);

            Assert.NotNull(pdfData);
            Assert.True(pdfData.Length > 300);
        }

        // --------------------------------------------------------------------
        [Fact]
        public void GeneratePdf_FromModel_ShouldRenderAllTokens()
        {
            var template = @"
            Usuário: {{Name}}
            Idade: {{Age}}
            Saldo: {{Balance}}
            Status: {{Status}}";

            var model = new
            {
                Name = "Renato",
                Age = 40,
                Balance = 1523.90m,
                Status = "Ativo"
            };

            var style = new PdfPlaceHolderStyle
            {
                Bold = true,
                TextColor = PdfTextColor.DarkBlue,
                FontSizePx = 14
            };

            byte[] pdfData = _composer.GeneratePdf(template, model, BaseConfig(), style);
            File.WriteAllBytes("Test_FM.pdf", pdfData);

            Assert.NotNull(pdfData);
            Assert.True(pdfData.Length > 300);
        }

        // --------------------------------------------------------------------
        [Fact]
        public void GeneratePdf_WithDataTable_ShouldRenderTableCorrectly()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Nome");
            dataTable.Columns.Add("Valor");
            dataTable.Columns.Add("Ativo");

            dataTable.Rows.Add("Produto A", "10,90", "Sim");
            dataTable.Rows.Add("Produto B", "8,50", "Não");
            dataTable.Rows.Add("Produto C", "12,00", "Sim");

            var config = BaseConfig();
            byte[] pdfData = _composer.GeneratePdf(dataTable, config);
            File.WriteAllBytes("Test_DT.pdf", pdfData);

            Assert.NotNull(pdfData);
            Assert.True(pdfData.Length > 500);
        }

        // ── NEW TESTS ──────────────────────────────────────────────────

        [Fact]
        public void GeneratePdf_WithStyledTable_ShouldRenderBorderedTable()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Produto");
            dataTable.Columns.Add("Qtd");
            dataTable.Columns.Add("Valor");

            dataTable.Rows.Add("Notebook", 2, "R$ 12.000,00");
            dataTable.Rows.Add("Mouse", 10, "R$ 250,00");
            dataTable.Rows.Add("Teclado", 5, "R$ 750,00");

            var config = BaseConfig();
            config.Table = new PdfTableConfig
            {
                Style = PdfTableStyle.Bordered,
                HeaderColor = "#1E3A5F",
                HeaderTextBold = true,
                AlternatingRowColor = "#F0F4F8"
            };

            byte[] pdfData = _composer.GeneratePdf(dataTable, config);
            File.WriteAllBytes("Test_ST.pdf", pdfData);

            Assert.NotNull(pdfData);
            Assert.True(pdfData.Length > 500);
        }

        // --------------------------------------------------------------------
        [Fact]
        public void GeneratePdf_MultiColumn_ShouldRenderTwoColumns()
        {
            var config = BaseConfig();
            config.Columns = new PdfColumnConfig
            {
                Count = 2,
                Ratios = new[] { 60f, 40f },
                Gap = 10
            };

            var leftPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
            {
                { new PdfBodyPlaceHolder { Key = "{{Nome}}" }, "ACME Ltda." },
                { new PdfBodyPlaceHolder { Key = "{{Doc}}" }, "00.000.000/0001-00" }
            };

            var rightPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
            {
                { new PdfBodyPlaceHolder { Key = "{{Data}}" }, "07/08/2026" },
                { new PdfBodyPlaceHolder { Key = "{{Total}}" }, "R$ 1.500,00" }
            };

            byte[] pdfData = _composer.GeneratePdf(
                "Nome: {{Nome}}\nDoc: {{Doc}}", leftPlaceholders,
                "Data: {{Data}}\nTotal: {{Total}}", rightPlaceholders,
                config);

            File.WriteAllBytes("Test_MC.pdf", pdfData);

            Assert.NotNull(pdfData);
            Assert.True(pdfData.Length > 300);
        }

        // --------------------------------------------------------------------
        [Fact]
        public void GeneratePdf_TableWithModelHeader_ShouldMergeBoth()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Produto");
            dataTable.Columns.Add("Qtd");
            dataTable.Columns.Add("Valor");

            dataTable.Rows.Add("Notebook", 1, "R$ 8.500,00");
            dataTable.Rows.Add("Mouse", 3, "R$ 360,00");

            var headerModel = new
            {
                Cliente = "ACME Ltda.",
                CNPJ = "00.000.000/0001-00",
                Data = "07/08/2026"
            };

            var tableConfig = new PdfTableConfig
            {
                Style = PdfTableStyle.Bordered,
                HeaderColor = "#1E3A5F"
            };

            var config = BaseConfig();

            byte[] pdfData = _composer.GeneratePdf(dataTable, headerModel, tableConfig, config);
            File.WriteAllBytes("Test_TH.pdf", pdfData);

            Assert.NotNull(pdfData);
            Assert.True(pdfData.Length > 500);
        }

        // --------------------------------------------------------------------
        [Fact]
        public void GeneratePdf_BackwardCompat_OldConfigStillWorks()
        {
            var oldConfig = new PdfPageConfiguration
            {
                MarginBottom = 20,
                MarginTop = 20,
                MarginLeft = 20,
                MarginRight = 20,
                FontFamily = PdfFontFamily.LiberationSans,
                Header = new PdfHeaderConfig
                {
                    Title = "Teste Compatibilidade"
                }
            };

            var pdfData = _composer.GeneratePdf("Corpo do documento...",
                new Dictionary<PdfBodyPlaceHolder, string>(), oldConfig);

            File.WriteAllBytes("Test_COMPAT.pdf", pdfData);

            Assert.NotNull(pdfData);
            Assert.True(pdfData.Length > 300);
        }
        // --------------------------------------------------------------------
        [Fact]
        public void GeneratePdf_MultiColumnList_ShouldRenderThreeColumns()
        {
            var config = BaseConfig();
            config.Columns = new PdfColumnConfig
            {
                Count = 3,
                Ratios = new[] { 40f, 30f, 30f },
                Gap = 8
            };

            var columns = new List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)>
            {
                ("Nome: {{Nome}}\nDoc: {{Doc}}", new Dictionary<PdfBodyPlaceHolder, string>
                {
                    { new PdfBodyPlaceHolder { Key = "{{Nome}}" }, "ACME Ltda." },
                    { new PdfBodyPlaceHolder { Key = "{{Doc}}" }, "00.000.000/0001-00" }
                }),
                ("Data: {{Data}}\nPedido: {{Pedido}}", new Dictionary<PdfBodyPlaceHolder, string>
                {
                    { new PdfBodyPlaceHolder { Key = "{{Data}}" }, "07/08/2026" },
                    { new PdfBodyPlaceHolder { Key = "{{Pedido}}" }, "PV-00015" }
                }),
                ("Total: {{Total}}\nStatus: {{Status}}", new Dictionary<PdfBodyPlaceHolder, string>
                {
                    { new PdfBodyPlaceHolder { Key = "{{Total}}" }, "R$ 1.500,00" },
                    { new PdfBodyPlaceHolder { Key = "{{Status}}" }, "Faturado" }
                })
            };

            byte[] pdfData = _composer.GeneratePdf(columns, config);
            File.WriteAllBytes("Test_MC3.pdf", pdfData);

            Assert.NotNull(pdfData);
            Assert.True(pdfData.Length > 300);
        }
    }
}
