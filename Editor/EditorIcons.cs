#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityEssentials
{
    /// <summary>
    /// Provides a Unity Editor window for browsing, searching, and managing built-in Unity editor icons.
    /// </summary>
    /// <remarks>The <see cref="EditorIcons"/> class allows developers to view and interact with Unity's
    /// built-in editor icons. It provides functionality to search for icons, preview them, and save them to files for
    /// external use.  The window can be opened via the Unity Editor menu under "Tools/Editor Icons".</remarks>
    public class EditorIcons : EditorWindow
    {
        [MenuItem("Tools/Editor Icons %e", priority = 1001)]
        public static void EditorIconsOpen()
        {
#if UNITY_2018
            var window = GetWindow<EditorIcons>("Editor Icons");
#else
            var window = CreateWindow<EditorIcons>("Editor Icons");
#endif
            window.ShowUtility();
            window.minSize = new(320, 450);
        }

        private static bool _viewBigIcons = true;
        private static bool _darkPreview = true;

        private Vector2 _scroll;

        private int _buttonSize = 70;
        private string _search = "";

        private bool _isWide => Screen.width > 550;
        private bool _doSearch => !string.IsNullOrWhiteSpace(_search) && _search != "";

        public void OnEnable()
        {
            var all_icons = Icon.References.Where(x => GetIcon(x) != null);
            List<string> unique = new();

            foreach (Texture2D x in Resources.FindObjectsOfTypeAll<Texture2D>())
            {
                GUIContent icoContent = GetIcon(x.name);
                if (icoContent == null) continue;

                if (!all_icons.Contains(x.name))
                    unique.Add(x.name);
            }

            //var icons = Icon.References.ToList().Concat(unique).ToArray();

            // Static list icons count : 1315 ( unique = 749 )
            // Found icons in resources : 1416 ( unique = 855 )

            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        public void OnGUI()
        {
            var pixelPerPoint = EditorGUIUtility.pixelsPerPoint;

            InitializeIcons();

            if (!_isWide)
                SearchGUI();

            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Save all icons to folder...", EditorStyles.miniButton)) SaveAllIcons();
                GUILayout.Label("Select what icons to show", GUILayout.Width(160));
                _viewBigIcons = GUILayout.SelectionGrid(
                  _viewBigIcons ? 1 : 0, new string[] { "Small", "Big" },
                  2, EditorStyles.toolbarButton) == 1;

                if (_isWide)
                    SearchGUI();
            }

            if (_isWide)
                GUILayout.Space(3);

            using (GUILayout.ScrollViewScope scope = new(_scroll))
            {
                GUILayout.Space(10);

                _scroll = scope.scrollPosition;

                _buttonSize = _viewBigIcons ? 70 : 40;

                // scrollbar_width = ~ 12.5
                var renderWidth = (Screen.width / pixelPerPoint - 13f);
                var gridWidth = Mathf.FloorToInt(renderWidth / _buttonSize);
                var marginLeft = (renderWidth - _buttonSize * gridWidth) / 2;

                int row = 0, index = 0;

                List<GUIContent> iconList;

                if (_doSearch) 
                    iconList = _iconContentListAll
                        .Where(icon => icon.tooltip
                            .ToLower()
                            .Contains(_search.ToLower()))
                        .ToList();
                else iconList = _viewBigIcons ? _iconContentListBig : _iconContentListSmall;

                while (index < iconList.Count)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(marginLeft);

                        for (var i = 0; i < gridWidth; ++i)
                        {
                            int k = i + row * gridWidth;

                            var icon = iconList[k];

                            if (GUILayout.Button(icon, _iconButtonStyle, GUILayout.Width(_buttonSize), GUILayout.Height(_buttonSize)))
                            {
                                EditorGUI.FocusTextInControl("");
                                _iconSelected = icon;
                            }

                            index++;

                            if (index == iconList.Count) break;
                        }
                    }

                    row++;
                }

                GUILayout.Space(10);
            }

            if (_iconSelected == null)
                return;

            GUILayout.FlexibleSpace();

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.MaxHeight(150)))
            {
                using (new GUILayout.VerticalScope(GUILayout.Width(130)))
                {
                    GUILayout.Space(2);

                    GUILayout.Button(_iconSelected,
                        _darkPreview ? _iconPreviewBlack : _iconPreviewWhite,
                        GUILayout.Width(128), GUILayout.Height(_viewBigIcons ? 88 : 40));

                    GUILayout.Space(5);

                    _darkPreview = GUILayout.SelectionGrid(
                      _darkPreview ? 1 : 0, new string[] { "Light", "Dark" },
                      2, EditorStyles.miniButton) == 1;

                    GUILayout.FlexibleSpace();
                }

                GUILayout.Space(10);

                using (new GUILayout.VerticalScope())
                {
                    var size = $"Size: {_iconSelected.image.width}x{_iconSelected.image.height}";
                    size += "\nIs Pro Skin Icon: " + (_iconSelected.tooltip.IndexOf("d_") == 0 ? "Yes" : "No");
                    size += $"\nTotal {_iconContentListAll.Count} icons";

                    GUILayout.Space(5);

                    EditorGUILayout.HelpBox(size, MessageType.None);

                    GUILayout.Space(5);

                    EditorGUILayout.TextField("EditorGUIUtility.IconContent(\"" + _iconSelected.tooltip + "\")");

                    GUILayout.Space(5);

                    if (GUILayout.Button("Copy to clipboard", EditorStyles.miniButton))
                        EditorGUIUtility.systemCopyBuffer = _iconSelected.tooltip;
                    if (GUILayout.Button("Save icon to file ...", EditorStyles.miniButton))
                        SaveIcon(_iconSelected.tooltip);
                }

                GUILayout.Space(10);

                if (GUILayout.Button("X", GUILayout.MinHeight(20), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false)))
                    _iconSelected = null;
            }
        }

        public void SearchGUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                if (_isWide)
                    GUILayout.Space(10);

#if UNITY_2018
                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarTextField);
#else
                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
#endif
                if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_mac_close_h"), EditorStyles.toolbarButton, GUILayout.Width(22)))
                    _search = "";
            }
        }

        private GUIContent GetIcon(string icon_name)
        {
            GUIContent valid = null;
            Debug.unityLogger.logEnabled = false;

            if (!string.IsNullOrEmpty(icon_name))
                valid = EditorGUIUtility.IconContent(icon_name);

            Debug.unityLogger.logEnabled = true;

            return valid?.image == null ? null : valid;
        }

        private void SaveIcon(string icon_name)
        {
            Texture2D texture = EditorGUIUtility.IconContent(icon_name).image as Texture2D;

            if (texture != null)
            {
                string path = EditorUtility.SaveFilePanel(
                    "Save icon", "", icon_name, "png");

                if (path != null)
                {
                    try
                    {
#if UNITY_2018
                        Texture2D outTexture = new(
                            texture.width, texture.height,
                            texture.format, true);
#else
                        Texture2D outTexture = new(
                            texture.width, texture.height,
                            texture.format, texture.mipmapCount, true);
#endif

                        Graphics.CopyTexture(texture, outTexture);

                        File.WriteAllBytes(path, outTexture.EncodeToPNG());
                    }
                    catch (Exception e) { Debug.LogError("Cannot save the icon : " + e.Message); }
                }
            }
            else { Debug.LogError("Cannot save the icon : null texture error!"); }
        }

        private void SaveAllIcons()
        {
            var folderpath = EditorUtility.SaveFolderPanel("", "", "");
            
            try
            {
                foreach (string icon in Icon.References)
                {
                    var split = icon.Split('/').Last();
                    Texture2D texture = EditorGUIUtility.IconContent(icon).image as Texture2D;

                    if (texture == null)
                        continue;

                    if (string.IsNullOrWhiteSpace(folderpath))
                    {
                        Debug.LogError("Folder path invalid...");
                        break;
                    }

                    var path = Path.Combine(folderpath, $"{split}.png");

                    if (File.Exists(path))
                    {
                        Debug.Log($"File already exists, skipping: {path}");
                        continue;
                    }

                    // Create a temporary RenderTexture
                    RenderTexture rt = RenderTexture.GetTemporary(
                        texture.width,
                        texture.height,
                        0,
                        RenderTextureFormat.ARGB32,
                        RenderTextureReadWrite.sRGB);

                    // Blit the texture to RenderTexture
                    Graphics.Blit(texture, rt);

                    // Create a readable Texture2D
                    Texture2D outTexture = new Texture2D(
                        texture.width,
                        texture.height,
                        TextureFormat.ARGB32,
                        false);

                    // Read the RenderTexture into Texture2D
                    RenderTexture.active = rt;
                    outTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                    outTexture.Apply();
                    RenderTexture.active = null;

                    // Clean up
                    RenderTexture.ReleaseTemporary(rt);

                    // Save to file
                    File.WriteAllBytes(path, outTexture.EncodeToPNG());
                    DestroyImmediate(outTexture);
                }
            }
            catch (Exception e) { Debug.LogError("Cannot save the icons: " + e.Message); }
        }

        private static GUIContent _iconSelected;
        private static List<GUIContent> _iconContentListAll;
        private static List<GUIContent> _iconContentListSmall;
        private static List<GUIContent> _iconContentListBig;
        private static List<string> _iconMissingNames;
        private static GUIStyle _iconButtonStyle = null;
        private static GUIStyle _iconPreviewBlack = null;
        private static GUIStyle _iconPreviewWhite = null;

        void AllTextures(ref GUIStyle style, Texture2D texture)
        {
            style.hover.background = style.onHover.background = style.focused.background = style.onFocused.background = style.active.background = style.onActive.background = style.normal.background = style.onNormal.background = texture;
            style.hover.scaledBackgrounds = style.onHover.scaledBackgrounds = style.focused.scaledBackgrounds = style.onFocused.scaledBackgrounds = style.active.scaledBackgrounds = style.onActive.scaledBackgrounds = style.normal.scaledBackgrounds = style.onNormal.scaledBackgrounds = new Texture2D[] { texture };
        }

        Texture2D Texture2DPixel(Color color)
        {
            Texture2D texture = new(1, 1);

            texture.SetPixel(0, 0, color);
            texture.Apply();

            return texture;
        }

        void InitializeIcons()
        {
            if (_iconContentListSmall != null) 
                return;

            _iconButtonStyle = new(EditorStyles.miniButton);
            _iconButtonStyle.margin = new(0, 0, 0, 0);
            _iconButtonStyle.fixedHeight = 0;

            _iconPreviewBlack = new(_iconButtonStyle);
            AllTextures(ref _iconPreviewBlack, Texture2DPixel(new Color(0.15f, 0.15f, 0.15f)));

            _iconPreviewWhite = new(_iconButtonStyle);
            AllTextures(ref _iconPreviewWhite, Texture2DPixel(new Color(0.85f, 0.85f, 0.85f)));

            _iconMissingNames = new();
            _iconContentListSmall = new();
            _iconContentListBig = new();
            _iconContentListAll = new();

            for (var i = 0; i < Icon.References.Length; ++i)
            {
                GUIContent icon = GetIcon(Icon.References[i]);

                if (icon == null)
                {
                    _iconMissingNames.Add(Icon.References[i]);

                    continue;
                }

                icon.tooltip = Icon.References[i];

                _iconContentListAll.Add(icon);

                if (!(icon.image.width <= 36 || icon.image.height <= 36))
                    _iconContentListBig.Add(icon);
                else _iconContentListSmall.Add(icon);
            }
        }

        // https://gist.github.com/MattRix/c1f7840ae2419d8eb2ec0695448d4321
        // https://unitylist.com/p/5c3/Unity-editor-icons
    }
}
#endif