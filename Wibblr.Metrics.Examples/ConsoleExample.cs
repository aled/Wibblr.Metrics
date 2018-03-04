using System;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.Examples
{
    public class ConsoleExample
    {
        static void Main(string[] args)
        {
            new KeyPressMonitor().Run(new TextWriterSink(Console.Out));
        }
    }
}
