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

        foreach (var prop in typeof(Flags).GetProperties())
        {
            var value = prop.GetValue(settings);
            if (value == null || (value is string str && string.IsNullOrEmpty(str)))
            {
                Log($"Flag '{prop.Name}' is not set. Prompting for value...");
                SetFlagvalue($"Please enter a value for '{prop.Name}': ", settings, prop, prop.PropertyType);
            }
        }

        await ConnectServer.Connect(settings);
        return 0;
    }

    public static void Log(string message, bool force = false, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        if (_verbose || force)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | [{filePath}:{lineNumber} {memberName}] {message}");
        }
    }

    private void SetFlagvalue(string message, object flag, PropertyInfo prop, Type flagType)
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
                        prop.SetValue(flag, intValue);
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
                        prop.SetValue(flag, boolValue);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter 'true' or 'false'.");
                    }
                }
                else if (flagType == typeof(string))
                {
                    prop.SetValue(flag, input);
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
