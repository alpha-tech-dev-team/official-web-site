using System.Text.Json;

namespace ApplicationsApi.Models;

public record ApplicationRequest(
    string? ApplicantName,
    string? Phone,
    Dictionary<string, JsonElement>? Data);

public record ChangePhoneRequest(string? Phone);

public class ApplicationResponse
{
    public Guid Id { get; init; }
    public string? Number { get; init; }
    public string PublicCode { get; init; } = "";
    public string ApplicantName { get; init; } = "";
    public string Phone { get; init; } = "";
    public string Status { get; init; } = "";
    public Dictionary<string, JsonElement> Data { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public static ApplicationResponse From(Application application) => new()
    {
        Id = application.Id,
        Number = application.Number,
        PublicCode = application.PublicCode,
        ApplicantName = application.ApplicantName,
        Phone = Mask(application.Phone),
        Status = application.Status.ToString().ToLowerInvariant(),
        Data = application.Data,
        CreatedAt = application.CreatedAt,
        SubmittedAt = application.SubmittedAt,
        UpdatedAt = application.UpdatedAt
    };

    private static string Mask(string phone) =>
        phone.Length >= 5 ? $"{phone[..3]}****{phone[^2..]}" : "***";
}
