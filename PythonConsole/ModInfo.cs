using ColossalFramework;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PythonConsole
{
    public class ModInfo : IUserMod
    {
        public string Name => "Python Console";

        public string Description => "[ALPHA 0.0.1]";

        public const string settingsFileName = "PythonConsoleMod";

        public static readonly SavedInputKey sc_toggleConsole = new SavedInputKey("sc_toggleConsole", settingsFileName, SavedInputKey.Encode(KeyCode.P, false, false, true), true);

        public ModInfo()
        {
            try
            {
                // Creating setting file - from SamsamTS
                if (GameSettings.FindSettingsFileByName(settingsFileName) == null)
                {
                    GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = settingsFileName } });
                }
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't load/create the setting file.");
                Debug.LogException(e);
            }
        }
    }
}
