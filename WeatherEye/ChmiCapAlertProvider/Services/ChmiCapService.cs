using ChmiCapAlertProvider.DAOs;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using System.Xml.Serialization;
using Wolverine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DevExpress.Xpo;


public class ChmiCapService : BackgroundService
{
    private readonly ILogger<ChmiCapService> _logger;
    private RestClient client;
    IServiceScopeFactory serviceScopeFactory;
    private readonly IConfiguration _configuration;

    public ChmiCapService(ILogger<ChmiCapService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        this.serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        client = new RestClient(_configuration.GetValue<string>("ChmiCapEndpoint", "https://www.chmi.cz/files/portal/docs/meteo/om/kap/kap_cz.xml"));
    }



    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while ((!stoppingToken.IsCancellationRequested))
        {
            _logger.LogInformation("ChmiCap Service is running at: {time}", DateTimeOffset.Now);


            using IServiceScope scope = serviceScopeFactory.CreateScope();

            var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();



            RestRequest request = new RestRequest();
            var response = await client.GetAsync(request);
            if (response.IsSuccessful)
            {
                _logger.LogInformation("ChmiCap data retrieved successfully at: {time}", DateTimeOffset.Now);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CAP.Alert));
                using StringReader reader = new StringReader(response.Content!);
                CAP.Alert? alert = (CAP.Alert?)xmlSerializer.Deserialize(reader);

                if (alert is null) continue;
                using var uow = new UnitOfWork();

                foreach (var info in alert.Info)
                {
                    if (info.Expires != DateTime.MinValue)
                    {
                        if (uow.Query<AlertDAO>().Any(x => x.SenderName == info.SenderName
                        && x.Event == info.Event
                        && x.Language == info.Language
                        && x.Onset == DateTime.SpecifyKind(info.Onset, DateTimeKind.Utc)
                        && x.Expires == DateTime.SpecifyKind(info.Expires, DateTimeKind.Utc)))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (uow.Query<AlertDAO>().Any(x => x.SenderName == info.SenderName
                        && x.Event == info.Event
                        && x.Language == info.Language
                        && x.Onset == DateTime.SpecifyKind(info.Onset, DateTimeKind.Utc)))
                        {
                            continue;
                        }
                    }
                    AlertDAO dto = new AlertDAO(uow);
                    dto.SetProperties(info);

                    await bus.PublishAsync(info);
                }

                await uow.CommitChangesAsync();
                // Process the response.Content as needed
            }
            else
            {
                _logger.LogError("Failed to retrieve ChmiCap data at: {time}. Status Code: {statusCode}", DateTimeOffset.Now, response.StatusCode);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }




    public override void Dispose()
    {
        // Dispose of any resources if necessary
        client.Dispose();
    }
}

