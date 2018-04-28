namespace Wibblr.Metrics.Core
{
    public struct WindowedBucket
    {
        public string name;
        public Window window;
        public int? from;
        public int? to;
        public long count;
    }
}
