using messageWorker;
using Serilog;


public class Program
{
    public static void Main(string[] args)
    {

        var path = Directory.GetCurrentDirectory();
        //serilog conf
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File($"{Path.Combine(path,"log.txt")}")
            .CreateLogger();
        try
        {

            Log.Information("Starting up the service {time}", DateTime.Now);
            CreateHostBuilder(args)
            .Build()
            .Run();

            return;

        }

        catch (Exception ex)
        {
            Log.Fatal(ex, "There was a problem starting up service");
            return;
        }
        finally
        {
            Log.CloseAndFlush();
        }


        
    }





    public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();
            })
            .UseSerilog();


}





