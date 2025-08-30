using MicroApp.UsersService.Application;

namespace MicroApp.UsersService.Infrastructure.Email;

public sealed class ConsoleEmailSender : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        Console.WriteLine($"[EMAIL] To: {toEmail}\nSubject: {subject}\nBody:\n{body}\n");
        return Task.CompletedTask;
    }
}