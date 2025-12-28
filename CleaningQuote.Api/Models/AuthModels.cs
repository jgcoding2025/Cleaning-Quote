namespace CleaningQuote.Api.Models;

public record RegisterRequest(string Email, string Password);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, DateTime ExpiresAtUtc);

public record ApiError(string Code, string Message);
