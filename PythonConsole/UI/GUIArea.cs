using UnityEngine;

namespace ModTools.UI
{
    internal sealed class GUIArea
    {
        private readonly Vector2 margin = new Vector2(8f, 8f);
        private readonly GUIWindow window;

        private Vector2 absoluteOffset;
        private Vector2 relativeOffset;

        private Vector2 absoluteSize;
        private Vector2 relativeSize = Vector2.one;

        private bool drawingArea;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811", Justification = "Used by Unity components")]
        public GUIArea(GUIWindow window)
        {
            this.window = window;
        }

        public GUIArea OffsetBy(float? horizontal = null, float? vertical = null)
        {
            absoluteOffset = new Vector2(
                horizontal ?? absoluteOffset.x,
                vertical ?? absoluteOffset.y);
            return this;
        }

        public GUIArea OffsetRelative(float? horizontal = null, float? vertical = null)
        {
            relativeOffset = new Vector2(
                Mathf.Clamp(horizontal ?? relativeOffset.x, -1f, 1f),
                Mathf.Clamp(vertical ?? relativeOffset.y, -1f, 1f));
            return this;
        }

        public GUIArea ChangeSizeBy(float? width = null, float? height = null)
        {
            absoluteSize = new Vector2(
                width ?? absoluteSize.x,
                height ?? absoluteSize.y);
            return this;
        }

        public GUIArea ChangeSizeRelative(float? width = null, float? height = null)
        {
            relativeSize = new Vector2(
                Mathf.Clamp(width ?? relativeSize.x, -1f, 1f),
                Mathf.Clamp(height ?? relativeSize.y, -1f, 1f));
            return this;
        }

        public bool Begin()
        {
            var bounds = GetBounds();
            if (bounds == Rect.zero)
            {
                return false;
            }

            GUILayout.BeginArea(bounds);
            drawingArea = true;
            return true;
        }

        public void End()
        {
            if (drawingArea)
            {
                GUILayout.EndArea();
                drawingArea = false;
            }
        }

        private Rect GetBounds()
        {
            var windowRect = window.WindowRect;

            return new Rect(
                absoluteOffset.x + relativeOffset.x * windowRect.width + margin.x,
                absoluteOffset.y + relativeOffset.y * windowRect.height + margin.y,
                absoluteSize.x + relativeSize.x * windowRect.width - margin.x * 2,
                absoluteSize.y + relativeSize.y * windowRect.height - margin.y * 2);
        }
    }
}