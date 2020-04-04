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
            //PushToItch();
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
                break;
            case BuildTargets.LINUX:
                buildPath += "/StandaloneLinux64/";
                bTarget = BuildTarget.StandaloneLinux64;
                extension = ".x64";
                break;
            case BuildTargets.MAC:
                buildPath += "/StandaloneOSX/";
                bTarget = BuildTarget.StandaloneOSX;
                break;
            case BuildTargets.HTML:
                buildPath += "/HTML/";
                bTarget = BuildTarget.WebGL;
                break;
            default:
                Debug.LogError("Unidentified / unsupported build platform: " + target.ToString());
                return;            
        }
        if (!Directory.Exists(buildPath))
            Directory.CreateDirectory(buildPath);

        Debug.Log("Building player");
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath , bTarget, BuildOptions.None);
    }

    private void PushToItch()
    {
        string bat = "set serverPath=" + _serverSCP + "\n";

        bat += "cd %1\ndel *.zip\n";
        bat += "7z a -spe StandaloneWindows.zip .\\StandaloneWindows\\*\n";
        bat += "7z a -spe StandaloneOSX.zip .\\StandaloneOSX\\*\n";
        bat += "7z a -spe StandaloneLinux64.zip .\\StandaloneLinux64\\*\n";
        bat += "scp -rp *.zip %serverPath%\nexit";

        string batPath = Application.dataPath + "/ABK/Scripts/PushBuilds.bat";
        File.WriteAllText(batPath, bat);

        string buildPath = Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName.Replace('\\', '/') + "/buildPath";
        System.Diagnostics.Process.Start("cmd.exe", "/k " + batPath + " " + buildPath);
    }


    [System.Serializable]
    public class BuildPathInfo
    {
        public string _windowsPath;
        public string _macPath;
        public string _linuxPath;
    }
}
