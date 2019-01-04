using System;
using System.Diagnostics;
using Serilog.Debugging;
using Serilog.Formatting.Json;
using Serilog.Sinks.RollingFile;
using Serilog.Sinks.SystemConsole.Themes;

namespace Serilog.Sinks.Elasticsearch.Sample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: SystemConsoleTheme.Literate)
                .WriteTo.Elasticsearch(
                    new ElasticsearchSinkOptions(new Uri(
                            "https://search-fsd-customer-poc-nested-4rbwenablqhr7jzyjcr2gk64fq.eu-west-1.es.amazonaws.com")) // for the docker-compose implementation
                        {
                            AutoRegisterTemplate = true,
                            //BufferBaseFilename = "./buffer",
                            RegisterTemplateFailure = RegisterTemplateRecovery.IndexAnyway,
                            FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                            EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                               EmitEventFailureHandling.WriteToFailureSink |
                                               EmitEventFailureHandling.RaiseCallback,
                            FailureSink = new RollingFileSink("./fail-{Date}.txt", new JsonFormatter(), null, null)
                        })
                .CreateLogger();

            Stopwatch stopwatch = new Stopwatch();

            // Enable the selflog output
            SelfLog.Enable(Console.Error);

            stopwatch.Start();
            for (var i = 0; i < 2; i++)
            {
                Log.Information("Hello, world!");

                int a = 10, b = 0;
                try
                {
                    Log.Debug("Dividing {A} by {B}", a, b);
                    Console.WriteLine(a / b);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Something went wrong");
                }

                // Introduce a failure by storing a field as a different type
                Log.Debug("Reusing {A} by {B}", "string", true);
            }
            stopwatch.Stop();
            Console.WriteLine("TIME ELAPSED: {0}", stopwatch.Elapsed);

            Log.CloseAndFlush();
            Console.Read();
        }
    }
}