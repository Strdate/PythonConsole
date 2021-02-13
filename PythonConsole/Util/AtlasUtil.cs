using System;
using System.IO;
using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;

namespace PythonConsole
{
    internal static class AtlasUtil
    {
        public static Texture2D LoadTextureFromAssembly(string path)
        {
            try {
                var assembly = Assembly.GetExecutingAssembly();
                using (var textureStream = assembly.GetManifestResourceStream(path)) {
                    return LoadTextureFromStream(textureStream);
                };
            }
            catch (Exception e) {
                Debug.LogException(e);
                return null;
            }
        }

        public static UITextureAtlas CreateAtlas(Texture2D[] sprites)
        {
            var atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            atlas.material = new Material(GetUIAtlasShader());

            var texture = new Texture2D(0, 0);
            var rects = texture.PackTextures(sprites, 0);

            for (var i = 0; i < rects.Length; ++i) {
                var sprite = sprites[i];
                var rect = rects[i];

                var spriteInfo = new UITextureAtlas.SpriteInfo {
                    name = sprite.name,
                    texture = sprite,
                    region = rect,
                    border = new RectOffset(),
                };

                atlas.AddSprite(spriteInfo);
            }

            atlas.material.mainTexture = texture;
            return atlas;
        }

        private static Shader GetUIAtlasShader() => UIView.GetAView().defaultAtlas.material.shader;

        private static Texture2D LoadTextureFromStream(Stream textureStream)
        {
            var buf = new byte[textureStream.Length];
            textureStream.Read(buf, 0, buf.Length);
            textureStream.Close();
            var tex = new Texture2D(36, 36, TextureFormat.ARGB32, true) { filterMode = FilterMode.Trilinear };
            tex.LoadImage(buf);
            return tex;
        }
    }
}