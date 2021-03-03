using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace PythonConsole
{
    [XmlRoot("Configuration")]
    public sealed class ModConfiguration
    {
        private const float RGB = 255f;
        private const float Opaque = 1f;

        #region General

        [XmlElement("tcpPort")]
        public int tcpPort { get; set; } = 0;

        [XmlElement("customPrefabsObject")]
        public bool CustomPrefabsObject { get; set; } = true;

        [XmlElement("extendGamePanels")]
        public bool ExtendGamePanels { get; set; } = true;

        [XmlElement("logLevel")]
        public int LogLevel { get; set; }

        [XmlElement("selectionTool")]
        public bool SelectionTool { get; set; } = true;

        #endregion

        #region Appearance

        [XmlElement("fontName")]
        public string FontName { get; set; } = "Courier New Bold";

        [XmlElement("fontSize")]
        public int FontSize { get; set; } = 14;

        [XmlElement("backgroundColor")]
        public Color BackgroundColor { get; set; } = new Color(100 / RGB, 100 / RGB, 110 / RGB, 235 / RGB);

        [XmlElement("titlebarColor")]
        public Color TitleBarColor { get; set; } = new Color(0 / RGB, 0 / RGB, 0 / RGB, Opaque);

        [XmlElement("titlebarTextColor")]
        public Color TitleBarTextColor { get; set; } = new Color(255 / RGB, 255 / RGB, 255 / RGB, Opaque);

        #endregion

        #region Window state

        [XmlElement("mainWindowRect")]
        public Rect MainWindowRect { get; set; } = new Rect(128, 128, 356, 300);

        [XmlElement("consoleRect")]
        public Rect ConsoleRect { get; set; } = new Rect(16f, 16f, 512f, 256f);

        [XmlElement("sceneExplorerRect")]
        public Rect SceneExplorerRect { get; set; } = new Rect(128, 440, 800, 500);

        [XmlElement("watchesRect")]
        public Rect WatchesRect { get; set; } = new Rect(504, 128, 800, 300);

        #endregion

        #region Script editor

        [XmlElement("scriptEditorWorkspacePath")]
        public string ScriptWorkspacePath { get; set; } = string.Empty;

        #endregion

        public static ModConfiguration Deserialize(string filename)
        {
            var serializer = new XmlSerializer(typeof(ModConfiguration));

            try
            {
                using (var reader = new StreamReader(filename)) 
                    return (ModConfiguration)serializer.Deserialize(reader);
            }
            catch (Exception e)
            {
                Debug.LogError("Error happened when deserializing configuration");
                Debug.LogException(e);
            }

            return null;
        }

        public void Serialize(string filename)
        {
            var serializer = new XmlSerializer(typeof(ModConfiguration));

            try
            {
                using (var writer = new StreamWriter(filename))
                    serializer.Serialize(writer, this);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to serialize configuration");
                Debug.LogException(ex);
            }
        }
    }
}