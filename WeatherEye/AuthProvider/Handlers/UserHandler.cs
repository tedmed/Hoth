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

            using UnitOfWork uow = new UnitOfWork();
            UserDAO? userDAO = uow.Query<UserDAO>().FirstOrDefault(u => u.Username == requst.Username && u.Email == requst.Email);
            if (userDAO is not null)
            {
                _logger.LogInformation("User found: {Username}, Oid: {Oid}", userDAO.Username, userDAO.Oid);
                return new UserOidResponse(userDAO.Oid);

            }
            else
            {
                UserDAO newUserDAO = new UserDAO(uow)
                {
                    Username = requst.Username,
                    Email = requst.Email
                };
                uow.CommitChanges();
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

            using UnitOfWork uow = new UnitOfWork();



            var userEmails = uow.Query<UserAlertPreferenceDAO>()
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


        [WolverineHandler]
        public UsersOidResponse UsersOidHandler(UsersOidRequest request)
        {
            _logger.LogInformation("UsersOidHandler called to retrieve all user Oids.");

            using UnitOfWork uow = new UnitOfWork();

            var usersOid = uow.Query<UserDAO>()
                .Select(u => u.Oid)
                .ToList();

            _logger.LogInformation("Retrieved {Count} user Oids.", usersOid.Count);

            return new UsersOidResponse(usersOid);
        }

        [WolverineHandler]
        public UserEmailResponse UserEmailRequestHandler(UserEmailRequest request)
        {
            _logger.LogInformation("UserEmailRequestHandler called with UserOid: {UserOid}", request.UserOid);
            using UnitOfWork uow = new UnitOfWork();
            UserDAO? userDAO = uow.GetObjectByKey<UserDAO>(request.UserOid);
            if (userDAO is not null)
            {
                _logger.LogInformation("User found: {Username}, Email: {Email}", userDAO.Username, userDAO.Email);
                return new UserEmailResponse(userDAO.Email);
            }
            else
            {
                _logger.LogWarning("User not found with Oid: {UserOid}", request.UserOid);
                return new UserEmailResponse(string.Empty);
            }
        }
    }
}
