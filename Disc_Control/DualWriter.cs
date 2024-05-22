using System;
using System.IO;
using System.Text;

namespace Disc_Control
{
    public class DualWriter : TextWriter
    {
        private readonly TextWriter consoleWriter;
        private readonly StringWriter stringWriter;

        public DualWriter(TextWriter consoleWriter, StringWriter stringWriter)
        {
            this.consoleWriter = consoleWriter;
            this.stringWriter = stringWriter;
        }

        public override Encoding Encoding => consoleWriter.Encoding;

        public override void Write(char value)
        {
            consoleWriter.Write(value);
            stringWriter.Write(value);
        }

        public override void WriteLine(string value)
        {
            consoleWriter.WriteLine(value);
            stringWriter.WriteLine(value);
        }

        public override void Flush()
        {
            consoleWriter.Flush();
            stringWriter.Flush();
        }
    }
}
