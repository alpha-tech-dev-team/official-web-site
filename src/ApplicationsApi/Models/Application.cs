
using System;
using System.Collections.Generic;
using System.Text.Json;
using ApplicationsApi.Services;

namespace ApplicationsApi.Models
{
    public enum ApplicationStatus
    {
        Draft,
        Submitted,
        Approved,
        Rejected,
        Cancelled
    }

    public class Application
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? Number { get; set; }
        public string PublicCode { get; set; } = CodeGenerator.NewPublicCode();

        public string ApplicantName { get; set; } = "";
        public string Phone { get; set; } = "";

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;

        public Dictionary<string,  JsonElement> Data { get; set; } = new Dictionary<string, JsonElement>();

        public int TemplateVersion { get;  set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
