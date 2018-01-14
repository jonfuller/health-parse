using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace HealthParse.Standard.Health
{
    public static class ZipUtilities
    {
        public static IEnumerable<T> ReadArchive<T>(Stream exportZip, Func<ZipArchiveEntry, bool> entryFilter, Func<ZipArchiveEntry, T> eachEntry)
        {
            using (var archive = new ZipArchive(exportZip, ZipArchiveMode.Read, true))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entryFilter(entry))
                    {
                        yield return eachEntry(entry);
                    }
                }
            }
        }
    }
}
