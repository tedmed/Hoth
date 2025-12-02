using CAP.DTOs;


public class CAPApiClient(HttpClient httpClient)
{
    public async Task<AlertInfoDTO[]> GetAlertsAsync(CancellationToken cancellationToken = default)
    {
        List<AlertInfoDTO>? AlertInfos = null;

        await foreach (var alert in httpClient.GetFromJsonAsAsyncEnumerable<AlertInfoDTO>("/CAP/Alarms", cancellationToken))
        {
            if (alert is not null)
            {
                AlertInfos ??= [];
                AlertInfos.Add(alert);
            }
        }

        return AlertInfos?.ToArray() ?? [];
    }

    public async Task<string[]> GetAvailableRegionsAsync(CancellationToken cancellationToken = default)
    {
        HashSet<string> areaDescs = new();

        await foreach (var alertInfo in httpClient.GetFromJsonAsAsyncEnumerable<string>("/CAP/AvailableRegions", cancellationToken))
        {
            if (alertInfo is not null)
            {
                areaDescs.Add(alertInfo);
            }
        }

        return areaDescs?.ToArray() ?? [];
    }

    public async Task<string[]> GetAvailableSpecificRegionsAsync(string AreaDesc,CancellationToken cancellationToken = default)
    {
        HashSet<string> specificAreasDescs = new();

        await foreach (var alertInfo in httpClient.GetFromJsonAsAsyncEnumerable<string>($"/CAP/AvailableSpecificRegions?AreaDesc={AreaDesc}", cancellationToken))
        {
            if (alertInfo is not null)
            {
                specificAreasDescs.Add(alertInfo);
            }
        }

        return specificAreasDescs?.ToArray() ?? [];
    }

    public async Task<AlertInfoDTO[]> GetUserSpecificAlertsAsync(CancellationToken cancellationToken = default)
    {
        List<AlertInfoDTO>? AlertInfos = null;

        await foreach (var alert in httpClient.GetFromJsonAsAsyncEnumerable<AlertInfoDTO>("/CAP/UserSpecificAlarms", cancellationToken))
        {
            if (alert is not null)
            {
                AlertInfos ??= [];
                AlertInfos.Add(alert);
            }
        }

        return AlertInfos?.ToArray() ?? [];
    }
}

