using HealthParse.Standard.Health;
using HealthParse.Standard.Mail;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthParseFunctions
{
    public static class MailUtility
    {
        public static MimeMessage ForwardMessage(MimeMessage original, string text, string to, string from)
        {
            return ConstructForward(original, new MailboxAddress(from), new MailboxAddress(to), builder =>
            {
                builder.TextBody = text;
            });
        }
        public static MimeMessage ProcessAppleHealthExportEmail(MimeMessage originalEmail, IEnumerable<Tuple<string, byte[]>> attachments, string from)
        {
            var exportAttachment = attachments.Single(a => a.Item1 == "export.zip");

            var attachment = ExcelReport.CreateReport(exportAttachment.Item2);
            var attachmentName = $"export.{originalEmail.Date.Date.ToString("yyyy-mm-dd")}.xlsx";

            return ConstructReply(originalEmail, new MailboxAddress(from), builder =>
            {
                builder.TextBody = @"Hey there, I saw your health data... good work!";
                builder.Attachments.Add(attachmentName, attachment);
            });
        }

        public static Result<MimeMessage> ProcessEmail(MimeMessage originalEmail, string from)
        {
            var attachments = originalEmail.LoadAttachments();

            try
            {
                if (attachments.Any(a => a.Item1 == "export.zip"))
                {
                    return Result.Success(ProcessAppleHealthExportEmail(originalEmail, attachments, from));
                }
            }
            catch (Exception e)
            {
                return Result.Failure(ConstructErrorMessage(originalEmail, from, e));
            }

            return Result.Failure(ConstructErrorMessage(originalEmail, from));
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

        private static MimeMessage ConstructReply(MimeMessage message, MailboxAddress from, Action<BodyBuilder> builderAction)
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
}
