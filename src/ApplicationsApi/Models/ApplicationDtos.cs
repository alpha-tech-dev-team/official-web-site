using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ApplicationsApi.Models
{
    public record ApplicationRequest
    {
        public string? ApplicantName { get; set; }
        public string? Phone { get; set; }
        public Dictionary<string, JsonElement>? Data { get; set; }
    }

    public record ApplicationResponse
    {
        public Guid Id { get; set; }
        public string? Number { get; set; }
        public string PublicCode { get; set; } = "";
        public string Status { get; set; } = "";
        public string ApplicantName { get; set; } = "";
        public string Phone { get; set; } = "";
        public Dictionary<string, JsonElement> Data { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public static ApplicationResponse From(Application a)
        {
            return new ApplicationResponse
            {
                Id = a.Id,
                Number = a.Number,
                PublicCode = a.PublicCode,
                Status = a.Status.ToString().ToLower(),
                ApplicantName = a.ApplicantName,
                Phone = a.Phone,
                Data = a.Data,
                CreatedAt = a.CreatedAt,
                SubmittedAt = a.SubmittedAt,
                UpdatedAt = a.UpdatedAt
            };
        }
    }
}