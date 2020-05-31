using System.Collections.Generic;

namespace Limiter
{
    internal class LimitedSizeLinkedList<T> : LinkedList<T>
    {
        private readonly int _maxSize;
        public int MaxSize => _maxSize;

        public LimitedSizeLinkedList(int maxSize) => _maxSize = maxSize;

        /// <summary>
        /// Adds <paramref name="element"/> at the last of the list and removes the fist item if
        /// <see cref="MaxSize"></see> is reached
        /// </summary>
        /// <param name="element">Element to add</param>
        public void Add(T element)
        {
            AddLast(element);
            if (Count > _maxSize)
                RemoveFirst();
        }
    }

}
