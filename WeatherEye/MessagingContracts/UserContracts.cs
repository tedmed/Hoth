using CAP.DTOs;
using Preferences.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessagingContracts
{
    public record SaveAlertPreferenceRequest(Guid UserOid, string AreaDesc, bool EmailNotification, bool InAppNotification);
    public record SaveAlertPreferenceResponse(Guid PreferenceOid);

    public record RemoveAlertPreferenceRequest(Guid UserOid,Guid PreferenceOid);
    public record RemoveAlertPreferenceResponse(Guid PreferenceOid);

    public record AlertPreferencesRequest(Guid UserOid);
    public record AlertPreferencesResponse(IList<AlertPreferenceDTO> AlertPreferences);

    public record UserOidRequest(string Username,string Email);
    public record UserOidResponse(Guid UserOid);

    public record SaveUserMobAppIdRequest(Guid UserOid,string MobAppId);
    public record SaveUserMobAppIdResponse(bool ok);

    public record InterestedUserEmailsRequest(AlertInfoDTO AlertInfo);
    public record InterestedUserEmailsResponse(IList<string> Emails);
}
