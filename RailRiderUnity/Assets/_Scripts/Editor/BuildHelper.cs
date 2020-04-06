using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildHelper : EditorWindow
{
    private bool _includeBundles;
    public enum BuildTargets { WINDOWS, LINUX, MAC, HTML };
    public BuildTargets _curTarget;
    public bool _deployBuilds;

    public string _serverSCP;
    public bool _loadCheck = false;

    // Start is called before the first frame update
    [MenuItem("BuildScript/Buildhtml")]
    static void Init()
    {
        BuildHelper window = (BuildHelper)EditorWindow.GetWindow(typeof(BuildHelper));
        window.Show();
    }

    private void OnGUI()
    {
        if(GUILayout.Button("Build And Push"))
        {
            BuildPlayer(BuildTargets.HTML);
            PushToItch();
        }
        if(GUILayout.Button("Build standalone"))
        {
            BuildPlayer(BuildTargets.WINDOWS);
            BuildPlayer(BuildTargets.LINUX);
            BuildPlayer(BuildTargets.MAC);
        }
    }

    private void BuildPlayer(BuildTargets target)
    {
        //add scenes to build list
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
            Debug.Log("Adding Scene: " + scenes[i]);
        }

        BuildTarget bTarget;
        Debug.Log("Preparing build for target: " + target.ToString());
        string buildPath = Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName.Replace('\\','/')+"/buildPath";
        string extension = ".exe";
        //determine platform specifics
        switch (target)
        {
            case BuildTargets.WINDOWS:
                buildPath += "/StandaloneWindows/";
                bTarget = BuildTarget.StandaloneWindows;
                Debug.Log("Building player");
                BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath + Application.productName + extension, bTarget, BuildOptions.None);
                break;
            case BuildTargets.LINUX:
                buildPath += "/StandaloneLinux64/";
                bTarget = BuildTarget.StandaloneLinux64;
                Debug.Log("Building player");
                BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath + Application.productName + extension, bTarget, BuildOptions.None);
                extension = ".x64";
                break;
            case BuildTargets.MAC:
                buildPath += "/StandaloneOSX/";
                bTarget = BuildTarget.StandaloneOSX;
                Debug.Log("Building player");
                BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath + Application.productName + extension, bTarget, BuildOptions.None);
                break;
            case BuildTargets.HTML:
                buildPath += "/HTML/";
                bTarget = BuildTarget.WebGL;
                Debug.Log("Building player");
                BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath, bTarget, BuildOptions.None);
                break;
            default:
                Debug.LogError("Unidentified / unsupported build platform: " + target.ToString());
                return;            
        }
        if (!Directory.Exists(buildPath))
            Directory.CreateDirectory(buildPath);

        
    }

    private void PushToItch()
    {
        string bat = "cd %1\nbutler push HTML rithmgaming/rail-rider:HTML5\nexit";

        string batPath = Application.dataPath + "/_Scripts/Bat/PushToItch.bat";
        File.WriteAllText(batPath, bat);

        string buildPath = Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName.Replace('\\', '/') + "/buildPath";
        System.Diagnostics.Process.Start("cmd.exe", "/k " + batPath + " " + buildPath);
        Debug.Log("Pushing to itch");
    }


    [System.Serializable]
    public class BuildPathInfo
    {
        public string _windowsPath;
        public string _macPath;
        public string _linuxPath;
    }
}
