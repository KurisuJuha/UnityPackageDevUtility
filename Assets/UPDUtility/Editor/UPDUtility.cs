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

        [MenuItem("KYapp/UPD")]
        private static void ShowWindow()
        {

            UPDUtility window = GetWindow<UPDUtility>();
            window.titleContent = new GUIContent("UPD");
        }
        private void OnGUI()
        {
            Debug.Log(UPDSetting.instance.PackageDescription);
            if (GUILayout.Button("Reset"))
            {
                UPDSetting.instance.Setup = false;
            }

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
                if (!UPDSetting.instance.Setup)
                {
                    UPDSetting.instance.FolderName = EditorGUILayout.TextField("FolderName", UPDSetting.instance.FolderName);

                    UPDSetting.instance.Author = EditorGUILayout.TextField("Author", UPDSetting.instance.Author);
                    UPDSetting.instance.PackageName = EditorGUILayout.TextField("PackageName", UPDSetting.instance.PackageName);
                    UPDSetting.instance.PackageDisplayName = EditorGUILayout.TextField("PackageDisplayName", UPDSetting.instance.PackageDisplayName);
                    UPDSetting.instance.PackageDescription = EditorGUILayout.TextField("PackageDescription", UPDSetting.instance.PackageDescription);
                    UPDSetting.instance.PackageRepository = EditorGUILayout.TextField("PackageRepositoryName", UPDSetting.instance.PackageRepository);

                    if (GUILayout.Button("Setup"))
                    {
                        UPDSetting.instance.Version = new Version()
                        {
                            a = 1,
                            b = 0,
                            c = 0,
                        };
                        UPDSetting.instance.Setup = true;
                        FolderPath(UPDSetting.instance.FolderName);
                        UPDSetting.instance.ProjectDirectory = UPDSetting.instance.FolderName + "/";
                        SaveJson(UPDSetting.instance);

                        UPDSetting.instance.save();
                    }
                }
                else
                {
                    if (GUILayout.Button("Publish"))
                    {
                        SaveJson(UPDSetting.instance);
                        string command = $"/c cd Assets & cd {UPDSetting.instance.ProjectDirectory.Replace("/", "\\")} & npm publish";
                        Process.Start("cmd.exe", command);
                        UPDSetting.instance.Version.c = UPDSetting.instance.Version.c + 1;

                        EditorUtility.SetDirty(UPDSetting.instance);
                        AssetDatabase.SaveAssets();
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

    [FilePath("UPDSetting/UPDSetting.data", FilePathAttribute.Location.ProjectFolder)]
    public class UPDSetting : ScriptableSingleton<UPDSetting>
    {
        [SerializeField]
        public bool Setup;

        [SerializeField]
        public string FolderName;

        [SerializeField]
        public string ProjectDirectory;

        [SerializeField]
        public Version Version;


        [SerializeField]
        public string Author;

        [SerializeField]
        public string PackageName;

        [SerializeField]
        public string PackageDisplayName;

        [SerializeField]
        public string PackageDescription;

        [SerializeField]
        public string PackageRepository;

        public void save()
        {
            Save(true);
        }
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