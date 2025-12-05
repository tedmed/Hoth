using CAP;
using CAP.DTOs;
using EmailNotificationService.Services;
using MessagingContracts;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;
using Wolverine.Attributes;

namespace EmailNotificationService.Handlers
{
    [WolverineHandler]
    public class AlertHandler
    {
        private readonly ILogger<AlertHandler> _logger;
        private readonly IEmailSender _emailSender;

        public AlertHandler(ILogger<AlertHandler> logger, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
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
                var userPreferencesResponse = await bus.InvokeAsync<AlertPreferencesResponse>(new AlertPreferencesRequest(userOid));
                var userPreferences = userPreferencesResponse.AlertPreferences.Where(x => x.EmailNotification).ToList();

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

                var grouped = relevantAlerts.GroupBy(x => new Tuple<string, string>(x.Event, x.AreaDesc)).GroupBy(x => x.Key.Item1).ToList();

                var userEmailResponse = await bus.InvokeAsync<UserEmailResponse>(new UserEmailRequest(userOid));
                if (string.IsNullOrWhiteSpace(userEmailResponse.Email))
                {
                    _logger.LogWarning("No email found for user {UserOid}", userOid);
                    continue;
                }
                string userEmail = userEmailResponse.Email;

                string emailHtml = GetEmailHtml(grouped);
                if (string.IsNullOrWhiteSpace(emailHtml)) continue;

                _emailSender.SendEmail(userEmail, "Nové výstrahy CAP", emailHtml, true);
            }

        }

        private static string GetEmailHtml(List<IGrouping<string, IGrouping<Tuple<string, string>, AlertInfoDTO>>> grouped)
        {
            var sb = new StringBuilder();

            foreach (var eventGroup in grouped)
            {
                var sample = eventGroup.SelectMany(g => g).First();

                string bg = GetSeverityColor(sample.Severity);

                sb.AppendLine($"<div style='border:1px solid #ccc;background:{bg};"
                            + "padding:12px;border-radius:6px;margin-bottom:18px;font-family:Arial;'>");

                sb.AppendLine($"<h2 style='margin:0 0 5px 0;font-size:18px;'>{sample.Event}</h2>");

                sb.AppendLine($"<div style='font-size:13px;color:#555;margin-bottom:10px;'>"
                            + $"Urgency: {GetUrgencyText(sample.Urgency)} • Severity: {GetSeverityText(sample.Severity)} • Certainty: {GetCertaintyText(sample.Certainty)}</div>");

                foreach (var areaGroup in eventGroup)
                {
                    string kraj = areaGroup.Key.Item2;
                    var oblasti = string.Join(", ",
                        areaGroup.Select(a => a.SpecificAreaDesc));

                    sb.AppendLine($"<h3 style='margin:10px 0 4px 0;font-size:15px;'>{kraj}</h3>");
                    sb.AppendLine($"<div style='font-size:14px;line-height:1.4;'>{oblasti}</div>");
                }

                sb.AppendLine("</div>");
            }

            string emailHtml = sb.ToString();
            return emailHtml;
        }

        static string GetSeverityColor(int severity) =>
                                      severity switch
    {
        (int)AlertInfoSeverity.Extreme => "#ffe5e5", // světle červená
        (int)AlertInfoSeverity.Severe => "#ffe9d6", // světle oranžová
        (int)AlertInfoSeverity.Moderate => "#fff9d6", // světle žlutá
        _ => "#f8f8f8"                        // minor/unknown – světle šedá
    };

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
