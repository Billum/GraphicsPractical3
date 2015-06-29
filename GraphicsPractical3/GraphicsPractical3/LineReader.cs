using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GraphicsPractical3
{
    public class LineReader : StreamReader
    {
        public LineReader(Stream s)
            : base(s)
        {

        }

        private bool alreadyHaveLine = false;
        private string line;

        public override string ReadLine()
        {
            if (alreadyHaveLine)
            {
                // If line in buffer, return it
                alreadyHaveLine = false;
                return line;
            }
            else
            {
                return base.ReadLine();
            }
        }

        public string ReadLineAhead()
        {
            if (!alreadyHaveLine)
            {
                // place line in buffer so it will be returned
                // by the actual read line later
                line = ReadLine();
                alreadyHaveLine = true;
            }

            return line;
        }
    }
}
