using System.Collections.Generic;

namespace OBSControlDeck
{
    public class SceneButtonSlot
    {
        public string? SceneName { get; set; }
        public string? HotkeyName { get; set; }
        public string? HotkeyOnlyDisplayName { get; set; }
    }

    public class Profile
    {
        public string ProfileName { get; set; } = string.Empty;
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 4455;
        public string Password { get; set; } = string.Empty;
        public bool AutoConnect { get; set; } = false;
        public bool AlwaysOnTop { get; set; } = true;
        public Dictionary<string, string> SceneMap { get; set; } = new();
        public string? ReplayHotkeyName { get; set; }
        public Dictionary<string, List<string>> SelectedSources { get; set; } = new();
        public string Scene1ButtonText { get; set; } = "SCENE 1";
        public string Scene2ButtonText { get; set; } = "SCENE 2";
        public string Scene3ButtonText { get; set; } = "SCENE 3";
        public string Scene4ButtonText { get; set; } = "SCENE 4";
        public string Scene5ButtonText { get; set; } = "SCENE 5";
        public string Scene6ButtonText { get; set; } = "SCENE 6";
        public string Scene7ButtonText { get; set; } = "MAIN";
        public string Scene8ButtonText { get; set; } = "SX";
        public string Scene9ButtonText { get; set; } = "DX";
        public string? Mic1InputName { get; set; }
        public string? Mic2InputName { get; set; }
        public string? WirelessInputName { get; set; }
        public Dictionary<string, Dictionary<string, string>> FunctionAliases { get; set; } = new();

        public Dictionary<string, List<string?>> FunctionSlotOrder { get; set; } = new();

        public List<string> SelectedScenes { get; set; } = new();

        public List<string?> SceneSlotOrder { get; set; } = new();

        public List<SceneButtonSlot?>? SceneButtonSlots { get; set; }

        public Dictionary<string, string> SceneAliases { get; set; } = new();
        public bool SceneAliasesMigrated { get; set; } = false;

        // Future extensible
    }
}