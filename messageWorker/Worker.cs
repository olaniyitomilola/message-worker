using static System.Net.WebRequestMethods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace messageWorker;

public class Worker : BackgroundService


{
    //Change delay to const

    private  int MINUTE = 60;
    private const int SECONDS = 1000;
    private readonly string? API_KEY = Environment.GetEnvironmentVariable("API_KEY");
    private decimal? StopLoss = 1.21m;
    private decimal moon = 1.30m;
    private bool messageSent = false;
    private bool moonmessageSent = false;
    private decimal? price;

    private string root = "https://api.coingecko.com/api/v3/simple/price?ids=akash-network&vs_currencies=usd";

    private HttpClient client;


    private readonly ILogger<Worker> _logger;

    private SendGridClient clientGrid;



    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {

        //start httpclient when service starts

        client = new HttpClient();
        clientGrid = new SendGridClient(API_KEY);
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

                   price = (decimal?) obj["akash-network"]?["usd"];



                    if (price < StopLoss)
                    {
                        _logger.LogInformation("Price is down, Akash is {price}",price);
                        try {
                            //shoot email
                           await sendEmail($"Problem, Akash is down, Price is {price}");
                            //change mail sent variable to true
                            messageSent = true;
                            //Multiply minutes by 2
                            MINUTE *= 2;

                        }
                       
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        
                    } 
                    else
                    {
                        if (price > moon)
                        {
                            _logger.LogInformation("Price is Up, Akash is {price}", price);
                            try
                            {
                                //shoot email
                                await sendEmail($"Dope, Akash is Mooning, Price is {price}");
                                //change mail sent variable to true
                                moonmessageSent = true;
                                
                                //Multiply minutes by 2
                                MINUTE *= 2;

                            }

                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                        }
                        else { Console.WriteLine($"No Action"); }

                        
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



            Console.WriteLine("new time set to {0} minutes", (MINUTE * SECONDS) / 60000);
            await Task.Delay(MINUTE * SECONDS, stoppingToken);
        }
    }

    private async Task sendEmail(string msg)
    {
        
        var from = new EmailAddress("temps@thetomilola.com", "Price Alert Worker");
        var to = new EmailAddress("olaniyitomilola@gmail.com", "Tomilola");
        var subject = "Price Alert from Worker";
        var htmlContent = $"<h1>{msg}</h1><div>check your portfolio<div>";
        var message = MailHelper.CreateSingleEmail(from, to, subject, msg, htmlContent);
        var response = await clientGrid.SendEmailAsync(message);

        
    }
}

