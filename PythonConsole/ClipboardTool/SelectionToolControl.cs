using System;
using System.Collections.Generic;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace PythonConsole
{
    public class SelectionToolControl : MonoBehaviour
    {
        //private UIButton button;
        private UITiledSprite bar;
        private UIComponent fullscreenContainer;

        private bool _keyPressProcessed;

        public void Awake()
        {
            var toolController = FindObjectOfType<ToolManager>().m_properties;
            if (toolController == null) {
                return;
            }

            toolController.AddExtraToolToController<SelectionTool>();
            /*var textureButton = AtlasUtil.LoadTextureFromAssembly("ModTools.SelectionToolButton.png");
            textureButton.name = "SelectionToolButton";*/
            var textureBar = AtlasUtil.LoadTextureFromAssembly("PythonConsole.ClipboardTool.SelectionToolBar.png");
            textureBar.name = "SelectionToolBar";

            var escButton = (UIButton)UIView.Find("Esc");
            var atlas = AtlasUtil.CreateAtlas(new[]
                {
                    //textureButton,
                    textureBar,
                    GetTextureByName(escButton.disabledBgSprite, escButton.atlas),
                    GetTextureByName(escButton.hoveredBgSprite, escButton.atlas),
                    GetTextureByName(escButton.pressedBgSprite, escButton.atlas),
                    GetTextureByName(escButton.normalBgSprite, escButton.atlas),
                });
            /*var buttonGo = new GameObject("SelectionToolButton");
            button = buttonGo.AddComponent<UIButton>();
            button.tooltip = "Mod Tools - Selection Tool";
            button.normalFgSprite = "SelectionToolButton";
            button.hoveredFgSprite = "SelectionToolButton";
            button.pressedFgSprite = "SelectionToolButton";
            button.disabledFgSprite = "SelectionToolButton";
            button.focusedFgSprite = "SelectionToolButton";
            button.normalBgSprite = escButton.normalBgSprite;
            button.focusedBgSprite = escButton.normalBgSprite;
            button.hoveredBgSprite = escButton.hoveredBgSprite;
            button.pressedBgSprite = escButton.pressedBgSprite;
            button.disabledBgSprite = escButton.disabledBgSprite;
            button.playAudioEvents = true;
            button.width = 46f;
            button.height = 46f;
            UIView.GetAView().AttachUIComponent(buttonGo);
            button.absolutePosition = escButton.absolutePosition - new Vector3(95, 0, 0);
            button.atlas = atlas;
            button.eventClicked += (c, e) => ToggleTool();
            button.isVisible = MainWindow.Instance.Config.SelectionTool;*/

            /*var dragGo = new GameObject("SelectionToolDragHandler");
            dragGo.transform.parent = button.transform;
            dragGo.transform.localPosition = Vector3.zero;
            var drag = dragGo.AddComponent<UIDragHandle>();
            drag.tooltip = button.tooltip;
            drag.width = button.width;
            drag.height = button.height;*/

            var barGo = new GameObject("SelectionToolBar");
            bar = barGo.AddComponent<UITiledSprite>();
            UIView.GetAView().AttachUIComponent(barGo);
            bar.atlas = atlas;
            bar.width = UIView.GetAView().fixedWidth;
            var relativePosition = bar.relativePosition;
            relativePosition.x = 0;
            bar.relativePosition = relativePosition;
            bar.height = 28;
            bar.zOrder = 18;
            bar.spriteName = "SelectionToolBar";
            bar.Hide();

            fullscreenContainer = UIView.Find("FullScreenContainer");
        }

        public void Update()
        {
            var tool = ToolsModifierControl.GetTool<SelectionTool>();
            if (tool == null) {
                return;
            }

            if (!tool.enabled && bar.isVisible) {
                bar.Hide();
            }

            if (ModInfo.ClipboardToolShortcut.IsPressed()) {
                if(!_keyPressProcessed) {
                    _keyPressProcessed = true;
                    ToggleTool();
                }
            } else {
                _keyPressProcessed = false;
            }
        }

        private static Texture2D GetTextureByName(string name, UITextureAtlas atlas)
            => atlas.sprites.Find(sprite => sprite.name == name).texture;

        private void ToggleTool()
        {
            var tool = ToolsModifierControl.GetTool<SelectionTool>();
            if (tool == null) {
                return;
            }

            if (tool.enabled) {
                ValueAnimator.Animate(
                    "BulldozerBar",
                    val => {
                        var relativePosition = bar.relativePosition;
                        relativePosition.y = val;
                        bar.relativePosition = relativePosition;
                    },
                    new AnimatedFloat(
                        fullscreenContainer.relativePosition.y + fullscreenContainer.size.y - bar.size.y,
                        fullscreenContainer.relativePosition.y + fullscreenContainer.size.y,
                        0.3f),
                    () => bar.Hide());

                ToolsModifierControl.SetTool<DefaultTool>();
            } else {
                ToolsModifierControl.mainToolbar.CloseEverything();
                ToolsModifierControl.SetTool<SelectionTool>();
                bar.Show();
                ValueAnimator.Animate(
                    "BulldozerBar",
                    val => {
                        var relativePosition = bar.relativePosition;
                        relativePosition.y = val;
                        bar.relativePosition = relativePosition;
                    },
                    new AnimatedFloat(
                        fullscreenContainer.relativePosition.y + fullscreenContainer.size.y,
                        fullscreenContainer.relativePosition.y + fullscreenContainer.size.y - bar.size.y,
                        0.3f));
            }
        }
    }
}