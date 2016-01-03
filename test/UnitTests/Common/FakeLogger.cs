// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using System;

namespace UnitTests.Common
{
    public class FakeLogger : ILogger
    {
        class FakeDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public IDisposable BeginScopeImpl(object state)
        {
            return new FakeDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
        }
    }

    public class FakeLogger<T> : ILogger<T>
    {
        class FakeDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public IDisposable BeginScopeImpl(object state)
        {
            return new FakeDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
        }
    }
}
