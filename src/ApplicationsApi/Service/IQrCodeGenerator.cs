namespace ApplicationsApi.Services;

public interface IQrCodeGenerator
{
    byte[] GeneratePng(string content);
}
