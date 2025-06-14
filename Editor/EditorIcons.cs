#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityEssentials
{
    public partial class EditorIcons
    {
        private static bool _viewBigIcons = false;
        private static bool _darkPreview = true;
        private int _buttonSize = 40;
        private string _search = "";

        private static GUIContent _iconSelected;
        private static List<GUIContent> _iconContentListAll;
        private static List<GUIContent> _iconContentListSmall;
        private static List<GUIContent> _iconContentListBig;
        private static GUIStyle _iconButtonStyle;
        private static GUIStyle _iconPreviewBlack;
        private static GUIStyle _iconPreviewWhite;

        private List<GUIContent> GetFilteredIconList()
        {
            if (string.IsNullOrWhiteSpace(_search))
                return _viewBigIcons ? _iconContentListBig : _iconContentListSmall;

            return _iconContentListAll
                .Where(icon => icon.tooltip
                    .ToLower()
                    .Contains(_search.ToLower()))
                .ToList();
        }

        private void InitializeIcons()
        {
            if (_iconContentListSmall != null)
                return;

            _iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                margin = new RectOffset(0, 0, 0, 0),
                fixedHeight = 0
            };

            _iconPreviewBlack = CreatePreviewStyle(new Color(0.26f, 0.26f, 0.26f));
            _iconPreviewWhite = CreatePreviewStyle(new Color(0.85f, 0.85f, 0.85f));

            _iconContentListSmall = new List<GUIContent>();
            _iconContentListBig = new List<GUIContent>();
            _iconContentListAll = new List<GUIContent>();

            foreach (string iconName in Icon.References)
            {
                var icon = GetIcon(iconName);
                if (icon == null) 
                    continue;

                icon.tooltip = iconName;
                _iconContentListAll.Add(icon);

                if (icon.image.width > 36 && icon.image.height > 36)
                    _iconContentListBig.Add(icon);
                else
                    _iconContentListSmall.Add(icon);
            }
        }

        private GUIStyle CreatePreviewStyle(Color bgColor)
        {
            var style = new GUIStyle(_iconButtonStyle);
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, bgColor);
            tex.Apply();

            style.normal =
            style.hover =
            style.active =
            style.focused = new GUIStyleState { background = tex };

            return style;
        }

        private GUIContent GetIcon(string iconName)
        {
            Debug.unityLogger.logEnabled = false;
            GUIContent icon = EditorGUIUtility.IconContent(iconName);
            Debug.unityLogger.logEnabled = true;
            return icon?.image == null ? null : icon;
        }

        private void SaveIcon(string iconName)
        {
            Texture2D texture = EditorGUIUtility.IconContent(iconName).image as Texture2D;
            if (texture == null)
            {
                Debug.LogError("Cannot save icon: null texture!");
                return;
            }

            string path = EditorUtility.SaveFilePanel("Save icon", "", iconName, "png");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                RenderTexture rt = RenderTexture.GetTemporary(
                    texture.width, texture.height,
                    0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

                Graphics.Blit(texture, rt);
                RenderTexture.active = rt;

                Texture2D outTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
                outTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                outTexture.Apply();

                File.WriteAllBytes(path, outTexture.EncodeToPNG());

                RenderTexture.ReleaseTemporary(rt);
                RenderTexture.active = null;
                Editor.DestroyImmediate(outTexture);
            }
            catch (Exception e)
            {
                Debug.LogError("Cannot save icon: " + e.Message);
            }
        }

        private void SaveAllIcons()
        {
            string folderPath = EditorUtility.SaveFolderPanel("Save All Icons", "", "");
            if (string.IsNullOrEmpty(folderPath)) return;

            try
            {
                foreach (string iconName in Icon.References)
                {
                    string fileName = iconName.Split('/').Last() + ".png";
                    string fullPath = Path.Combine(folderPath, fileName);

                    if (File.Exists(fullPath)) continue;

                    Texture2D texture = EditorGUIUtility.IconContent(iconName).image as Texture2D;
                    if (texture == null) continue;

                    // Same saving logic as in SaveIcon but without dialog
                    RenderTexture rt = RenderTexture.GetTemporary(
                        texture.width, texture.height,
                        0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

                    Graphics.Blit(texture, rt);
                    RenderTexture.active = rt;

                    Texture2D outTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
                    outTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                    outTexture.Apply();

                    File.WriteAllBytes(fullPath, outTexture.EncodeToPNG());

                    RenderTexture.ReleaseTemporary(rt);
                    RenderTexture.active = null;
                    Editor.DestroyImmediate(outTexture);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Cannot save icons: " + e.Message);
            }
        }
    }
}
#endif