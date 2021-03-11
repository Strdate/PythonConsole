using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PythonConsole
{
    public static  class Util
    {
        public static void Assert(object obj, string ex)
        {
            if (obj == null) {
                throw new Exception(ex);
            }
        }

        public static string NormalizePath(string path)
        {
            try {
                return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
            } catch { return null; }
        }
    }
}
