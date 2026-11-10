using Spectre.Console.Cli;

namespace cli;

public class Program
{
    public static void Main(string[] args)
    {
        var app = new CommandApp<Initialize>();
        app.Run(args);
    }
}