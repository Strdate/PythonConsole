using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace ModTools
{
    [XmlRoot("Configuration")]
    public sealed class ModConfiguration
    {
        private const float RGB = 255f;
        private const float Opaque = 1f;

        #region General

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

        #region Console

        [XmlElement("useModToolsConsole")]
        public bool UseModToolsConsole { get; set; } = true;

        [XmlElement("hiddenNotifications")]
        public int HiddenNotifications { get; set; }

        [XmlElement("logExceptionsToConsole")]
        public bool LogExceptionsToConsole { get; set; } = true;

        [XmlElement("consoleMaxHistoryLength")]
        public int ConsoleMaxHistoryLength { get; set; } = 1024;

        [XmlElement("consoleFormatString")]
        public string ConsoleFormatString { get; set; } = "[{{type}}] {{caller}}: {{message}}";

        [XmlElement("showConsoleOnMessage")]
        public bool ShowConsoleOnMessage { get; set; }

        [XmlElement("showConsoleOnWarning")]
        public bool ShowConsoleOnWarning { get; set; }

        [XmlElement("showConsoleOnError")]
        public bool ShowConsoleOnError { get; set; } = true;

        [XmlElement("consoleAutoScrollToBottom")]
        public bool ConsoleAutoScrollToBottom { get; set; } = true;

        #region Appearance

        [XmlElement("consoleMessageColor")]
        public Color ConsoleMessageColor { get; set; } = Color.white;

        [XmlElement("consoleWarningColor")]
        public Color ConsoleWarningColor { get; set; } = Color.yellow;

        [XmlElement("consoleErrorColor")]
        public Color ConsoleErrorColor { get; set; } = new Color(0.7f, 0.1f, 0.1f, 1f);

        [XmlElement("consoleExceptionColor")]
        public Color ConsoleExceptionColor { get; set; } = new Color(1f, 0f, 0f, 1f);

        #endregion

        #endregion

        #region Scene explorer

        [XmlElement("sceneExplorerSortAlphabetically")]
        public bool SortItemsAlphabetically { get; set; } = true;

        [XmlElement("sceneExplorerMaxHierarchyDepth")]
        public int MaxHierarchyDepth { get; set; } = 32;

        [XmlElement("sceneExplorerEvaluatePropertiesAutomatically")]
        public bool EvaluateProperties { get; set; } = true;

        [XmlElement("sceneExplorerShowFields")]
        public bool ShowFields { get; set; } = true;

        [XmlElement("sceneExplorerShowConsts")]
        public bool ShowConsts { get; set; }

        [XmlElement("sceneExplorerShowProperties")]
        public bool ShowProperties { get; set; } = true;

        [XmlElement("sceneExplorerShowMethods")]
        public bool ShowMethods { get; set; }

        [XmlElement("sceneExplorerShowModifiers")]
        public bool ShowModifiers { get; set; }

        [XmlElement("sceneExplorerShowInheritedMembers")]
        public bool ShowInheritedMembers { get; set; }

        #region Appearance

        [XmlElement("sceneExplorerTreeIdentSpacing")]
        public float TreeIdentSpacing { get; set; } = 16f;

        [XmlElement("gameObjectColor")]
        public Color GameObjectColor { get; set; } = new Color(80 / RGB, 200 / RGB, 180 / RGB, Opaque);

        [XmlElement("enabledComponentColor")]
        public Color EnabledComponentColor { get; set; } = new Color(220 / RGB, 220 / RGB, 170 / RGB, Opaque);

        [XmlElement("disabledComponentColor")]
        public Color DisabledComponentColor { get; set; } = new Color(180 / RGB, 180 / RGB, 190 / RGB, Opaque);

        [XmlElement("selectedComponentColor")]
        public Color SelectedComponentColor { get; set; } = new Color(80 / RGB, 160 / RGB, 220 / RGB, Opaque);

        [XmlElement("nameColor")]
        public Color NameColor { get; set; } = new Color(220 / RGB, 220 / RGB, 170 / RGB, Opaque);

        [XmlElement("typeColor")]
        public Color TypeColor { get; set; } = new Color(80 / RGB, 200 / RGB, 180 / RGB, Opaque);

        [XmlElement("keywordColor")]
        public Color KeywordColor { get; set; } = new Color(220 / RGB, 160 / RGB, 220 / RGB, Opaque);

        [XmlElement("modifierColor")]
        public Color ModifierColor { get; set; } = new Color(80 / RGB, 160 / RGB, 220 / RGB, Opaque);

        [XmlElement("memberTypeColor")]
        public Color MemberTypeColor { get; set; } = new Color(220 / RGB, 160 / RGB, 220 / RGB, Opaque);

        [XmlElement("valueColor")]
        public Color ValueColor { get; set; } = new Color(160 / RGB, 220 / RGB, 255 / RGB, Opaque);

        #endregion

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