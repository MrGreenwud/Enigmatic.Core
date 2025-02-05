using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Enigmatic.Core.Editor.Style
{
    internal class StyleEditorWindow : EnigmaticWindow
    {
        private List<StyleContainer> m_Contaners = new List<StyleContainer>();
        private StyleContainer m_SelectedContaner;
        private GUIStyle m_SelectedStyleElement;

        private bool m_IsExpandedNoramlState;
        private bool m_IsExpandedHoverState;
        private bool m_IsExpandedActiveState;
        private bool m_IsExpandedOnNormalState;
        private bool m_IsExpandedOnHoverState;
        private bool m_IsExpandedOnActiveState;
        private bool m_IsExpandedFontSettings;
        private bool m_IsExpandedAdditionalSettings;

        private Vector2 m_SettingsScrollPosition;

        private bool isDark;

        private Dictionary<StyleContainer, string> m_SaveFilePath;
        private ObjectPicker<StyleContainer> m_ObjectPicker;

        private float ListColumnWidth => 150;
        private float PropertyColumnWidth => 500;
        private float ColumHeight => position.height - 27;

        [MenuItem("Tools/Enigmatic/StyleEditor")]
        public static void Open()
        {
            StyleEditorWindow window = GetWindow<StyleEditorWindow>();
            window.titleContent = new GUIContent("Style Editor");

            window.minSize = new Vector2(813, 602);
            window.maxSize = new Vector2(813, 602);

            window.Show();
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            m_SelectedContaner = null;
            m_SelectedStyleElement = null;

            isDark = EditorGUIUtility.isProSkin;

            m_Contaners.Clear();

            m_SaveFilePath = new Dictionary<StyleContainer, string>();
            m_ObjectPicker = new ObjectPicker<StyleContainer>();

            m_ObjectPicker.OnPick += LoadStyleContaner;
        }

        protected override void OnDraw()
        {
            base.OnDraw();

            m_ObjectPicker.Update();

            DrawingUtilities.BeginToolBar(position.width);
            {
                if (EnigmaticGUILayout.Button("File", EditorStyles.toolbarButton))
                    ShowFileMenu();
            }
            DrawingUtilities.EndToolBar();

            EnigmaticGUILayout.Space(1);

            EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.Padding(3), EnigmaticGUILayout.ElementSpacing(3),
                    EnigmaticGUILayout.Width(position.width), EnigmaticGUILayout.Height(position.height));
            {
                //Style List
                DrawingUtilities.BeginColum(ListColumnWidth, ColumHeight, -1);
                {
                    DrawingUtilities.ColumnTitle(EnigmaticStyles.columnBackground, 
                        "Styles", ListColumnWidth, true, 50, AddStyle, RemoveStyle);

                    DrawStyles();
                }
                DrawingUtilities.EndColum();

                //Element List
                DrawingUtilities.BeginColum(ListColumnWidth, ColumHeight, -1);
                {
                    DrawingUtilities.ColumnTitle(EnigmaticStyles.columnBackground,
                        "Style Elements", ListColumnWidth, true, 0, AddStyleElement, RemoveStyleElement);

                    DrawStyleElements();
                }
                DrawingUtilities.EndColum();

                //Settings
                DrawingUtilities.BeginColum(PropertyColumnWidth, ColumHeight, 0);
                {
                    Action drawStyleType = () => 
                    {
                        EnigmaticGUILayout.Space(330);

                        string styleName = isDark == true ? "Dark" : "Light";

                        if(EnigmaticGUILayout.ButtonCentric(styleName, new Vector2(100, 21), 
                            new Vector2(0, 1), EditorStyles.toolbarDropDown))
                        {
                            SelectStyleType();
                        }
                    };

                    DrawingUtilities.ColumnTitle(EnigmaticStyles.columnBackground, "Settings", PropertyColumnWidth, 
                        false, 0, null, null, drawStyleType);

                    DrawSettings();
                }
                DrawingUtilities.EndColum();
            }
            EnigmaticGUILayout.EndHorizontal();
        }

        private void DrawStyles()
        {
            foreach(StyleContainer contaner in m_Contaners)
                DrawStyle(contaner);
        }

        private void DrawStyle(StyleContainer contaner)
        {
            float width = EnigmaticGUILayout.GetActiveGroup().GetFreeArea().x;
            Vector2 size = new Vector2(width, 20);

            GUIStyle style = EnigmaticGUIUtility.GetHasSelected(m_SelectedContaner == contaner,
                EnigmaticStyles.columnBackground, EnigmaticStyles.columnBackgroundSelected);

            if (EnigmaticGUILayout.Button(contaner.Tag, size, style))
            {
                m_SelectedContaner = contaner;
                Repaint();
            }
        }

        private void DrawStyleElements()
        {
            if (m_SelectedContaner == null)
                return;

            m_SelectedContaner.ForEach(isDark, DrawStyleElement);
        }

        private void DrawStyleElement(GUIStyle gUIStyle)
        {
            float width = EnigmaticGUILayout.GetActiveGroup().GetFreeArea().x;
            Vector2 size = new Vector2(width, 20);

            GUIStyle style = EnigmaticGUIUtility.GetHasSelected(m_SelectedStyleElement == gUIStyle,
                EnigmaticStyles.columnBackground, EnigmaticStyles.columnBackgroundSelected);

            if (EnigmaticGUILayout.Button(gUIStyle.name, size, style))
            {
                m_SelectedStyleElement = gUIStyle;
                Repaint();
            }
        }

        private void DrawSettings()
        {
            if (m_SelectedContaner == null)
                return;

            EnigmaticGUILayout.BeginVertical(EnigmaticGUILayout.Width(PropertyColumnWidth), 
                EnigmaticGUILayout.ElementSpacing(-1), EnigmaticGUILayout.Height(ColumHeight),
                EnigmaticGUILayout.Padding(1));
            {
                DrawRenameSettings();
                DrawStyleSettings();
            }
            EnigmaticGUILayout.EndVertical();
        }

        private void DrawRenameSettings()
        {
            EnigmaticGUILayout.BeginVertical(EnigmaticGUILayout.Width(PropertyColumnWidth - 6),
                EnigmaticGUILayout.ExpandHeight(true));
            {
                if (m_SelectedContaner != null)
                    m_SelectedContaner.Tag = EnigmaticGUILayout.TextField("Style Tag: ", m_SelectedContaner.Tag, 120, -1);

                if (m_SelectedStyleElement != null)
                {
                    string name = EnigmaticGUILayout.TextField("Element Name: ", m_SelectedStyleElement.name, 120, -1);
                    int index = m_SelectedContaner.IndexOf(isDark, m_SelectedStyleElement);
                    m_SelectedContaner.Rename(index, name);
                }

                EnigmaticGUILayout.Space(3);

                EnigmaticGUILayout.Image(new Vector2(PropertyColumnWidth - 6, 0), EnigmaticStyles.columnBackground);
            }
            EnigmaticGUILayout.EndVertical();
        }

        private void DrawStyleSettings()
        {
            if (m_SelectedStyleElement == null)
                return;

            Rect rect = EnigmaticGUILayout.GetActiveGroup().GetNext();
            rect.size = EnigmaticGUILayout.GetActiveGroup().GetFreeArea();
            rect.height = ColumHeight - rect.height - 10;

            EnigmaticGUILayout.BeginVerticalScrollView(rect, rect, m_SettingsScrollPosition,
                EnigmaticGUILayout.Padding(0), EnigmaticGUILayout.ElementSpacing(-1));
            {
                EnigmaticGUILayout.Space(10);

                DrawGUIStyleStateSettings(m_SelectedStyleElement.normal, ref m_IsExpandedNoramlState, "Normal");
                DrawGUIStyleStateSettings(m_SelectedStyleElement.hover, ref m_IsExpandedHoverState, "Hover");
                DrawGUIStyleStateSettings(m_SelectedStyleElement.active, ref m_IsExpandedActiveState, "Active");
                DrawGUIStyleStateSettings(m_SelectedStyleElement.onNormal, ref m_IsExpandedOnNormalState, "OnNormal");
                DrawGUIStyleStateSettings(m_SelectedStyleElement.onHover, ref m_IsExpandedOnHoverState, "OnHover");
                DrawGUIStyleStateSettings(m_SelectedStyleElement.onActive, ref m_IsExpandedOnActiveState, "OnActive");

                if (EnigmaticGUILayout.BeginFoldout(ref m_IsExpandedAdditionalSettings, "Additional", Repaint, PropertyColumnWidth - 2))
                {
                    DrawRectOffset(m_SelectedStyleElement.border, "Border");
                    DrawRectOffset(m_SelectedStyleElement.margin, "Margin");
                    DrawRectOffset(m_SelectedStyleElement.padding, "Padding");
                    DrawRectOffset(m_SelectedStyleElement.overflow, "Overflow");
                }
                EnigmaticGUILayout.EndFoldout(m_IsExpandedAdditionalSettings);

                if (EnigmaticGUILayout.BeginFoldout(ref m_IsExpandedFontSettings, "Font", Repaint, PropertyColumnWidth - 2))
                {
                    m_SelectedStyleElement.font = EnigmaticGUILayout.ObjectField("Font", m_SelectedStyleElement.font, 120, -1);
                    m_SelectedStyleElement.fontSize = EnigmaticGUILayout.IntField("Font Size", m_SelectedStyleElement.fontSize, 120, -1);
                    m_SelectedStyleElement.fontStyle = (FontStyle)EnigmaticGUILayout.EnumPopup("Font Size", m_SelectedStyleElement.fontStyle, 120, -1);
                }
                EnigmaticGUILayout.EndFoldout(m_IsExpandedFontSettings);
            }
            m_SettingsScrollPosition = EnigmaticGUILayout.EndScrollView(Repaint);
        }
        
        private void ShowFileMenu()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Save"), false, () => 
            {
                m_SaveFilePath.Clear();

                foreach (StyleContainer contaner in m_Contaners)
                {
                    string path = AssetDatabase.GetAssetPath(contaner);

                    if (string.IsNullOrEmpty(path))
                    {
                        path = EnigmaticData.styles;
                        EnigmaticData.CreateAsset(contaner, path, contaner.Tag, "asset");
                    }

                    EditorUtility.SetDirty(contaner);
                }

                AssetDatabase.SaveAssets();
            });
            
            menu.AddItem(new GUIContent("Load"), false, () => 
            {
                m_ObjectPicker.Show(null, false, "");
            });

            Rect rect = EnigmaticGUILayout.GetLastGUIRect();
            menu.DropDown(rect);
        }

        private void LoadStyleContaner(StyleContainer contaner)
        {
            if (m_Contaners.Contains(contaner))
                return;

            m_Contaners.Add(contaner);
        }

        private void DrawGUIStyleStateSettings(GUIStyleState styleState, ref bool isExpanded, string name)
        {
            EnigmaticGUILayout.BeginVertical(EnigmaticGUILayout.Width(PropertyColumnWidth),
                EnigmaticGUILayout.ExpandHeight(true), EnigmaticGUILayout.Padding(0));
            {
                if(EnigmaticGUILayout.BeginFoldout(ref isExpanded, name, Repaint, PropertyColumnWidth - 2))
                {
                    styleState.textColor = EnigmaticGUILayout.ColorField("TextColor", styleState.textColor, 120, -1);
                    styleState.background = EnigmaticGUILayout.ObjectField("Background", styleState.background, 120, -1);
                }
                EnigmaticGUILayout.EndFoldout(isExpanded);
            }
            EnigmaticGUILayout.EndVertical();
        }

        private void DrawRectOffset(RectOffset rectOffset, string name)
        {
            Vector2 freeArea = EnigmaticGUILayout.GetActiveGroup().GetFreeArea();

            EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.Width(freeArea.x),
                EnigmaticGUILayout.ExpandWidth(true), EnigmaticGUILayout.ExpandHeight(true));
            {
                EnigmaticGUILayout.Label(name, 120);

                EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.Width(freeArea.x - 120), 
                    EnigmaticGUILayout.ExpandWidth(true), EnigmaticGUILayout.ExpandHeight(true));
                {
                    EnigmaticGUILayout.BeginVertical(EnigmaticGUILayout.Width((freeArea.x - 130) / 2),
                        EnigmaticGUILayout.ExpandHeight(true));
                    {
                        rectOffset.left = EnigmaticGUILayout.IntField("L", rectOffset.left, 10, -1);
                        rectOffset.right = EnigmaticGUILayout.IntField("R", rectOffset.right, 10, -1);
                    }
                    EnigmaticGUILayout.EndVertical();

                    EnigmaticGUILayout.BeginVertical(EnigmaticGUILayout.Width((freeArea.x - 130) / 2),
                        EnigmaticGUILayout.ExpandHeight(true));
                    {
                        rectOffset.top = EnigmaticGUILayout.IntField("T", rectOffset.top, 10, -1);
                        rectOffset.bottom = EnigmaticGUILayout.IntField("B", rectOffset.bottom, 10, -1);
                    }
                    EnigmaticGUILayout.EndVertical();
                }
                EnigmaticGUILayout.EndHorizontal();
            }
            EnigmaticGUILayout.EndHorizontal();
        }

        private void SelectStyleType()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Dark"), false, () => 
            { 
                m_SelectedStyleElement = null;
                isDark = true;
            });

            menu.AddItem(new GUIContent("Light"), false, () => 
            { 
                m_SelectedStyleElement = null;
                isDark = false;
            });

            Rect rect = EnigmaticGUILayout.GetLastGUIRect();
            menu.DropDown(rect);
        }

        private void AddStyle()
        {
            StyleContainer contaner = CreateInstance<StyleContainer>();
            contaner.Tag = "New Style";
            contaner.Construct();

            m_Contaners.Add(contaner);
        }

        private void RemoveStyle()
        {
            if(m_SelectedContaner == null)
                return;

            m_Contaners.Remove(m_SelectedContaner);
        }

        private void AddStyleElement()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Box"), false, AddStyleElement, GUI.skin.box);
            menu.AddItem(new GUIContent("Lable"), false, AddStyleElement, GUI.skin.label);
            menu.AddItem(new GUIContent("Button"), false, AddStyleElement, GUI.skin.button);
            menu.AddItem(new GUIContent("EditorLable"), false, AddStyleElement, EditorStyles.label);

            Rect rect = EnigmaticGUILayout.GetLastGUIRect();
            menu.DropDown(rect);
        }

        private void AddStyleElement(object data)
        {
            if (m_SelectedContaner == null)
                return;

            GUIStyle preset = new GUIStyle((GUIStyle)data);
            preset.name = "New Element";

            m_SelectedContaner.AddGUIStyle(preset);

            Repaint();
        }

        private void RemoveStyleElement()
        {
            if (m_SelectedStyleElement == null)
                return;

            m_SelectedContaner.RemoveGUIStyle(isDark, m_SelectedStyleElement);
            m_SelectedStyleElement = null;

            Repaint();
        }
    }
}