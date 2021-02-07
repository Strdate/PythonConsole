using PythonConsole;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModTools.UI
{
    internal abstract class GUIWindow : MonoBehaviour, IDestroyableObject, IUIObject
    {
        private const float UIScale = 1.0f;

        private static readonly List<GUIWindow> Windows = new List<GUIWindow>();

        private static GUIWindow resizingWindow;
        private static Vector2 resizeDragHandle = Vector2.zero;

        private static GUIWindow movingWindow;
        private static Vector2 moveDragHandle = Vector2.zero;

        private readonly int id;
        private readonly bool resizable;
        private readonly bool hasTitlebar;

        private GUISkin skin;
        private string cachedFontName = string.Empty;
        private int cachedFontSize;

        private Vector2 minSize = Vector2.zero;
        private Rect windowRect = new Rect(0, 0, 64, 64);

        private bool visible;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811", Justification = ".ctor of a Unity component")]
        protected GUIWindow(string title, Rect rect, bool resizable = true, bool hasTitlebar = true)
        {
            id = UnityEngine.Random.Range(1024, int.MaxValue);
            Title = title;
            windowRect = rect;
            this.resizable = resizable;
            this.hasTitlebar = hasTitlebar;
            minSize = new Vector2(64.0f, 64.0f);
            Windows.Add(this);
        }

        public Rect WindowRect => windowRect;

        public bool Visible
        {
            get => visible;

            set
            {
                var wasVisible = visible;
                visible = value;
                if (visible && !wasVisible)
                {
                    GUI.BringWindowToFront(id);
                    OnWindowOpened();
                }
            }
        }

        protected static Texture2D BgTexture { get; set; }

        protected static Texture2D ResizeNormalTexture { get; set; }

        protected static Texture2D ResizeHoverTexture { get; set; }

        protected static Texture2D CloseNormalTexture { get; set; }

        protected static Texture2D CloseHoverTexture { get; set; }

        protected static Texture2D MoveNormalTexture { get; set; }

        protected static Texture2D MoveHoverTexture { get; set; }

        protected string Title { get; set; }

        private static ModConfiguration Config => UnityPythonObject.Instance.Config;

        public void UpdateFont()
        {
            if (cachedFontName == Config.FontName && cachedFontSize == Config.FontSize)
            {
                return;
            }

            skin.font = Font.CreateDynamicFontFromOSFont(Config.FontName, Config.FontSize);
            cachedFontName = Config.FontName;
            cachedFontSize = Config.FontSize;
        }

        public void OnDestroy()
        {
            OnWindowDestroyed();
            Windows.Remove(this);
        }

        public void OnGUI()
        {
            if (skin == null)
            {
                BgTexture = new Texture2D(1, 1);
                BgTexture.SetPixel(0, 0, Config.BackgroundColor);
                BgTexture.Apply();

                ResizeNormalTexture = new Texture2D(1, 1);
                ResizeNormalTexture.SetPixel(0, 0, Color.white);
                ResizeNormalTexture.Apply();

                ResizeHoverTexture = new Texture2D(1, 1);
                ResizeHoverTexture.SetPixel(0, 0, Color.blue);
                ResizeHoverTexture.Apply();

                CloseNormalTexture = new Texture2D(1, 1);
                CloseNormalTexture.SetPixel(0, 0, Color.red);
                CloseNormalTexture.Apply();

                CloseHoverTexture = new Texture2D(1, 1);
                CloseHoverTexture.SetPixel(0, 0, Color.white);
                CloseHoverTexture.Apply();

                MoveNormalTexture = new Texture2D(1, 1);
                MoveNormalTexture.SetPixel(0, 0, Config.TitleBarColor);
                MoveNormalTexture.Apply();

                MoveHoverTexture = new Texture2D(1, 1);
                MoveHoverTexture.SetPixel(0, 0, Config.TitleBarColor * 1.2f);
                MoveHoverTexture.Apply();

                skin = ScriptableObject.CreateInstance<GUISkin>();
                skin.box = new GUIStyle(GUI.skin.box);
                skin.button = new GUIStyle(GUI.skin.button);
                skin.horizontalScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
                skin.horizontalScrollbarLeftButton = new GUIStyle(GUI.skin.horizontalScrollbarLeftButton);
                skin.horizontalScrollbarRightButton = new GUIStyle(GUI.skin.horizontalScrollbarRightButton);
                skin.horizontalScrollbarThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
                skin.horizontalSlider = new GUIStyle(GUI.skin.horizontalSlider);
                skin.horizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
                skin.label = new GUIStyle(GUI.skin.label);
                skin.scrollView = new GUIStyle(GUI.skin.scrollView);
                skin.textArea = new GUIStyle(GUI.skin.textArea);
                skin.textField = new GUIStyle(GUI.skin.textField);
                skin.toggle = new GUIStyle(GUI.skin.toggle);
                skin.verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
                skin.verticalScrollbarDownButton = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
                skin.verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
                skin.verticalScrollbarUpButton = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
                skin.verticalSlider = new GUIStyle(GUI.skin.verticalSlider);
                skin.verticalSliderThumb = new GUIStyle(GUI.skin.verticalSliderThumb);
                skin.window = new GUIStyle(GUI.skin.window);
                skin.window.normal.background = BgTexture;
                skin.window.onNormal.background = BgTexture;

                skin.settings.cursorColor = GUI.skin.settings.cursorColor;
                skin.settings.cursorFlashSpeed = GUI.skin.settings.cursorFlashSpeed;
                skin.settings.doubleClickSelectsWord = GUI.skin.settings.doubleClickSelectsWord;
                skin.settings.selectionColor = GUI.skin.settings.selectionColor;
                skin.settings.tripleClickSelectsLine = GUI.skin.settings.tripleClickSelectsLine;
            }

            if (!Visible)
            {
                return;
            }

            var oldSkin = GUI.skin;
            if (skin != null)
            {
                UpdateFont();
                GUI.skin = skin;
            }

            var matrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(UIScale, UIScale, UIScale));

            windowRect = GUI.Window(id, windowRect, WindowFunction, string.Empty);

            OnWindowDrawn();

            GUI.matrix = matrix;

            GUI.skin = oldSkin;
        }

        public void MoveResize(Rect newWindowRect) => windowRect = newWindowRect;

        protected static bool IsMouseOverWindow()
        {
            var mouse = Input.mousePosition;
            mouse.y = Screen.height - mouse.y;
            return Windows.FindIndex(window => window.Visible && window.windowRect.Contains(mouse)) >= 0;
        }

        protected abstract void DrawWindow();

        protected virtual void HandleException(Exception ex)
        {
        }

        protected virtual void OnWindowDrawn()
        {
        }

        protected virtual void OnWindowOpened()
        {
        }

        protected virtual void OnWindowClosed()
        {
        }

        protected virtual void OnWindowResized(Vector2 size)
        {
        }

        protected virtual void OnWindowMoved(Vector2 position)
        {
        }

        protected virtual void OnWindowDestroyed()
        {
        }

        private void WindowFunction(int windowId)
        {
            GUILayout.Space(8.0f);

            try
            {
                DrawWindow();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            GUILayout.Space(16.0f);

            var mouse = Input.mousePosition;
            mouse.y = Screen.height - mouse.y;

            DrawBorder();

            if (hasTitlebar)
            {
                DrawTitlebar(mouse);
                DrawCloseButton(mouse);
            }

            if (resizable)
            {
                DrawResizeHandle(mouse);
            }
        }

        private void DrawBorder()
        {
            var leftRect = new Rect(0.0f, 0.0f, 1.0f, windowRect.height);
            var rightRect = new Rect(windowRect.width - 1.0f, 0.0f, 1.0f, windowRect.height);
            var bottomRect = new Rect(0.0f, windowRect.height - 1.0f, windowRect.width, 1.0f);
            GUI.DrawTexture(leftRect, MoveNormalTexture);
            GUI.DrawTexture(rightRect, MoveNormalTexture);
            GUI.DrawTexture(bottomRect, MoveNormalTexture);
        }

        private void DrawTitlebar(Vector3 mouse)
        {
            var moveRect = new Rect(windowRect.x * UIScale, windowRect.y * UIScale, windowRect.width * UIScale, 20.0f);
            var moveTex = MoveNormalTexture;

            // TODO: reduce nesting
            if (!GUIUtility.hasModalWindow)
            {
                if (movingWindow != null)
                {
                    if (movingWindow == this)
                    {
                        moveTex = MoveHoverTexture;

                        if (Input.GetMouseButton(0))
                        {
                            var pos = new Vector2(mouse.x, mouse.y) + moveDragHandle;
                            windowRect.x = pos.x;
                            windowRect.y = pos.y;
                            if (windowRect.x < 0.0f)
                            {
                                windowRect.x = 0.0f;
                            }

                            if (windowRect.x + windowRect.width > Screen.width)
                            {
                                windowRect.x = Screen.width - windowRect.width;
                            }

                            if (windowRect.y < 0.0f)
                            {
                                windowRect.y = 0.0f;
                            }

                            if (windowRect.y + windowRect.height > Screen.height)
                            {
                                windowRect.y = Screen.height - windowRect.height;
                            }
                        }
                        else
                        {
                            movingWindow = null;
                            UnityPythonObject.Instance.SaveConfig();

                            OnWindowMoved(windowRect.position);
                        }
                    }
                }
                else if (moveRect.Contains(mouse))
                {
                    moveTex = MoveHoverTexture;
                    if (Input.GetMouseButton(0) && resizingWindow == null)
                    {
                        movingWindow = this;
                        moveDragHandle = new Vector2(windowRect.x, windowRect.y) - new Vector2(mouse.x, mouse.y);
                    }
                }
            }

            GUI.DrawTexture(new Rect(0.0f, 0.0f, windowRect.width * UIScale, 20.0f), moveTex, ScaleMode.StretchToFill);
            GUI.contentColor = Config.TitleBarTextColor;
            GUI.Label(new Rect(8.0f, 0.0f, windowRect.width * UIScale, 20.0f), Title);
            GUI.contentColor = Color.white;
        }

        private void DrawCloseButton(Vector3 mouse)
        {
            var closeRect = new Rect(windowRect.x * UIScale + windowRect.width * UIScale - 20.0f, windowRect.y * UIScale, 16.0f, 8.0f);
            var closeTex = CloseNormalTexture;

            if (!GUIUtility.hasModalWindow && closeRect.Contains(mouse))
            {
                closeTex = CloseHoverTexture;

                if (Input.GetMouseButton(0))
                {
                    resizingWindow = null;
                    movingWindow = null;
                    Visible = false;
                    OnWindowClosed();
                }
            }

            GUI.DrawTexture(new Rect(windowRect.width - 20.0f, 0.0f, 16.0f, 8.0f), closeTex, ScaleMode.StretchToFill);
        }

        private void DrawResizeHandle(Vector3 mouse)
        {
            var resizeRect = new Rect(windowRect.x * UIScale + windowRect.width * UIScale - 16.0f, windowRect.y * UIScale + windowRect.height * UIScale - 8.0f, 16.0f, 8.0f);
            var resizeTex = ResizeNormalTexture;

            // TODO: reduce nesting
            if (!GUIUtility.hasModalWindow)
            {
                if (resizingWindow != null)
                {
                    if (resizingWindow == this)
                    {
                        resizeTex = ResizeHoverTexture;

                        if (Input.GetMouseButton(0))
                        {
                            var size = new Vector2(mouse.x, mouse.y) + resizeDragHandle - new Vector2(windowRect.x, windowRect.y);

                            if (size.x < minSize.x)
                            {
                                size.x = minSize.x;
                            }

                            if (size.y < minSize.y)
                            {
                                size.y = minSize.y;
                            }

                            windowRect.width = size.x;
                            windowRect.height = size.y;

                            if (windowRect.x + windowRect.width >= Screen.width)
                            {
                                windowRect.width = Screen.width - windowRect.x;
                            }

                            if (windowRect.y + windowRect.height >= Screen.height)
                            {
                                windowRect.height = Screen.height - windowRect.y;
                            }
                        }
                        else
                        {
                            resizingWindow = null;
                            UnityPythonObject.Instance.SaveConfig();
                            OnWindowResized(windowRect.size);
                        }
                    }
                }
                else if (resizeRect.Contains(mouse))
                {
                    resizeTex = ResizeHoverTexture;
                    if (Input.GetMouseButton(0))
                    {
                        resizingWindow = this;
                        resizeDragHandle = new Vector2(windowRect.x + windowRect.width, windowRect.y + windowRect.height) - new Vector2(mouse.x, mouse.y);
                    }
                }
            }

            GUI.DrawTexture(new Rect(windowRect.width - 16.0f, windowRect.height - 8.0f, 16.0f, 8.0f), resizeTex, ScaleMode.StretchToFill);
        }
    }
}