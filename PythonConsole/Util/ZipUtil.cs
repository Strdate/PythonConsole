using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PythonConsole
{
    public static class ZipUtil
    {
        public static void ExtractFileToDirectory(string zipFileName, string outputDirectory)
        {
            ZipFile zip = ZipFile.Read(zipFileName);
            Directory.CreateDirectory(outputDirectory);
            foreach (ZipEntry e in zip)
            {
                e.Extract(outputDirectory, ExtractExistingFileAction.OverwriteSilently);
            }
        }
    }
}
