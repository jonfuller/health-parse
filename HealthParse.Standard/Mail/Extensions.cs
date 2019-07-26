using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MimeKit;

namespace HealthParse.Standard.Mail
{
    public static class Extensions
    {
        public static string HashedEmail(this MailboxAddress address)
        {
            return Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.Default.GetBytes(address.Address)));
        }

        public static IEnumerable<(string name, byte[] data)> LoadAttachments(this MimeMessage message)
        {
            return message.Attachments
                .Select(attachment => new { attachment, filename = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name })
                .Select(x => new { x.attachment, x.filename, data = LoadAttachment(x.attachment) })
                .Select(x => (name: x.filename, x.data));
        }
        private static byte[] LoadAttachment(MimeEntity attachment)
        {
            using (var stream = new MemoryStream())
            {
                if (attachment is MessagePart rfc822)
                {
                    rfc822.Message.WriteTo(stream);
                }
                else
                {
                    var part = (MimePart)attachment;

                    part.Content.DecodeTo(stream);
                }
                stream.Position = 0;

                return stream.ToArray();
            }
        }
    }
}
