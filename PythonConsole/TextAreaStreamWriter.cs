using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonConsole
{
    public class TextAreaStreamWriter : StreamWriter
    {
        public TextAreaStreamWriter(Stream stream)
            : base(stream)
        {

        }

        public override void WriteLine()
        {
            base.WriteLine();
            UIWindow.Instance.OutputText = UIWindow.Instance.OutputText + "\n";
        }

        public override void WriteLine(string value)
        {
            base.WriteLine(value);
            UIWindow.Instance.OutputText = UIWindow.Instance.OutputText + value + "\n";
        }

        public override void Write(string value)
        {
            base.Write(value);
            UIWindow.Instance.OutputText = UIWindow.Instance.OutputText + value;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
