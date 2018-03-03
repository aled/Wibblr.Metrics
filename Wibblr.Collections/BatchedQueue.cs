using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        private LinkedList<List<T>> batchList;
        private int count;

        public BatchedQueue(int batchSize, int maxCapacity)
        {
            this.batchSize = batchSize;
            this.maxCapacity = maxCapacity;

            batchList = new LinkedList<List<T>>();
        }

        // Add item to back of queue. If already full, this item is
        // discarded.
        public bool Enqueue(T item)
        {
            if (count < maxCapacity)
            {
                count++;
                if (batchList.Count == 0 || batchList.Last.Value.Count >= batchSize)
                    batchList.AddLast(new List<T>());
                
                batchList.Last.Value.Add(item);
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
            batchList.AddFirst(batch);
            count += batch.Count;

            while (count > maxCapacity)
            {
                int itemsToDelete = count - maxCapacity;
                if (itemsToDelete >= batchList.Last.Value.Count)
                {
                    count -= batchList.Last.Value.Count();
                    batchList.RemoveLast();

                }
                else
                {
                    var numItemsDropped = batchList.Last.Value.DropLast(itemsToDelete);
                    count -= numItemsDropped;
                    break;
                }
            }
        }

        public int Count() => count;

        public List<T> DequeueBatch()
        {
            var batch = batchList.First.Value;
            batchList.RemoveFirst();
            count -= batch.Count();

            if (batchList.Count == 0)
                batchList.AddFirst(new List<T>());

            return batch;
        }

        public string AsString()
        {
            var s = new StringBuilder();
            s.Append("[");
            foreach (var batch in batchList)
            {
                s.Append("(");
                s.Append(string.Join(",", batch));
                s.Append(")");
            }
            s.Append("]");
            return s.ToString();
        }
    }
}