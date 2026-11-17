using Spectre.Console.Cli;

namespace cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var app = new CommandApp<Initialize>();
        return await app.RunAsync(args);
    }
}