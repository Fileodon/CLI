using System.ComponentModel;
using Spectre.Console.Cli;

namespace cli;

public class Flags : CommandSettings
{
    [CommandArgument(0, "<authcode>")]
    [Description("Generated authentication code from the website")]
    public string AuthCode { get; set; }
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

    [CommandOption("--host")]
    [Description("Master server host")]
    public string? Host { get; set; }

    [CommandOption("--port")]
    [Description("Master server port")]
    public int? Port { get; set; }
}

