using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace AllSpecPlugin;

public class BaseConfigs : BasePluginConfig
{

    [JsonPropertyName("AllSpecCommand")]
    public string AllSpecCommand { get; set; } = "css_allspec";

    [JsonPropertyName("AllSpecCommandFlag")]
    public string AllSpecCommandFlag { get; set; } = "@css/generic";

}