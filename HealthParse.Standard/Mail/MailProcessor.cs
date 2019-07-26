using System;
using System.Linq;
using HealthParse.Standard.Mail.Processors;
using HealthParse.Standard.Settings;
using Microsoft.ApplicationInsights;
using MimeKit;

namespace HealthParse.Standard.Mail
{
    public static class MailProcessor
    {
        public static Result<MimeMessage> ProcessEmail(MimeMessage originalEmail, string from, ISettingsStore settingsStore, TelemetryClient telemetry)
        {
            var userId = originalEmail.From.Mailboxes.First().HashedEmail();
            var settings = settingsStore.GetCurrentSettings(userId);
            var customSheets = settingsStore.GetCustomSheets(userId).ToList();

            var attachments = originalEmail.LoadAttachments().ToList();
            var handlers = new IMailHandler[]
            {
                new AppleHealthAttachmentMailHandler(from, settings, customSheets),
                new AppleHealthGoogleDriveMailHandler(from, settings, customSheets),
                new SettingsUpdateMailHandler(from, settingsStore),
                new HelpMailHandler(from), // <-- catch all
            };

            try
            {
                return handlers
                    .First(h => h.CanHandle(originalEmail, attachments))
                    .Process(originalEmail, attachments);
            }
            catch (Exception e)
            {
                return Result.Failure(MailUtility.ConstructErrorMessage(originalEmail, from, e), e);
            }
        }
    }
}