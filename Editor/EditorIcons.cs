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
        private string _iconNameFilter = string.Empty;

        private static bool s_viewBigIcons = true;

        private static GUIContent s_iconSelected;
        private static List<GUIContent> s_iconContentListAll;
        private static List<GUIContent> s_iconContentListSmall;
        private static List<GUIContent> s_iconContentListBig;
        private static GUIStyle s_iconButtonStyle;

        private List<GUIContent> GetFilteredIconList()
        {
            if (string.IsNullOrWhiteSpace(_iconNameFilter))
                return s_viewBigIcons ? s_iconContentListBig : s_iconContentListSmall;

            return s_iconContentListAll
                .Where(icon => icon.tooltip
                    .ToLower()
                    .Contains(_iconNameFilter.ToLower()))
                .ToList();
        }

        private void InitializeIcons()
        {
            if (s_iconContentListSmall != null)
                return;

            s_iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                margin = new RectOffset(0, 0, 0, 0),
                fixedHeight = 0
            };

            s_iconContentListSmall = new List<GUIContent>();
            s_iconContentListBig = new List<GUIContent>();
            s_iconContentListAll = new List<GUIContent>();

            foreach (string iconName in EditorIcons.References)
            {
                var icon = GetIcon(iconName);
                if (icon == null)
                    continue;

                icon.tooltip = iconName;
                s_iconContentListAll.Add(icon);

                if (icon.image.width > 36 && icon.image.height > 36)
                    s_iconContentListBig.Add(icon);
                else s_iconContentListSmall.Add(icon);
            }

            s_iconSelected = null;
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
            var texture = EditorGUIUtility.IconContent(iconName).image as Texture2D;
            if (texture == null)
            {
                Debug.LogError("Cannot save icon: null texture!");
                return;
            }

            string path = EditorUtility.SaveFilePanel("Save icon", "", iconName, "png");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                var renderTexture = RenderTexture.GetTemporary(
                    texture.width, texture.height,
                    0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

                Graphics.Blit(texture, renderTexture);
                RenderTexture.active = renderTexture;

                var outTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
                outTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                outTexture.Apply();

                File.WriteAllBytes(path, outTexture.EncodeToPNG());

                RenderTexture.ReleaseTemporary(renderTexture);
                RenderTexture.active = null;
                Editor.DestroyImmediate(outTexture);
            }
            catch (Exception ex) { Debug.LogError("Cannot save icon: " + ex.Message); }
        }

        private void SaveAllIcons()
        {
            string folderPath = EditorUtility.SaveFolderPanel("Save All Icons", "", "");
            if (string.IsNullOrEmpty(folderPath)) return;

            try
            {
                foreach (string iconName in EditorIcons.References)
                {
                    string fileName = iconName.Split('/').Last() + ".png";
                    string fullPath = Path.Combine(folderPath, fileName);

                    if (File.Exists(fullPath))
                        continue;

                    var texture = EditorGUIUtility.IconContent(iconName).image as Texture2D;
                    if (texture == null)
                        continue;

                    // Same saving logic as in SaveIcon but without dialog
                    var renderTexture = RenderTexture.GetTemporary(
                        texture.width, texture.height,
                        0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

                    Graphics.Blit(texture, renderTexture);
                    RenderTexture.active = renderTexture;

                    var outTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
                    outTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                    outTexture.Apply();

                    File.WriteAllBytes(fullPath, outTexture.EncodeToPNG());

                    RenderTexture.ReleaseTemporary(renderTexture);
                    RenderTexture.active = null;
                    Editor.DestroyImmediate(outTexture);
                }
            }
            catch (Exception ex) { Debug.LogError("Cannot save icons: " + ex.Message); }
        }
    }
}
#endif