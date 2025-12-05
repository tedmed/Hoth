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
            using UnitOfWork uow = new();

            var Alert = uow.Query<AlertDAO>().OrderByDescending(x => x.Sent).FirstOrDefault();
            if(Alert is null)
            {
                return new(Array.Empty<AlertInfoDTO>());
            }

            var alertDAOs = Alert.AlertInfos;

            List<AlertInfoDTO> alertInfoDTOs = new();

            foreach (var info in alertDAOs)
            {
                alertInfoDTOs.AddRange(info.TransformToDTOs());                
            }
            _logger.LogInformation($"Returning {alertInfoDTOs.Count} alerts");
            return new AlertResponse(alertInfoDTOs);
        }

        [WolverineHandler]
        public AlertAreaResponse HandleAreaRequest(MessagingContracts.AlertAreaRequest request)
        {
            _logger.LogInformation("Handling AlertAreaRequest");
            using UnitOfWork uow = new();
           
            return new AlertAreaResponse(uow.Query<AreaDAO>().Select(x => x.AreaDesc).ToList());
        }

        [WolverineHandler]
        public AlertSpecificAreaResponse HandleSpecificAreaRequest(AlertSpecificAreaRequest request)
        {
            _logger.LogInformation("Handling AlertSpecificAreaRequest");
            using UnitOfWork uow = new();

            return new AlertSpecificAreaResponse(uow.Query<SpecificAreaDAO>().Where(x => x.Area.AreaDesc == request.AreaDesc).Select(x => x.Description).ToList());
        }

        [WolverineHandler]
        public AlertAllSpecificAreasResponse HandleAllSpecificAreasRequest(AlertAllSpecificAreasRequest request)
        {
            _logger.LogInformation("Handling AlertSpecificAreaRequest");
            using UnitOfWork uow = new();

            return new AlertAllSpecificAreasResponse(uow.Query<SpecificAreaDAO>().Select(x => new Tuple<string,string>(x.Area.AreaDesc,x.Description)).ToList());
        }
    }
}
