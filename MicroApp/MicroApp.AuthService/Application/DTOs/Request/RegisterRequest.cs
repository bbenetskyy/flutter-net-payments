namespace AuthService.Application.DTOs;

record RegisterRequest(string Email, string Password, string DisplayName);
