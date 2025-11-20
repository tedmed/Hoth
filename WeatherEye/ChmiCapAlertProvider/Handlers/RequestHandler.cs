using CAP;
using CAP.DTOs;
using ChmiCapAlertProvider.DAO;
using DevExpress.Xpo;
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

            XPQuery<AlertDAO> alertDAOs = Session.DefaultSession.Query<AlertDAO>();

            var relevantAlerts = alertDAOs.ToList();

            List<AlertInfoDTO> alertInfosDTOs = new();


            foreach (var info in alertDAOs)
            {
                foreach (var area in info.Areas)
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
                        Onset = info.Onset ?? DateTime.MinValue,
                        Expires = info.Expires ?? DateTime.MinValue,
                        Headline = info.Headline,
                        Description = info.Description,
                        Instruction = info.Instruction,
                        AreaDesc = area.AreaDesc
                    };
                    alertInfosDTOs.Add(dto);

                }
            }
            _logger.LogInformation($"Returning {alertInfosDTOs.Count} alerts");
            return new AlertResponse(alertInfosDTOs);
        }

        [WolverineHandler]
        public AlertAreaResponse HandleAreaRequest(MessagingContracts.AlertAreaRequest request)
        {
            _logger.LogInformation("Handling AlertAreaRequest");
            XPQuery<AreaDAO> areas = Session.DefaultSession.Query<AreaDAO>();
           
            return new AlertAreaResponse(areas.Select(x => x.AreaDesc).ToList());
        }
    }
}
