using System;
using Limiter.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Limiter.Tests
{
    public class MicrosoftExtensionLoggingLimiterStateLoggerTests
    {
        [Fact]
        public void GenerateEventIds_Should_BeOrdered()
        {
            var eventIds = MicrosoftExtensionLoggingLimiterStateLogger.GenerateEventIds();
            var prevItem = default(EventId?);
            foreach (var item in eventIds)
            {
                if (prevItem != null && prevItem.Value.Id > item.Id)
                    throw new Exception($"{nameof(MicrosoftExtensionLoggingLimiterStateLogger.GenerateEventIds)} returned unordered array");
                prevItem = item;
            }
        }
    }
}
