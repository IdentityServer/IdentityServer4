using Microsoft.Extensions.WebEncoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace UnitTests.Common
{
    public class FakeHtmlEncoder : IHtmlEncoder
    {
        public string HtmlEncode(string value)
        {
            return value;
        }

        public void HtmlEncode(string value, int startIndex, int charCount, TextWriter output)
        {
        }

        public void HtmlEncode(char[] value, int startIndex, int charCount, TextWriter output)
        {
        }
    }
}
