using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;

namespace AllSpecPlugin;

public partial class AllSpecPlugin : BasePlugin, IPluginConfig<BaseConfigs>
{
    public override string ModuleName => "AllSpec Plugin";
    public override string ModuleVersion => "1.0.2";
    public override string ModuleAuthor => "DPL";
    public override string ModuleDescription => "Moves all human players to the spectators team";

    public required BaseConfigs Config { get; set; }

    public override void Load(bool hotReload)
    {
        AddCommand(Config.AllSpecCommand, Localizer["command_description"], OnAllSpecCommand);
    }
    public void OnConfigParsed(BaseConfigs config)
    {
        Config = config;
    }

    private void OnAllSpecCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (!string.IsNullOrEmpty(Config.AllSpecCommandFlag) && !AdminManager.PlayerHasPermissions(caller, Config.AllSpecCommandFlag))
        {
            caller?.PrintToChat($"{Localizer["prefix"]} {Localizer["NoPermissions"]}");
            return;
        }

        int count = 0;
        var players = Utilities.GetPlayers().Where(p => !p.IsBot && p.IsValid && p.Team != CsTeam.Spectator);

        foreach (var player in players)
        {
            try
            {
                player.ChangeTeam(CsTeam.Spectator);
                count++;
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"Error moving player to spectators: {ex.Message}");
            }
        }

        string movedMessage = $"{Localizer["prefix"]} {string.Format(Localizer["moved_count"], count)}";
        Server.PrintToConsole(movedMessage);
        caller?.PrintToChat(movedMessage);
    }

}