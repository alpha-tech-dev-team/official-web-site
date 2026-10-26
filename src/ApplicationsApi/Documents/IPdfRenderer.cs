using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApplicationsApi.Documents;

public interface IPdfRenderer
{
    byte[] Render(string text);
}

public class QuestPdfRenderer : IPdfRenderer
{
    public byte[] Render(string text)
    {
        var lines = (text ?? string.Empty)
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n');

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

                        if (i == 0)
                            column.Item().Text(line).FontSize(18).Bold();  
                        else if (line.Length == 0)
                            column.Item().Height(8);                     
                        else
                            column.Item().Text(line);
                    }
                });
            });
        }).GeneratePdf();
    }
}