using System;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly LimitedSizeLinkedList<DateTime> _records;

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public Limiter(int maxCount, TimeSpan interval, ITime time)
        {
            // ToDo: check bounds of maxCount
            _maxCount = maxCount;
            _interval = interval;
            _time = time;
            _records = new LimitedSizeLinkedList<DateTime>(_maxCount);
        }

        public async Task WaitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
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
                        _records.Add(now);
                        return;
                    }
                    var timeToWait = firstInInterval!.Value + _interval - now;
                    await _time.Delay(timeToWait, cancellationToken).ConfigureAwait(false);
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
