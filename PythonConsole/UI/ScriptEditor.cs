using System;
using System.Collections.Generic;
using System.IO;
using ModTools.UI;
using ModTools.Utils;
using PythonConsole;
using UnityEngine;

namespace ModTools.Scripting
{
    internal sealed class ScriptEditor : GUIWindow
    {
        private const string TextAreaControlName = "PythonScriptEditorTextArea";
        private const string OutputAreaControlName = "PythonScriptOutputTextArea";
        private const string ExampleScriptFileName = "ExampleScript.py";

        private const float HeaderHeight = 120.0f;
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

        private ScriptEditorFile currentFile;

        public ScriptEditor()
            : base("Script Editor", new Rect(16.0f, 16.0f, 640.0f, 480.0f))
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
            projectWorkspacePath = UnityPythonObject.Instance.Config.ScriptWorkspacePath;
            if (projectWorkspacePath.Length == 0)
            {
                lastError = "Invalid project workspace path";
                return;
            }

            var exampleFileExists = false;

            projectFiles.Clear();

            try
            {
                foreach (var file in FileUtil.ListFilesInDirectory(projectWorkspacePath))
                {
                    if (Path.GetExtension(file) == ".cs")
                    {
                        if (Path.GetFileName(file) == ExampleScriptFileName)
                        {
                            exampleFileExists = true;
                        }

                        projectFiles.Add(new ScriptEditorFile(File.ReadAllText(file), file));
                    }
                }

                if (!exampleFileExists)
                {
                    var exampleFile = new ScriptEditorFile(ScriptEditorFile.DefaultSource, Path.Combine(projectWorkspacePath, ExampleScriptFileName));
                    projectFiles.Add(exampleFile);
                    SaveProjectFile(exampleFile);
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
            GUILayout.Label("Scripts are stored in project workspace. To add a script create a new .py file in workspace and click 'Reload'", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

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
                if (GUILayout.Button(Path.GetFileName(file.Path), GUILayout.ExpandWidth(false)))
                {
                    currentFile = file;
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
                PythonConsole.PythonConsole.Instance.ScheduleExecution(currentFile.Source);
            }

            GUI.enabled = true;

            if (GUILayout.Button("Clear output"))
            {
                lastError = string.Empty;
                output = string.Empty;
            }

            GUILayout.Label(lastError != "" ? "Last error: " + lastError : "");

            GUILayout.FlexibleSpace();

            if (currentFile == null)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Save"))
            {
                try
                {
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
                SaveAllProjectFiles();
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
    }
}