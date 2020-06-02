using System;
using System.ComponentModel;

namespace Limiter.Internal
{
    public enum LimiterStateEnum : int
    {
        Unknown = 0,
        WaitOnLock = 1,
        WaitUntilExpire = 2,
        Return = 3,
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ILimiterStateLogger
    {
        void Log(string resourceName, LimiterStateEnum state, DateTime? waitTime);
    }


    public sealed class NullLimiterStateLogger : ILimiterStateLogger
    {
        public static readonly NullLimiterStateLogger Instance = new NullLimiterStateLogger();
        public void Log(string resourceName, LimiterStateEnum state, DateTime? waitTime) { }
        private NullLimiterStateLogger() { }
    }
}
