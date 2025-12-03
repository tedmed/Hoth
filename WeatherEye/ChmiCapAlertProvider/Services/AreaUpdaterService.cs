using ChmiCapAlertProvider.DAO;
using ChmiCapAlertProvider.XmlModels;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ChmiCapAlertProvider.Services
{
    public class AreaUpdaterService : BackgroundService
    {
        private readonly ILogger<AreaUpdaterService> _logger;
        private RestClient client;
        IServiceScopeFactory serviceScopeFactory;
        private readonly IConfiguration _configuration;

        public AreaUpdaterService(ILogger<AreaUpdaterService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
            client = new RestClient(_configuration.GetValue<string>("CISORPEndpoint", "https://apl2.czso.cz/iSMS/do_cis_export?kodcis=65&typdat=1&cisvaz=80113_97&cisjaz=203&format=0"));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            PeriodicTimer periodicTimer = new(TimeSpan.FromHours(1));

            do
            {
                try
                {

                    _logger.LogInformation("Area Updater Service is running at: {time}", DateTimeOffset.Now);

                    using IServiceScope scope = serviceScopeFactory.CreateScope();

                    RestRequest request = new RestRequest();
                    var response = await client.GetAsync(request);
                    if (response.IsSuccessful && response.Content is not null)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(EXPORT));
                        EXPORT res = null;
                        using (var reader = new StringReader(response.Content))
                        {
                            res = (EXPORT)serializer.Deserialize(reader);
                        }

                        if (res is null)
                        {
                            _logger.LogError("data failed to load correctly");
                            continue;
                        }

                        var kraje = res.DATA.Select(x => x[1].TEXT).Distinct().ToList();

                        //var kraje = // csvModels.Select(x => new { x.chodnota2, x.text2 }).Distinct().ToList();

                        var uow = new UnitOfWork();

                        foreach (var kraj in kraje)
                        {
                            var existingKraj = uow.FindObject<AreaDAO>(CriteriaOperator.FromLambda<AreaDAO>(x => x.AreaDesc == kraj));
                            if (existingKraj is not null) continue;

                            AreaDAO newKraj = new(uow)
                            {
                                AreaDesc = kraj,
                            };
                            _logger.LogInformation("Added new area: {area}", newKraj.AreaDesc);

                        }
                        uow.CommitChanges();
                        uow.Dispose();

                        uow = new();

                        foreach (var area in res.DATA)
                        {
                            string specificAreaCisorp = area[0].CHODNOTA;
                            string specificAreaName = area[0].TEXT;

                            var existingSpecificArea = uow.FindObject<SpecificAreaDAO>(CriteriaOperator.FromLambda<SpecificAreaDAO>(x => x.CisorpId == specificAreaCisorp));

                            if (existingSpecificArea is not null) continue;

                            var krajName = area[1].TEXT;
                            var kraj = uow.FindObject<AreaDAO>(CriteriaOperator.FromLambda<AreaDAO>(x => x.AreaDesc == krajName));

                            if (kraj is null)
                            {
                                _logger.LogError($"Kraj not found: {krajName}");
                                continue;
                            }

                            existingSpecificArea = new(uow)
                            {
                                CisorpId = specificAreaCisorp,
                                EmmaId = $"CZ{specificAreaCisorp}",
                                Description = specificAreaName
                            };

                            kraj.SpecificAreas.Add(existingSpecificArea);
                        }
                        uow.CommitChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Area Updater Service");
                }
            }
            while (await periodicTimer.WaitForNextTickAsync(stoppingToken));

        }

        public override void Dispose()
        {
            client.Dispose();
            base.Dispose();
        }

        private class CsvModel
        {
            //"kodjaz","typvaz","akrcis1","kodcis1","chodnota1","text1","akrcis2","kodcis2","uroven2","chodnota2","text2"

            public string kodjaz { get; set; }
            public string typvaz { get; set; }
            public string akrcis1 { get; set; }
            public string kodcis1 { get; set; }
            public string chodnota1 { get; set; }
            public string text1 { get; set; }
            public string akrcis2 { get; set; }
            public string kodcis2 { get; set; }
            public string uroven2 { get; set; }
            public string chodnota2 { get; set; }
            public string text2 { get; set; }
        }


    }


}
