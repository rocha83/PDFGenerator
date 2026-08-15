# Rochas.PDFGenerator

[English](#english) | [Português](#português) | [Español](#español) | [Deutsch](#deutsch) | [Français](#français)

---

## English

**.NET library for generating PDFs from templates, models (T), or DataTables**, with full support for **headers with logos**, **footer pagination**, **font styles and colors**, **watermarks**, **styled tables**, **multi-column layouts**, and highly customizable placeholders.

Based on *QuestPDF* and compatible with **.NET Standard 2.1+**.

Main features:
- **Template/custom content composition** — inline templates with `{{Placeholder}}` keys and per-placeholder styles
- **Embedded fonts** — Liberation Sans, Comic Neue, JetBrains Mono, Montserrat, Liberation Serif, or a custom font
- **Styling and text alignment** — bold, italic, underline, font size and color per placeholder
- **Multi-column and table formats** — side-by-side columns with configurable ratios and styled tabular data

### Setup

```bash
dotnet add package Rochas.PDFGenerator
```

### Overview

The main class is `PDFComposer`, offering **7 PDF generation modes**:

| Mode | Description |
|------|-------------|
| **Template + Placeholders** | Replace keys `{{Name}}` with individual styles |
| **Generic Model (T)** | Object automatically mapped to placeholders |
| **DataTable** | Tabular PDF with headers/rows |
| **Multi-Column** | Two templates side by side with configurable ratios |
| **Multi-Column List** | N columns via template list |
| **Styled Table** | DataTable with borders, alternating rows, custom header |
| **Table + Model Header** | Object header + item table |

### Configuration (PdfConfig)

The `PdfConfig` class centralizes all settings:

```csharp
var config = new PdfConfig
{
    MarginLeft = 40,
    MarginRight = 40,
    MarginTop = 50,
    MarginBottom = 50,

    FontFamily = PdfFontFamily.Montserrat,
    CustomFontBytes = File.ReadAllBytes("fonts/Montserrat-Regular.ttf"),

    Header = new PdfHeaderConfig
    {
        LogoBytes = File.ReadAllBytes("images/logo.png"),
        LogoAlign = PdfLogoAlignment.Left,
        Title = "Report Title"
    },

    WatermarkBytes = File.ReadAllBytes("images/watermark.png"),
    WatermarkOpacity = 30,
    FooterPagination = true
};
```

> **Backward compatible:** `PdfPageConfiguration` and `PdfHeaderComposition` still work as aliases.

The `PDFComposer` constructor also accepts document metadata: `author`, `title`, `subject`, and `creationDate`.

### Mode 1 — Template + Placeholders (Quick Start)

```csharp
var composer = new PDFComposer();
var template = "Client: {{Name}}\nDate: {{Date}}";

var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Name}}", Style = new PdfPlaceHolderStyle { Bold = true, FontSizePx = 16 } }, "ACME Ltd" },
    { new PdfBodyPlaceHolder { Key = "{{Date}}", Style = new PdfPlaceHolderStyle { Italic = true } }, DateTime.Now.ToString("dd/MM/yyyy") }
};

byte[] pdf = composer.GeneratePdf(template, placeholders, config);
```

### Modes 2 & 3 — Generic Model (T) and DataTable

```csharp
var client = new { Name = "ACME Ltd", Document = "00.000.000/0001-00" };
byte[] pdf = composer.GeneratePdf("Client: {{Name}}\nDoc: {{Document}}", client, config);

DataTable table = new DataTable();
table.Columns.Add("Product");
table.Columns.Add("Quantity");
table.Rows.Add("Notebook", 10);
byte[] pdf = composer.GeneratePdf(table, config);
```

### Modes 4 & 5 — Multi-Column (2 or N columns)

```csharp
config.Columns = new PdfColumnConfig
{
    Count = 2,
    Ratios = new[] { 60f, 40f },
    Gap = 10,
    FitMode = PdfColumnFitMode.Proportional // Proportional (default) | AutoFit
};

var leftPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Name}}" }, "ACME Ltd" },
    { new PdfBodyPlaceHolder { Key = "{{Doc}}" }, "00.000.000/0001-00" }
};

var rightPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Date}}" }, "08/07/2026" },
    { new PdfBodyPlaceHolder { Key = "{{Total}}" }, "$1,500.00" }
};

byte[] pdf = composer.GeneratePdf(
    "Name: {{Name}}\nDoc: {{Doc}}", leftPlaceholders,
    "Date: {{Date}}\nTotal: {{Total}}", rightPlaceholders,
    config);

// N columns via a list of (template, placeholders) tuples
var columns = new List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)>
{
    ("Name: {{Name}}\nDoc: {{Doc}}", leftPlaceholders),
    ("Date: {{Date}}\nOrder: {{Order}}", centerPlaceholders),
    ("Total: {{Total}}\nStatus: {{Status}}", rightPlaceholders)
};

byte[] pdf = composer.GeneratePdf(columns, config);
```

### Modes 6 & 7 — Styled Tables

```csharp
config.Table = new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,   // Bordered | Striped | Minimal | Grid
    HeaderColor = "#1E3A5F",
    HeaderTextBold = true,
    AlternatingRowColor = "#F0F4F8",
    FontSize = 10,
    RowPadding = 5
};

DataTable table = new DataTable();
table.Columns.Add("Product");
table.Columns.Add("Qty");
table.Columns.Add("Value");
table.Rows.Add("Notebook", 2, "$12,000.00");

byte[] pdf = composer.GeneratePdf(table, config);

var headerModel = new { Client = "ACME Ltd", CNPJ = "00.000.000/0001-00", Date = "08/07/2026" };
var tableConfig = new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,
    HeaderColor = "#1E3A5F"
};

byte[] pdf = composer.GeneratePdf(table, headerModel, tableConfig, config);
```

### License

GPL v2 — free for commercial and personal use.
## Português

**Biblioteca .NET para geração de PDFs a partir de templates, modelos (T) ou DataTables**, com suporte completo a **cabeçalhos com logotipo**, **paginação no rodapé**, **estilos e cores de fontes**, **marca-d'água**, **tabelas estilizadas**, **layout multi-colunas** e placeholders altamente customizáveis.

Baseada em *QuestPDF* e compatível com **.NET Standard 2.1+**.

Principais recursos:
- **Composição de template/conteúdo customizado** — templates inline com chaves `{{Placeholder}}` e estilos individuais por placeholder
- **Fontes embutidas** — Liberation Sans, Comic Neue, JetBrains Mono, Montserrat, Liberation Serif ou fontes customizadas
- **Estilos e alinhamento de texto** — negrito, itálico, sublinhado, tamanho e cor por placeholder
- **Layout multi-colunas e formato de tabela** — colunas lado a lado com proporções configuráveis e dados tabulares estilizados

### Instalação

```bash
dotnet add package Rochas.PDFGenerator
```

### Visão Geral

A classe principal é `PDFComposer`, oferecendo **7 modos** de geração de PDF:

| Modo | Descrição |
|------|-----------|
| **Template + Placeholders** | Substituição de chaves `{{Nome}}` com estilos individuais |
| **Modelo Genérico (T)** | Objeto mapeado automaticamente para placeholders |
| **DataTable** | PDF tabular com cabeçalhos/linhas |
| **Multi-Colunas** | Dois templates lado a lado com proporções configuráveis |
| **Lista Multi-Colunas** | N colunas via lista de templates |
| **Tabela Estilizada** | DataTable com bordas, cores alternadas, header customizado |
| **Tabela + Model Header** | Header com dados do objeto + tabela de itens |

### Configuração (PdfConfig)

A classe `PdfConfig` centraliza todas as configurações:

```csharp
var config = new PdfConfig
{
    MarginLeft = 40,
    MarginRight = 40,
    MarginTop = 50,
    MarginBottom = 50,

    FontFamily = PdfFontFamily.Montserrat,
    CustomFontBytes = File.ReadAllBytes("fonts/Montserrat-Regular.ttf"),

    Header = new PdfHeaderConfig
    {
        LogoBytes = File.ReadAllBytes("images/logo.png"),
        LogoAlign = PdfLogoAlignment.Left,
        Title = "Report Title"
    },

    WatermarkBytes = File.ReadAllBytes("images/watermark.png"),
    WatermarkOpacity = 30,
    FooterPagination = true
};
```

> **Compatibilidade:** `PdfPageConfiguration` e `PdfHeaderComposition` continuam funcionando como aliases.

O construtor de `PDFComposer` também aceita metadados do documento: `author`, `title`, `subject` e `creationDate`.

### Modo 1 — Template + Placeholders (Início Rápido)

```csharp
var composer = new PDFComposer();
var template = "Client: {{Name}}\nDate: {{Date}}";

var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Name}}", Style = new PdfPlaceHolderStyle { Bold = true, FontSizePx = 16 } }, "ACME Ltd" },
    { new PdfBodyPlaceHolder { Key = "{{Date}}", Style = new PdfPlaceHolderStyle { Italic = true } }, DateTime.Now.ToString("dd/MM/yyyy") }
};

byte[] pdf = composer.GeneratePdf(template, placeholders, config);
```

### Modos 2 e 3 — Modelo Genérico (T) e DataTable

```csharp
var client = new { Name = "ACME Ltd", Document = "00.000.000/0001-00" };
byte[] pdf = composer.GeneratePdf("Client: {{Name}}\nDoc: {{Document}}", client, config);

DataTable table = new DataTable();
table.Columns.Add("Product");
table.Columns.Add("Quantity");
table.Rows.Add("Notebook", 10);
byte[] pdf = composer.GeneratePdf(table, config);
```

### Modos 4 e 5 — Multi-Colunas (2 ou N colunas)

```csharp
config.Columns = new PdfColumnConfig { Count = 2, Ratios = new[] { 60f, 40f }, Gap = 10 };

var leftPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Name}}" }, "ACME Ltd" },
    { new PdfBodyPlaceHolder { Key = "{{Doc}}" }, "00.000.000/0001-00" }
};

var rightPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Date}}" }, "08/07/2026" },
    { new PdfBodyPlaceHolder { Key = "{{Total}}" }, "$1,500.00" }
};

byte[] pdf = composer.GeneratePdf(
    "Name: {{Name}}\nDoc: {{Doc}}", leftPlaceholders,
    "Date: {{Date}}\nTotal: {{Total}}", rightPlaceholders,
    config);

// N colunas via lista de tuplas (template, placeholders)
var columns = new List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)>
{
    ("Name: {{Name}}\nDoc: {{Doc}}", leftPlaceholders),
    ("Date: {{Date}}\nOrder: {{Order}}", centerPlaceholders),
    ("Total: {{Total}}\nStatus: {{Status}}", rightPlaceholders)
};

byte[] pdf = composer.GeneratePdf(columns, config);
```

### Modos 6 e 7 — Tabelas Estilizadas

```csharp
config.Table = new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,   // Bordered | Striped | Minimal | Grid
    HeaderColor = "#1E3A5F",
    HeaderTextBold = true,
    AlternatingRowColor = "#F0F4F8",
    FontSize = 10,
    RowPadding = 5
};

DataTable table = new DataTable();
table.Columns.Add("Product");
table.Columns.Add("Qty");
table.Columns.Add("Value");
table.Rows.Add("Notebook", 2, "$12,000.00");

byte[] pdf = composer.GeneratePdf(table, config);

var headerModel = new { Client = "ACME Ltd", CNPJ = "00.000.000/0001-00", Date = "08/07/2026" };
var tableConfig = new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,
    HeaderColor = "#1E3A5F"
};

byte[] pdf = composer.GeneratePdf(table, headerModel, tableConfig, config);
```

### Licença

GPL v2 — livre para uso comercial e pessoal.
## Español

**Biblioteca .NET para generar PDFs a partir de plantillas, modelos (T) o DataTables**, con soporte completo de **encabezados con logotipo**, **paginación en el pie de página**, **estilos y colores de fuente**, **marca de agua**, **tablas estilizadas**, **diseño multi-columna** y placeholders altamente personalizables.

Basada en *QuestPDF* y compatible con **.NET Standard 2.1+**.

Características principales:
- **Composición de plantillas/contenido personalizado** — plantillas en línea con claves `{{Placeholder}}` y estilos individuales por placeholder
- **Fuentes integradas** — Liberation Sans, Comic Neue, JetBrains Mono, Montserrat, Liberation Serif o fuentes personalizadas
- **Estilos y alineación de texto** — negrita, cursiva, subrayado, tamaño y color por placeholder
- **Formato multi-columna y de tabla** — columnas lado a lado con proporciones configurables y datos tabulares estilizados

### Instalación

```bash
dotnet add package Rochas.PDFGenerator
```

### Descripción general

La clase principal es `PDFComposer`, que ofrece **7 modos** de generación de PDF:

| Modo | Descripción |
|------|-------------|
| **Template + Placeholders** | Reemplaza claves `{{Nombre}}` con estilos individuales |
| **Modelo Genérico (T)** | Objeto asignado automáticamente a placeholders |
| **DataTable** | PDF tabular con encabezados/filas |
| **Multi-Columna** | Dos plantillas lado a lado con proporciones configurables |
| **Lista Multi-Columna** | N columnas mediante una lista de plantillas |
| **Tabla Estilizada** | DataTable con bordes, filas alternadas, encabezado personalizado |
| **Tabla + Modelo en Encabezado** | Encabezado con datos del objeto + tabla de ítems |

### Configuración (PdfConfig)

La clase `PdfConfig` centraliza todas las configuraciones:

```csharp
var config = new PdfConfig
{
    MarginLeft = 40,
    MarginRight = 40,
    MarginTop = 50,
    MarginBottom = 50,

    FontFamily = PdfFontFamily.Montserrat,
    CustomFontBytes = File.ReadAllBytes("fonts/Montserrat-Regular.ttf"),

    Header = new PdfHeaderConfig
    {
        LogoBytes = File.ReadAllBytes("images/logo.png"),
        LogoAlign = PdfLogoAlignment.Left,
        Title = "Report Title"
    },

    WatermarkBytes = File.ReadAllBytes("images/watermark.png"),
    WatermarkOpacity = 30,
    FooterPagination = true
};
```

> **Retrocompatible:** `PdfPageConfiguration` y `PdfHeaderComposition` siguen funcionando como alias.

El constructor de `PDFComposer` también acepta metadatos del documento: `author`, `title`, `subject` y `creationDate`.

### Modo 1 — Template + Placeholders (Inicio Rápido)

```csharp
var composer = new PDFComposer();
var template = "Client: {{Name}}\nDate: {{Date}}";

var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Name}}", Style = new PdfPlaceHolderStyle { Bold = true, FontSizePx = 16 } }, "ACME Ltd" },
    { new PdfBodyPlaceHolder { Key = "{{Date}}", Style = new PdfPlaceHolderStyle { Italic = true } }, DateTime.Now.ToString("dd/MM/yyyy") }
};

byte[] pdf = composer.GeneratePdf(template, placeholders, config);
```

### Modos 2 y 3 — Modelo Genérico (T) y DataTable

```csharp
var client = new { Name = "ACME Ltd", Document = "00.000.000/0001-00" };
byte[] pdf = composer.GeneratePdf("Client: {{Name}}\nDoc: {{Document}}", client, config);

DataTable table = new DataTable();
table.Columns.Add("Product");
table.Columns.Add("Quantity");
table.Rows.Add("Notebook", 10);
byte[] pdf = composer.GeneratePdf(table, config);
```

### Modos 4 y 5 — Multi-Columna (2 o N columnas)

```csharp
config.Columns = new PdfColumnConfig { Count = 2, Ratios = new[] { 60f, 40f }, Gap = 10 };

var leftPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Name}}" }, "ACME Ltd" },
    { new PdfBodyPlaceHolder { Key = "{{Doc}}" }, "00.000.000/0001-00" }
};

var rightPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Date}}" }, "08/07/2026" },
    { new PdfBodyPlaceHolder { Key = "{{Total}}" }, "$1,500.00" }
};

byte[] pdf = composer.GeneratePdf(
    "Name: {{Name}}\nDoc: {{Doc}}", leftPlaceholders,
    "Date: {{Date}}\nTotal: {{Total}}", rightPlaceholders,
    config);

// N columnas mediante una lista de tuplas (template, placeholders)
var columns = new List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)>
{
    ("Name: {{Name}}\nDoc: {{Doc}}", leftPlaceholders),
    ("Date: {{Date}}\nOrder: {{Order}}", centerPlaceholders),
    ("Total: {{Total}}\nStatus: {{Status}}", rightPlaceholders)
};

byte[] pdf = composer.GeneratePdf(columns, config);
```

### Modos 6 y 7 — Tablas Estilizadas

```csharp
config.Table = new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,   // Bordered | Striped | Minimal | Grid
    HeaderColor = "#1E3A5F",
    HeaderTextBold = true,
    AlternatingRowColor = "#F0F4F8",
    FontSize = 10,
    RowPadding = 5
};

DataTable table = new DataTable();
table.Columns.Add("Product");
table.Columns.Add("Qty");
table.Columns.Add("Value");
table.Rows.Add("Notebook", 2, "$12,000.00");

byte[] pdf = composer.GeneratePdf(table, config);

var headerModel = new { Client = "ACME Ltd", CNPJ = "00.000.000/0001-00", Date = "08/07/2026" };
var tableConfig = new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,
    HeaderColor = "#1E3A5F"
};

byte[] pdf = composer.GeneratePdf(table, headerModel, tableConfig, config);
```

### Licencia

GPL v2 — libre para uso comercial y personal.
## Deutsch

**.NET-Bibliothek zum Erzeugen von PDFs aus Vorlagen, Modellen (T) oder DataTables** – mit vollständiger Unterstützung für **Kopfzeilen mit Logo**, **Seitennummern in der Fußzeile**, **Schriftstile und -farben**, **Wasserzeichen**, **formatierte Tabellen**, **mehrspaltige Layouts** und hochgradig anpassbare Platzhalter.

Basiert auf *QuestPDF* und ist mit **.NET Standard 2.1+** kompatibel.

Wichtigste Funktionen:
- **Vorlagen-/benutzerdefinierte Inhaltskomposition** — Inline-Vorlagen mit `{{Placeholder}}`-Schlüsseln und individuellen Platzhalterstilen
- **Eingebettete Schriftarten** — Liberation Sans, Comic Neue, JetBrains Mono, Montserrat, Liberation Serif oder benutzerdefinierte Schriftarten
- **Formatierung und Textausrichtung** — fett, kursiv, unterstrichen, Schriftgröße und -farbe pro Platzhalter
- **Mehrspaltiges und Tabellenformat** — Spalten nebeneinander mit konfigurierbaren Verhältnissen und formatierte Tabellendaten

### Installation

```bash
dotnet add package Rochas.PDFGenerator
```

### Überblick

Die Hauptklasse ist `PDFComposer` und bietet **7 Modi** zur PDF-Erzeugung:

| Modus | Beschreibung |
|-------|--------------|
| **Vorlage + Platzhalter** | Ersetzt Schlüssel `{{Name}}` mit individuellen Stilen |
| **Generisches Modell (T)** | Objekt wird automatisch auf Platzhalter abgebildet |
| **DataTable** | Tabellarisches PDF mit Kopfzeilen/Zeilen |
| **Mehrspaltig** | Zwei Vorlagen nebeneinander mit konfigurierbaren Verhältnissen |
| **Mehrspaltige Liste** | N Spalten über eine Vorlagenliste |
| **Formatierte Tabelle** | DataTable mit Rändern, wechselnden Zeilen, benutzerdefinierter Kopfzeile |
| **Tabelle + Modell-Kopfzeile** | Objekt-Kopfzeile + Tabelle mit Einträgen |

### Konfiguration (PdfConfig)

Die Klasse `PdfConfig` bündelt alle Einstellungen:

```csharp
var config = new PdfConfig
{
    MarginLeft = 40,
    MarginRight = 40,
    MarginTop = 50,
    MarginBottom = 50,

    FontFamily = PdfFontFamily.Montserrat,
    CustomFontBytes = File.ReadAllBytes("fonts/Montserrat-Regular.ttf"),

    Header = new PdfHeaderConfig
    {
        LogoBytes = File.ReadAllBytes("images/logo.png"),
        LogoAlign = PdfLogoAlignment.Left,
        Title = "Report Title"
    },

    WatermarkBytes = File.ReadAllBytes("images/watermark.png"),
    WatermarkOpacity = 30,
    FooterPagination = true
};
```

> **Abwärtskompatibel:** `PdfPageConfiguration` und `PdfHeaderComposition` funktionieren weiterhin als Aliase.

Der Konstruktor von `PDFComposer` akzeptiert zusätzlich die Dokument-Metadaten `author`, `title`, `subject` und `creationDate`.

### Modus 1 — Vorlage + Platzhalter (Schnellstart)

```csharp
var composer = new PDFComposer();
var template = "Client: {{Name}}\nDate: {{Date}}";

var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Name}}", Style = new PdfPlaceHolderStyle { Bold = true, FontSizePx = 16 } }, "ACME Ltd" },
    { new PdfBodyPlaceHolder { Key = "{{Date}}", Style = new PdfPlaceHolderStyle { Italic = true } }, DateTime.Now.ToString("dd/MM/yyyy") }
};

byte[] pdf = composer.GeneratePdf(template, placeholders, config);
```

### Modi 2 und 3 — Generisches Modell (T) und DataTable

```csharp
var client = new { Name = "ACME Ltd", Document = "00.000.000/0001-00" };
byte[] pdf = composer.GeneratePdf("Client: {{Name}}\nDoc: {{Document}}", client, config);

DataTable table = new DataTable();
table.Columns.Add("Product");
table.Columns.Add("Quantity");
table.Rows.Add("Notebook", 10);
byte[] pdf = composer.GeneratePdf(table, config);
```

### Modi 4 und 5 — Mehrspaltig (2 oder N Spalten)

```csharp
config.Columns = new PdfColumnConfig { Count = 2, Ratios = new[] { 60f, 40f }, Gap = 10 };

var leftPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Name}}" }, "ACME Ltd" },
    { new PdfBodyPlaceHolder { Key = "{{Doc}}" }, "00.000.000/0001-00" }
};

var rightPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Date}}" }, "08/07/2026" },
    { new PdfBodyPlaceHolder { Key = "{{Total}}" }, "$1,500.00" }
};

byte[] pdf = composer.GeneratePdf(
    "Name: {{Name}}\nDoc: {{Doc}}", leftPlaceholders,
    "Date: {{Date}}\nTotal: {{Total}}", rightPlaceholders,
    config);

// N Spalten über eine Liste von (template, placeholders)-Tupeln
var columns = new List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)>
{
    ("Name: {{Name}}\nDoc: {{Doc}}", leftPlaceholders),
    ("Date: {{Date}}\nOrder: {{Order}}", centerPlaceholders),
    ("Total: {{Total}}\nStatus: {{Status}}", rightPlaceholders)
};

byte[] pdf = composer.GeneratePdf(columns, config);
```

### Modi 6 und 7 — Formatierte Tabellen

```csharp
config.Table = new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,   // Bordered | Striped | Minimal | Grid
    HeaderColor = "#1E3A5F",
    HeaderTextBold = true,
    AlternatingRowColor = "#F0F4F8",
    FontSize = 10,
    RowPadding = 5
};

DataTable table = new DataTable();
table.Columns.Add("Product");
table.Columns.Add("Qty");
table.Columns.Add("Value");
table.Rows.Add("Notebook", 2, "$12,000.00");

byte[] pdf = composer.GeneratePdf(table, config);

var headerModel = new { Client = "ACME Ltd", CNPJ = "00.000.000/0001-00", Date = "08/07/2026" };
var tableConfig = new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,
    HeaderColor = "#1E3A5F"
};

byte[] pdf = composer.GeneratePdf(table, headerModel, tableConfig, config);
```

### Lizenz

GPL v2 — frei für kommerzielle und private Nutzung.
## Français

**Bibliothèque .NET pour générer des PDF à partir de modèles, de modèles (T) ou de DataTables**, avec prise en charge complète des **en-têtes avec logo**, de la **pagination en pied de page**, des **styles et couleurs de police**, des **filigranes**, des **tableaux stylisés**, des **dispositions multi-colonnes** et de placeholders hautement personnalisables.

Basée sur *QuestPDF* et compatible avec **.NET Standard 2.1+**.

Principales fonctionnalités :
- **Composition de modèle/contenu personnalisé** — modèles en ligne avec clés `{{Placeholder}}` et styles individuels par placeholder
- **Polices intégrées** — Liberation Sans, Comic Neue, JetBrains Mono, Montserrat, Liberation Serif ou polices personnalisées
- **Styles et alignement du texte** — gras, italique, souligné, taille et couleur par placeholder
- **Format multi-colonnes et tableau** — colonnes côte à côte avec proportions configurables et données tabulaires stylisées

### Installation

```bash
dotnet add package Rochas.PDFGenerator
```

### Vue d'ensemble

La classe principale est `PDFComposer`, qui propose **7 modes** de génération de PDF :

| Mode | Description |
|------|-------------|
| **Modèle + Placeholders** | Remplace les clés `{{Nom}}` avec des styles individuels |
| **Modèle générique (T)** | Objet mappé automatiquement sur les placeholders |
| **DataTable** | PDF tabulaire avec en-têtes/lignes |
| **Multi-colonnes** | Deux modèles côte à côte avec proportions configurables |
| **Liste multi-colonnes** | N colonnes via une liste de modèles |
| **Tableau stylisé** | DataTable avec bordures, lignes alternées, en-tête personnalisé |
| **Tableau + modèle d'en-tête** | En-tête avec les données de l'objet + tableau des éléments |

### Configuration (PdfConfig)

La classe `PdfConfig` centralise tous les réglages :

```csharp
var config = new PdfConfig
{
    MarginLeft = 40,
    MarginRight = 40,
    MarginTop = 50,
    MarginBottom = 50,

    FontFamily = PdfFontFamily.Montserrat,
    CustomFontBytes = File.ReadAllBytes("fonts/Montserrat-Regular.ttf"),

    Header = new PdfHeaderConfig
    {
        LogoBytes = File.ReadAllBytes("images/logo.png"),
        LogoAlign = PdfLogoAlignment.Left,
        Title = "Report Title"
    },

    WatermarkBytes = File.ReadAllBytes("images/watermark.png"),
    WatermarkOpacity = 30,
    FooterPagination = true
};
```

> **Rétrocompatible :** `PdfPageConfiguration` et `PdfHeaderComposition` fonctionnent toujours comme alias.

Le constructeur de `PDFComposer` accepte également les métadonnées du document `author`, `title`, `subject` et `creationDate`.

### Mode 1 — Modèle + Placeholders (Démarrage rapide)

```csharp
var composer = new PDFComposer();
var template = "Client: {{Name}}\nDate: {{Date}}";

var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Name}}", Style = new PdfPlaceHolderStyle { Bold = true, FontSizePx = 16 } }, "ACME Ltd" },
    { new PdfBodyPlaceHolder { Key = "{{Date}}", Style = new PdfPlaceHolderStyle { Italic = true } }, DateTime.Now.ToString("dd/MM/yyyy") }
};

byte[] pdf = composer.GeneratePdf(template, placeholders, config);
```

### Modes 2 et 3 — Modèle générique (T) et DataTable

```csharp
var client = new { Name = "ACME Ltd", Document = "00.000.000/0001-00" };
byte[] pdf = composer.GeneratePdf("Client: {{Name}}\nDoc: {{Document}}", client, config);

DataTable table = new DataTable();
table.Columns.Add("Product");
table.Columns.Add("Quantity");
table.Rows.Add("Notebook", 10);
byte[] pdf = composer.GeneratePdf(table, config);
```

### Modes 4 et 5 — Multi-colonnes (2 ou N colonnes)

```csharp
config.Columns = new PdfColumnConfig { Count = 2, Ratios = new[] { 60f, 40f }, Gap = 10 };

var leftPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Name}}" }, "ACME Ltd" },
    { new PdfBodyPlaceHolder { Key = "{{Doc}}" }, "00.000.000/0001-00" }
};

var rightPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Date}}" }, "08/07/2026" },
    { new PdfBodyPlaceHolder { Key = "{{Total}}" }, "$1,500.00" }
};

byte[] pdf = composer.GeneratePdf(
    "Name: {{Name}}\nDoc: {{Doc}}", leftPlaceholders,
    "Date: {{Date}}\nTotal: {{Total}}", rightPlaceholders,
    config);

// N colonnes via une liste de tuples (modèle, placeholders)
var columns = new List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)>
{
    ("Name: {{Name}}\nDoc: {{Doc}}", leftPlaceholders),
    ("Date: {{Date}}\nOrder: {{Order}}", centerPlaceholders),
    ("Total: {{Total}}\nStatus: {{Status}}", rightPlaceholders)
};

byte[] pdf = composer.GeneratePdf(columns, config);
```

### Modes 6 et 7 — Tableaux stylisés

```csharp
config.Table = new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,   // Bordered | Striped | Minimal | Grid
    HeaderColor = "#1E3A5F",
    HeaderTextBold = true,
    AlternatingRowColor = "#F0F4F8",
    FontSize = 10,
    RowPadding = 5
};

DataTable table = new DataTable();
table.Columns.Add("Product");
table.Columns.Add("Qty");
table.Columns.Add("Value");
table.Rows.Add("Notebook", 2, "$12,000.00");

byte[] pdf = composer.GeneratePdf(table, config);

var headerModel = new { Client = "ACME Ltd", CNPJ = "00.000.000/0001-00", Date = "08/07/2026" };
var tableConfig = new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,
    HeaderColor = "#1E3A5F"
};

byte[] pdf = composer.GeneratePdf(table, headerModel, tableConfig, config);
```

### Licence

GPL v2 — libre pour un usage commercial et personnel.
