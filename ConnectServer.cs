using System.Text.Json;

namespace cli;

internal static class ConnectServer
{
    public static void Connect(Flags flags)
    {

    }
    private class IdRequest
    {
        public int id { get; set; }
    }

    private static async Task<int> SendRequest(Flags flags)
    {

        RequestType request = new()
        {
            displayName = flags.Name,
            organizationId = flags.Org,
            host = flags.Host,
            port = flags.Port,
            status = 2,
            url = $"{flags.Api}/api/organization/{flags.Org}/SFTPConnection"
        };

        using HttpClient client = new();
        HttpContent content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
        using var resp = await client.PostAsync(request.url, content);
        if (resp is null || !resp.IsSuccessStatusCode)
        {
            return -1;
        }

        var responseContent = await resp.Content.ReadAsStringAsync();
        var idRequest = JsonSerializer.Deserialize<IdRequest>(responseContent);
        return idRequest is null ? -1 : idRequest.id;
    }

    private static async Task<string> PollRequest(Flags flags, int id)
    {
        int errCount = 0;
        while (true)
        {
            await Task.Delay(5000);
            using HttpClient client = new();
            var url = $"{flags.Api}/api/organization/{flags.Org}/SFTPConnection/{id}";
            using var resp = await client.GetAsync(url);
            if (resp is null || !resp.IsSuccessStatusCode)
            {
                errCount++;
                if (errCount > 3)
                {
                    return string.Empty;
                }
                continue;
            }


            var content = await resp.Content.ReadAsStringAsync();
            PollingType type = JsonSerializer.Deserialize<PollingType>(content);

            if (type.status == 0)
            {
                Initialize.Log("Polling success!", true);
                return type.publickey;
            }
            else
            {
                Initialize.Log("Polling has not been accepted yet, retrying in 5 seconds!", true);
            }
        }
    }
}
