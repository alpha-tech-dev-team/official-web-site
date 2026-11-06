using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApplicationsApi.Documents;

public record PdfQrBlock(byte[] Png, string Url);

public interface IPdfRenderer
{
    byte[] Render(string text, PdfQrBlock? qr = null);
}

public class QuestPdfRenderer : IPdfRenderer
{
    private const string QrMarker = "{{qr}}";

    public byte[] Render(string text, PdfQrBlock? qr = null) =>
        CreateDocument(text, qr).GeneratePdf();

    public IDocument CreateDocument(string text, PdfQrBlock? qr = null)
    {
        var lines = (text ?? string.Empty)
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n');

        if (qr is not null && !lines.Any(line => string.Equals(line.Trim(), QrMarker, StringComparison.Ordinal)))
            throw new InvalidOperationException("В шаблоне документа нет метки {{qr}}");

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(style => style.FontSize(12).FontFamily("Lato"));

                page.Content().Column(column =>
                {
                    column.Spacing(4);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];

                        if (string.Equals(line.Trim(), QrMarker, StringComparison.Ordinal))
                        {
                            if (qr is null)
                                continue;

                            column.Item().PaddingTop(8).AlignCenter().Width(430).Row(row =>
                            {
                                row.ConstantItem(110).Image(qr.Png);
                                row.ConstantItem(12);
                                row.RelativeItem().AlignMiddle().Text($"Проверить: {qr.Url}").FontSize(10);
                            });
                            continue;
                        }

                        if (i == 0)
                            column.Item().Text(line).FontSize(18).Bold();
                        else if (line.Length == 0)
                            column.Item().Height(8);
                        else
                            column.Item().Text(line);
                    }
                });
            });
        });
    }
}
