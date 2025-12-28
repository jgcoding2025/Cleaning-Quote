using CleaningQuote.Api.Data;
using CleaningQuote.Api.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CleaningQuotes")
    ?? builder.Configuration["CLEANING_QUOTES_DB"]
    ?? "Data Source=CleaningQuotesTracker.db;Cache=Shared;Mode=ReadWriteCreate";

var db = new ApiDb(connectionString);
ApiDatabaseInitializer.Initialize(db);

builder.Services.AddSingleton(db);
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<TokenRepository>();
builder.Services.AddSingleton<QuoteRepository>();

var app = builder.Build();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api")
        && !context.Request.Path.StartsWithSegments("/api/auth")
        && !context.Request.Path.StartsWithSegments("/api/health"))
    {
        var tokenRepository = context.RequestServices.GetRequiredService<TokenRepository>();
        var header = context.Request.Headers.Authorization.ToString();
        var token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header["Bearer ".Length..].Trim()
            : string.Empty;

        if (!tokenRepository.ValidateToken(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ApiError("unauthorized", "Missing or invalid token."));
            return;
        }
    }

    await next();
});

app.MapPost("/api/auth/register", (
    RegisterRequest request,
    UserRepository users,
    TokenRepository tokens) =>
{
    if (users.HasAnyUsers())
    {
        return Results.Problem(statusCode: StatusCodes.Status403Forbidden,
            title: "Registration closed",
            detail: "A user already exists. Registration is closed.");
    }

    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new ApiError("invalid_request", "Email and password are required."));
    }

    if (request.Password.Length < 8)
    {
        return Results.BadRequest(new ApiError("weak_password", "Password must be at least 8 characters."));
    }

    var userId = users.CreateUser(request.Email, request.Password);
    var (token, expiresAtUtc) = tokens.CreateToken(userId, TimeSpan.FromDays(30));

    return Results.Ok(new AuthResponse(token, expiresAtUtc));
});

app.MapPost("/api/auth/login", (
    LoginRequest request,
    UserRepository users,
    TokenRepository tokens) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new ApiError("invalid_request", "Email and password are required."));
    }

    var userId = users.ValidateCredentials(request.Email, request.Password);
    if (userId is null)
    {
        return Results.BadRequest(new ApiError("invalid_credentials", "Email or password is incorrect."));
    }

    var (token, expiresAtUtc) = tokens.CreateToken(userId.Value, TimeSpan.FromDays(30));
    return Results.Ok(new AuthResponse(token, expiresAtUtc));
});

app.MapGet("/api/quotes", (QuoteRepository quotes) =>
{
    return Results.Ok(quotes.GetSummaries());
});

app.MapGet("/api/quotes/{quoteId:guid}", (Guid quoteId, QuoteRepository quotes) =>
{
    var quote = quotes.GetById(quoteId);
    return quote is null
        ? Results.NotFound(new ApiError("not_found", "Quote not found."))
        : Results.Ok(quote);
});

app.MapPut("/api/quotes/{quoteId:guid}", (Guid quoteId, QuoteDetail quote, QuoteRepository quotes) =>
{
    if (quote.QuoteId != quoteId)
    {
        return Results.BadRequest(new ApiError("id_mismatch", "Quote ID in body does not match route."));
    }

    var updated = quotes.Update(quote);
    return updated
        ? Results.NoContent()
        : Results.NotFound(new ApiError("not_found", "Quote not found."));
});

app.Run();
