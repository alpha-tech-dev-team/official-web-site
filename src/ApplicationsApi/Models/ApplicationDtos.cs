using System.Text.Json;

namespace ApplicationsApi.Models;

public record ApplicationRequest(
    string? ApplicantName,
    string? Phone,
    Dictionary<string, JsonElement>? Data);

public record ApplicationResponse(
    Guid Id,
    string? Number,
    string PublicCode,
    string Status, 
    string ApplicantName,
    string Phone,
    Dictionary<string, JsonElement> Data,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime UpdatedAt)
{
    public static ApplicationResponse From(Application a) => new(
        a.Id, a.Number, a.PublicCode, a.Status.ToString().ToLowerInvariant(),
        a.ApplicantName, a.Phone, a.Data, a.CreatedAt, a.SubmittedAt, a.UpdatedAt);
}