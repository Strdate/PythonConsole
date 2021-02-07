using UnityEngine;

namespace ModTools.UI
{
    internal static class GUIComboBox
    {
        private const string ExpandDownButtonText = " ▼ ";
        private static PopupWindow popupWindow;

        public static int Box(int itemIndex, string[] items, string callerId)
        {
            switch (items.Length)
            {
                case 0:
                    return -1;

                case 1:
                    GUILayout.Label(items[0]);
                    return 0;
            }

            if (popupWindow != null
                && callerId == popupWindow.OwnerId
                && popupWindow.CloseAndGetSelection(out var newSelectedIndex))
            {
                itemIndex = newSelectedIndex;
                Object.Destroy(popupWindow);
                popupWindow = null;
            }

            var popupSize = GetPopupDimensions(items);

            GUILayout.Box(items[itemIndex], GUILayout.Width(popupSize.x));
            var popupPosition = GUIUtility.GUIToScreenPoint(GUILayoutUtility.GetLastRect().position);
            if (GUILayout.Button(ExpandDownButtonText, GUILayout.Width(24f)) && EnsurePopupWindow())
            {
                popupWindow.Show(callerId, items, itemIndex, popupPosition, popupSize);
            }

            return itemIndex;
        }

        private static bool EnsurePopupWindow()
        {
            if (popupWindow != null)
            {
                return true;
            }

            return false;
            /*var modTools = Object.FindObjectOfType<MainWindow>();
            if (modTools == null)
            {
                return false;
            }

            if (modTools.GetComponent<PopupWindow>() == null)
            {
                popupWindow = modTools.gameObject.AddComponent<PopupWindow>();
            }

            return popupWindow != null;*/
        }

        private static Vector2 GetPopupDimensions(string[] items)
        {
            float width = 0;
            float height = 0;

            for (var i = 0; i < items.Length; ++i)
            {
                var itemSize = GUI.skin.button.CalcSize(new GUIContent(items[i]));
                if (itemSize.x > width)
                {
                    width = itemSize.x;
                }

                height += itemSize.y;
            }

            return new Vector2(width + 36, height + 36);
        }

        private sealed class PopupWindow : MonoBehaviour, IUIObject, IGameObject
        {
            private const float MaxPopupHeight = 400f;

            private static readonly GUIStyle WindowStyle = CreateWindowStyle();

            private readonly int popupWindowId = GUIUtility.GetControlID(FocusType.Passive);
            private readonly GUIStyle hoverStyle;

            private Vector2 popupScrollPosition = Vector2.zero;

            private Rect popupRect;
            private Vector2? mouseClickPoint;
            private bool readyToClose;
            private int selectedIndex;

            private string[] popupItems;

            public PopupWindow()
            {
                hoverStyle = CreateHoverStyle();
            }

            public string OwnerId { get; private set; }

            public void Show(string ownerId, string[] items, int currentIndex, Vector2 position, Vector2 popupSize)
            {
                OwnerId = ownerId;
                popupItems = items;
                selectedIndex = currentIndex;
                popupRect = new Rect(position, new Vector2(popupSize.x, Mathf.Min(MaxPopupHeight, popupSize.y)));
                popupScrollPosition = default;
                mouseClickPoint = null;
                readyToClose = false;
            }

            public bool CloseAndGetSelection(out int currentIndex)
            {
                if (readyToClose)
                {
                    currentIndex = selectedIndex;
                    Close();
                    return true;
                }

                currentIndex = -1;
                return false;
            }

            public void OnGUI()
            {
                if (OwnerId != null)
                {
                    GUI.ModalWindow(popupWindowId, popupRect, WindowFunction, string.Empty, WindowStyle);
                }
            }

            public void Update()
            {
                if (OwnerId == null)
                {
                    return;
                }

                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                {
                    var mousePos = Input.mousePosition;
                    mousePos.y = Screen.height - mousePos.y;
                    mouseClickPoint = mousePos;
                }
                else
                {
                    mouseClickPoint = null;
                }
            }

            private static GUIStyle CreateHoverStyle()
            {
                var result = new GUIStyle(GUI.skin.label);
                result.hover.textColor = Color.yellow;
                var t = new Texture2D(1, 1);
                t.SetPixel(0, 0, default);
                t.Apply();
                result.hover.background = t;
                result.font = GUI.skin.font;

                return result;
            }

            private static GUIStyle CreateWindowStyle()
            {
                var background = new Texture2D(16, 16, TextureFormat.RGBA32, mipmap: false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                };

                for (var x = 0; x < background.width; x++)
                {
                    for (var y = 0; y < background.height; y++)
                    {
                        if (x == 0 || x == background.width - 1 || y == 0 || y == background.height - 1)
                        {
                            background.SetPixel(x, y, new Color(0, 0, 0, 1));
                        }
                        else
                        {
                            background.SetPixel(x, y, new Color(0.05f, 0.05f, 0.05f, 0.95f));
                        }
                    }
                }

                background.Apply();

                var result = new GUIStyle(GUI.skin.window);
                result.normal.background = background;
                result.onNormal.background = background;
                result.border.top = result.border.bottom;
                result.padding.top = result.padding.bottom;

                return result;
            }

            private void WindowFunction(int windowId)
            {
                if (OwnerId == null)
                {
                    return;
                }

                popupScrollPosition = GUILayout.BeginScrollView(popupScrollPosition, false, false);

                var oldSelectedIndex = selectedIndex;
                selectedIndex = GUILayout.SelectionGrid(selectedIndex, popupItems, xCount: 1, hoverStyle);

                GUILayout.EndScrollView();

                if (oldSelectedIndex != selectedIndex || mouseClickPoint.HasValue && !popupRect.Contains(mouseClickPoint.Value))
                {
                    readyToClose = true;
                }
            }

            private void Close()
            {
                OwnerId = null;
                popupItems = null;
                selectedIndex = -1;
                mouseClickPoint = null;
            }
        }
    }
}