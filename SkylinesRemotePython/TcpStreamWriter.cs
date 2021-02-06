using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkylinesRemotePython
{
    public class TcpStreamWriter : StreamWriter
    {
        private ClientHandler client;
        public TcpStreamWriter(Stream stream, ClientHandler client)
            : base(stream)
        {
            this.client = client;
        }

        public override void WriteLine()
        {
            base.WriteLine();
            WriteImpl("\n");
        }

        public override void WriteLine(string value)
        {
            base.WriteLine(value);
            WriteImpl(value + "\n");
        }

        public override void Write(string value)
        {
            base.Write(value);
            WriteImpl(value);
        }

        private void WriteImpl(string value)
        {
            client.SendMessage(value, "c_output_message");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
