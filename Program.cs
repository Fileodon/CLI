using Spectre.Console.Cli;

namespace cli;

public class Program
{
    public static async Task Main(string[] args)
    {
        var app = new CommandApp<Initialize>();
        await app.RunAsync(args);
    }
}