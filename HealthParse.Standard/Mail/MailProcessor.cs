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
            var handlers = new IMailProcessor[]
            {
                new AppleHealthAttachmentMailProcessor(from, settings, customSheets),
                new AppleHealthGoogleDriveMailProcessor(from, settings, customSheets),
                new SettingsUpdateMailProcessor(from, settingsStore),
                new HelpMailProcessor(from), // <-- catch all
            };

            try
            {
                var processor = handlers.First(h => h.CanHandle(originalEmail, attachments));
                var result = Benchmark.Benchmark.Time(() => processor.Process(originalEmail, attachments));
                telemetry.TrackEvent(
                    processor.GetType().Name,
                    metrics: Events.Metrics.Init()
                        .Then(Events.Metrics.Duration, result.Elapsed.TotalMinutes));
                return result.Value;
            }
            catch (Exception e)
            {
                return Result.Failure(MailUtility.ConstructErrorMessage(originalEmail, from, e), e);
            }
        }
    }
}