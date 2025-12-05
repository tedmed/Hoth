using CAP;
using Preferences.DTO;


public class UserApiClient(HttpClient httpClient)
{
    public async Task<Guid?> GetUserOidAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<Guid>("/User/Info", cancellationToken);
    }

    public async Task SaveAlertPreference(string Region,string SpecificArea, AlertInfoSeverity alertInfoSeverity, AlertInfoCertainty alertInfoCertainty, bool emailNotification = false, bool inAppNotification = false, CancellationToken cancellationToken = default)
    {

        AlertPreferenceDTO alertPreference = new()
        {
            AreaDesc = Region,
            SpecificAreaDesc = SpecificArea,
            EmailNotification = emailNotification,
            InAppNotification = inAppNotification,
            AlertInfoSeverity = (int)alertInfoSeverity,
            AlertInfoCertainty = (int)alertInfoCertainty,
        };

        var response = await httpClient.PostAsJsonAsync("/User/Preferences", alertPreference, cancellationToken);
        response.EnsureSuccessStatusCode();
        //var savedPreferenceId = await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: cancellationToken);
        //return savedPreferenceId;
    }

    public async Task DeleteAlertPreferenceAsync(string PreferenceOid,CancellationToken cancellationToken = default)
    {        
        var response = await httpClient.DeleteAsync($"/User/Preferences?PreferenceOid={PreferenceOid}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<AlertPreferenceDTO[]> GetAlertPreferenceAsync(CancellationToken cancellationToken = default)
    {
        var pref = await httpClient.GetFromJsonAsync<AlertPreferenceDTO[]?>("/User/Preferences", cancellationToken);

        return pref ?? Array.Empty<AlertPreferenceDTO>();
    }
}

