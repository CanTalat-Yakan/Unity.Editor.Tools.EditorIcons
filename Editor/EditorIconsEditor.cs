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
                .SetBody(editor.Body)
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

            _viewBigIcons = GUILayout.SelectionGrid(
                _viewBigIcons ? 1 : 0,
                new string[] { "Small", "Big" },
                2,
                EditorStyles.toolbarButton
            ) == 1;

            GUILayout.Space(10);
            _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
        }

        private void Body()
        {
            GUILayout.Space(4);

            var iconList = GetFilteredIconList();

            float renderWidth = Screen.width / EditorGUIUtility.pixelsPerPoint - 13f;
            int gridWidth = Mathf.FloorToInt(renderWidth / _buttonSize);
            float marginLeft = (renderWidth - _buttonSize * gridWidth) / 2;

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
                        if (GUILayout.Button(icon, _iconButtonStyle, GUILayout.Width(_buttonSize), GUILayout.Height(_buttonSize)))
                        {
                            EditorGUI.FocusTextInControl("");
                            _iconSelected = icon;
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
            if (_iconSelected == null)
                return;

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                DrawPreviewSection();
                DrawInfoSection();

                if (GUILayout.Button("X",
                    GUILayout.MinHeight(20),
                    GUILayout.ExpandHeight(false),
                    GUILayout.ExpandWidth(false)))
                {
                    _iconSelected = null;
                }
            }
        }

        private void DrawPreviewSection()
        {
            using (new GUILayout.VerticalScope(GUILayout.Width(130)))
            {
                GUILayout.Space(4);
                var preview = _darkPreview ? _iconPreviewBlack : _iconPreviewWhite;
                GUILayout.Button(_iconSelected, preview, GUILayout.Width(128), GUILayout.Height(68));
                GUILayout.Space(5);
                var selected = _darkPreview ? 1 : 0;
                _darkPreview = GUILayout.SelectionGrid(selected, new string[] { "Light", "Dark" }, 2, EditorStyles.miniButton) == 1;
                GUILayout.FlexibleSpace();
            }
            GUILayout.Space(10);
        }

        private void DrawInfoSection()
        {
            using (new GUILayout.VerticalScope())
            {
                string size = $"Size: {_iconSelected.image.width}x{_iconSelected.image.height}";
                size += "\nIs Pro Skin Icon: " + (_iconSelected.tooltip.IndexOf("d_") == 0 ? "Yes" : "No");
                size += $"\nTotal {_iconContentListAll.Count} icons";

                GUILayout.Space(5);
                EditorGUILayout.HelpBox(size, MessageType.None);
                GUILayout.Space(5);
                EditorGUILayout.TextField("EditorGUIUtility.IconContent(\"" + _iconSelected.tooltip + "\")");
                GUILayout.Space(5);

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Copy to clipboard", EditorStyles.miniButton))
                        EditorGUIUtility.systemCopyBuffer = _iconSelected.tooltip;
                    if (GUILayout.Button("Save icon to file ...", EditorStyles.miniButton))
                        SaveIcon(_iconSelected.tooltip);
                }
            }
            GUILayout.Space(10);
        }

    }
}
#endif