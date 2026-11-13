using System.Text.Json;

namespace cli;

internal static class ConnectServer
{
    public static async Task<int> Connect(Flags flags)
    {
        if (flags.Delete)
        {
            Initialize.Log("Delete connection requested", true);
            return 0;
        }

        if (string.IsNullOrEmpty(flags.Api) || flags.Org == 0 || string.IsNullOrEmpty(flags.Name) || string.IsNullOrEmpty(flags.Host) || flags.Port == 0 || string.IsNullOrEmpty(flags.Token))
        {
            Initialize.Log("One or more required flags are missing. Please provide all required flags.", true);
            return 1;
        }

        int id;
        try
        {
            id = await SendRequest(flags);
        }
        catch (HttpRequestException ex)
        {
            Initialize.Log($"Could not send the registration request: {ex.Message}", true);
            return 1;
        }

        if (id <= 0)
        {
            return 1;
        }

        string key;
        try
        {
            key = await PollRequest(flags, id);
        }
        catch (HttpRequestException ex)
        {
            Initialize.Log($"Could not poll the registration request: {ex.Message}", true);
            return 1;
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            Initialize.Log("The API did not return an SSH public key; connection setup was not performed.", true);
            return 1;
        }

        Initialize.Log("Installing the returned SSH key locally.");
        SetupConnection.Setup(key);
        return 0;
    }
    private class IdRequest
    {
        public int id { get; set; }
    }

    private static async Task<int> SendRequest(Flags flags)
    {
        RequestType request = new()
        {
            authCode = flags.Token,
            displayName = flags.Name,
            host = flags.Host,
            port = flags.Port,
            url = $"{flags.Api}/api/organization/{flags.Org}/sftpconnection"
        };

        Initialize.Log($"Sending connection request to {request.url}.");
        using HttpClient client = new();
        using HttpContent content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
        using var resp = await client.PostAsync(request.url, content);
        if (!resp.IsSuccessStatusCode)
        {
            Initialize.Log($"Registration request failed with HTTP {(int)resp.StatusCode} ({resp.StatusCode}).", true);
            return -1;
        }

        var responseContent = await resp.Content.ReadAsStringAsync();
        var idRequest = JsonSerializer.Deserialize<IdRequest>(responseContent);
        if (idRequest is null || idRequest.id <= 0)
        {
            Initialize.Log("The API response did not contain a valid connection ID.", true);
            return -1;
        }

        Initialize.Log($"Connection request accepted with ID {idRequest.id}.");
        return idRequest.id;
    }

    private static async Task<string> PollRequest(Flags flags, int id)
    {
        int errCount = 0;
        while (true)
        {
            Initialize.Log("Waiting 5 seconds before checking whether the connection was accepted.");
            await Task.Delay(5000);
            using HttpClient client = new();
            var url = $"{flags.Api}/api/organization/{flags.Org}/sftpconnection/{id}/poll";
            using var resp = await client.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                errCount++;
                Initialize.Log($"Polling failed with HTTP {(int)resp.StatusCode} ({resp.StatusCode}); attempt {errCount} of 4.", true);
                if (errCount > 3)
                {
                    return string.Empty;
                }
                continue;
            }

            var content = await resp.Content.ReadAsStringAsync();
            PollingType? type = JsonSerializer.Deserialize<PollingType>(content);
            if (type is null)
            {
                Initialize.Log("The API returned an invalid polling response.", true);
                return string.Empty;
            }

            if (type.status == 0)
            {
                Initialize.Log("Polling success!", true);
                return type.publickey;
            }

            Initialize.Log("The connection has not been accepted yet.");
        }
    }
}
