using System;
using System.Collections.Generic;
using System.Linq;

namespace Wibblr.Collections
{
    // Queue that maintains items in batches.
    // Items may be added to the back of the queue; batches may be taken or 
    // added to the front of the queue.
    // Items will be discarded from the back of the queue if they do not fit.
    public class BatchedQueue<T>
    {
        private int maxCapacity;
        private int batchSize;

        private LinkedList<List<T>> buffer;
        private int count;

        public BatchedQueue(int batchSize, int maxCapacity)
        {
            this.batchSize = batchSize;
            this.maxCapacity = maxCapacity;

            buffer = new LinkedList<List<T>>();
        }

        // Add item to back of queue. If already full, this item is
        // discarded.
        public bool Enqueue(T item)
        {
            if (count < maxCapacity)
            {
                count++;
                if (buffer.Count == 0 || buffer.Last.Value.Count >= batchSize)
                    buffer.AddLast(new List<T>());
                
                buffer.Last.Value.Add(item);
                return true;
            }
            return false;
        }

        public int Enqueue(IEnumerable<T> items)
        {
            var numItemsAdded = 0;
            foreach (var item in items)
            {
                if (!Enqueue(item))
                    break;

                numItemsAdded++;
            }
            return numItemsAdded;
        }

        // Add batch to front of queue. If already full, items from the 
        // back of the queue are discarded.
        public void EnqueueToFront(List<T> batch)
        {
            buffer.AddFirst(batch);
            count += batch.Count;

            while (count > maxCapacity)
            {
                int itemsToDelete = count - maxCapacity;
                if (itemsToDelete >= buffer.Last.Value.Count)
                {
                    buffer.RemoveLast();
                }
                else
                {
                    buffer.Last.Value.DropLast(itemsToDelete);
                    break;
                }
            }
        }

        public int Count() => count;

        public List<T> DequeueBatch()
        {
            var batch = buffer.First.Value;
            buffer.RemoveFirst();
            count -= batch.Count();
            return batch;
        }
    }
}