using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.Examples
{
    class JsonObjectsExample
    {
        static void Main(string[] args)
        {
            var sink = new FileSink(new JsonObjectsSerializer(), new DateTimeFileNamingStrategy("HH_mm"));
            new KeyPressMonitor().Run(sink);
        }
    }
}
