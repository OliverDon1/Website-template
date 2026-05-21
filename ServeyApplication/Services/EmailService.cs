using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Survey App", _config["Ethereal:Username"]));
        email.To.Add(new MailboxAddress("", toEmail));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = htmlContent };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _config["Ethereal:Host"],
            int.Parse(_config["Ethereal:Port"]),
            SecureSocketOptions.StartTls
        );

        await smtp.AuthenticateAsync(
            _config["Ethereal:Username"],
            _config["Ethereal:Password"]
        );

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        Console.WriteLine("Email sent (Ethereal). Check your inbox at:");
        Console.WriteLine("https://ethereal.email/messages");
    }
}