using static System.Net.WebRequestMethods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace messageWorker;

public class Worker : BackgroundService


{
    //Change delay to const

    private const int MINUTE = 5;
    private const int SECONDS = 1000;

    private string firstRoot = "https://api.coingecko.com/api/v3/ping";
    private string root = "https://api.coingecko.com/api/v3/simple/price?ids=akash-network&vs_currencies=usd";

    private HttpClient client;


    private readonly ILogger<Worker> _logger;



    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {

        //start httpclient when service starts

        client = new HttpClient();
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        //shot down http when service shuts down

        client.Dispose();

        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //_logger.LogInformation("The info is being logged now @ {time}", DateTime.Now);

            //load my website

            try
            {
                var result = await client.GetAsync(root);
                if (result.IsSuccessStatusCode)
                {
                    //GET THE PRICE OF AKASH AND SET ALERT WHEN IT GOES LOWER
                    //This is still in string format, convert to JSON
                    var response = await result.Content.ReadAsStringAsync();

                    //cannot write object to log
                    JObject obj = JObject.Parse(response);

                    if ((decimal)obj["akash-network"]["usd"] > 1)
                    {
                        _logger.LogInformation("Price is down, Akash is {price}", (string) obj["akash-network"]["usd"]);
                        Console.WriteLine($"Problem, Akash is down, Price is {obj["akash-network"]["usd"]}");
                    }
                    else
                    {
                        Console.WriteLine("nothing");
                    }

                    //Console.WriteLine(obj["akash-network"]);


                }
                else
                {

                    //normally shoot a mail
                    _logger.LogCritical("The website is down. Status Code {statusCode}", result.StatusCode);
                }

            } catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            


            await Task.Delay(MINUTE * SECONDS, stoppingToken);
        }
    }
}

