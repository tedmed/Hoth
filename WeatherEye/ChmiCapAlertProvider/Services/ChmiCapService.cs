using CAP.DTOs;
using ChmiCapAlertProvider.DAO;
using ChmiCapAlertProvider.XmlModels;
using DevExpress.Xpo;
using MessagingContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Linq.Expressions;
using System.Xml.Serialization;
using Wolverine;


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
        client = new RestClient(_configuration.GetValue<string>("ChmiCapEndpoint", "https://www.chmi.cz/files/portal/docs/meteo/om/bulletiny/XOCZ50_OKPR.xml"));
    }



    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        PeriodicTimer periodicTimer = new(TimeSpan.FromSeconds(30));

        while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
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

                    if (alert is null) return;
                    using var uow = new UnitOfWork();

                    var existingAlert = uow.Query<AlertDAO>().Where(x => x.Identifier == alert.Identifier).FirstOrDefault();

                    if (existingAlert is null)
                    {
                        AlertDAO alertDto = new(uow)
                        {
                            Identifier = alert.Identifier,
                            Sent = DateTime.SpecifyKind(alert.Sent, DateTimeKind.Utc),
                        };
                        existingAlert = alertDto;

                    }
                    else
                    {
                        _logger.LogInformation("Alert with Identifier {identifier} already exists. Skipping.", alert.Identifier);
                        return;
                    }


                    foreach (var info in alert.Info)
                    {
                        AlertInfoDAO dao = new AlertInfoDAO(uow);
                        dao.SetProperties(info, alert, uow);
                        existingAlert.AlertInfos.Add(dao);                     
                        uow.CommitChanges();
                    }

                    await uow.CommitChangesAsync();
                    await bus.PublishAsync(new AlertsUpdated());
                }         
            else
            {
                _logger.LogError("Failed to retrieve ChmiCap data at: {time}. Status Code: {statusCode}", DateTimeOffset.Now, response.StatusCode);
            }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing ChmiCap data at: {time}", DateTimeOffset.Now);
            }
        }
    }




    public override void Dispose()
    {
        // Dispose of any resources if necessary
        client.Dispose();
    }
}

