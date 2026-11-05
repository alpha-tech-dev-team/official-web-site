using System.Text;
using System.Threading.RateLimiting;
using ApplicationsApi.Auth;
using ApplicationsApi.Documents;
using ApplicationsApi.Models;
using ApplicationsApi.Services;
using ApplicationsApi.Telegram;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

string Env(string name) => Environment.GetEnvironmentVariable(name)
    ?? throw new Exception($"Задай переменную окружения {name}");

var jwtKey = Env("JWT_KEY");
if (jwtKey.Length < 32) throw new Exception("JWT_KEY должен быть не короче 32 символов");
var adminLogin = Env("ADMIN_LOGIN");
var adminPassword = Env("ADMIN_PASSWORD");
var botUsername = Env("BOT_USERNAME");
var botToken = Env("BOT_TOKEN");

builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

builder.Services.AddSingleton<IApplicationRepository, InMemoryApplicationRepository>();
builder.Services.AddSingleton<IVerificationSessionRepository, InMemoryVerificationSessionRepository>();
builder.Services.AddSingleton<IPhoneVerificationRepository, InMemoryPhoneVerificationRepository>();
builder.Services.AddSingleton<VerificationService>();
builder.Services.AddSingleton<ITemplateStore, FileTemplateStore>();
builder.Services.AddSingleton<IPdfRenderer, QuestPdfRenderer>();
builder.Services.AddSingleton(new AdminStore(adminLogin, adminPassword));
builder.Services.AddSingleton(new TokenService(jwtKey));
builder.Services.AddSingleton(new TelegramOptions(botToken));
builder.Services.AddHttpClient();
builder.Services.AddHostedService<BotPoller>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var origin = Environment.GetEnvironmentVariable("FRONTEND_ORIGIN") ?? "http://localhost:5173";
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origin).AllowAnyHeader().AllowAnyMethod()));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TokenService.Issuer,
            ValidateAudience = true,
            ValidAudience = TokenService.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization(o => o.AddPolicy("Admin", p => p.RequireRole("admin")));

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    o.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    o.AddPolicy("verification", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    o.AddPolicy("verification-status", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/login", (LoginRequest req, AdminStore admins, TokenService tokens) =>
        admins.Verify(req.Login, req.Password)
            ? Results.Ok(new { token = tokens.CreateToken(req.Login!), expiresInMinutes = TokenService.LifetimeMinutes })
            : Results.Json(new { error = "Неверный логин или пароль" }, statusCode: StatusCodes.Status401Unauthorized))
    .RequireRateLimiting("login");

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

app.MapPost("/api/verification/start", async (StartVerificationRequest req, VerificationService verification) =>
{
    if (req.ApplicationId == Guid.Empty)
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["applicationId"] = new[] { "Укажите applicationId" }
        });

    var result = await verification.StartAsync(req.ApplicationId, req.Phone);

    return result.Outcome switch
    {
        StartOutcome.Started => Results.Ok(new StartVerificationResponse(
            result.Verification!.SessionId,
            $"https://t.me/{botUsername}?start={result.Verification.Token}",
            result.Verification.ExpiresAt)),
        StartOutcome.InvalidPhone => Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["phone"] = new[] { "Введите белорусский мобильный номер, например +375 29 123-45-67" }
        }),
        StartOutcome.ApplicationNotFound => Results.NotFound(),
        StartOutcome.ApplicationNotAllowed =>
            Results.Conflict(new { error = "Для этой заявки верификация недоступна" }),
        StartOutcome.PhoneMismatch =>
            Results.Conflict(new { error = "Номер не совпадает с номером в заявке" }),
        _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
    };
}).RequireRateLimiting("verification");

app.MapGet("/api/verification/{sessionId:guid}", async (Guid sessionId, IVerificationSessionRepository sessions) =>
{
    var session = await sessions.GetAsync(sessionId);
    if (session is null) return Results.NotFound();

    var status = session.Status == VerificationStatus.Pending && session.ExpiresAt < DateTime.UtcNow
        ? VerificationStatus.Expired
        : session.Status;

    return Results.Ok(new VerificationStatusResponse(
        status.ToString().ToLowerInvariant(), session.Reason, session.ExpiresAt));
}).RequireRateLimiting("verification-status");

var admin = app.MapGroup("/api/admin").RequireAuthorization("Admin");

admin.MapGet("/applications", async (IApplicationRepository repo) =>
    Results.Ok((await repo.ListAsync()).Select(ApplicationResponse.From)));

admin.MapGet("/applications/{id:guid}", async (Guid id, IApplicationRepository repo) =>
{
    var entity = await repo.GetAsync(id);
    return entity is null ? Results.NotFound() : Results.Ok(ApplicationResponse.From(entity));
});

admin.MapGet("/applications/{id:guid}/preview", async (
    Guid id, IApplicationRepository repo, ITemplateStore templates) =>
{
    var entity = await repo.GetAsync(id);
    if (entity is null) return Results.NotFound();

    var template = templates.Load(entity.TemplateVersion);
    var text = TemplateEngine.Render(template, ApplicationValues.From(entity));
    return Results.Text(text, "text/plain; charset=utf-8");
});

admin.MapGet("/applications/{id:guid}/pdf", async (
    Guid id, IApplicationRepository repo, ITemplateStore templates, IPdfRenderer pdf) =>
{
    var entity = await repo.GetAsync(id);
    if (entity is null) return Results.NotFound();
    if (entity.Status == ApplicationStatus.Draft)
        return Results.Conflict(new { error = "Заявка ещё не подана, печатать нечего" });

    var template = templates.Load(entity.TemplateVersion);
    var text = TemplateEngine.Render(template, ApplicationValues.From(entity));
    return Results.File(pdf.Render(text), "application/pdf");
});

admin.MapGet("/verification-sessions", async (IVerificationSessionRepository sessions) =>
{
    var list = await sessions.ListAsync();
    return Results.Ok(list.Select(s => new
    {
        s.Id,
        s.ApplicationId,
        Status = (s.Status == VerificationStatus.Pending && s.ExpiresAt < DateTime.UtcNow
                ? VerificationStatus.Expired
                : s.Status).ToString().ToLowerInvariant(),
        s.Reason,
        s.ExpiresAt,
        s.CreatedAt
    }));
});

admin.MapGet("/phone-verifications", async (IPhoneVerificationRepository verifications) =>
{
    var list = await verifications.ListAsync();
    return Results.Ok(list.Select(v => new
    {
        v.Id,
        v.ApplicationId,
        Phone = MaskPhone(v.Phone),
        v.Method,
        v.VerifiedAt,
        v.RevokedAt
    }));
});

app.Run();

static string MaskPhone(string phone) =>
    phone.Length >= 5 ? $"{phone[..3]}*****{phone[^2..]}" : "***";

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
