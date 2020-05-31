using System;
using System.Threading;
using System.Threading.Tasks;

namespace Limiter
{
    public interface ITime
    {
        /// <inheritdoc cref="DateTimeOffset.UtcNow" />
        DateTimeOffset UtcNow { get; }

        /// <inheritdoc cref="Task.Delay(TimeSpan, CancellationToken)" />
        Task Delay(TimeSpan delay, CancellationToken cancellationToken);
    }


    public class SystemTime : ITime
    {
        /// <inheritdoc/>
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

        /// <inheritdoc/>
        public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
            => Task.Delay(delay, cancellationToken);
    }
}
