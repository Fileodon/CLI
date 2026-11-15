using System.Reflection;
using System.Runtime.CompilerServices;
using Spectre.Console.Cli;

namespace cli;

public class Initialize : AsyncCommand<Flags>
{
    private static string CustomKey = @"0drtRpzgMFwH1wkSPwMDrn3H1BE1mfbPSPLJZ57czp/qHh835X6FFbFxUSjc5m6JwDN55PCV4W2/QNAuOGfziU/MW0Ibc9Ut0wV7SZbcbI0+d3N3fKXONUNN6XNE9loAwReGUvHleF4DjKdAtizurMEi8OUIA5Eq1Srx4KoQ/JBPqMXnlWp0K1JOUWEwSPLtKjQcKX4OdAujUMpLrUPOH7rGeRMB3cT1Sa9vCHu6hLQzJfYJhhLSLqr46IzKwp8JgzE+AfNhiu0GXyezjv9HtLMPtLF5C6muzyQ7tDDOXo4bT5MMnakUWqWPucZH7cek8QIu6BaJ8JAYLA3gG+nF16mWnGgsRMneaIppvqvKszt+Jab+PbX1PEZLS98ps4WgoH5HyjDJO/NN83k9ZxkjZm30DPr/Rt4NXo13K2OiscoaSca0QjZcU7J9o9Yeq1AEGcNro1SzgaFRHbCjK+X9QgFa1fYF3VyU+7cKucgkAHYN2e86OGZZX2DRD79ZvIyY8rdSTwH0Z7Cwrf76YmBEjfYxbJmMQ0g47xzaW7ojOr8JsDmlFOmUpdSme51nyQIWl6EHq/QTgVckss8INm0/Lu/pPWHOJ3NeEBxkUHl01DDsO3UfknqLUz0xWkB7O3M7vWVB7ksrprsj4aryt9mqMzwrTspegPLBLT5buJ9oHDtHCV4b1pSFZ7IJNWDPsBKIgy+ybigiVCx8Gj2dCcU6PywO5fzrSIYHUuo=";

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
