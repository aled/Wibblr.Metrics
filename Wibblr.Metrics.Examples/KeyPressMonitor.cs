using System;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.Examples
{
    class KeyPressMonitor
    {
        static void Main(string[] args)
        {
            var eventCollector = new EventCollector(new TextWriterSink(Console.Out), resolutionMillis: 500, flushIntervalMillis: 2000);

            Console.WriteLine("Press some keys; enter to stop recording events");
            char key;
            do
            {
                key = Console.ReadKey(true).KeyChar;
                eventCollector.RecordEvent(key.ToString());
            } while (key != '\r');

            eventCollector.Dispose();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
