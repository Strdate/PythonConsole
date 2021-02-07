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
        private const float FooterHeight = 60.0f;
        private const float OutputHeight = 180.0f;

        private readonly List<ScriptEditorFile> projectFiles = new List<ScriptEditorFile>();

        private readonly GUIArea headerArea;
        private readonly GUIArea editorArea;
        private readonly GUIArea footerArea;
        private readonly GUIArea outputArea;

        //private IModEntryPoint currentMod;
        private string lastError = string.Empty;
        private string output = string.Empty;

        private Vector2 editorScrollPosition = Vector2.zero;
        private Vector2 outputScrollPosition = Vector2.zero;
        private Vector2 projectFilesScrollPosition = Vector2.zero;

        private string projectWorkspacePath = string.Empty;

        private bool renamingFile;
        private bool deletingFile;
        private string newFileName;

        private ScriptEditorFile currentFile;

        public ScriptEditor()
            : base("Python Console", new Rect(16.0f, 16.0f, 640.0f, 480.0f))
        {
            headerArea = new GUIArea(this)
                .OffsetBy(vertical: 32f)
                .ChangeSizeRelative(height: 0)
                .ChangeSizeBy(height: HeaderHeight);

            editorArea = new GUIArea(this)
                .OffsetBy(vertical: 32.0f + HeaderHeight)
                .ChangeSizeBy(height: -(32.0f + HeaderHeight + FooterHeight + OutputHeight));

            footerArea = new GUIArea(this)
                .OffsetRelative(vertical: 1f)
                .OffsetBy(vertical: -FooterHeight-OutputHeight)
                .ChangeSizeRelative(height: 0)
                .ChangeSizeBy(height: FooterHeight);

            outputArea = new GUIArea(this)
                .OffsetRelative(vertical: 1f)
                .OffsetBy(vertical: -OutputHeight)
                .ChangeSizeRelative(height: 0)
                .ChangeSizeBy(height: OutputHeight);
        }

        public void ReloadProjectWorkspace()
        {
            AbortActions();
            string configPath = UnityPythonObject.Instance.Config.ScriptWorkspacePath;
            projectWorkspacePath = configPath == null || configPath == "" ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"SkylinesPython") : configPath;
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
                            if (currentFile == null)
                            {
                                currentFile = fileContent;
                            }
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
                    AbortActions();
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

            if (GUIUtility.keyboardControl == editor.controlID && Event.current.Equals(Event.KeyboardEvent("tab")))
            {
                if (text.Length > editor.cursorIndex)
                {
                    text = text.Insert(editor.cursorIndex, "\t");
                    editor.cursorIndex++;
                    editor.selectIndex = editor.cursorIndex;
                }

                Event.current.Use();
            }

            if (currentFile != null)
            {
                currentFile.Source = text;
            }

            GUILayout.EndScrollView();

            editorArea.End();
        }

        private void DrawFooter()
        {
            footerArea.Begin();

            GUILayout.BeginHorizontal();

            /*if (currentMod != null)
            {
                GUI.enabled = false;
            }*/

            if (GUILayout.Button("Execute"))
            {
                AbortActions();
                PythonConsole.Instance.ScheduleExecution(currentFile.Source);
                lastError = string.Empty;
            }

            GUI.enabled = true;

            if (GUILayout.Button("Clear output"))
            {
                AbortActions();
                lastError = string.Empty;
                output = string.Empty;
            }

            GUILayout.Label(lastError != "" ? "Last error: " + lastError : "");

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
                        AbortActions();
                    }
                    if (GUILayout.Button("Cancel"))
                    {
                        AbortActions();
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
                    AbortActions();
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
                        AbortActions();
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
                    AbortActions();
                    SaveAllProjectFiles();
                }
            }

            GUILayout.EndHorizontal();

            footerArea.End();
        }

        private void DrawOutput()
        {
            outputArea.Begin();

            outputScrollPosition = GUILayout.BeginScrollView(outputScrollPosition);

            GUI.SetNextControlName(OutputAreaControlName);

            GUILayout.TextArea(output != "" ? output : "Script output...", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.EndScrollView();

            outputArea.End();
        }

        private void AbortActions()
        {
            renamingFile = false;
            deletingFile = false;
        }
    }
}