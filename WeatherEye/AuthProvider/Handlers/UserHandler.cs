using DevExpress.ExpressApp.Core;
using DevExpress.Pdf.Native;
using DevExpress.Xpo;
using MessagingContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.DAO;
using Wolverine.Attributes;

namespace UserService.Handlers
{
    [WolverineHandler]
    public class UserHandler
    {
        private readonly ILogger<UserHandler> _logger;
        private readonly IConfiguration _configuration;
        public UserHandler(ILogger<UserHandler> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [WolverineHandler]
        public UserOidResponse UserOidRequestHandler(UserOidRequest requst)
        {
            _logger.LogInformation("UserOidRequestHandler called with Username: {Username}, email: {email}", requst.Username, requst.Email);

            XPQuery<UserDAO> alertDAOs = Session.DefaultSession.Query<UserDAO>();
            UserDAO? userDAO = alertDAOs.FirstOrDefault(u => u.Username == requst.Username && u.Email == requst.Email);
            if (userDAO is not null)
            {
                _logger.LogInformation("User found: {Username}, Oid: {Oid}", userDAO.Username, userDAO.Oid);
                return new UserOidResponse(userDAO.Oid);

            }
            else
            {
                UserDAO newUserDAO = new UserDAO(Session.DefaultSession)
                {
                    Username = requst.Username,
                    Email = requst.Email
                };
                newUserDAO.Save();
                _logger.LogInformation("New user created: {Username}, Oid: {Oid}", newUserDAO.Username, newUserDAO.Oid);
                return new UserOidResponse(newUserDAO.Oid);
            }
        }

        [WolverineHandler]
        public SaveUserMobAppIdResponse SaveUserMobAppIdRequestHandler(SaveUserMobAppIdRequest request)
        {
            using var uow = new UnitOfWork();
            _logger.LogInformation("SaveUserMobAppIdRequestHandler called with UserOid: {UserOid}, MobAppId: {MobAppId}", request.UserOid, request.MobAppId);

            UserDAO userDAO = uow.GetObjectByKey<UserDAO>(request.UserOid);
            if (userDAO is null)
            {
                _logger.LogWarning("User not found with Oid: {UserOid}", request.UserOid);
                return new(false);
            }

            userDAO.MobAppDeviceId = request.MobAppId;
            uow.CommitChanges();

            _logger.LogInformation("User MobAppId updated for UserOid: {UserOid}, MobAppId: {MobAppId}", request.UserOid, request.MobAppId);
            return new(true);
        }

        [WolverineHandler]
        public InterestedUserEmailsResponse InterestedUserEmailsRequestHandler(InterestedUserEmailsRequest request)
        {
            _logger.LogInformation("InterestedUserEmailsRequestHandler called for AlertInfo: Event - {event}, AreaDesc - {areaDesc}", request.AlertInfo.Event, request.AlertInfo.AreaDesc);

            XPQuery<UserAlertPreferenceDAO> preferenceDAOs = Session.DefaultSession.Query<UserAlertPreferenceDAO>();

            var userEmails = preferenceDAOs
                .Where(p =>
                    p.AreaDesc == request.AlertInfo.AreaDesc &&
                    
                    p.EmailNotification == true
                )
                .Select(p => p.User.Email)
                .Distinct()
                .ToList();

            _logger.LogInformation("Found {Count} interested user emails for AlertInfo: Event - {event}, AreaDesc - {areaDesc}", userEmails.Count, request.AlertInfo.Event, request.AlertInfo.AreaDesc);
            return new InterestedUserEmailsResponse(userEmails);
        }
    }
}
