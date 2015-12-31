using Microsoft.Extensions.Logging;

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
