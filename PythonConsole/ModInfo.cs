using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PythonConsole
{
    public class ModInfo : IUserMod
    {
        public string Name => "Python Console";

        public string Description => "Press ALT+S to open the console [BETA 0.1.8]";

        public const string settingsFileName = "PythonConsole";

        public static ModInfo Instance { get; private set; }

        public static readonly SavedInputKey ScriptEditorShortcut = new SavedInputKey("ScriptEditorShortcut", settingsFileName, SavedInputKey.Encode(KeyCode.S, false, false, true), true);
        public static readonly SavedInputKey ClipboardToolShortcut = new SavedInputKey("ClipboardToolShortcut", settingsFileName, SavedInputKey.Encode(KeyCode.A, false, false, true), true);
        public static readonly SettingsBool F5toExec = new SettingsBool("Use F5 to execute scipts", "Pressing F5 shortcut will execute current script", "F5toExec", true);
        public static readonly SettingsBool SyncExecution = new SettingsBool("Execute scripts synchronously (requires engine restart)", "Script execution will freeze simulation, but it may take less time", "SyncExecution", false);
        public static readonly SettingsBool ShowRemoteConsole = new SettingsBool("Debug: Show remote python console", "The external python program will run in a visible console window (requires engine restart)", "ShowRemoteConsole", false);
        public static readonly SettingsBool DoNotLaunchRemoteConsole = new SettingsBool("Debug: Do not launch remote python console", "The external python program won't launch automatically (you need to start it yourself)", "DoNotLaunchRemoteConsole", false);

        public static readonly string RemotePythonFolder = Path.Combine(DataLocation.executableDirectory, "SkylinesRemotePython");
        public static readonly int DEF_PORT = 6672;
        public ModInfo()
        {
            Instance = this;
            try {
                // Creating setting file - from SamsamTS
                if (GameSettings.FindSettingsFileByName(settingsFileName) == null) {
                    GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = settingsFileName } });
                }
            }
            catch (Exception e) {
                Debug.Log("Couldn't load/create the setting file.");
                Debug.LogException(e);
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            try {
                UIHelper group = helper.AddGroup(Name) as UIHelper;
                UIPanel panel = group.self as UIPanel;

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);

                F5toExec.Draw(group);
                SyncExecution.Draw(group, (b) => {
                    PythonConsole.CreateInstance();
                });
                ShowRemoteConsole.Draw(group);
                DoNotLaunchRemoteConsole.Draw(group);

                group.AddSpace(10);

                group.AddButton("Kill python engine", () => {
                    PythonConsole.KillInstance();
                });

            }
            catch (Exception e) {
                Debug.Log("OnSettingsUI failed");
                Debug.LogException(e);
            }
        }
    }
}
