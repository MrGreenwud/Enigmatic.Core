using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Enigmatic.Core
{
    public static class EnigmaticData
    {
        public static readonly string main = $"Enigmatic";

        public static readonly string resources = $"Resources/Enigmatic";
        public static readonly string resourcesEditor = $"{resources}/Editor"; //Editor
        public static readonly string source = $"{main}/Source";

        public static readonly string sourceEditor = $"{source}/Editor";//Editor

        //Editor
        public static readonly string textures = $"{sourceEditor}/Texture"; //Editor

        //GUIDManager
        public static readonly string GUIDHistoryContaner = $"{resourcesEditor}/GUIDHistory"; //Editor

        //Styles
        public static readonly string styles = $"{resourcesEditor}/Styles"; //Editor

        //Input
        public static readonly string inputStorege = $"{resources}/KFInput";

        public static readonly string inputEditorSettings = $"{inputStorege}/EditorInputSettings.asset";
        public static readonly string inputSettings = $"{inputStorege}/InputSettings.asset";

        public static readonly string inputProviders = $"{inputStorege}/Providers";
        public static readonly string inputMaps = $"{inputStorege}/Maps";

        //SearchedTree
        public static readonly string enigmaticTrees = $"{source}/SearchedTree"; //Editor
        public static readonly string treeStorege = $"{resourcesEditor}/SearchedTree"; //Editor
        public static string enigmaticTree => GetFullPath($"{source}/SearchedTree"); //Editor

        //EditorStyle settings
        public static readonly string editorStyleSettings = $"{source}/Editor/EnigmaticStylesSettings.asset"; //Editor

        //NodeGraph EditorInfo
        public static readonly string nodeGraphEditorInfo = $"{resourcesEditor}/NodeGraph"; //Editor

        public static string GetFullPath(string path) => $"{Application.dataPath}/{path}";
        
        public static string GetUnityPath(string path) => $"Assets/{path}"; //Editor
        
        public static string GetUniformPath(string path)
        {
            Queue<string> elments = path.Split('/').ToQueue();
            string resulPath = string.Empty;

            bool isFindRootFolder = false;

            while(elments.Count > 0)
            {
                if (isFindRootFolder)
                {
                    resulPath += $"{elments.Dequeue()}";

                    if (elments.Count > 0)
                        resulPath += "/";
                }
                else if (elments.Dequeue() == "Assets"
                    && elments.Contains("Assets") == false)
                {
                    isFindRootFolder = true;
                }
            }

            return resulPath;
        }

        public static string GetPath(params string[] paths)
        {
            string path = string.Empty;

            foreach (string p in paths)
                path += $"/{p}";

            return path;
        }

        public static T LoadResources<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>($"Enigmatic/{path}");
        }

#if UNITY_EDITOR

        public static UnityEngine.Object[] LoadAllAssetsAtPathWithExtantion(string path, string extantion, Type type)
        {
            List<UnityEngine.Object> assets = new List<UnityEngine.Object>();

            string[] paths =
                Directory.GetFiles(GetFullPath(path), extantion)
                .Select((x) => GetUnityPath(GetUniformPath(x))).ToArray();

            foreach (string p in paths)
                assets.Add(AssetDatabase.LoadAssetAtPath(p, type));

            return assets.ToArray();
        }

        public static T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object
        {
            return (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
        }

        public static Texture2D LoadEditorTexture(string fileName, string additionalPath = "")
        {
            return LoadEditorResources(GetUnityPath($"{textures}/{additionalPath}/{fileName}")) as Texture2D;
        } //Editor

        public static UnityEngine.Object LoadEditorResources(string path)
        {
            return EditorGUIUtility.Load(path);
        } //Editor

        public static void CreateAsset(UnityEngine.Object asset, string path, string name, string extantion)
        {
            if (Directory.Exists(GetFullPath(path)) == false)
            {
                Directory.CreateDirectory(GetFullPath(path));
                Debug.Log($"Directory {GetFullPath(path)} created!");
            }

            Debug.Log(GetFullPath(path));

            path = $"{GetUnityPath(path)}/{name}.{extantion}";

            AssetDatabase.CreateAsset(asset, path);
        }
#endif
    }
}
