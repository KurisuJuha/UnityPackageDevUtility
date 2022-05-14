using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace KYapp.UPD
{
    public class UPDUtility : EditorWindow
    {
        public UPDSetting Setting;

        [MenuItem("KYapp/UPD")]
        private static void ShowWindow()
        {
            UPDUtility window = GetWindow<UPDUtility>();
            window.titleContent = new GUIContent("UPD");
        }
        private void OnGUI()
        {
            if (Setting == null)
            {
                var guids = AssetDatabase.FindAssets("UPDSetting");
                if (guids.Length == 0)
                {
                    throw new FileNotFoundException("UPDSetting does not found");
                }

                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var obj = AssetDatabase.LoadAssetAtPath<UPDSetting>(path);
                Setting = obj;
            }
            if (!EditorPrefs.GetBool("UPDSetup"))
            {
                EditorPrefs.SetString("UPDScope", EditorGUILayout.TextField("Scope", EditorPrefs.GetString("UPDScope")));
                EditorPrefs.SetString("UPDRepository", EditorGUILayout.TextField("GithubUserName", EditorPrefs.GetString("UPDRepository")));
                if (GUILayout.Button("UPDSetup"))
                {
                    EditorPrefs.SetBool("UPDSetup", true);
                }
            }
            else
            {
                //セットアップボタン
                if (!Setting.Setup)
                {
                    Setting.FolderName = EditorGUILayout.TextField("FolderName", Setting.FolderName);
                    Setting.IsAssets = EditorGUILayout.Toggle("IsAssets", Setting.IsAssets);

                    Setting.PackageName = EditorGUILayout.TextField("PackageName", Setting.PackageName);
                    Setting.PackageDisplayName = EditorGUILayout.TextField("PackageDisplayName", Setting.PackageDisplayName);
                    Setting.PackageDescription = EditorGUILayout.TextField("PackageDescription", Setting.PackageDescription);
                    Setting.PackageRepository = EditorGUILayout.TextField("PackageRepositoryName", Setting.PackageRepository);

                    if (GUILayout.Button("Setup"))
                    {
                        //アセットかどうか
                        Setting.Setup = true;
                        if (!Setting.IsAssets)
                        {
                            FolderPath("Packages");
                            FolderPath("Packages/" + Setting.FolderName);
                            Setting.ProjectDirectory = "Packages/" + Setting.FolderName + "/";
                        }
                        else
                        {
                            FolderPath(Setting.FolderName);
                            Setting.ProjectDirectory = Setting.FolderName + "/";
                        }
                        Debug.Log(Setting.ProjectDirectory);
                        CreateTextFile(Setting.ProjectDirectory + "package.json", "{" + $"\"name\": \"{EditorPrefs.GetString("UPDScope") + "." + Setting.PackageName}\",\"version\": \"1.0.0\",\"displayName\": \"{Setting.PackageDisplayName}\",\"description\": \"{Setting.PackageDescription}\",\"repository\": \"github:{EditorPrefs.GetString("UPDRepository") + "/" + Setting.PackageRepository}\"" + "}");
                    }
                }
            }
        }

        static void FolderPath(string folderPath)
        {
            string path = "Assets/" + folderPath;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            AssetDatabase.ImportAsset(path);
        }
        static void CreateTextFile(string name, string content)
        {
            string path = "Assets/" + name;
            Debug.Log(path);
            StreamWriter sw = File.CreateText(path);
            sw.Write(content);
            sw.Close();
        }
    }

    [CreateAssetMenu()]
    public class UPDSetting : ScriptableObject
    {
        public bool Setup;
        public bool IsAssets;
        public string FolderName;
        public string ProjectDirectory;

        public string PackageName;
        public string PackageDisplayName;
        public string PackageDescription;
        public string PackageRepository;
    }
}