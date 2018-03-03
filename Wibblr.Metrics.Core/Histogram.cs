using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Store a histogram.
    /// Number of buckets and their values are configurable by the user.
    /// The first bucket starts at negative maxValue, and the last bucket
    /// ends at positive maxValue.
    /// </summary>
    public class Histogram
    {
        private long[] buckets;
        private int[] thresholds;

        public Histogram(params int[] thresholds)
        {
            this.thresholds = thresholds
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            if (this.thresholds.Length < 1)
                throw new ArgumentException("Must contain at least one value", nameof(thresholds));

            buckets = new long[this.thresholds.Length + 1];
        }

        public void Add(float v)
        {
            var i = Array.BinarySearch(thresholds, (int)v);

            // if positive, the search result is the index of the threshold
            // which exactly matches the number to add. In this case the bucket
            // to use is one higher (because the lower bound of the bucket is inclusive).
            // If negative, bitwise NOT of the search result is the index of the first element
            // that was larger, or one greater than the length of the array. 
            // In this case the bucket to use is the NOT of the search result.
            if (i >= 0)
                i++;
            else
                i = ~i;
            
            buckets[i]++;
        }

        public float? EstimatedValue(int bucketIndex, long valueIndex)
        {
            // Items in the first or last buckets cannot be given any sensible
            // interpolated value, as these buckets stretch to the min/max values.
            if (bucketIndex == 0 || bucketIndex == buckets.Length - 1)
                return null;

            // if there is 1 item in the bucket, it is 1/2 way between the min and max
            // if there are 2 items in the bucket, they are at 1/3 and 2/3 between the min and max
            float fractionBetweenMinAndMax = (valueIndex + 1f) / (buckets[bucketIndex] + 1f);

            float min = thresholds[bucketIndex - 1];
            float max = thresholds[bucketIndex];
            return min + ((max - min) * fractionBetweenMinAndMax);
        }

        // Get an estimated value for any item in the histogram.
        public float? this[long index]
        {
            get
            {    
                // Use a linear search to find the correct bucket
                var cumulativeCount = 0L;
                for (int i = 0; i < buckets.Length; i++)
                {
                    if (index >= cumulativeCount && index < cumulativeCount + buckets[i])
                        return EstimatedValue(i, index - cumulativeCount);

                    cumulativeCount += buckets[i];
                }

                throw new ArgumentException("index must be less than the total number of values in the histogram", nameof(index));
            }
        }

        public float? Percentile(float percentile)
        {
            if (percentile < 0 || percentile > 1)
                throw new ArgumentException("Must be between 0 and 1", nameof(percentile));

            long totalCount = buckets.Sum();
            float index = (totalCount - 1) * percentile;

            int wholeIndex = (int)index;
            float? val = this[wholeIndex];

            if (val.HasValue)
            {
                float fracIndex = index - wholeIndex;
                if (fracIndex > 0)
                    val += fracIndex * (this[wholeIndex + 1] - val);
            }

            return val;
        }

        /// <summary>
        /// Returns the percentage of items below each threshold value.
        /// </summary>
        /// <returns>The percentages.</returns>
        public (int, float)[] ThresholdPercentages()
        {
            long totalCount = buckets.Sum();
            var cumulativeCount = 0f;
            var ret = new(int, float)[thresholds.Length];
            for (int i = 0; i < thresholds.Length; i++)
            {
                cumulativeCount += buckets[i];
                ret[i] = (thresholds[i], cumulativeCount / totalCount);
            }
            return ret;
        }

        public IEnumerable<Bucket> Buckets()
        {
            yield return new Bucket
            {
                from = null,
                to = thresholds[0],
                count = buckets[0]
            };

            for (int i = 1; i < buckets.Length - 1; i++)
            {
                yield return new Bucket
                {
                    from = thresholds[i - 1],
                    to = thresholds[i],
                    count = buckets[i]
                };
            }

            yield return new Bucket
            {
                from = thresholds[thresholds.Length - 1],
                to = null,
                count = buckets[thresholds.Length]
            };
        }

        public string AsString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < thresholds.Length; i++)
            {
                sb.Append(buckets[i]);
                sb.Append(" |");
                sb.Append(thresholds[i]);
                sb.Append("| ");
            }
            sb.Append(buckets[buckets.Length - 1]);

            return sb.ToString();
        }
    }
}
