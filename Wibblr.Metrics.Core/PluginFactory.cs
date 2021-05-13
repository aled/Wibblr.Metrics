using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Wibblr.Metrics.Core
{
    public class PluginFactory
    {
        public IEnumerable<T> LoadPlugin<T>(Assembly currentlyExecutingAssembly, string pluginName) where T: class
        {
            var pluginFilename = pluginName.Contains(".")
               ? $"{pluginName}.dll"
               : $"Wibblr.Metrics.Plugins.{pluginName}.dll";

            var assembly = LoadAssembly(currentlyExecutingAssembly, pluginFilename);

            return CreatePluginObjects<T>(assembly);
        }

        private Assembly LoadAssembly(Assembly currentlyExecutingAssembly, string pluginFilename)
        {
            var path = Path.Join(new FileInfo(currentlyExecutingAssembly.Location).Directory.FullName, pluginFilename);
            var bytes = File.ReadAllBytes(path);
            return Assembly.Load(bytes);
        }

        private IEnumerable<T> CreatePluginObjects<T>(Assembly assembly) where T: class
        {
            return assembly
                .GetTypes()
                .Where(t => typeof(T).IsAssignableFrom(t))
                .Select(t => Activator.CreateInstance(t) as T)
                .ToList();
        }
    }
}
