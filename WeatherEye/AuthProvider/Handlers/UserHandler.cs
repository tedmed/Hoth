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
            if (userDAO != null)
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
        public SaveAlertPreferenceResponse SaveAlertPreferenceHandler(SaveAlertPreferenceRequest request)
        {
            _logger.LogInformation("SaveAlertPreferenceHandler called with UserOid: {UserOid}, AreaDesc: {AreaDesc}", request.UserOid, request.AreaDesc);
            XPQuery<UserDAO> userDAOs = Session.DefaultSession.Query<UserDAO>();
            UserDAO? userDAO = userDAOs.FirstOrDefault(u => u.Oid == request.UserOid);
            if (userDAO == null)
            {
                _logger.LogWarning("User not found with Oid: {UserOid}", request.UserOid);
                throw new Exception("User not found");
            }
            UserAlertPreferenceDAO newPreference = new UserAlertPreferenceDAO(Session.DefaultSession)
            {
                AreaDesc = request.AreaDesc,
                User = userDAO
            };
            newPreference.Save();
            _logger.LogInformation("New alert preference saved for UserOid: {UserOid}, PreferenceOid: {PreferenceOid}", request.UserOid, newPreference.Oid);
            return new SaveAlertPreferenceResponse(newPreference.Oid);
        }

        [WolverineHandler]
        public AlertPreferencesResponse GetAlertPreferencesHandler(AlertPreferencesRequest request)
        {
            _logger.LogInformation("GetAlertPreferencesHandler called with UserOid: {UserOid}", request.UserOid);
            XPQuery<UserAlertPreferenceDAO> preferenceDAOs = Session.DefaultSession.Query<UserAlertPreferenceDAO>();
            var areaDescs = preferenceDAOs
                .Where(p => p.User.Oid == request.UserOid)
                .Select(p => p.AreaDesc)
                .ToList();
            _logger.LogInformation("Found {Count} alert preferences for UserOid: {UserOid}", areaDescs.Count, request.UserOid);
            return new AlertPreferencesResponse(areaDescs);
        }
    }
}
