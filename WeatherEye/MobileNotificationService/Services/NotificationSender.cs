using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace MobileNotificationService.Services
{
    public interface INotificationSender
    {
        Task SendNotificationAsync(string token, string title, string body, string imageUrl = "");
    }
    public class NotificationSender : INotificationSender 
    {
        private readonly ILogger<NotificationSender> _logger;
        private readonly IConfiguration _configuration;

        public NotificationSender(IConfiguration configuration, ILogger<NotificationSender> logger)
        {
            _logger = logger;
            _configuration = configuration;

            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("firebase-adminsdk.json")
            });
        }

        public async Task SendNotificationAsync(string token, string title, string body, string imageUrl = "")
        {
            var message = new Message()
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                    
                }
            };

            if(string.IsNullOrWhiteSpace(imageUrl) == false)
            {
                message.Notification.ImageUrl = imageUrl;
            }

            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Notification sent: " + response);
        }
    }
}
