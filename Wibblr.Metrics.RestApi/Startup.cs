using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using Wibblr.Metrics.Core;
using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.RestApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Wibblr.Metrics.RestApi", Version = "v1" });
            });

            // Load plugins
            var databasePlugins = new List<IDatabasePlugin>();
            var loadPluginsConfig = Configuration.GetSection("Plugins").Get<List<string>>();
            var executingAssembly = Assembly.GetExecutingAssembly();
            foreach (var pluginName in loadPluginsConfig)
            {
                try
                {
                    databasePlugins.AddRange(new PluginFactory().LoadPlugin<IDatabasePlugin>(executingAssembly, pluginName));
                }
                catch (Exception e)
                {
                    Console.Write($"Error loading database plugin '{pluginName}'; error '{e.Message}'");
                }
            }

            var databaseType = Configuration["DatabaseType"];
            var databasePlugin = databasePlugins.FirstOrDefault(x => x.Name == databaseType);
            if (databasePlugin == null)
                throw new ApplicationException($"Unsupported database type '{databaseType}'");

            var databaseConnectionSettings = Configuration.GetSection("Database:Connection").Get<DatabaseConnectionSettings>();
            var databaseTablesSettings = Configuration.GetSection("Database:Tables").Get<DatabaseTablesSettings>();
            var metricsWriterSettings = Configuration.GetSection("MetricsWriter").Get<MetricsWriterSettings>();

            databasePlugin.Initialize(databaseConnectionSettings, databaseTablesSettings, metricsWriterSettings);

            services.AddSingleton(databasePlugin);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wibblr.Metrics.RestApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
