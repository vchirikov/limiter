using System;
using System.ComponentModel;
using System.Linq;
using Limiter.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Limiter.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MicrosoftExtensionLoggingLimiterStateLogger : ILimiterStateLogger
    {
        private readonly ILogger _logger;

        private readonly Action<ILogger, string, LimiterStateEnum, DateTime, Exception> _waitUntilExpireLoggerMessage;
        private readonly Action<ILogger, string, LimiterStateEnum, Exception> _returnLoggerMessage;
        private readonly Action<ILogger, string, LimiterStateEnum, Exception> _waitOnLockLoggerMessage;

        private static readonly Lazy<EventId[]> _events = new Lazy<EventId[]>(() => GenerateEventIds());

        private const int EventIdOffset = 1337;

        internal static EventId[] GenerateEventIds()
            => Enum.GetNames(typeof(LimiterStateEnum))
                .Select(name => new EventId((int)Enum.Parse(typeof(LimiterStateEnum), name) + EventIdOffset, name))
                .ToArray();

        public MicrosoftExtensionLoggingLimiterStateLogger(ILogger logger, LogLevel logLevel, string waitUntilExpireFormatString, string returnFormatString, string waitOnLockFormatString)
        {
            _logger = logger;
            _waitUntilExpireLoggerMessage = LoggerMessage.Define<string, LimiterStateEnum, DateTime>(logLevel, _events.Value[(int)LimiterStateEnum.WaitUntilExpire], waitUntilExpireFormatString);
            _returnLoggerMessage = LoggerMessage.Define<string, LimiterStateEnum>(logLevel, _events.Value[(int)LimiterStateEnum.Return], returnFormatString);
            _waitOnLockLoggerMessage = LoggerMessage.Define<string, LimiterStateEnum>(logLevel, _events.Value[(int)LimiterStateEnum.WaitOnLock], waitOnLockFormatString);
        }

        public void Log(string resourceName, LimiterStateEnum state, DateTime? waitTime)
        {
            switch (state)
            {
                case LimiterStateEnum.WaitOnLock:
                    _waitOnLockLoggerMessage(_logger, resourceName, state, null!);
                    break;
                case LimiterStateEnum.WaitUntilExpire:
                    _waitUntilExpireLoggerMessage(_logger,
                        resourceName,
                        state,
                        waitTime ?? throw new LimiterException("Wait time is null", new ArgumentNullException(nameof(waitTime))),
                        null!);
                    break;
                case LimiterStateEnum.Return:
                    _returnLoggerMessage(_logger, resourceName, state, null!);
                    break;
                default:
                    throw new LimiterException("Unknown limiter state reported");
            }
        }
    }
}