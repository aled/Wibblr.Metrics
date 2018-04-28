namespace Wibblr.Metrics.Core
{
    public struct Metric
    {
        public string name;
        public Window window;

        public Metric(string name, Window window)
        {
            this.name = name;
            this.window = window;
        }
    }
} 
