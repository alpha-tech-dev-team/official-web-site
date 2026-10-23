using ApplicationsApi.Models;
using ApplicationsApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IApplicationRepository, InMemoryApplicationRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var origin = Environment.GetEnvironmentVariable("FRONTEND_ORIGIN") ?? "http://localhost:5173";
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origin).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

var group = app.MapGroup("/api/applications");

group.MapPost("/", async (ApplicationRequest req, IApplicationRepository repo) =>
{
    var errors = Validate(req, out var phone);
    if (errors.Count > 0) return Results.ValidationProblem(errors);

    var entity = new Application
    {
        ApplicantName = req.ApplicantName!.Trim(),
        Phone = phone,
        Data = req.Data ?? new()
    };
    await repo.AddAsync(entity);
    return Results.Created($"/api/applications/{entity.Id}", ApplicationResponse.From(entity));
});

group.MapGet("/{id:guid}", async (Guid id, IApplicationRepository repo) =>
{
    var entity = await repo.GetAsync(id);
    return entity is null ? Results.NotFound() : Results.Ok(ApplicationResponse.From(entity));
});

group.MapGet("/", async (IApplicationRepository repo) =>
    Results.Ok((await repo.ListAsync()).Select(ApplicationResponse.From)));

group.MapPut("/{id:guid}", async (Guid id, ApplicationRequest req, IApplicationRepository repo) =>
{
    var entity = await repo.GetAsync(id);
    if (entity is null) return Results.NotFound();
    if (entity.Status != ApplicationStatus.Draft)
        return Results.Conflict(new { error = "Поданную заявку нельзя изменить" });

    var errors = Validate(req, out var phone);
    if (errors.Count > 0) return Results.ValidationProblem(errors);

    entity.ApplicantName = req.ApplicantName!.Trim();
    entity.Phone = phone;
    entity.Data = req.Data ?? new();
    entity.UpdatedAt = DateTime.UtcNow;
    await repo.UpdateAsync(entity);
    return Results.Ok(ApplicationResponse.From(entity));
});

group.MapPost("/{id:guid}/submit", async (Guid id, IApplicationRepository repo) =>
{
    var entity = await repo.GetAsync(id);
    if (entity is null) return Results.NotFound();
    if (entity.Status != ApplicationStatus.Draft)
        return Results.Conflict(new { error = "Заявка уже подана" });

    entity.Number = await repo.NextNumberAsync();
    entity.Status = ApplicationStatus.Submitted;
    entity.SubmittedAt = DateTime.UtcNow;
    entity.UpdatedAt = DateTime.UtcNow;
    await repo.UpdateAsync(entity);
    return Results.Ok(ApplicationResponse.From(entity));
});

app.Run();

static Dictionary<string, string[]> Validate(ApplicationRequest req, out string phone)
{
    var errors = new Dictionary<string, string[]>();

    var name = req.ApplicantName?.Trim() ?? "";
    if (name.Length < 2 || name.Length > 200)
        errors["applicantName"] = new[] { "Укажите ФИО (от 2 до 200 символов)" };

    phone = Phone.Normalize(req.Phone) ?? "";
    if (phone == "")
        errors["phone"] = new[] { "Введите белорусский мобильный номер, например +375 29 123-45-67" };

    if (req.Data is { Count: > 50 })
        errors["data"] = new[] { "Слишком много полей (максимум 50)" };
    else if (req.Data is not null && req.Data.Keys.Any(k => k.Length is 0 or > 100))
        errors["data"] = new[] { "Имя поля должно быть от 1 до 100 символов" };

    return errors;
}