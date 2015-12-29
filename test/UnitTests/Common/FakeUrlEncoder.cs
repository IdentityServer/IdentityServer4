using Microsoft.Extensions.WebEncoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace UnitTests.Common
{
    public class FakeUrlEncoder : IUrlEncoder
    {
        public string UrlEncode(string value)
        {
            return value;
        }

        public void UrlEncode(string value, int startIndex, int charCount, TextWriter output)
        {
        }

        public void UrlEncode(char[] value, int startIndex, int charCount, TextWriter output)
        {
        }
    }
}
