#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace UnityEssentials
{
    public partial class EditorIconSearch
    {
        public EditorWindowDrawer Window;
        public Action Repaint;
        public Action Close;

        [MenuItem("Tools/Editor Icons %e", priority = 1001)]
        public static void ShowWindow()
        {
            var editor = new EditorIconSearch();
            editor.Window = new EditorWindowDrawer("Editor Icons", new(320, 450), new(700, 600))
                .SetInitialization(editor.Initialization)
                .SetHeader(editor.Header, EditorWindowStyle.Toolbar)
                .SetBody(editor.Body, EditorWindowStyle.Margin)
                .SetFooter(editor.Footer)
                .GetRepaintEvent(out editor.Repaint)
                .GetCloseEvent(out editor.Close)
                .ShowUtility();
        }

        private void Initialization() =>
            InitializeIcons();

        private void Header()
        {
            if (GUILayout.Button("Save all icons to folder...", EditorStyles.toolbarButton))
                SaveAllIcons();

            s_viewBigIcons = GUILayout.SelectionGrid(
                s_viewBigIcons ? 1 : 0,
                new string[] { "Small", "Big" },
                2,
                EditorStyles.toolbarButton
            ) == 1;

            GUILayout.Space(4);
            _iconNameFilter = EditorGUILayout.TextField(_iconNameFilter, EditorStyles.toolbarSearchField);
        }

        private void Body()
        {
            GUILayout.Space(4);

            var iconList = GetFilteredIconList();

            float renderWidth = Screen.width / EditorGUIUtility.pixelsPerPoint - 24;

            int maxIconsPerRow = Mathf.FloorToInt(renderWidth / 40);
            maxIconsPerRow = Mathf.Max(1, maxIconsPerRow);

            float iconSize = Mathf.Max(40, renderWidth / maxIconsPerRow);

            int gridWidth = maxIconsPerRow;
            float marginLeft = (renderWidth - iconSize * gridWidth) / 2;

            int row = 0;
            int index = 0;

            while (index < iconList.Count)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(marginLeft);

                    for (int i = 0; i < gridWidth; ++i)
                    {
                        int k = i + row * gridWidth;
                        if (k >= iconList.Count) break;

                        var icon = iconList[k];
                        if (GUILayout.Button(icon, s_iconButtonStyle, GUILayout.Width(iconSize), GUILayout.Height(iconSize)))
                        {
                            EditorGUI.FocusTextInControl("");
                            s_iconSelected = icon;
                        }
                        index++;
                    }
                }
                row++;
            }
            GUILayout.Space(4);
        }

        private void Footer()
        {
            if (s_iconSelected == null)
                return;

            GUILayout.Space(-1);
            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                var iconTexture = s_iconSelected.image as Texture2D;
                var iconRect = GUILayoutUtility.GetRect(100, 100, GUILayout.Width(100), GUILayout.Height(100));

                if (iconTexture != null)
                {
                    int maxBox = 100;
                    int smallBox = 75;

                    int targetBox = (iconTexture.width <= smallBox && iconTexture.height <= smallBox) ? smallBox : maxBox;

                    // Calculate scale to fit (never exceed targetBox)
                    float scale = Mathf.Min((float)targetBox / iconTexture.width, (float)targetBox / iconTexture.height);

                    float drawWidth = iconTexture.width * scale;
                    float drawHeight = iconTexture.height * scale;

                    // Center the icon in the 100x100 rect
                    var drawRect = new Rect(
                        iconRect.x + (iconRect.width - drawWidth) / 2f,
                        iconRect.y + (iconRect.height - drawHeight) / 2f,
                        drawWidth,
                        drawHeight);

                    // Draw as a button
                    GUI.DrawTexture(drawRect, iconTexture, ScaleMode.ScaleToFit, true);
                }

                GUILayout.Space(10);
                using (new GUILayout.VerticalScope())
                {
                    string iconInfo = $"Size: {s_iconSelected.image.width}x{s_iconSelected.image.height}";
                    iconInfo += "\nIs Pro Skin Icon: " + (s_iconSelected.tooltip.IndexOf("d_") == 0 ? "Yes" : "No");
                    iconInfo += $"\nTotal {s_iconContentListAll.Count} icons";

                    GUILayout.Space(12);
                    GUILayout.Label(iconInfo, EditorStyles.miniLabel);
                    GUILayout.Space(4);
                    EditorGUILayout.TextField("EditorGUIUtility.IconContent(\"" + s_iconSelected.tooltip + "\")");
                    GUILayout.Space(4);

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Copy to clipboard", EditorStyles.miniButton))
                            EditorGUIUtility.systemCopyBuffer = s_iconSelected.tooltip;
                        if (GUILayout.Button("Save icon to file ...", EditorStyles.miniButton))
                            SaveIcon(s_iconSelected.tooltip);
                    }
                }
            }
        }
    }
}
#endif