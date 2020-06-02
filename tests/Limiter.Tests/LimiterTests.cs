using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace Limiter.Tests
{
    public class LimiterTests
    {
        [Fact]
        public async Task WaitAsync_Should_ReturnWithoutDelayIfSlotsAreEmpty()
        {
            int maxCount = 3;
            var interval = TimeSpan.FromMinutes(3);
            var time = A.Fake<ITime>();
            A.CallTo(() => time.UtcNow).Returns(new DateTimeOffset(new DateTime(2000, 1, 1), default));

            var limiter = new Limiter(maxCount, interval, time);

            await limiter.WaitAsync().ConfigureAwait(false);

            A.CallTo(() => time.UtcNow).MustHaveHappenedOnceExactly();
            A.CallTo(() => time.Delay(A<TimeSpan>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task WaitAsync_Should_ReturnWithDelayIfSlotsAreFilled()
        {
            int maxCount = 5;
            var interval = TimeSpan.FromMinutes(3);
            var time = A.Fake<ITime>();
            var date = new DateTimeOffset(new DateTime(2000, 1, 1), default);
            A.CallTo(() => time.UtcNow).Returns(date);

            var limiter = new Limiter(maxCount, interval, time);
            var tasks = Enumerable.Range(0, maxCount).Select(x => limiter.WaitAsync());

            var delay = Task.Delay(1_000);
            if (delay == await Task.WhenAny(delay, Task.WhenAll(tasks)).ConfigureAwait(false))
                throw new TimeoutException("Limiter too slow and didn't return slots");

            await limiter.WaitAsync().ConfigureAwait(false);

            A.CallTo(() => time.UtcNow).MustHaveHappened(maxCount + 1, Times.Exactly);
            A.CallTo(() => time.Delay(A<TimeSpan>.That.IsEqualTo(interval), A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WaitAsync_Should_StopTimeDelayOnCancel()
        {
            using var cts = new CancellationTokenSource();
            var limiter = new Limiter(1, TimeSpan.FromSeconds(10), SystemTime.Instance);

            await limiter.WaitAsync(cts.Token).ConfigureAwait(false);
            var task = limiter.WaitAsync(cts.Token);
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await task.ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task WaitAsync_Should_StopLockingOnCancel()
        {
            using var cts = new CancellationTokenSource();
            var time = A.Fake<ITime>();
            var date = new DateTimeOffset(new DateTime(2000, 1, 1), default);
            A.CallTo(() => time.UtcNow).Returns(date);
            A.CallTo(() => time.Delay(A<TimeSpan>._, A<CancellationToken>._))
                .ReturnsLazily(call => Task.Delay(20_000, (CancellationToken)call.Arguments[1]));

            var limiter = new Limiter(1, TimeSpan.FromSeconds(10), time);

            await limiter.WaitAsync(cts.Token).ConfigureAwait(false);
            var waitTaskOnDelay = limiter.WaitAsync(cts.Token);

            var taskWaitOnLock = limiter.WaitAsync(cts.Token);
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await taskWaitOnLock.ConfigureAwait(false)).ConfigureAwait(false);
        }
    }

}
