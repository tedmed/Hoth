using CAP;
using CAP.DTOs;
using MessagingContracts;
using MobileNotificationService.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;
using Wolverine.Attributes;

namespace MobileNotificationService.Handlers
{
    [WolverineHandler]
    public class AlertHandler
    {
        private readonly ILogger<AlertHandler> _logger;
        private readonly INotificationSender _notificationSender;

        public AlertHandler(ILogger<AlertHandler> logger, INotificationSender notificationSender)
        {
            _logger = logger;
            _notificationSender = notificationSender;
        }

        [WolverineHandler]
        public async Task HandleNewAlertCreated(AlertsUpdated alert, IMessageBus bus)
        {
            //if (alert.AlertInfo.Language != "cs") return;

            var usersResponse = await bus.InvokeAsync<UsersOidResponse>(new UsersOidRequest());
            var userOids = usersResponse.Users;

            var alertsResponse = await bus.InvokeAsync<AlertResponse>(new AlertRequest());
            var alerts = alertsResponse.records.Where(x => x.Language == "cs");

            foreach (var userOid in userOids)
            {
                HashSet<AlertInfoDTO> relevantAlerts = await GetRelevantAlerts(bus, alerts, userOid);

                var grouped = relevantAlerts.GroupBy(x => new Tuple<string, string>(x.Event, x.AreaDesc)).GroupBy(x => x.Key.Item1).ToList();

                var UserMobAppTokenResponse = await bus.InvokeAsync<UserMobAppTokenResponse>(new UserMobAppTokenRequest(userOid));
                if (string.IsNullOrWhiteSpace(UserMobAppTokenResponse.MobAppId))
                {
                    _logger.LogWarning("No email found for user {UserOid}", userOid);
                    continue;
                }

                foreach (var eventGroup in grouped)
                {
                    StringBuilder messageBuilder = new StringBuilder();
                    messageBuilder.AppendLine($"Událost: {eventGroup.Key}");
                    messageBuilder.AppendLine();
                    foreach (var areaGroup in eventGroup)
                    {
                        messageBuilder.AppendLine($"Oblast: {areaGroup.Key.Item2}");
                        foreach (var alertInfo in areaGroup)
                        {
                            messageBuilder.AppendLine($"- {alertInfo.Headline}");
                            messageBuilder.AppendLine($"  Popis: {alertInfo.Description}");
                            messageBuilder.AppendLine($"  Urgency: {GetUrgencyText(alertInfo.Urgency)}, Severity: {GetSeverityText(alertInfo.Severity)}, Certainty: {GetCertaintyText(alertInfo.Certainty)}");
                            messageBuilder.AppendLine();
                        }
                    }
                    await _notificationSender.SendNotificationAsync(UserMobAppTokenResponse.MobAppId, $"Událost: {eventGroup.Key}", messageBuilder.ToString());
                }

            }

        }

        private static async Task<HashSet<AlertInfoDTO>> GetRelevantAlerts(IMessageBus bus, IEnumerable<AlertInfoDTO> alerts, Guid userOid)
        {
            var userPreferencesResponse = await bus.InvokeAsync<AlertPreferencesResponse>(new AlertPreferencesRequest(userOid));
            var userPreferences = userPreferencesResponse.AlertPreferences.Where(x => x.InAppNotification).ToList();

            HashSet<AlertInfoDTO> relevantAlerts = new();

            foreach (var pref in userPreferences)
            {
                var alertQuery = alerts.Where(a => a.AreaDesc == pref.AreaDesc);

                if (string.IsNullOrWhiteSpace(pref.SpecificAreaDesc) == false)
                {
                    alertQuery = alertQuery.Where(a => a.SpecificAreaDesc == pref.SpecificAreaDesc);
                }

                alertQuery = alertQuery.Where(a => a.Certainty <= pref.AlertInfoCertainty);
                alertQuery = alertQuery.Where(a => a.Severity <= pref.AlertInfoSeverity);


                foreach (var alertInfo in alertQuery)
                {
                    relevantAlerts.Add(alertInfo);
                }

            }

            return relevantAlerts;
        }

        private static string GetSeverityText(int? severity)
        {
            return Enum.GetName<CAP.AlertInfoSeverity>((CAP.AlertInfoSeverity)(severity ?? 0)) ?? "";
        }

        private static string GetCertaintyText(int? certainty)
        {
            return Enum.GetName<CAP.AlertInfoCertainty>((CAP.AlertInfoCertainty)(certainty ?? 0)) ?? "";
        }

        private static string GetUrgencyText(int? urgency)
        {
            return Enum.GetName<CAP.AlertInfoUrgency>((CAP.AlertInfoUrgency)(urgency ?? 0)) ?? "";
        }

    }
}
