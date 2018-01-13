using HealthParse.Standard;
using Microsoft.WindowsAzure.Storage.Blob;
using MimeKit;
using System.IO;

namespace HealthParse.Mail
{
    public static class EmailStorage
    {
        public static string SaveEmailToStorage(MimeMessage email, CloudBlobContainer container)
        {
            var filename = Path.GetFileName(Path.GetTempFileName());
            var blockBlob = container.GetBlockBlobReference(filename);
            var messageBytes = email.ToBytes();
            blockBlob.UploadFromByteArrayAsync(messageBytes, 0, messageBytes.Length);

            return filename;
        }

        public static MimeMessage LoadEmailFromStorage(string filename, CloudBlobContainer container)
        {
            var blockBlob = container.GetBlockBlobReference(filename);
            using (var stream = new MemoryStream())
            {
                blockBlob.DownloadToStreamAsync(stream).Wait();

                stream.Position = 0;
                return MimeMessage.Load(stream);
            }
        }
    }
}
