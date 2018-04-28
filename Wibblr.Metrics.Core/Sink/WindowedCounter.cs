namespace Wibblr.Metrics.Core
{
    public struct WindowedCounter
    {
        public string name;
        public Window window;
        public long count;
    }
}
