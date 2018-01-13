using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HealthParse.Standard.Mail
{
    public static class Extensions
    {
        public static IEnumerable<Tuple<string, byte[]>> LoadAttachments(this MimeMessage message)
        {
            return message.Attachments
                .Select(attachment => new { attachment, filename = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name })
                .Select(x => new { x.attachment, x.filename, data = LoadAttachment(x.attachment) })
                .Select(x => Tuple.Create(x.filename, x.data));
        }
        private static byte[] LoadAttachment(MimeEntity attachment)
        {
            using (var stream = new MemoryStream())
            {
                if (attachment is MessagePart)
                {
                    var rfc822 = (MessagePart)attachment;

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
