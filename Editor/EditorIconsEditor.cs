#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace UnityEssentials
{
    public partial class EditorIcons
    {
        public EditorWindowDrawer Window;
        public Action Repaint;
        public Action Close;

        [MenuItem("Tools/Editor Icons %e", priority = 1001)]
        public static void ShowWindow()
        {
            var editor = new EditorIcons();
            editor.Window = new EditorWindowDrawer("Editor Icons", new(320, 450), new(700, 600))
                .SetInitialization(editor.Initialization)
                .SetHeader(editor.Header, EditorWindowDrawer.GUISkin.Toolbar)
                .SetBody(editor.Body, EditorWindowDrawer.GUISkin.Margin)
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

            GUILayout.Space(10);
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
                DrawPreviewSection();
                DrawInfoSection();

                if (GUILayout.Button("X",
                    GUILayout.MinHeight(20),
                    GUILayout.ExpandHeight(false),
                    GUILayout.ExpandWidth(false)))
                {
                    s_iconSelected = null;
                }
            }
        }

        private void DrawPreviewSection()
        {
            using (new GUILayout.VerticalScope(GUILayout.Width(130)))
            {
                GUILayout.Space(4);
                var preview = s_lightPreview ? s_iconPreviewWhite : s_iconPreviewBlack;
                GUILayout.Button(s_iconSelected, preview, GUILayout.Width(128), GUILayout.Height(68));
                GUILayout.Space(5);
                var selected = s_lightPreview ? 0 : 1;
                s_lightPreview = GUILayout.SelectionGrid(selected, new string[] { "Light", "Dark" }, 2, EditorStyles.miniButton) == 0;
            }
            GUILayout.Space(10);
        }

        private void DrawInfoSection()
        {
            using (new GUILayout.VerticalScope())
            {
                string size = $"Size: {s_iconSelected.image.width}x{s_iconSelected.image.height}";
                size += "\nIs Pro Skin Icon: " + (s_iconSelected.tooltip.IndexOf("d_") == 0 ? "Yes" : "No");
                size += $"\nTotal {s_iconContentListAll.Count} icons";

                GUILayout.Space(5);
                EditorGUILayout.HelpBox(size, MessageType.None);
                GUILayout.Space(5);
                EditorGUILayout.TextField("EditorGUIUtility.IconContent(\"" + s_iconSelected.tooltip + "\")");
                GUILayout.Space(5);

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Copy to clipboard", EditorStyles.miniButton))
                        EditorGUIUtility.systemCopyBuffer = s_iconSelected.tooltip;
                    if (GUILayout.Button("Save icon to file ...", EditorStyles.miniButton))
                        SaveIcon(s_iconSelected.tooltip);
                }
            }
            GUILayout.Space(10);
        }

    }
}
#endif