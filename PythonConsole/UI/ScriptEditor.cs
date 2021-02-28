using System;
using System.Collections.Generic;
using System.IO;
using PythonConsole;
using UnityEngine;

namespace PythonConsole
{
    internal sealed class ScriptEditor : GUIWindow
    {
        private const string TextAreaControlName = "PythonScriptEditorTextArea";
        private const string OutputAreaControlName = "PythonScriptOutputTextArea";
        private const string ExampleScriptFileName = "Script.py";

        private const float HeaderHeight = 90.0f;
        private const float FooterHeight = 55.0f;
        private const float OutputHeight = 200.0f;

        private readonly List<ScriptEditorFile> projectFiles = new List<ScriptEditorFile>();

        private readonly GUIArea headerArea;
        private readonly GUIArea editorArea;
        private readonly GUIArea footerArea;
        private readonly GUIArea outputArea;

        //private IModEntryPoint currentMod;
        private string lastError = string.Empty;
        private string output = "Starting remote python engine...\n";

        private Vector2 editorScrollPosition = Vector2.zero;
        private Vector2 outputScrollPosition = Vector2.zero;
        private Vector2 projectFilesScrollPosition = Vector2.zero;

        private string projectWorkspacePath = string.Empty;

        private bool outputVisible = true;
        private bool renamingFile;
        private bool deletingFile;
        private string newFileName;

        private bool _capsProcessed;
        private bool _f5Processed;
        private DateTime _f5PressedTime = DateTime.Now;

        private ScriptEditorFile currentFile;

        private readonly ModalUI modalUI = new ModalUI();

        public ScriptEditor()
            : base("Python Console", new Rect(16.0f, 16.0f, 800.0f, 540.0f))
        {
            headerArea = new GUIArea(this);

            editorArea = new GUIArea(this, new Vector2(8, 0));

            footerArea = new GUIArea(this, new Vector2(8, 4));

            outputArea = new GUIArea(this, new Vector2(8, 4));
            ResetAreaHeights();
        }

        private void ResetAreaHeights()
        {
            headerArea.OffsetBy(vertical: 32f)
                .ChangeSizeRelative(height: 0)
                .ChangeSizeBy(height: HeaderHeight);

            editorArea.OffsetBy(vertical: 32.0f + HeaderHeight)
                .ChangeSizeBy(height: -(32.0f + HeaderHeight + FooterHeight + (outputVisible ? OutputHeight : 0)));

            footerArea.OffsetRelative(vertical: 1f)
                .OffsetBy(vertical: -FooterHeight - (outputVisible ? OutputHeight : 0))
                .ChangeSizeRelative(height: 0)
                .ChangeSizeBy(height: FooterHeight);

            outputArea.OffsetRelative(vertical: 1f)
                .OffsetBy(vertical: -(outputVisible ? OutputHeight : 0))
                .ChangeSizeRelative(height: 0)
                .ChangeSizeBy(height: (outputVisible ? OutputHeight : 0));
        }

        public void ReloadProjectWorkspace()
        {
            AbortFileActions();
            bool isDefaultPath = false;
            string configPath = UnityPythonObject.Instance.Config.ScriptWorkspacePath;
            projectWorkspacePath = configPath;
            if(configPath == null || configPath == "") {
                projectWorkspacePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SkylinesPython");
                isDefaultPath = true;
            }
            if (projectWorkspacePath.Length == 0)
            {
                lastError = "Invalid project workspace path";
                return;
            }

            var exampleFileExists = false;

            projectFiles.Clear();
            System.IO.Directory.CreateDirectory(projectWorkspacePath);

            try
            {
                foreach (var file in FileUtil.ListFilesInDirectory(projectWorkspacePath))
                {
                    if (Path.GetExtension(file) == ".py")
                    {
                        var fileContent = new ScriptEditorFile(File.ReadAllText(file), file);
                        projectFiles.Add(fileContent);
                        if (Path.GetFileName(file) == ExampleScriptFileName)
                        {
                            exampleFileExists = true;
                            currentFile = fileContent;
                        }
                    }
                }

                if (!exampleFileExists)
                {
                    var exampleFile = new ScriptEditorFile(ScriptEditorFile.DefaultSource, Path.Combine(projectWorkspacePath, ExampleScriptFileName));
                    projectFiles.Add(exampleFile);
                    SaveProjectFile(exampleFile);
                    if (currentFile == null)
                    {
                        currentFile = exampleFile;
                    }
                }

                if(isDefaultPath) {
                    try {
                        string archivePath = Path.Combine(ModPath.Instsance.AssemblyPath, "ExamplePythonScripts.zip");
                        string destPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.Combine("SkylinesPython","Examples"));
                        using (var unzip = new Unzip(archivePath)) {
                            unzip.ExtractToDirectory(destPath);
                        }
                    } catch { }
                }
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return;
            }

            lastError = string.Empty;
        }

        public void PrintOutput(string text)
        {
            output += text;
            outputScrollPosition = new Vector2(outputScrollPosition.x, Mathf.Infinity);
        }

        public void PrintError(string text)
        {
            output += "Error: " + text + "\n";
            lastError = text;
            outputScrollPosition = new Vector2(outputScrollPosition.x, Mathf.Infinity);
        }

        protected override void DrawWindow()
        {
            DrawHeader();

            if (projectFiles.Count > 0)
            {
                DrawEditor();
                DrawFooter();
                DrawOutput();
            }
            else
            {
                editorArea.Begin();
                GUILayout.Label("Select a valid project workspace path");
                editorArea.End();
            }
        }

        protected override void HandleException(Exception ex)
        {
            //Logger.Error("Exception in ScriptEditor - " + ex.Message);
            Visible = false;
        }

        private static void SaveProjectFile(ScriptEditorFile file) => File.WriteAllText(file.Path, file.Source);

        private void SaveAllProjectFiles()
        {
            try
            {
                foreach (var file in projectFiles)
                {
                    SaveProjectFile(file);
                }
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return;
            }

            lastError = string.Empty;
        }

        public void OnUpdate()
        {
            var middleButtonState = MouseButtonState.None;
            if (Input.GetMouseButtonDown(2)) {
                middleButtonState = MouseButtonState.Pressed;
            } else if (Input.GetMouseButtonUp(2)) {
                middleButtonState = MouseButtonState.Released;
            } else if (Input.GetMouseButton(2)) {
                middleButtonState = MouseButtonState.Held;
            }

            modalUI.Update(IsMouseOverWindow(), middleButtonState);
        }

        private void DrawHeader()
        {
            headerArea.Begin();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Project workspace:", GUILayout.ExpandWidth(false));
            var newProjectWorkspacePath = GUILayout.TextField(projectWorkspacePath, GUILayout.ExpandWidth(true));
            if (!newProjectWorkspacePath.Equals(projectWorkspacePath))
            {
                projectWorkspacePath = newProjectWorkspacePath.Trim();
                UnityPythonObject.Instance.Config.ScriptWorkspacePath = projectWorkspacePath;
                UnityPythonObject.Instance.SaveConfig();
            }

            if (GUILayout.Button("Reload", GUILayout.Width(100)))
            {
                ReloadProjectWorkspace();
            }

            GUILayout.EndHorizontal();

            projectFilesScrollPosition = GUILayout.BeginScrollView(projectFilesScrollPosition);
            GUILayout.BeginHorizontal();

            foreach (var file in projectFiles)
            {
                string caption = Path.GetFileName(file.Path);
                if (GUILayout.Button(currentFile == file ? "= " + caption + " =" : caption, GUILayout.ExpandWidth(false)))
                {
                    AbortFileActions();
                    currentFile = file;
                }
            }

            if (GUILayout.Button("+ New File", GUILayout.ExpandWidth(false)))
            {
                for(int i = 0; ;i++)
                {
                    string path = Path.Combine(projectWorkspacePath, "NewFile" + (i == 0 ? "" : i.ToString()) + ".py");
                    if(!File.Exists(path))
                    {
                        var newFile = new ScriptEditorFile("print(\"Hello world!\")", path);
                        projectFiles.Add(newFile);
                        SaveProjectFile(newFile);
                        currentFile = newFile;
                        break;
                    }
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            headerArea.End();
        }

        private void DrawEditor()
        {
            editorArea.Begin();

            editorScrollPosition = GUILayout.BeginScrollView(editorScrollPosition);

            GUI.SetNextControlName(TextAreaControlName);

            var text = GUILayout.TextArea(currentFile != null ? currentFile.Source : "No file loaded..", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

            if (GUIUtility.keyboardControl == editor.controlID && Event.current.capsLock)
            {
                if(!_capsProcessed) {
                    _capsProcessed = true;
                    if (text.Length >= editor.cursorIndex) {
                        text = text.Insert(editor.cursorIndex, "  ");
                        editor.cursorIndex += 2;
                        editor.selectIndex = editor.cursorIndex;
                    }
                }
            } else {
                _capsProcessed = false;
            }

            if (GUIUtility.keyboardControl == editor.controlID && Event.current.keyCode == KeyCode.F5) {
                if(ModInfo.F5toExec.value && !_f5Processed && (DateTime.Now - _f5PressedTime).TotalMilliseconds > 500) {
                    _f5Processed = true;
                    _f5PressedTime = DateTime.Now;
                    Execute();
                }
            } else {
                _f5Processed = false;
            }

            if (currentFile != null)
            {
                currentFile.Source = text;
            }

            GUILayout.EndScrollView();

            editorArea.End();
        }

        private void Execute()
        {
            if(PythonConsole.Instance?.State == ConsoleState.Ready) {
                AbortFileActions();
                PythonConsole.Instance.ScheduleExecution(currentFile.Source);
                lastError = string.Empty;
            }
        }

        private void DrawFooter()
        {
            footerArea.Begin();

            GUILayout.BeginHorizontal();

            ConsoleState state = PythonConsole.Instance?.State ?? ConsoleState.Initializing;

            if(state != ConsoleState.Dead) {
                GUI.enabled = state == ConsoleState.Ready;

                if (GUILayout.Button("Execute (F5)")) {
                    Execute();
                }

                GUI.enabled = state == ConsoleState.ScriptRunning;

                if (GUILayout.Button("Abort")) {
                    AbortFileActions();
                    PythonConsole.Instance.AbortScript();
                }
            } else {
                if (GUILayout.Button("Restart engine")) {
                    PythonConsole.CreateInstance();
                }
            }

            GUI.enabled = true;

            GUILayout.Label(state == ConsoleState.ScriptRunning ? "Executing..." : (state == ConsoleState.ScriptAborting ? "Aborting..." : (lastError != "" ? "Last error: " + lastError : "")));

            GUILayout.FlexibleSpace();

            if(renamingFile)
            {
                try
                {
                    GUILayout.Label("New name:", GUILayout.ExpandWidth(false));
                    newFileName = GUILayout.TextField(newFileName, GUILayout.Width(150));
                    if (GUILayout.Button("OK"))
                    {
                        string newPath = Path.Combine(Path.GetDirectoryName(currentFile.Path), newFileName + ".py");
                        File.Move(currentFile.Path, newPath);
                        currentFile.Rename(newPath);
                        AbortFileActions();
                    }
                    if (GUILayout.Button("Cancel"))
                    {
                        AbortFileActions();
                    }
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                    return;
                }
            } else if(deletingFile)
            {
                GUILayout.Label("Delete?", GUILayout.ExpandWidth(false));
                if (GUILayout.Button("Yes"))
                {
                    try
                    {
                        File.Delete(currentFile.Path);
                    }
                    catch (Exception ex)
                    {
                        lastError = ex.Message;
                        return;
                    }
                    ReloadProjectWorkspace();
                }
                if (GUILayout.Button("No"))
                {
                    AbortFileActions();
                }
            }
            else
            {
                if (GUILayout.Button("Rename"))
                {
                    renamingFile = true;
                    newFileName = Path.GetFileNameWithoutExtension(currentFile.Path);
                }
                if (GUILayout.Button("Delete"))
                {
                    deletingFile = true;
                }
                if (currentFile == null)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("Save"))
                {
                    try
                    {
                        AbortFileActions();
                        SaveProjectFile(currentFile);
                    }
                    catch (Exception ex)
                    {
                        lastError = ex.Message;
                        return;
                    }

                    lastError = string.Empty;
                }

                GUI.enabled = true;

                if (GUILayout.Button("Save all"))
                {
                    AbortFileActions();
                    SaveAllProjectFiles();
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUI.enabled = true;

            if (GUILayout.Button(outputVisible ? "Hide output" : "Show output", GUILayout.ExpandWidth(false))) {
                AbortFileActions();
                outputVisible = !outputVisible;
                ResetAreaHeights();
            }

            if(outputVisible) {
                if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false))) {
                    AbortFileActions();
                    lastError = string.Empty;
                    output = string.Empty;
                }
            }

            GUILayout.EndHorizontal();

            footerArea.End();
        }

        private void DrawOutput()
        {
            outputArea.Begin();
            if (outputVisible) {
                outputScrollPosition = GUILayout.BeginScrollView(outputScrollPosition);

                GUI.SetNextControlName(OutputAreaControlName);

                GUILayout.TextArea(output != "" ? output : "Script output...", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                GUILayout.EndScrollView();
            }
            outputArea.End();
        }

        private void AbortFileActions()
        {
            renamingFile = false;
            deletingFile = false;
        }
    }
}