namespace ApplicationsApi.Services;

public interface IPublicVerifyUrlBuilder
{
    string Build(string publicCode);
}

public sealed class PublicVerifyUrlBuilder : IPublicVerifyUrlBuilder
{
    private readonly string _baseUrl;

    public PublicVerifyUrlBuilder(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException(
                "PUBLIC_VERIFY_BASE_URL должен быть абсолютным http(s)-адресом",
                nameof(baseUrl));
        }

        _baseUrl = uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
    }

    public string Build(string publicCode) =>
        $"{_baseUrl}/verify/{Uri.EscapeDataString(publicCode)}";
}
