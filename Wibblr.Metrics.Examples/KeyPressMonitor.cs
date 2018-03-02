using System;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.Examples
{
    class KeyPressMonitor
    {
        static void Main(string[] args)
        {
            var sink = new TextWriterSink(Console.Out);
            var counterCollector = new MetricsCollector(sink, TimeSpan.FromMilliseconds(1000), TimeSpan.FromSeconds(1));

            Console.WriteLine("Press some keys; enter to stop recording events");
            char key;
            do
            {
                key = Console.ReadKey(true).KeyChar;
                counterCollector.IncrementCounter(key.ToString());
            } while (key != '\r' && key != '\n');  // \r = windows; \n = mac

            counterCollector.Dispose();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
