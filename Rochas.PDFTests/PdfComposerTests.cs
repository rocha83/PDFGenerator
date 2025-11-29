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

        private PdfPageConfiguration BaseConfig() =>
            new PdfPageConfiguration
            {
                MarginBottom = 20,
                MarginTop = 20,
                MarginLeft = 20,
                MarginRight = 20,
                FontFamily = PdfFontFamily.LiberationSans,
                HeaderComposition = new PdfHeaderComposition(),
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

            var ph = new Dictionary<PdfBodyPlaceHolder, string>
        {
            { new PdfBodyPlaceHolder { Key = "{{Nome}}", Style = new PdfPlaceHolderStyle { Bold = true } }, "Renato Rocha" },
            { new PdfBodyPlaceHolder { Key = "{{Categoria}}", Style = new PdfPlaceHolderStyle { Italic = true } }, "Administrador" }
        };

            byte[] pdfData = _composer.GeneratePdf(template, ph, BaseConfig());
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

            var ph = new Dictionary<PdfBodyPlaceHolder, string>();

            var pdfData = _composer.GeneratePdf(template, ph, BaseConfig());
            File.WriteAllBytes("Test_LB.pdf", pdfData);

            Assert.NotNull(pdfData);
            Assert.True(pdfData.Length > 1000);
        }

        // --------------------------------------------------------------------
        [Fact]
        public void GeneratePdf_WithHeaderAndWatermark_ShouldWorkTogether()
        {
            var cfg = BaseConfig();
            cfg.HeaderComposition = new PdfHeaderComposition()
            {
                Title = "Relatório Integrado",
                LogoBytes = DummyImage(),
                TitleStyle = new PdfPlaceHolderStyle { Bold = true, FontSizePx = 22 }
            };

            cfg.WatermarkBytes = DummyImage();
            cfg.WatermarkOpacity = 30;

            var pdfData = _composer.GeneratePdf("Corpo do documento...", new Dictionary<PdfBodyPlaceHolder, string>(), cfg);
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
            // cria DataTable
            var dt = new DataTable();
            dt.Columns.Add("Nome");
            dt.Columns.Add("Valor");
            dt.Columns.Add("Ativo");

            dt.Rows.Add("Produto A", "10,90", "Sim");
            dt.Rows.Add("Produto B", "8,50", "Não");
            dt.Rows.Add("Produto C", "12,00", "Sim");

            var cfg = BaseConfig();
            byte[] pdfData = _composer.GeneratePdf(dt, cfg);
            File.WriteAllBytes("Test_DT.pdf", pdfData);

            Assert.NotNull(pdfData);
            Assert.True(pdfData.Length > 500);
        }
    }
}