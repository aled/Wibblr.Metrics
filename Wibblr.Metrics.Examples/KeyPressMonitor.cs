using System;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.Examples
{
    class KeyPressMonitor
    {
        static void Main(string[] args)
        {
            var eventCollector = new EventCollector(new TextWriterSink(Console.Out), resolutionMillis: 500, flushIntervalMillis: 2000);

            Console.WriteLine("Press some keys; enter to exit");
            char key;
            while(true)
            {
                key = Console.ReadKey(true).KeyChar;
                eventCollector.RecordEvent(key.ToString());
            }
        }
    }
}
