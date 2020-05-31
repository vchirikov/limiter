using Xunit;

namespace Limiter.Tests
{
    public class LimitedSizeLinkedListTests
    {
        [Fact]
        public void Add_Should_AddElement()
        {
            var list = new LimitedSizeLinkedList<int>(3) {
                4
            };
            Assert.Single(list);
        }

        [Fact]
        public void Add_Should_RemoveFirstIfSizeIsReached()
        {
            var list = new LimitedSizeLinkedList<int>(2) {
                1,
                2,
                3,
            };

            Assert.Equal(2, list.Count);
            Assert.Equal(2, list.First.Value);
        }

        [Fact]
        public void Add_Should_AddToLast()
        {
            var list = new LimitedSizeLinkedList<int>(3) {
                1,
                2,
                3,
            };

            Assert.Equal(3, list.Last.Value);
        }
    }

}
