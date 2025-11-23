using DevExpress.ExpressApp.Core;
using DevExpress.Pdf.Native;
using DevExpress.Xpo;
using MessagingContracts;
using Preferences.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UserService.DAO;
using Wolverine.Attributes;

namespace UserService.Handlers
{
    [WolverineHandler]
    public class PreferencesHandler
    {
        private readonly ILogger<PreferencesHandler> _logger;
        private readonly IConfiguration _configuration;
        public PreferencesHandler(ILogger<PreferencesHandler> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [WolverineHandler]
        public AlertPreferencesResponse GetAlertPreferencesHandler(AlertPreferencesRequest request)
        {
            _logger.LogInformation("GetAlertPreferencesHandler called with UserOid: {UserOid}", request.UserOid);
            using UnitOfWork uow = new UnitOfWork();
            
            var areaDescs = uow.Query<UserAlertPreferenceDAO>()
                .Where(p => p.User.Oid == request.UserOid)
                .ToList();

            List<AlertPreferenceDTO> preferenceDTOs = areaDescs.Select(p => new AlertPreferenceDTO(request.UserOid.ToString())
            {
                PreferenceOid = p.Oid.ToString(),
                AreaDesc = p.AreaDesc,
                EmailNotification = p.EmailNotification,
                InAppNotification = p.InAppNotification
            }).ToList();

            _logger.LogInformation("Found {Count} alert preferences for UserOid: {UserOid}", areaDescs.Count, request.UserOid);
            return new AlertPreferencesResponse(preferenceDTOs);
        }

        [WolverineHandler]
        public SaveAlertPreferenceResponse SaveAlertPreferenceHandler(SaveAlertPreferenceRequest request)
        {
            using var uow = new UnitOfWork();
            _logger.LogInformation("SaveAlertPreferenceHandler called with UserOid: {UserOid}, AreaDesc: {AreaDesc}", request.UserOid, request.AreaDesc);

            UserDAO userDAO = uow.GetObjectByKey<UserDAO>(request.UserOid);

            if (userDAO is null)
            {
                _logger.LogWarning("User not found with Oid: {UserOid}", request.UserOid);
                throw new Exception("User not found");
            }

            XPQuery<UserAlertPreferenceDAO> preferences = new(uow);
            var preference = preferences
                .Where(x =>
                    x.User == userDAO &&
                    x.AreaDesc == request.AreaDesc &&
                    x.EmailNotification == request.EmailNotification &&
                    x.InAppNotification == request.InAppNotification
                )
                .FirstOrDefault();


            if (preference is not null)
            {
                return new SaveAlertPreferenceResponse(preference.Oid);
            }


            UserAlertPreferenceDAO newPreference = new UserAlertPreferenceDAO(uow)
            {
                AreaDesc = request.AreaDesc,
                EmailNotification = request.EmailNotification,
                InAppNotification = request.InAppNotification,
                User = userDAO
            };

            uow.CommitChanges();
            _logger.LogInformation("New alert preference saved for UserOid: {UserOid}, PreferenceOid: {PreferenceOid}", request.UserOid, newPreference.Oid);
            return new SaveAlertPreferenceResponse(newPreference.Oid);
        }

        [WolverineHandler]
        public RemoveAlertPreferenceResponse RemoveAlertPreferenceHandler(RemoveAlertPreferenceRequest request)
        {
            using var uow = new UnitOfWork();
            UserDAO userDAO = uow.GetObjectByKey<UserDAO>(request.UserOid);

            if (userDAO is null)
            {
                _logger.LogWarning("User not found with Oid: {UserOid}", request.UserOid);
                throw new Exception("User not found");
            }

            var preference = uow.GetObjectByKey<UserAlertPreferenceDAO>(request.PreferenceOid);
            if (preference is null)
            {
                throw new Exception("Preference not found");
            }

            Guid removedOid = preference.Oid;
            preference.Delete();
            uow.CommitChanges();
            _logger.LogInformation("Alert preference removed for UserOid: {UserOid}, PreferenceOid: {PreferenceOid}", request.UserOid, removedOid);
            return new RemoveAlertPreferenceResponse(removedOid);
        }
    }
}
