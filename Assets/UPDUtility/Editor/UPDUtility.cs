using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System;
using System.Text;
using Debug = UnityEngine.Debug;

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
                return;
            }
            else
            {
                if (!EditorPrefs.GetBool("UPDSetup"))
                {
                    EditorPrefs.SetString("UPDScope", EditorGUILayout.TextField("Scope", EditorPrefs.GetString("UPDScope")));
                    EditorPrefs.SetString("UPDRepository", EditorGUILayout.TextField("GithubUserName", EditorPrefs.GetString("UPDRepository")));
                    if (GUILayout.Button("UPDSetup"))
                    {
                        EditorPrefs.SetBool("UPDSetup", true);
                    }
                    return;
                }
                else
                {
                    //セットアップボタン
                    if (!Setting.Setup)
                    {
                        Setting.FolderName = EditorGUILayout.TextField("FolderName", Setting.FolderName);

                        Setting.Author = EditorGUILayout.TextField("Author", Setting.Author);
                        Setting.PackageName = EditorGUILayout.TextField("PackageName", Setting.PackageName);
                        Setting.PackageDisplayName = EditorGUILayout.TextField("PackageDisplayName", Setting.PackageDisplayName);
                        Setting.PackageDescription = EditorGUILayout.TextField("PackageDescription", Setting.PackageDescription);
                        Setting.PackageRepository = EditorGUILayout.TextField("PackageRepositoryName", Setting.PackageRepository);

                        if (GUILayout.Button("Setup"))
                        {
                            Setting.Version = new Version()
                            {
                                a = 1,
                                b = 0,
                                c = 0,
                            };
                            Setting.Setup = true;
                            FolderPath(Setting.FolderName);
                            Setting.ProjectDirectory = Setting.FolderName + "/";
                            SaveJson(Setting);

                            EditorUtility.SetDirty(Setting);
                            AssetDatabase.SaveAssets();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Publish"))
                        {
                            SaveJson(Setting);
                            string command = $"/c cd Assets & cd {Setting.ProjectDirectory.Replace("/", "\\")} & npm publish";
                            Process.Start("cmd.exe", command);
                            Setting.Version.c = Setting.Version.c + 1;
                        }
                    }
                }
            }
        }

        static void SaveJson(UPDSetting Setting)
        {
            CreateTextFile(Setting.ProjectDirectory + "package.json", "{" + $"\"author\": \"{Setting.Author}\",\"name\": \"{EditorPrefs.GetString("UPDScope") + "." + Setting.PackageName}\",\"version\": \"{Setting.Version}\",\"displayName\": \"{Setting.PackageDisplayName}\",\"description\": \"{Setting.PackageDescription}\",\"repository\": \"github:{EditorPrefs.GetString("UPDRepository") + "/" + Setting.PackageRepository}\"" + "}");
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
            StreamWriter sw = File.CreateText(path);
            sw.Write(content);
            sw.Close();
        }
    }

    [CreateAssetMenu()]
    public class UPDSetting : ScriptableObject
    {
        public bool Setup;
        public string FolderName;
        public string ProjectDirectory;
        public Version Version;

        public string Author;
        public string PackageName;
        public string PackageDisplayName;
        public string PackageDescription;
        public string PackageRepository;
    }

    [Serializable]
    public class Version
    {
        public int a;
        public int b;
        public int c;

        public override string ToString()
        {
            return a + "." + b + "." + c;
        }
    }
}