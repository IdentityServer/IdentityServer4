using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests.Common
{
    public class FakeLoggerFactory : ILoggerFactory
    {
        public LogLevel MinimumLevel
        {
            get
            {
                return LogLevel.Debug;
            }

            set
            {
            }
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FakeLogger();
        }

        public void Dispose()
        {
        }
    }
}
