namespace messageWorker;

public class Worker : BackgroundService


{
    //Change delay to const

    private const int MINUTE = 5;
    private const int SECONDS = 1000;

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
                var result = await client.GetAsync("https://www.thetomilola.com");
                if (result.IsSuccessStatusCode)
                {
                    _logger.LogInformation("The Website is up. Status Code {statusCode}", result.StatusCode);
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

