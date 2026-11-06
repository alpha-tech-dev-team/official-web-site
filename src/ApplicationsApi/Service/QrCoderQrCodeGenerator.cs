using QRCoder;

namespace ApplicationsApi.Services;

public sealed class QrCoderQrCodeGenerator : IQrCodeGenerator
{
    public byte[] GeneratePng(string content)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);

        return new PngByteQRCode(data).GetGraphic(10);
    }
}
