using System.Reflection;
using System.Runtime.CompilerServices;
using Spectre.Console.Cli;

namespace cli;

public class Initialize : AsyncCommand<Flags>
{
    private static bool _verbose;
    public override async Task<int> ExecuteAsync(CommandContext context, Flags settings, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        if (settings.Verbose)
        {
            Console.WriteLine("Verbose output enabled");
            _verbose = true;
        }
        if (settings.Delete)
        {
            Console.WriteLine("Delete connection requested");
        }

        if (string.IsNullOrEmpty(settings.AuthCode))
        {
            while (true)
            {
                Console.Write("Authentication code: ");
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) || input.Length != 8)
                {
                    continue;
                }

                settings.AuthCode = input;
                break;
            }
        }

        if (string.IsNullOrEmpty(settings.Api))
        {
            while (true)
            {
                Console.Write("API URL: ");
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                settings.Api = input;
                break;
            }
        }

        if (string.IsNullOrEmpty(settings.Host))
        {
            var ip = await FetchPublicIp();
            if (string.IsNullOrEmpty(ip))
            {
                throw new Exception("Could not fetch public IP address. Please provide the --host flag.");
            }
            settings.Host = ip;
        }

        return await ConnectServer.Connect(settings);
    }


    private static async Task<string> FetchPublicIp()
    {
        using HttpClient client = new();
        return await client.GetStringAsync("https://api.ipify.org");
    }

    public static void Log(string message, bool force = false, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        if (_verbose || force)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | [{filePath}:{lineNumber} {memberName}] {message}");
        }
    }

    private void SetFlagvalue(string message, Flags settings, PropertyInfo prop, Type flagType)
    {
        try
        {
            do
            {
                Console.Write(message);
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                if (flagType == typeof(int))
                {
                    if (int.TryParse(input, out int intValue))
                    {
                        prop.SetValue(settings, intValue);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a valid integer.");
                    }
                }
                else if (flagType == typeof(bool))
                {
                    if (bool.TryParse(input, out bool boolValue))
                    {
                        prop.SetValue(settings, boolValue);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter 'true' or 'false'.");
                    }
                }
                else if (flagType == typeof(string))
                {
                    prop.SetValue(settings, input);
                }
                break;
            } while (true);

        }
        catch (Exception ex)
        {
            Log(ex.Message);
            throw;
        }
    }
}
