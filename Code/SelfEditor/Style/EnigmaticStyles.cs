﻿using Enigmatic.Core.Editor.Style;
using UnityEditor;
using UnityEngine;

namespace Enigmatic.Core
{
    [InitializeOnLoad]
    public static class EnigmaticStyles
    {
        public static bool IsDark => EditorGUIUtility.isProSkin;
        private static StyleContainer m_Style;

        [InitializeOnLoadMethod]
        public static void Register()
        {
            string path = EnigmaticData.GetUnityPath($"{EnigmaticData.sourceEditor}/Enigmatic.asset");
            m_Style = EnigmaticData.LoadAssetAtPath<StyleContainer>(path);

            Debug.Log(m_Style);

            if (m_Style == null)
            {
                m_Style = ScriptableObject.CreateInstance<StyleContainer>();
                m_Style.Construct();
            }

            m_Style.ReginsterGUIStyles();
        }

        #region Main
        
        //Main
        private static GUIStyle sm_WhiteBox;
        private static GUIStyle sm_ArrowRightIcon;

        private static GUIStyle sm_AddButton;
        private static GUIStyle sm_SubstractButton;

        private static GUIStyle sm_ToolBarButton;

        private static Color sm_DarkThemeBlue = new Color(0.227f, 0.447f, 0.690f, 1);
        private static Color sm_AltDarkThemeBlue = new Color(0.2724f, 0.5364f, 0.828f, 1);

        #endregion

        #region Massege

        //Massege
        private static GUIStyle sm_ErrorIcon;
        private static GUIStyle sm_WarningIcon;
        private static GUIStyle sm_MessegeIcon;

        #endregion

        #region Inspector

        //Inspector
        private static GUIStyle sm_SplitterLabel;
        private static GUIStyle sm_FolderLabel;
        private static GUIStyle sm_GameObjectNameLabelPrefabConnection;

        private static GUIStyle sm_GameObjectIcon;
        private static GUIStyle sm_GameObjectPrefabIcon;
        private static GUIStyle sm_LightIcon;

        private static GUIStyle sm_LayersIcon;
        private static GUIStyle sm_TagIcon;

        private static GUIStyle sm_FolderIcon;

        private static GUIStyle sm_TreeHierarchy;
        private static GUIStyle sm_TreeHierarchyContinue;
        private static GUIStyle sm_TreeHierarchyEnd;

        public static Color DarkThemeBlueElementSelected = new Color(0.19295f, 0.37995f, 0.5865f, 0.85f);

        #endregion

        #region Atlaser

        private static GUIStyle sm_Chacker64;
        private static GUIStyle sm_Chacker128;
        private static GUIStyle sm_Chacker256;
        private static GUIStyle sm_Chacker512;
        private static GUIStyle sm_Chacker1024;
        private static GUIStyle sm_Chacker2048;

        #endregion

        #region Node Graph

        //Node Graph
        private static GUIStyle sm_NodeTitle;
        private static GUIStyle sm_NodeLitleTitle;

        private static GUIStyle sm_NodeBox;
        private static GUIStyle sm_SelectedOutLine;
        private static GUIStyle sm_Port;

        private static GUIStyle sm_InputDataIcon;
        private static GUIStyle sm_OutputDataIcon;
        private static GUIStyle sm_Parametor;

        private static GUIStyle sm_SubgraphNodeIcon;
        private static GUIStyle sm_NodeEditorWindowIcon;
        private static GUIStyle sm_NodeEditorInspectorWindowIcon;

        #endregion

        #region Main Property

        public static GUIStyle whiteBox
        {
            get
            {
                if (sm_WhiteBox == null)
                {
                    Texture2D whiteTexture = new Texture2D(1, 1);
                    whiteTexture.SetPixel(0, 0, new Color(1, 1, 1, 0.05f));
                    whiteTexture.Apply();

                    sm_WhiteBox = new GUIStyle(GUI.skin.box);
                    sm_WhiteBox.normal.background = whiteTexture;
                }

                return sm_WhiteBox;
            }
        }

        public static GUIStyle arrowRightIcon
        {
            get
            {
                if (sm_ArrowRightIcon == null)
                {
                    sm_ArrowRightIcon = new GUIStyle(GUI.skin.box);
                    sm_ArrowRightIcon.normal.background = EnigmaticData.LoadEditorTexture("ArrowRight.png") as Texture2D;
                }

                return sm_ArrowRightIcon;
            }
        }

        public static GUIStyle addButton
        {
            get
            {
                if (sm_AddButton == null)
                {
                    sm_AddButton = new GUIStyle(GUI.skin.box);
                    sm_AddButton.normal.background = EnigmaticData.LoadEditorTexture("AddButton.png") as Texture2D;
                }

                return sm_AddButton;
            }
        }

        public static GUIStyle substractButton
        {
            get
            {
                if (sm_SubstractButton == null)
                {
                    sm_SubstractButton = new GUIStyle(GUI.skin.box);
                    sm_SubstractButton.normal.background = EnigmaticData.LoadEditorTexture("SubstractButton.png") as Texture2D;
                }

                return sm_SubstractButton;
            }
        }

        public static GUIStyle columnBackground => m_Style.GetGUIStyle(IsDark, "ColumnBackground");

        public static GUIStyle columnBackgroundSelected => m_Style.GetGUIStyle(IsDark, "ColumnBackgroundSelected");

        public static GUIStyle toolbarButton
        { 
            get
            {
                if(sm_ToolBarButton == null)
                {
                    sm_ToolBarButton = new GUIStyle(EditorStyles.label);
                    sm_ToolBarButton.normal.textColor = GUI.skin.label.normal.textColor;
                    sm_ToolBarButton.alignment = TextAnchor.MiddleCenter;
                    sm_ToolBarButton.fontStyle = FontStyle.Bold;
                    sm_ToolBarButton.fontSize = 20;
                }

                return sm_ToolBarButton;
            }
        }

        public static GUIStyle elementMoveIcon => m_Style.GetGUIStyle(IsDark, "ElementMoveIcon");//

        public static Color DarkThemeBlue => sm_DarkThemeBlue;
        public static Color AltDarkThemeBlue => sm_AltDarkThemeBlue;

        #endregion

        #region Massege Property

        public static GUIStyle errorIcon
        {
            get
            {
                if (sm_ErrorIcon == null)
                {
                    sm_ErrorIcon = new GUIStyle(GUI.skin.box);
                    sm_ErrorIcon.normal.background = EnigmaticData.LoadEditorTexture("ErrorIcon.png");
                }

                return sm_ErrorIcon;
            }
        }

        #endregion

        #region Inspector Property

        public static GUIStyle splitterLabel
        {
            get
            {
                if(sm_SplitterLabel == null)
                {
                    sm_SplitterLabel = new GUIStyle(EditorStyles.boldLabel);
                    sm_SplitterLabel.fontSize = 14;
                    sm_SplitterLabel.alignment = TextAnchor.MiddleCenter;
                }

                return sm_SplitterLabel;
            }
        }

        public static GUIStyle folderLabel
        {
            get
            {
                if(sm_FolderLabel == null)
                {
                    sm_FolderLabel = new GUIStyle(EditorStyles.boldLabel);
                    sm_FolderLabel.fontStyle = FontStyle.BoldAndItalic; ;
                }

                return sm_FolderLabel;
            }
        }

        public static GUIStyle gameObjectNameLabelPrefabConnection
        {
            get
            {
                if(sm_GameObjectNameLabelPrefabConnection == null)
                {
                    sm_GameObjectNameLabelPrefabConnection = new GUIStyle(EditorStyles.label);
                    sm_GameObjectNameLabelPrefabConnection.normal.textColor = BluePrefabConnected;
                }

                return sm_GameObjectNameLabelPrefabConnection;
            }
        }

        public static GUIStyle gameObjectIcon
        {
            get
            {
                if (sm_GameObjectIcon == null)
                {
                    sm_GameObjectIcon = new GUIStyle(GUI.skin.box);
                    sm_GameObjectIcon.normal.background = EnigmaticData.LoadEditorTexture("GameObject.png");
                }

                return sm_GameObjectIcon;
            }
        }

        public static GUIStyle gameObjectPrefabIcon
        {
            get
            {
                if (sm_GameObjectPrefabIcon == null)
                {
                    Texture2D texture = gameObjectIcon.normal.background;
                    Color color = new Color(0.227f, 0.447f, 0.690f, 1) * 1.4f;
                    sm_GameObjectPrefabIcon = new GUIStyle(GUI.skin.box);
                    Texture2D newTexure = MultiplyColor(texture, color).normal.background;
                    sm_GameObjectPrefabIcon.normal.background = newTexure;
                }

                return sm_GameObjectPrefabIcon;
            }
        }

        public static GUIStyle layersIcon
        {
            get
            {
                if (sm_LayersIcon == null)
                {
                    sm_LayersIcon = new GUIStyle(GUI.skin.box);
                    sm_LayersIcon.normal.background = EnigmaticData.LoadEditorTexture("Layers.png") as Texture2D;
                }

                return sm_LayersIcon;
            }
        }

        public static GUIStyle tagIcon
        {
            get
            {
                if (sm_TagIcon == null)
                {
                    sm_TagIcon = new GUIStyle(GUI.skin.box);
                    sm_TagIcon.normal.background = EnigmaticData.LoadEditorTexture("Tag.png") as Texture2D;
                }

                return sm_TagIcon;
            }
        }

        public static GUIStyle folderIcon
        {
            get
            {
                if (sm_FolderIcon == null)
                {
                    sm_FolderIcon = new GUIStyle();
                    sm_FolderIcon.normal.background = EnigmaticData.LoadEditorTexture("BaseFoldor.png") as Texture2D;
                }

                return sm_FolderIcon;
            }
        }

        public static GUIStyle treeHierarchy
        {
            get
            {
                if (sm_TreeHierarchy == null)
                {
                    sm_TreeHierarchy = new GUIStyle();
                    sm_TreeHierarchy.normal.background = EnigmaticData.LoadEditorTexture("TreeHierarchy.png") as Texture2D;
                }

                return sm_TreeHierarchy;
            }
        }

        public static GUIStyle treeHierarchyContinue
        {
            get
            {
                if (sm_TreeHierarchyContinue == null)
                {
                    sm_TreeHierarchyContinue = new GUIStyle();
                    sm_TreeHierarchyContinue.normal.background = EnigmaticData.LoadEditorTexture("TreeHierarchyContinue.png") as Texture2D;
                }

                return sm_TreeHierarchyContinue;
            }
        }

        public static GUIStyle treeHierarchyEnd
        {
            get
            {
                if (sm_TreeHierarchyEnd == null)
                {
                    sm_TreeHierarchyEnd = new GUIStyle();
                    sm_TreeHierarchyEnd.normal.background = EnigmaticData.LoadEditorTexture("TreeHierarchyEnd.png") as Texture2D;
                }

                return sm_TreeHierarchyEnd;
            }
        }

        public static Color BluePrefabConnected => new Color(0.227f, 0.447f, 0.690f, 1) * 1.4f;
        
        public static Color BackgroundColor
        {
            get
            {
                Color color = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f, 1) : new Color(0.76f, 0.76f, 0.76f);
                color.a = 1;
                return color;
            }
        }

        public static Color DarkedBackgroundColor
        {
            get
            {
                Color color = BackgroundColor * 0.90f;
                color.a = 1;
                return color;
            }
        }

        public static Color HeverColor
        {
            get
            {
                Color color = BackgroundColor * 1.2f;
                color.a = 0.5f;
                return color;
            }
        }

        #endregion

        #region Node Graph Property

        public static GUIStyle nodeTitleStyle
        {
            get
            {
                if (sm_NodeTitle == null)
                {
                    sm_NodeTitle = new GUIStyle(GUI.skin.label);

                    sm_NodeTitle.fontSize = 13;
                    sm_NodeTitle.fontStyle = FontStyle.Bold;
                    sm_NodeTitle.alignment = TextAnchor.MiddleCenter;
                }

                return sm_NodeTitle;
            }
        }

        public static GUIStyle nodeLitleTitleStyle
        {
            get
            {
                if (sm_NodeLitleTitle == null)
                {
                    sm_NodeLitleTitle = new GUIStyle(GUI.skin.label);

                    sm_NodeLitleTitle.fontSize = 8;
                    sm_NodeLitleTitle.alignment = TextAnchor.MiddleCenter;
                }

                return sm_NodeLitleTitle;
            }
        }

        public static GUIStyle nodeBox
        {
            get
            {
                if (sm_NodeBox == null)
                {
                    sm_NodeBox = new GUIStyle(GUI.skin.box);
                    sm_NodeBox.normal.background = EditorGUIUtility.Load("Assets/Enigmatic/Source/Editor/Texture/NodeBox.png") as Texture2D;
                    sm_NodeBox.border = new RectOffset(22, 22, 45, 22); // Регулируйте отступы, чтобы избежать обрезания текста
                    sm_NodeBox.padding = new RectOffset(12, 12, 12, 12); // Регулируйте отступы, чтобы избежать обрезания текста
                }

                return sm_NodeBox;
            }
        }

        public static GUIStyle selectedOutLine
        {
            get
            {
                if (sm_SelectedOutLine == null)
                {
                    sm_SelectedOutLine = new GUIStyle(GUI.skin.box);
                    sm_SelectedOutLine.normal.background = EditorGUIUtility.Load("Assets/Enigmatic/Source/Editor/Texture/NodeSelected.png") as Texture2D;
                    sm_SelectedOutLine.border = new RectOffset(16, 16, 16, 16); // Регулируйте отступы, чтобы избежать обрезания текста
                }

                return sm_SelectedOutLine;
            }
        }

        public static GUIStyle Port
        {
            get
            {
                if (sm_Port == null)
                {
                    sm_Port = new GUIStyle();
                    sm_Port.normal.background = EnigmaticData.LoadEditorTexture("Port.png");
                }

                return sm_Port;
            }
        }

        public static GUIStyle inputData
        {
            get
            {
                if (sm_InputDataIcon == null)
                {
                    sm_InputDataIcon = new GUIStyle();
                    sm_InputDataIcon.normal.background = EnigmaticData.LoadEditorTexture("InputData.png");
                }

                return sm_InputDataIcon;
            }
        }

        public static GUIStyle outputData
        {
            get
            {
                if (sm_OutputDataIcon == null)
                {
                    sm_OutputDataIcon = new GUIStyle();
                    sm_OutputDataIcon.normal.background = EnigmaticData.LoadEditorTexture("OutputData.png");
                }

                return sm_OutputDataIcon;
            }
        }

        public static GUIStyle parameter
        {
            get
            {
                if (sm_Parametor == null)
                {
                    sm_Parametor = new GUIStyle();
                    sm_Parametor.normal.background = EnigmaticData.LoadEditorTexture("Parametor.png");
                }

                return sm_Parametor;
            }
        }

        public static GUIStyle subgraphNodeIcon
        {
            get
            {
                if (sm_SubgraphNodeIcon == null)
                {
                    sm_SubgraphNodeIcon = new GUIStyle();
                    sm_SubgraphNodeIcon.normal.background = EditorGUIUtility.Load("Assets/Enigmatic/Source/Editor/Texture/SubGraphNode.png") as Texture2D;
                }

                return sm_SubgraphNodeIcon;
            }
        }

        public static GUIStyle nodeEditorWindowIcon
        {
            get
            {
                if (sm_NodeEditorWindowIcon == null)
                {
                    sm_NodeEditorWindowIcon = new GUIStyle(GUI.skin.box);
                    sm_NodeEditorWindowIcon.normal.background = EditorGUIUtility.Load("Assets/Enigmatic/Source/Editor/Texture/NodeEditorGraphWindowIcon.png") as Texture2D;
                }

                return sm_NodeEditorWindowIcon;
            }
        }

        public static GUIStyle nodeEditorInspectorWindowIcon
        {
            get
            {
                if (sm_NodeEditorInspectorWindowIcon == null)
                {
                    sm_NodeEditorInspectorWindowIcon = new GUIStyle(GUI.skin.box);
                    sm_NodeEditorInspectorWindowIcon.normal.background = EditorGUIUtility.Load("Assets/Enigmatic/Source/Editor/Texture/NodeEditorGraphInspectorWindowIcon.png") as Texture2D;
                }

                return sm_NodeEditorInspectorWindowIcon;
            }
        }

        #endregion

        public static GUIStyle chacker64
        {
            get
            {
                if (sm_Chacker64 == null)
                {
                    sm_Chacker64 = new GUIStyle(GUI.skin.box);
                    sm_Chacker64.normal.background = EnigmaticData.LoadEditorTexture("Chacker64.png") as Texture2D;
                }

                return sm_Chacker64;
            }
        }

        public static GUIStyle chacker128
        {
            get
            {
                if (sm_Chacker128 == null)
                {
                    sm_Chacker128 = new GUIStyle(GUI.skin.box);
                    sm_Chacker128.normal.background = EnigmaticData.LoadEditorTexture("Chacker128.png") as Texture2D;
                }

                return sm_Chacker128;
            }
        }

        public static GUIStyle chacker256
        {
            get
            {
                if (sm_Chacker256 == null)
                {
                    sm_Chacker256 = new GUIStyle(GUI.skin.box);
                    sm_Chacker256.normal.background = EnigmaticData.LoadEditorTexture("Chacker256.png") as Texture2D;
                }

                return sm_Chacker256;
            }
        }

        public static GUIStyle chacker512
        {
            get
            {
                if (sm_Chacker512 == null)
                {
                    sm_Chacker512 = new GUIStyle(GUI.skin.box);
                    sm_Chacker512.normal.background = EnigmaticData.LoadEditorTexture("Chacker512.png") as Texture2D;
                }

                return sm_Chacker512;
            }
        }

        public static GUIStyle chacker1024
        {
            get
            {
                if (sm_Chacker1024 == null)
                {
                    sm_Chacker1024 = new GUIStyle(GUI.skin.box);
                    sm_Chacker1024.normal.background = EnigmaticData.LoadEditorTexture("Chacker1024.png") as Texture2D;
                }

                return sm_Chacker1024;
            }
        }

        public static GUIStyle chacker2048
        {
            get
            {
                if (sm_Chacker2048 == null)
                {
                    sm_Chacker2048 = new GUIStyle(GUI.skin.box);
                    sm_Chacker2048.normal.background = EnigmaticData.LoadEditorTexture("Chacker2048.png") as Texture2D;
                }

                return sm_Chacker2048;
            }
        }

        public static GUIStyle foldoutButtonClose => m_Style.GetGUIStyle(IsDark, "FoldoutButtonClose");

        public static GUIStyle foldoutButtonOpen => m_Style.GetGUIStyle(IsDark, "FoldoutButtonOpen");

        public static GUIStyle foldoutBackground => m_Style.GetGUIStyle(IsDark, "FoldoutBackground");

        public static GUIStyle toolBarButton
        {
            get
            {
                if (sm_ToolBarButton == null)
                {
                    sm_ToolBarButton = new GUIStyle(GUI.skin.box);
                    sm_ToolBarButton.normal.background = EnigmaticData.LoadEditorTexture("ToolBarButton.png");
                    sm_ToolBarButton.border = new RectOffset(17, 17, 2, 2); // Регулируйте отступы, чтобы избежать обрезания текста
                    sm_ToolBarButton.padding = new RectOffset(6, 6, 6, 6);
                }

                return sm_ToolBarButton;
            }
        }

        public static GUIStyle MultiplyColor(Texture2D texture, Color color)
        {
            Texture2D newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

            for (int x = 0; x <= newTexture.width; x++)
            {
                for (int y = 0; y <= newTexture.height; y++)
                {
                    Color originColor = texture.GetPixel(x, y);
                    newTexture.SetPixel(x, y, originColor * color);
                }
            }

            newTexture.Apply();

            GUIStyle result = new GUIStyle();
            result.normal.background = newTexture;

            return result;
        }

        public static Texture2D Rotate90Degrees(Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;

            Texture2D rotatedTexture = new Texture2D(height, width);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color pixelColor = texture.GetPixel(x, y);
                    rotatedTexture.SetPixel(y, width - 1 - x, pixelColor);
                }
            }

            rotatedTexture.Apply();

            return rotatedTexture;
        }
    }
}
