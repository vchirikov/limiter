using System;
using System.Threading;
using System.Threading.Tasks;
using Limiter.Internal;

namespace Limiter
{
    public interface ILimiter
    {
        Task WaitAsync(CancellationToken cancellationToken = default);
    }

    public class Limiter : ILimiter
    {
        private readonly int _maxCount;
        private readonly TimeSpan _interval;
        private readonly ITime _time;
        private readonly ILimiterStateLogger _stateLogger;
        private readonly LimitedSizeLinkedList<DateTime> _records;
        private readonly string _resourceName;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public Limiter(int maxCount, TimeSpan interval, string? resourceName = null)
            : this(maxCount, interval, SystemTime.Instance, NullLimiterStateLogger.Instance, resourceName) { }

        // for testing
        internal Limiter(int maxCount, TimeSpan interval, ILimiterStateLogger stateLogger, string? resourceName = null)
            : this(maxCount, interval, SystemTime.Instance, stateLogger, resourceName) { }

        internal Limiter(int maxCount, TimeSpan interval, ITime time)
            : this(maxCount, interval, time, NullLimiterStateLogger.Instance, resourceName: null) { }

        internal Limiter(int maxCount, TimeSpan interval, ITime time, ILimiterStateLogger stateLogger, string? resourceName = null)
        {
            // ToDo: check bounds of maxCount
            _maxCount = maxCount;
            _interval = interval;
            _time = time;
            _records = new LimitedSizeLinkedList<DateTime>(_maxCount);
            _resourceName = resourceName ?? "";
            _stateLogger = stateLogger;
        }

        public async Task WaitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _stateLogger.Log(_resourceName, LimiterStateEnum.WaitOnLock, waitTime: null);
                await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    var now = _time.UtcNow.UtcDateTime;
                    var startInterval = now - _interval;
                    int count = 0;
                    DateTime? firstInInterval = null;
                    foreach (var item in _records)
                    {
                        if (item <= startInterval)
                            continue;

                        firstInInterval ??= item;
                        ++count;
                    }

                    if (count < _maxCount)
                    {
                        _stateLogger.Log(_resourceName, LimiterStateEnum.Return, waitTime: null);
                        _records.Add(now);
                        return;
                    }
                    var firstExpired = firstInInterval!.Value + _interval;
                    _stateLogger.Log(_resourceName, LimiterStateEnum.WaitUntilExpire, waitTime: firstExpired);
                    await _time.Delay(firstExpired - now, cancellationToken).ConfigureAwait(false);
                    _stateLogger.Log(_resourceName, LimiterStateEnum.Return, waitTime: null);
                    _records.Add(firstExpired);
                }
                finally
                {
                    _lock.Release();
                }
            }
            catch (TaskCanceledException ex)
            {
                throw new OperationCanceledException("The operation was canceled.", ex, cancellationToken);
            }
        }
    }
}
