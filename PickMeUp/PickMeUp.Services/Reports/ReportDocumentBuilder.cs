using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PickMeUp.Services.Reports
{
    public class ReportDocumentBuilder : IReportDocumentBuilder
    {
        private static readonly Color BrandColor = Color.FromHex("#3F51B5");
        private static readonly Color MutedColor = Color.FromHex("#6B7280");
        private static readonly Color GridColor = Color.FromHex("#D1D5DB");
        private static readonly Color StripeColor = Color.FromHex("#F3F4F6");

        static ReportDocumentBuilder()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] Render(ReportDefinition definition)
        {
            var generatedAt = DateTime.UtcNow;

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(text => text.FontSize(9));

                    page.Header().Element(header => ComposeHeader(header, definition));
                    page.Content().Element(content => ComposeContent(content, definition));
                    page.Footer().Element(footer => ComposeFooter(footer, generatedAt));
                });
            }).GeneratePdf();
        }

        private static void ComposeHeader(IContainer container, ReportDefinition definition)
        {
            container.PaddingBottom(12).Column(column =>
            {
                column.Item().Text("PickMeUp").FontSize(14).SemiBold().FontColor(BrandColor);
                column.Item().PaddingTop(2).Text(definition.Title).FontSize(18).Bold();
                column.Item().PaddingTop(2).Text(definition.PeriodLabel).FontSize(10).FontColor(MutedColor);

                if (!string.IsNullOrWhiteSpace(definition.Scope))
                {
                    column.Item().Text(definition.Scope).FontSize(10).FontColor(MutedColor);
                }

                column.Item().PaddingTop(8).LineHorizontal(1).LineColor(BrandColor);
            });
        }

        private static void ComposeContent(IContainer container, ReportDefinition definition)
        {
            container.PaddingVertical(6).Column(column =>
            {
                column.Spacing(14);

                if (definition.Summary.Count > 0)
                {
                    column.Item().Background(StripeColor).Padding(8).Column(summary =>
                    {
                        summary.Spacing(3);

                        foreach (var line in definition.Summary)
                        {
                            summary.Item().Text(line).FontSize(10);
                        }
                    });
                }

                foreach (var table in definition.Tables)
                {
                    column.Item().Element(item => ComposeTable(item, table));
                }
            });
        }

        private static void ComposeTable(IContainer container, ReportTable table)
        {
            container.Column(column =>
            {
                column.Spacing(6);

                if (!string.IsNullOrWhiteSpace(table.Caption))
                {
                    column.Item().Text(table.Caption).FontSize(12).SemiBold();
                }

                if (table.Rows.Count == 0)
                {
                    column.Item().Text(table.EmptyMessage).FontSize(10).Italic().FontColor(MutedColor);
                    return;
                }

                column.Item().Table(grid =>
                {
                    grid.ColumnsDefinition(columns =>
                    {
                        foreach (var definition in table.Columns)
                        {
                            columns.RelativeColumn(definition.RelativeWidth);
                        }
                    });

                    grid.Header(header =>
                    {
                        foreach (var definition in table.Columns)
                        {
                            header.Cell()
                                .Background(BrandColor)
                                .Padding(5)
                                .AlignedText(definition.AlignRight, definition.Header)
                                .FontColor(Colors.White)
                                .SemiBold();
                        }
                    });

                    for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                    {
                        var row = table.Rows[rowIndex];
                        var background = rowIndex % 2 == 1 ? StripeColor : Colors.White;

                        for (var cellIndex = 0; cellIndex < table.Columns.Count; cellIndex++)
                        {
                            var value = cellIndex < row.Count ? row[cellIndex] : string.Empty;

                            grid.Cell()
                                .Background(background)
                                .BorderBottom(0.5f)
                                .BorderColor(GridColor)
                                .Padding(5)
                                .AlignedText(table.Columns[cellIndex].AlignRight, value);
                        }
                    }
                });
            });
        }

        private static void ComposeFooter(IContainer container, DateTime generatedAt)
        {
            container.PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text($"Generated {generatedAt:yyyy-MM-dd HH:mm} UTC")
                    .FontSize(8).FontColor(MutedColor);

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(style => style.FontSize(8).FontColor(MutedColor));
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        }
    }

    internal static class ReportTextExtensions
    {
        internal static QuestPDF.Fluent.TextSpanDescriptor AlignedText(
            this IContainer container, bool alignRight, string value)
        {
            var target = alignRight ? container.AlignRight() : container;
            return target.Text(value);
        }
    }
}
