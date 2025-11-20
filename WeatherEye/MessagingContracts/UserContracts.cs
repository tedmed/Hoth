using System;
using System.Collections.Generic;
using System.Text;

namespace MessagingContracts
{
    public record SaveAlertPreferenceRequest(Guid UserOid, string AreaDesc);
    public record SaveAlertPreferenceResponse(Guid PreferenceOid);

    public record AlertPreferencesRequest(Guid UserOid);
    public record AlertPreferencesResponse(IEnumerable<string> AreaDescs);

    public record UserOidRequest(string Username,string Email);
    public record UserOidResponse(Guid UserOid);
}
