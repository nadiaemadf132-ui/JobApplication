using JobApplication.Application.Interfaces;
using JobApplication.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace JobApplication.Infrastructure.Email
{
    public class AuthEmailService : IAuthEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly AppUrlOptions _appUrlOptions;
        private readonly ILogger<AuthEmailService> _logger;

        public AuthEmailService(
            IEmailSender emailSender,
            IOptions<AppUrlOptions> appUrlOptions,
            ILogger<AuthEmailService> logger)
        {
            _emailSender = emailSender;
            _appUrlOptions = appUrlOptions.Value;
            _logger = logger;
        }

        public Task SendEmailConfirmationAsync(string userId, string email, string token)
        {
            var link = BuildUrl("api/auth/confirm-email", ("userId", userId), ("token", token));
            var body =
                "<p>Welcome! Please confirm your email address by opening the link below:</p>" +
                $"<p><a href=\"{Encode(link)}\">Confirm my email</a></p>" +
                "<p>If you did not create an account, you can ignore this message.</p>";

            return SafeSendAsync(email, "Confirm your email", body);
        }

        public Task SendPasswordResetAsync(string email, string token)
        {
            var endpoint = BuildUrl("api/auth/reset-password");
            var body =
                "<p>We received a request to reset your password.</p>" +
                $"<p>Send a <b>POST</b> request to <code>{Encode(endpoint)}</code> with this JSON body:</p>" +
                $"<pre>{{ \"email\": \"{Encode(email)}\", \"token\": \"{Encode(token)}\", " +
                "\"newPassword\": \"...\", \"confirmNewPassword\": \"...\" }</pre>" +
                "<p>If you did not request this, you can ignore this message; your password will not change.</p>";

            return SafeSendAsync(email, "Reset your password", body);
        }

        public Task SendEmailChangeConfirmationAsync(string userId, string newEmail, string token)
        {
            var link = BuildUrl("api/auth/confirm-email-change",
                ("userId", userId), ("newEmail", newEmail), ("token", token));
            var body =
                "<p>Please confirm that this is your new email address by opening the link below:</p>" +
                $"<p><a href=\"{Encode(link)}\">Confirm my new email</a></p>" +
                "<p>If you did not request this change, you can ignore this message.</p>";

            return SafeSendAsync(newEmail, "Confirm your new email", body);
        }

        private string BuildUrl(string path, params (string Key, string Value)[] query)
        {
            var url = $"{_appUrlOptions.ApiBaseUrl.TrimEnd('/')}/{path}";
            if (query.Length == 0)
                return url;

            return url + "?" + string.Join("&",
                query.Select(q => $"{q.Key}={Uri.EscapeDataString(q.Value)}"));
        }

        private static string Encode(string value) => WebUtility.HtmlEncode(value);

        // A mail-delivery failure must not fail the request (and must not reveal whether an account exists).
        // The failure is logged; the user can request a new email.
        private async Task SafeSendAsync(string to, string subject, string htmlBody)
        {
            try
            {
                await _emailSender.SendAsync(to, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send '{Subject}' email.", subject);
            }
        }
    }
}
