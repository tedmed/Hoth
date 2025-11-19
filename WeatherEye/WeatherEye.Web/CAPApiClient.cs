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
        HashSet<string> AlertInfos = new();

        await foreach (var alertInfo in httpClient.GetFromJsonAsAsyncEnumerable<string>("/CAP/AvailableRegions", cancellationToken))
        {
            if (alertInfo is not null)
            {
                AlertInfos.Add(alertInfo);
            }
        }

        return AlertInfos?.ToArray() ?? [];
    }
}

