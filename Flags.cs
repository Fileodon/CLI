using System.ComponentModel;
using Spectre.Console.Cli;

namespace cli;

public class Flags : CommandSettings
{
    [CommandOption("-v|--verbose")]
    [Description("Enable verbose output")]
    [DefaultValue(false)]
    public bool Verbose { get; set; }
    [CommandOption("-d|--delete")]
    [Description("Delete the connection to the API")]
    [DefaultValue(false)]
    public bool Delete { get; set; }

    [CommandOption("--api")]
    [Description("Api URI")]
    [DefaultValue("")]
    public string Api { get; set; }

    [CommandOption("--org")]
    [Description("Organisation ID")]
    public int Org { get; set; }

    [CommandOption("--name")]
    [Description("Server name")]
    public string Name { get; set; }

    [CommandOption("--host")]
    [Description("Master server host")]
    public string Host { get; set; }

    [CommandOption("--port")]
    [Description("Master server port")]
    public int Port { get; set; }

    [CommandOption("--token")]
    [Description("API token")]
    public string Token { get; set; }

}

