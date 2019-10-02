using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.InfluxDB;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NLog.Web;
using System;

namespace OcelotTeste.Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config => config.AddJsonFile("ocelot.json"))
                .ConfigureMetricsWithDefaults((hostingContext, builder) =>
                {
                    builder.Report.ToInfluxDb(options =>
                    {
                        options.InfluxDb.BaseUri = new Uri(hostingContext.Configuration["MetricsReport:InfluxDBUrl"]);
                        options.InfluxDb.Database = hostingContext.Configuration["MetricsReport:DataBase"];
                        options.InfluxDb.UserName = hostingContext.Configuration["MetricsReport:InfluxDBUser"];
                        options.InfluxDb.Password = hostingContext.Configuration["MetricsReport:InfluxDBPassword"];
                        options.InfluxDb.CreateDataBaseIfNotExists = true;
                        options.HttpPolicy.BackoffPeriod = TimeSpan.FromSeconds(30);
                        options.HttpPolicy.FailuresBeforeBackoff = 5;
                        options.HttpPolicy.Timeout = TimeSpan.FromSeconds(10);
                        options.MetricsOutputFormatter = new MetricsInfluxDbLineProtocolOutputFormatter();
                        options.FlushInterval = TimeSpan.FromSeconds(5);
                    });
                })
                .UseMetrics()
                .UseStartup<Startup>()
                .UseNLog();
        }
            
    }
}
