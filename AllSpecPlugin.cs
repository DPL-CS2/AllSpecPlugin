using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Plugin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;

namespace AllSpecPlugin;

public class AllSpecPlugin : BasePlugin
{
    public override string ModuleName => "AllSpec Plugin";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "DPL";
    public override string ModuleDescription => "Moves all human players to the spectators team";

    private List<string> allowedSteamIDs = new();
    private Dictionary<string, string> messages = new();
    private string currentLanguage = "en_us";

    public override void Load(bool hotReload)
    {
        EnsureDirectoryStructure();
        LoadPluginConfig();
        LoadLanguageFile();
        LoadAdminList();

        AddCommand("css_allspec", messages["command_description"], OnAllSpecCommand);
    }

    private void EnsureDirectoryStructure()
    {
        Directory.CreateDirectory(Path.Combine(ModuleDirectory, "config"));
        Directory.CreateDirectory(Path.Combine(ModuleDirectory, "lang"));

        string configPath = Path.Combine(ModuleDirectory, "config", "plugin_config.json");
        if (!File.Exists(configPath))
        {
            var defaultConfig = new PluginConfig { Language = "en_us" };
            File.WriteAllText(configPath, JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true }));
        }

        string adminsPath = Path.Combine(ModuleDirectory, "config", "admins.json");
        if (!File.Exists(adminsPath))
        {
            var defaultAdmins = new AdminConfig { AllowedSteamIDs = new List<string> { "76561198000000000" } };
            File.WriteAllText(adminsPath, JsonSerializer.Serialize(defaultAdmins, new JsonSerializerOptions { WriteIndented = true }));
        }

        string langPathEN = Path.Combine(ModuleDirectory, "lang", "en_us.json");
        if (!File.Exists(langPathEN))
        {
            var enMessages = new Dictionary<string, string>
            {
                { "command_description", "Move all players to spectators" },
                { "no_permission", "You do not have permission to use this command." },
                { "moved_count", "{0} players were moved to spectators." },
            };
            File.WriteAllText(langPathEN, JsonSerializer.Serialize(enMessages, new JsonSerializerOptions { WriteIndented = true }));
        }

        string langPathPT = Path.Combine(ModuleDirectory, "lang", "pt_br.json");
        if (!File.Exists(langPathPT))
        {
            var ptMessages = new Dictionary<string, string>
            {
                { "command_description", "Move todos para o time de espectadores" },
                { "no_permission", "Você não tem permissão para usar este comando." },
                { "moved_count", "{0} jogadores foram movidos para espectador." },
            };
            File.WriteAllText(langPathPT, JsonSerializer.Serialize(ptMessages, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    private void LoadPluginConfig()
    {
        string configPath = Path.Combine(ModuleDirectory, "config", "plugin_config.json");
        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<PluginConfig>(json);
            if (config != null && !string.IsNullOrEmpty(config.Language))
                currentLanguage = config.Language;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AllSpecPlugin] Error loading plugin_config.json: {ex.Message}");
        }
    }

    private void LoadLanguageFile()
    {
        string langPath = Path.Combine(ModuleDirectory, "lang", $"{currentLanguage}.json");
        try
        {
            if (File.Exists(langPath))
            {
                var json = File.ReadAllText(langPath);
                var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (loaded != null)
                    messages = loaded;
            }
            else
            {
                Console.WriteLine($"[AllSpecPlugin] Language file {currentLanguage}.json not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AllSpecPlugin] Error loading language file: {ex.Message}");
        }
    }

    private void LoadAdminList()
    {
        string path = Path.Combine(ModuleDirectory, "config", "admins.json");
        try
        {
            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<AdminConfig>(json);
            if (config?.AllowedSteamIDs != null)
                allowedSteamIDs = config.AllowedSteamIDs;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AllSpecPlugin] Error loading admins.json: {ex.Message}");
        }
    }

    private void OnAllSpecCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller == null || !allowedSteamIDs.Contains(caller.SteamID.ToString()))
        {
            caller?.PrintToChat(messages["no_permission"]);
            return;
        }

        int count = 0;
        foreach (var p in Utilities.GetPlayers().Where(p => !p.IsBot && p.IsValid))
        {
            p.ChangeTeam(CsTeam.Spectator);
            count++;
        }

        string resultMsg = string.Format(messages["moved_count"], count);
        Server.PrintToConsole($"[AllSpecPlugin] {resultMsg}");
        caller?.PrintToChat(resultMsg);
    }

    private class PluginConfig
    {
        public string Language { get; set; } = "en_us";
    }

    private class AdminConfig
    {
        public List<string> AllowedSteamIDs { get; set; } = new();
    }
}
