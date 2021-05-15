using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.IO;
using Microsoft.Extensions.Configuration;
using Wibblr.Metrics.Core;
using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Examples
{
    public class KeyPressMonitor
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            new KeyPressMonitor().Run(configuration);
        }

        private T Choose<T>(string title, Func<T, string> getName, params T[] choices)
        {
            int choice;
            do
            {
                Console.WriteLine(title);

                for (int i = 0; i < choices.Length; i++)
                    Console.WriteLine($"{i + 1}. {getName(choices[i])}");

            } while (!int.TryParse(Console.ReadLine(), out choice));

            return choices[choice - 1];
        }

        private string Choose(string title, params string[] choices)
        {
            return Choose(title, x => x, choices);
        }

        private IMetricsSerializer ChooseSerializer()
        {
            switch (Choose("Select serializer", "Text", "Json objects", "Json chrometracing"))
            {
                case "Text":
                    return new LineSerializer();
                case "Json objects":
                    return new JsonObjectsSerializer();
                case "Json chrometracing":
                    return new ChromeTracingSerializer();
                default:
                    throw new Exception("Unknown choice");
            }
        }

        private IDatabasePlugin ChooseDatabase(params string[] pluginNames)
        {
            Console.WriteLine("Loading database plugins...");
            var factory = new PluginFactory();

            var availablePlugins = pluginNames
                .SelectMany(name => factory.LoadPlugin<IDatabasePlugin>(Assembly.GetExecutingAssembly(), name))
                .ToArray();         

            return Choose("Select database plugin:", x => x.Name, availablePlugins);
        }

        private IMetricsSink ChooseSink()
        {
            switch (Choose("Select target:", "Console", "File", "Database", "Rest API"))
            {
                case "Console":
                    return new TextWriterSink(Console.Out, ChooseSerializer());
                case "File":
                    return new FileSink(ChooseSerializer(), new DateTimeFileNamingStrategy("ddMMyy-HHmm"));
                case "Database":
                    return ChooseDatabase("CockroachDb", "SqlServer");
                case "Rest API":
                    return new RestClient(new MetricsWriterSettings { BatchSize = 3, MaxQueuedRows = 100 }, "https://localhost:5001/Upload");
                default:
                    throw new Exception("Unknown choice");
            }
        }

        public void Run(IConfigurationRoot configuration)
        {
            var sink = ChooseSink();

            var metricsWriterSettings = configuration.GetSection("MetricsWriter").Get<MetricsWriterSettings>();
            var metricsCollectorSettings = configuration.GetSection("MetricsCollector").Get<MetricsCollectorSettings>();

            if (sink is IDatabasePlugin databasePlugin)
            {
                var databaseConnectionSettings = configuration.GetSection("Database:Connection").Get<DatabaseConnectionSettings>();
                var databaseTablesSettings = configuration.GetSection("Database:Tables").Get<DatabaseTablesSettings>();
                databasePlugin.Initialize(databaseConnectionSettings, databaseTablesSettings, metricsWriterSettings);
            }

            using (var metrics = new MetricsCollector(sink, metricsCollectorSettings))
            {
                Console.WriteLine("Press some keys; enter to exit");

                metrics.RegisterThresholds("latency", new int[] { 0, 50, 75, 100, 1000, 2000, 10000 });
                var stopwatch = new Stopwatch();

                char key;
                do
                {
                    stopwatch.Start();
                    key = Console.ReadKey(true).KeyChar;
                    stopwatch.Stop();

                    metrics.IncrementCounter(key.ToString());
                    metrics.IncrementBucket("latency", stopwatch.ElapsedMilliseconds);
                    metrics.Event(key.ToString());

                    // letters a-z: lowercase is start of interval, uppercase is end.
                    if (key >= 'a' && key <= 'z')
                        metrics.StartInterval("session", key.ToString());
                    else if (key >= 'A' && key <= 'Z')
                        metrics.EndInterval("session", key.ToString().ToLower());

                    stopwatch.Reset();

                } while (key != '\r' && key != '\n');
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
