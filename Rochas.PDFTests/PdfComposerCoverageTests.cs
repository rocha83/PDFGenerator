namespace Rochas.PDFTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using Rochas.PDFGenerator.Enumerators;
    using Rochas.PDFGenerator.Helpers;
    using Xunit;

    public class PDFComposerCoverageTests
    {
        private readonly PDFComposer _composer = new PDFComposer();

        private static byte[] DummyImage()
        {
            return File.ReadAllBytes("Resources/Images/dummy.png");
        }

        private static byte[] MontserratFont()
        {
            return File.ReadAllBytes("Resources/Fonts/Montserrat/Montserrat-Regular.ttf");
        }

        private static PdfConfig BaseConfig()
        {
            return new PdfConfig
            {
                MarginBottom = 20,
                MarginTop = 20,
                MarginLeft = 20,
                MarginRight = 20,
                FontFamily = PdfFontFamily.LiberationSans,
                Header = new PdfHeaderConfig(),
                WatermarkOpacity = 50
            };
        }

        private static DataTable SampleTable()
        {
            var table = new DataTable();
            table.Columns.Add("Produto");
            table.Columns.Add("Qtd");
            table.Columns.Add("Valor");
            table.Rows.Add("Notebook", 2, "R$ 8.500,00");
            table.Rows.Add("Mouse", 3, "R$ 360,00");
            return table;
        }

        private static void AssertValidPdf(byte[] pdf)
        {
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 300);
            Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(pdf)[..4]);
        }

        // ── PDFComposer constructors ────────────────────────────────

        [Fact]
        public void ParameterizedConstructor_WithMetadata_ShouldGeneratePdf()
        {
            var composer = new PDFComposer("author-test", "title-test", "subject-test", new DateTime(2026, 8, 14, 10, 0, 0));
            var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
            {
                { new PdfBodyPlaceHolder { Key = "{{X}}" }, "valor" }
            };

            AssertValidPdf(composer.GeneratePdf("Token: {{X}}", placeholders, BaseConfig()));
        }

        [Fact]
        public void ParameterizedConstructor_WithNullMetadata_ShouldNotThrow()
        {
            var composer = new PDFComposer(null, null, null, null);

            AssertValidPdf(composer.GeneratePdf("Corpo do documento...", new Dictionary<PdfBodyPlaceHolder, string>(), BaseConfig()));
        }

        // ── Mode 5: DataTable + PdfTableConfig ─────────────────────

        [Fact]
        public void GeneratePdf_WithTableConfigOverload_ShouldRenderStyledTable()
        {
            var config = BaseConfig();
            var tableConfig = new PdfTableConfig
            {
                Style = PdfTableStyle.Bordered,
                HeaderColor = "#1E3A5F",
                AlternatingRowColor = "#F0F4F8",
                BorderColor = "#CCCCCC",
                ShowBorders = true
            };

            AssertValidPdf(_composer.GeneratePdf(SampleTable(), tableConfig, config));
        }

        // ── PdfBodyStyler.ResolveRawColorHex + PdfTableDocument branches ──

        [Fact]
        public void GeneratePdf_WithAllRawColorNames_ShouldResolveEveryHex()
        {
            var colors = new[]
            {
                "black", "white", "gray", "grey", "lightgray", "lightgrey",
                "darkgray", "darkgrey", "blue", "darkblue", "green", "darkgreen",
                "red", "darkred", "yellow", "orange", "brown", "cyan", "unknown-name"
            };

            foreach (var color in colors)
            {
                var config = BaseConfig();
                config.Table = new PdfTableConfig
                {
                    Style = PdfTableStyle.Bordered,
                    HeaderColor = color,
                    AlternatingRowColor = "#F0F4F8",
                    BorderColor = color
                };

                AssertValidPdf(_composer.GeneratePdf(SampleTable(), config));
            }
        }

        [Fact]
        public void GeneratePdf_WithHexHeaderAndBorders_ShouldRender()
        {
            var config = BaseConfig();
            config.Table = new PdfTableConfig
            {
                Style = PdfTableStyle.Bordered,
                HeaderColor = "#112233",
                RowPadding = 4,
                HeaderFontSize = 11
            };

            AssertValidPdf(_composer.GeneratePdf(SampleTable(), config));
        }

        [Fact]
        public void GeneratePdf_WithAllTableStylesAndNoAltColor_ShouldRender()
        {
            foreach (PdfTableStyle style in Enum.GetValues(typeof(PdfTableStyle)))
            {
                var config = BaseConfig();
                config.Table = new PdfTableConfig
                {
                    Style = style,
                    ShowBorders = false
                };

                AssertValidPdf(_composer.GeneratePdf(SampleTable(), config));
            }
        }

        [Fact]
        public void GeneratePdf_TableWithNullHeaderColor_ShouldFallbackDefault()
        {
            var config = BaseConfig();
            config.Table = new PdfTableConfig { Style = PdfTableStyle.Minimal, HeaderColor = null };

            AssertValidPdf(_composer.GeneratePdf(SampleTable(), config));
        }

        // ── PdfBodyStyler.ResolveColorHex + ApplyStyleToSpan branches ──

        [Fact]
        public void GeneratePdf_WithAllTextColors_ShouldApplyEveryColor()
        {
            foreach (PdfTextColor color in Enum.GetValues(typeof(PdfTextColor)))
            {
                var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
                {
                    {
                        new PdfBodyPlaceHolder
                        {
                            Key = "{{T}}",
                            Style = new PdfPlaceHolderStyle
                            {
                                TextColor = color,
                                Bold = true,
                                Italic = true,
                                Underline = true,
                                FontSizePx = 12
                            }
                        },
                        "texto colorido"
                    }
                };

                AssertValidPdf(_composer.GeneratePdf("Cor: {{T}}", placeholders, BaseConfig()));
            }
        }

        [Fact]
        public void GeneratePdf_WithCustomHexColor_ShouldResolveCustomValue()
        {
            var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
            {
                {
                    new PdfBodyPlaceHolder
                    {
                        Key = "{{T}}",
                        Style = new PdfPlaceHolderStyle
                        {
                            TextColor = PdfTextColor.CustomHex,
                            CustomTextColor = "#4CAF50"
                        }
                    },
                    "verde custom"
                }
            };

            AssertValidPdf(_composer.GeneratePdf("Custom: {{T}}", placeholders, BaseConfig()));
        }

        [Fact]
        public void GeneratePdf_WithCustomHexWithoutValue_ShouldFallbackBlack()
        {
            var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
            {
                {
                    new PdfBodyPlaceHolder
                    {
                        Key = "{{T}}",
                        Style = new PdfPlaceHolderStyle
                        {
                            TextColor = PdfTextColor.CustomHex,
                            CustomTextColor = "   "
                        }
                    },
                    "sem cor"
                }
            };

            AssertValidPdf(_composer.GeneratePdf("Sem cor: {{T}}", placeholders, BaseConfig()));
        }

        [Fact]
        public void GeneratePdf_WithCustomFontFamily_ShouldApplyOrIgnore()
        {
            var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
            {
                {
                    new PdfBodyPlaceHolder
                    {
                        Key = "{{T}}",
                        Style = new PdfPlaceHolderStyle
                        {
                            CustomFontFamily = "NonExistentFontFamily",
                            Underline = true
                        }
                    },
                    "fonte custom"
                }
            };

            AssertValidPdf(_composer.GeneratePdf("Fonte: {{T}}", placeholders, BaseConfig()));
        }

        [Fact]
        public void GeneratePdf_PlaceholderWithNullStyle_ShouldUseDefaultStyle()
        {
            var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
            {
                { new PdfBodyPlaceHolder { Key = "{{T}}", Style = null }, "sem estilo" }
            };

            AssertValidPdf(_composer.GeneratePdf("Texto: {{T}}", placeholders, BaseConfig()));
        }

        [Fact]
        public void GeneratePdf_TokenWithoutMatchingPlaceholder_ShouldKeepToken()
        {
            var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
            {
                { new PdfBodyPlaceHolder { Key = "{{Presente}}" }, "aqui" }
            };

            AssertValidPdf(_composer.GeneratePdf("Tem: {{Presente}}; Ausente: {{Inexistente}}", placeholders, BaseConfig()));
        }

        // ── ComposeHeader branches ─────────────────────────────────

        [Fact]
        public void GeneratePdf_HeaderLogoOnly_ShouldRenderCenteredLogo()
        {
            var config = BaseConfig();
            config.Header = new PdfHeaderConfig { LogoBytes = DummyImage(), Title = null };

            AssertValidPdf(_composer.GeneratePdf("Corpo...", new Dictionary<PdfBodyPlaceHolder, string>(), config));
        }

        [Fact]
        public void GeneratePdf_HeaderTitleOnly_ShouldRenderTitle()
        {
            var config = BaseConfig();
            config.Header = new PdfHeaderConfig
            {
                LogoBytes = null,
                Title = "Somente Título",
                TitleStyle = new PdfPlaceHolderStyle { Bold = false, FontSizePx = 14 }
            };

            AssertValidPdf(_composer.GeneratePdf("Corpo...", new Dictionary<PdfBodyPlaceHolder, string>(), config));
        }

        [Fact]
        public void GeneratePdf_HeaderAllAlignmentsAndStyles_ShouldRender()
        {
            foreach (var logoAlign in new[] { PdfLogoAlignment.Left, PdfLogoAlignment.Right })
            {
                foreach (var bold in new[] { true, false })
                {
                    var config = BaseConfig();
                    config.Header = new PdfHeaderConfig
                    {
                        LogoBytes = DummyImage(),
                        LogoAlign = logoAlign,
                        Title = "Título Alinhado",
                        TitleStyle = new PdfPlaceHolderStyle { Bold = bold, FontSizePx = 18 }
                    };

                    AssertValidPdf(_composer.GeneratePdf("Corpo...", new Dictionary<PdfBodyPlaceHolder, string>(), config));
                }
            }
        }

        // ── Watermark / footer / pagination branches ───────────────

        [Fact]
        public void GeneratePdf_WithoutFooterPagination_ShouldRenderSinglePageFooter()
        {
            var config = BaseConfig();
            config.FooterPagination = false;

            AssertValidPdf(_composer.GeneratePdf("Conteúdo sem paginação", new Dictionary<PdfBodyPlaceHolder, string>(), config));
        }

        [Fact]
        public void GeneratePdf_WithWatermark_ShouldApplyOpacity()
        {
            var config = BaseConfig();
            config.WatermarkBytes = DummyImage();
            config.WatermarkOpacity = 15;

            AssertValidPdf(_composer.GeneratePdf("Com marca d'água", new Dictionary<PdfBodyPlaceHolder, string>(), config));
        }

        // ── Model + DataTable mapping branches ─────────────────────

        [Fact]
        public void GeneratePdf_FromModelWithNullProperty_ShouldRenderEmpty()
        {
            var model = new { Nome = "Cliente", Documento = (string?)null, Valor = 10m };
            var style = new PdfPlaceHolderStyle { Bold = true, FontSizePx = 10 };

            AssertValidPdf(_composer.GeneratePdf("{{Nome}} | {{Documento}} | {{Valor}}", model, BaseConfig(), style));
        }

        [Fact]
        public void GeneratePdf_FromDataTableWithoutRows_ShouldRenderHeaders()
        {
            var table = new DataTable();
            table.Columns.Add("Produto");
            table.Columns.Add("Qtd");

            AssertValidPdf(_composer.GeneratePdf(table, BaseConfig()));
        }

        [Fact]
        public void GeneratePdf_TableWithModelHeaderNullValue_ShouldRender()
        {
            var table = SampleTable();
            var headerModel = new { Cliente = "ACME", Contrato = (string?)null };
            var tableConfig = new PdfTableConfig { Style = PdfTableStyle.Grid };

            AssertValidPdf(_composer.GeneratePdf(table, headerModel, tableConfig, BaseConfig()));
        }

        // ── Multi-column branches ──────────────────────────────────

        private static Dictionary<PdfBodyPlaceHolder, string> SimplePlaceholders(string key, string value)
        {
            return new Dictionary<PdfBodyPlaceHolder, string>
            {
                { new PdfBodyPlaceHolder { Key = key }, value }
            };
        }

        [Fact]
        public void GeneratePdf_MultiColumnWithoutColumnConfig_ShouldUseDefaultRatios()
        {
            var config = BaseConfig();
            config.Columns = null;

            var columns = new List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)>
            {
                ("Esq: {{A}}", SimplePlaceholders("{{A}}", "1")),
                ("Dir: {{B}}", SimplePlaceholders("{{B}}", "2"))
            };

            AssertValidPdf(_composer.GeneratePdf(columns, config));
        }

        [Fact]
        public void GeneratePdf_MultiColumnWithRatiosAndBlankLines_ShouldRender()
        {
            var config = BaseConfig();
            config.Columns = new PdfColumnConfig { Count = 3, Ratios = new[] { 50f }, Gap = 5 };

            var columns = new List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)>
            {
                ("Linha 1\n\nLinha 2\n{{A}}", SimplePlaceholders("{{A}}", "ok")),
                ("Col 2\n{{B}}", SimplePlaceholders("{{B}}", "ok")),
                ("Col 3\n{{C}}", SimplePlaceholders("{{C}}", "ok"))
            };

            AssertValidPdf(_composer.GeneratePdf(columns, config));
        }

        [Fact]
        public void GeneratePdf_MultiColumnWithPaddingLineBreak_ShouldRenderBlankItem()
        {
            var config = BaseConfig();
            config.Columns = new PdfColumnConfig { Count = 2 };

            var columns = new List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)>
            {
                ("Topo\n\nFundo", new Dictionary<PdfBodyPlaceHolder, string>()),
                ("Esquerda\nDireita", new Dictionary<PdfBodyPlaceHolder, string>())
            };

            AssertValidPdf(_composer.GeneratePdf(columns, config));
        }

        // ── Font registration branches ─────────────────────────────

        [Fact]
        public void GeneratePdf_WithComicNeueFont_ShouldRegisterFont()
        {
            var config = BaseConfig();
            config.FontFamily = PdfFontFamily.ComicNeue;

            AssertValidPdf(_composer.GeneratePdf("Fonte ComicNeue", new Dictionary<PdfBodyPlaceHolder, string>(), config));
        }

        [Theory]
        [InlineData(PdfFontFamily.JetBrainsMono)]
        [InlineData(PdfFontFamily.Montserrat)]
        [InlineData(PdfFontFamily.LiberationSerif)]
        public void GeneratePdf_WithMissingSystemFonts_ShouldNotThrow(PdfFontFamily family)
        {
            var config = BaseConfig();
            config.FontFamily = family;

            AssertValidPdf(_composer.GeneratePdf("Fonte sem arquivo local", new Dictionary<PdfBodyPlaceHolder, string>(), config));
        }

        [Fact]
        public void GeneratePdf_WithCustomFontBytes_ShouldRegisterCustomFont()
        {
            var config = BaseConfig();
            config.FontFamily = PdfFontFamily.Custom;
            config.CustomFontBytes = MontserratFont();

            AssertValidPdf(_composer.GeneratePdf("Fonte customizada", new Dictionary<PdfBodyPlaceHolder, string>(), config));
        }

        [Fact]
        public void GeneratePdf_WithCustomFontWithoutBytes_ShouldSkipRegistration()
        {
            var config = BaseConfig();
            config.FontFamily = PdfFontFamily.Custom;
            config.CustomFontBytes = null;

            AssertValidPdf(_composer.GeneratePdf("Sem fonte custom", new Dictionary<PdfBodyPlaceHolder, string>(), config));
        }

        // ── Config property accessors ──────────────────────────────

        [Fact]
        public void PdfConfig_DefaultAndSetters_ShouldExposeValues()
        {
            var config = new PdfConfig();
            Assert.Null(config.CustomFontBytes);
            Assert.Null(config.Chart);

            var chart = new PdfChartConfig { Width = 640, Height = 480, Title = "Grafico", Labels = new[] { "A" }, Values = new[] { 1m }, Colors = new[] { "#000" } };
            Assert.Equal(PdfChartType.VerticalBar, chart.Type);
            Assert.Equal(640f, chart.Width);
            Assert.Equal(480f, chart.Height);
            Assert.Equal("Grafico", chart.Title);
            Assert.Equal(new[] { "A" }, chart.Labels);
            Assert.Equal(new[] { 1m }, chart.Values);
            Assert.Equal(new[] { "#000" }, chart.Colors);
            Assert.True(chart.ShowValues);
            Assert.True(chart.ShowGrid);

            config.CustomFontBytes = new byte[] { 1, 2, 3 };
            config.Chart = chart;
            Assert.NotNull(config.CustomFontBytes);
            Assert.Same(chart, config.Chart);

            var columnConfig = new PdfColumnConfig();
            Assert.Equal(2, columnConfig.Count);
            Assert.Null(columnConfig.DividerColor);
            columnConfig.DividerStyle = PdfColumnDividerStyle.Solid;
            Assert.Equal(PdfColumnDividerStyle.Solid, columnConfig.DividerStyle);
        }
    }
}