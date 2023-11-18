using System;

namespace Components.Main.Grids.GridPathFinder
{
    public class PriorityQueue<T> where T : IHeapItem<T>
    {
        private T[] _items = new T[20];
        private int _currentItemCount;

        public void Add(T item)
        {
            if (_currentItemCount == _items.Length)
            {
                ResizeArray();
            }

            item.HeapIndex = _currentItemCount;
            _items[_currentItemCount] = item;
            SortUp(item);
            _currentItemCount++;
        }

        private void ResizeArray()
        {
            T[] largerArray = new T[_items.Length * 2];
            Array.Copy(_items, largerArray, _items.Length);
            _items = largerArray;
        }

        public T RemoveFirst()
        {
            if (_currentItemCount == 0)
            {
                throw new InvalidOperationException("Cannot remove from an empty priority queue.");
            }

            T firstItem = _items[0];
            _currentItemCount--;
            _items[0] = _items[_currentItemCount];
            _items[0].HeapIndex = 0;
            SortDown(_items[0]);

            return firstItem;
        }


        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        public int Count => _currentItemCount;

        public bool Contains(T item)
        {
            return Equals(_items[item.HeapIndex], item);
        }

        private void SortDown(T item)
        {
            while (true)
            {
                int childIndexLeft = item.HeapIndex * 2 + 1;
                int childIndexRight = item.HeapIndex * 2 + 2;
                int swapIndex;

                if (childIndexLeft < _currentItemCount)
                {
                    swapIndex = childIndexLeft;

                    if (childIndexRight < _currentItemCount)
                    {
                        if (_items[childIndexLeft]
                            .CompareTo(_items[childIndexRight]) <
                            0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }

                    if (item.CompareTo(_items[swapIndex]) < 0)
                    {
                        Swap(item, _items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        private void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                T parentItem = _items[parentIndex];

                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }

                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        private void Swap(T itemA, T itemB)
        {
            _items[itemA.HeapIndex] = itemB;
            _items[itemB.HeapIndex] = itemA;
            (itemA.HeapIndex, itemB.HeapIndex) = (itemB.HeapIndex, itemA.HeapIndex);
        }
    }
}