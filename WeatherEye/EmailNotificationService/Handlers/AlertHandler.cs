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
        public async Task HandleNewAlertCreated(NewAlertCreated alert, IMessageBus bus)
        {
            if (alert.AlertInfo.Language != "cs") return;
            var emailsResponse = await bus.InvokeAsync<InterestedUserEmailsResponse>(new InterestedUserEmailsRequest(alert.AlertInfo));
            foreach (var email in emailsResponse.Emails)
            {
                string body = alert.AlertInfo.Onset + " - " + alert.AlertInfo.Expires
                    + Environment.NewLine
                    + Environment.NewLine
                    + alert.AlertInfo.Description
                    + Environment.NewLine
                    + Environment.NewLine
                    + alert.AlertInfo.Instruction;
                _emailSender.SendEmail(email, $"{alert.AlertInfo.Event} - {alert.AlertInfo.AreaDesc}", body);
            }

        }
    }
}
