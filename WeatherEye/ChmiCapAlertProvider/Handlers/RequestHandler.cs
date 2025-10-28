using CAP;
using CAP.DTOs;
using MessagingContracts;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Wolverine;
using Wolverine.Attributes;

namespace ChmiCapAlertProvider.Handlers
{
    [WolverineHandler]
    public class RequestHandler
    {
        private readonly ILogger<RequestHandler> _logger;
        private readonly IConfiguration _configuration;
        private readonly RestClient _client;

        public RequestHandler(ILogger<RequestHandler> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _client = new RestClient(_configuration.GetValue<string>("ChmiCapEndpoint", "https://www.chmi.cz/files/portal/docs/meteo/om/kap/kap_cz.xml"));
        }
        [WolverineHandler]
        public void HandleAlert(AlertInfo info)
        {
            _logger.LogInformation("Handling AlertInfo Request: Event - {event}, Headline - {headline}, Expires - {expires}", info.Event, info.Headline, info.Expires);
        }

        [WolverineHandler]
        public AlertResponse HandleAlertRequest(MessagingContracts.AlertRequest request)
        {
            _logger.LogInformation("Handling AlertRequest");
            RestRequest restRequest = new RestRequest();
            var response = _client.GetAsync(restRequest).Result;
            List<AlertInfoDTO> alertInfosDTOs= new();
            
            if (response.IsSuccessful)
            {
                _logger.LogInformation("ChmiCap data retrieved successfully at: {time}", DateTimeOffset.Now);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CAP.Alert));
                using StringReader reader = new StringReader(response.Content!);
                CAP.Alert? alert = (CAP.Alert?)xmlSerializer.Deserialize(reader);
                if (alert is not null)
                {
                    foreach (var info in alert.Info)
                    {
                        foreach (var area in info.Area)
                        {
                            _logger.LogInformation("Found AlertInfo: Event - {event}, Headline - {headline}, Area - {areaDesc}, Expires - {expires}", info.Event, info.Headline, area.AreaDesc, info.Expires);
                            var dto = new AlertInfoDTO()
                            {
                                SenderName = info.SenderName,
                                Event = info.Event,
                                Urgency = Enum.GetName(typeof(AlertInfoUrgency), info.Urgency) ?? string.Empty,
                                Severity = Enum.GetName(typeof(AlertInfoSeverity), info.Severity) ?? string.Empty,
                                Certainty = Enum.GetName(typeof(AlertInfoCertainty), info.Certainty) ?? string.Empty,
                                Language = info.Language,
                                Onset = info.Onset,
                                Expires = info.Expires,
                                Headline = info.Headline,
                                Description = info.Description,
                                Instruction = info.Instruction,
                                AreaDesc = area.AreaDesc
                            };
                            alertInfosDTOs.Add(dto);
                        }
                    }
                }

            }
            _logger.LogInformation($"Returning {alertInfosDTOs.Count} alerts");
            //return JsonSerializer.Serialize(alertInfos);
            return new AlertResponse(alertInfosDTOs);
            //return alertInfos;

        }
    }
}
