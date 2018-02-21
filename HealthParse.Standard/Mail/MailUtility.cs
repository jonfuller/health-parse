using System;
using System.Diagnostics;
using System.Linq;
using HealthParse.Standard.Mail.Processors;
using HealthParse.Standard.Settings;
using Microsoft.ApplicationInsights;
using MimeKit;

namespace HealthParse.Standard.Mail
{
    public static class MailUtility
    {
        public const string HelpDocUrl = "https://docs.google.com/document/d/1o3N199npwOfPKSN_CymXELOJJ-7ACZpEkgSLkL1ibxI/edit?usp=sharing";

        public static MimeMessage ForwardMessage(MimeMessage original, string text, string to, string from)
        {
            return ConstructForward(original, new MailboxAddress(from), new MailboxAddress(to), builder =>
            {
                builder.TextBody = text;
            });
        }

        public static Result<MimeMessage> ProcessEmail(MimeMessage originalEmail, string from,
            ISettingsStore settingsStore)
        {
            return ProcessEmail(originalEmail, from, settingsStore, new TelemetryClient());
        }

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
                var result = Benchmark.Time(() => processor.Process(originalEmail, attachments));
                telemetry.TrackEvent(
                    processor.GetType().Name,
                    metrics: Events.Metrics.Init()
                        .Then(Events.Metrics.Duration, result.Elapsed.TotalMinutes));
                return result.Value;
            }
            catch (Exception e)
            {
                return Result.Failure(ConstructErrorMessage(originalEmail, from, e), e);
            }
        }

        public static MimeMessage ConstructErrorMessage(MimeMessage originalEmail, string from)
        {
            return ConstructReply(originalEmail, new MailboxAddress(from), builder =>
            {
                builder.TextBody = @"Something went wrong... sorry about that! Did you attach the ""export.zip"" from the Apple Health export?";
            });
        }
        public static MimeMessage ConstructErrorMessage(MimeMessage originalEmail, string from, Exception error)
        {
            originalEmail.Headers.Add("X-APH-error", error.ToString());
            return ConstructReply(originalEmail, new MailboxAddress(from), builder =>
            {
                builder.TextBody = @"Something went wrong... there was an error. We'll take a look into it.";
            });
        }

        private static MimeMessage ConstructForward(MimeMessage messageToForward, MailboxAddress from, MailboxAddress forwardee, Action<BodyBuilder> builderAction)
        {
            var message = new MimeMessage();
            message.From.Add(from);
            message.To.Add(forwardee);
            message.Subject = $"[ERROR] Fwd: {messageToForward.Subject}";

            var builder = new BodyBuilder();
            builder.Attachments.Add(new MessagePart { Message = messageToForward });
            builderAction(builder);

            message.Body = builder.ToMessageBody();

            return message;
        }

        public static MimeMessage ConstructReply(MimeMessage message, MailboxAddress from, Action<BodyBuilder> builderAction)
        {
            var reply = new MimeMessage();

            reply.From.Add(from);

            // reply to the sender of the message
            if (message.ReplyTo.Count > 0)
            {
                reply.To.AddRange(message.ReplyTo);
            }
            else if (message.From.Count > 0)
            {
                reply.To.AddRange(message.From);
            }
            else if (message.Sender != null)
            {
                reply.To.Add(message.Sender);
            }

            if (!message.Subject.StartsWith("Re:", StringComparison.OrdinalIgnoreCase))
                reply.Subject = "Re: " + message.Subject;
            else
                reply.Subject = message.Subject;

            // construct the In-Reply-To and References headers
            if (!string.IsNullOrEmpty(message.MessageId))
            {
                reply.InReplyTo = message.MessageId;
                foreach (var id in message.References)
                    reply.References.Add(id);
                reply.References.Add(message.MessageId);
            }

            var builder = new BodyBuilder();
            builderAction(builder);

            reply.Body = builder.ToMessageBody();

            return reply;
        }
    }
    public class BenchmarkResult<T> : BenchmarkResult
    {
        public T Value { get; }
        public BenchmarkResult(T value, TimeSpan elapsed) : base(elapsed)
        {
            Value = value;
        }
    }

    public class BenchmarkResult
    {
        public TimeSpan Elapsed { get; }

        public BenchmarkResult(TimeSpan elapsed)
        {
            Elapsed = elapsed;
        }
    }
    public static class Benchmark
    {
        public static BenchmarkResult<T> Time<T>(Func<T> toRun)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var value = toRun();
            stopWatch.Stop();

            return new BenchmarkResult<T>(value, stopWatch.Elapsed);
        }

        public static BenchmarkResult Time(Action toRun)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            toRun();
            stopWatch.Stop();

            return new BenchmarkResult(stopWatch.Elapsed);
        }
    }
}