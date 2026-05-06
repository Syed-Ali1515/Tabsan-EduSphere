namespace Tabsan.EduSphere.Application.DTOs.Auth;

/// <summary>Request body for creating an Admin user account.</summary>
public sealed record CreateAdminUserRequest(string Username, string? Email, string Password);

/// <summary>Request body for updating an Admin user account.</summary>
public sealed record UpdateAdminUserRequest(string? Email, bool IsActive, string? NewPassword);
