using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Networking;

public class UploadManager : MonoBehaviour
{
	public enum BuildTargets {WINDOWS, LINUX, MAC};
	static string _url="http://74.208.168.174:443/";
	//Procures and zips builds
	[MenuItem("Uploader/StageBuilds")]
	public static void StageBuilds()
	{
		BuildPlayer(BuildTargets.WINDOWS);
		BuildPlayer(BuildTargets.LINUX);
		BuildPlayer(BuildTargets.MAC);
	}

	//Verifies header, title, description, images
	[MenuItem("Uploader/ValidateContent")]
	public static bool ValidateContent()
	{
		string uploaderDir = Application.dataPath+"/_Uploader";
		//check for header
		string headerDir = uploaderDir+"/Header";
		if(!Directory.Exists(headerDir)){
			FastLogger("@Error$: Missing _Uploader/Header/ directory","#BA001C");
			return false;
		}
		else if(!File.Exists(headerDir+"/header.png")){
			FastLogger("@Error$: Missing Header/header.png (412x74px)","#BA001C");
			return false;
		}
		else{
			FastLogger("@Verified$: Header");
		}
		//check for at least one screenshot
		string screensDir = uploaderDir+"/Screens";
		if(!Directory.Exists(screensDir)){
			FastLogger("@Error$: Missing _Uploader/Screens/ directory", "#BA001C");
			return false;
		}
		else{
			string [] screens = Directory.GetFiles(screensDir, "*.png");
			if(screens.Length<1){
				FastLogger("@Error$: Missing Screens/*.png (177x133px)", "#BA001C");
				return false;
			}
			else{
				FastLogger("@Verified$: Screens");
			}
		}
		//check for description
		string descriptionDir = uploaderDir+"/Description";
		if(!Directory.Exists(descriptionDir)){
			FastLogger("@Error$: Missing _Uploader/Description/ directory", "#BA001C");
			return false;
		}
		else if(!File.Exists(descriptionDir+"/description.txt")){
			FastLogger("@Error$: Missing Description/description.txt", "#BA001C");
			return false;
		}
		else{
			FastLogger("@Verified$: Description");
		}
		return true;
	}

	//downloads manifest, generates Id, updates manifest, uploads all the things
	[MenuItem("Uploader/Ship")]
	public static void Ship()
	{
		//download manifest
		string rawManifest="";
		using(UnityWebRequest request = UnityWebRequest.Get(_url+"manifest.json"))
		{
			var response = request.SendWebRequest();
			while (!response.isDone){
				if(request.isNetworkError){
					FastLogger("@Error$: Internet required to ship package", "#BA001C");
					return;
				}
			}
			FastLogger("@Downloading$ manifest.json");
			rawManifest=request.downloadHandler.text;
		}
		FastLogger("@Parsing Manifest$");
		GameDataArray gda = JsonUtility.FromJson<GameDataArray>(rawManifest);
		string id = Application.productName.Replace(" ","");
		FastLogger("@Shipping ID$: "+id);

		//create GameData obj
		GameData gameData;
		gameData.title = Application.productName;
		gameData.header = "header.png";
		string [] screens = Directory.GetFiles(Application.dataPath+"/_Uploader/Screens", "*.png");
		for(int i=0; i<screens.Length; i++)
		{
			string [] parts = screens[i].Split('\\');
			screens[i] = parts[parts.Length-1];
			FastLogger("@Prepping screenshot$: "+screens[i]);
		}
		gameData.screens=screens;
		string desc = File.ReadAllText(Application.dataPath+"/_Uploader/Description/description.txt");
		gameData.desc=desc;
		gameData.id=id;

		//check if GameData exists or needs to be created
		int existsIndex=-1;
		for(int i=0; i<gda.games.Length; i++){
			if(gda.games[i].id==gameData.id){
				existsIndex=i;
				break;
			}
		}
		GameData[] newGames;
		if(existsIndex==-1){
			newGames = new GameData[gda.games.Length+1];
			for(int i=0; i<gda.games.Length; i++){
				newGames[i]=gda.games[i];
			}
			newGames[newGames.Length-1]=gameData;
			gda.games=newGames;
		}
		else{
			gda.games[existsIndex]=gameData;
		}

		//write updated manifest
		FastLogger("@Updating manifest$");
		File.WriteAllText(Application.dataPath+"/_Uploader/Manifest/manifest.json",JsonUtility.ToJson(gda));

		//server path http://74.208.168.174:443/+ $(id)
		//write a batch script that
		//zip the windows build
		//zip the mac build
		//zip the linux build
		//scp the zips
		//scp the manifest
		//scp the header
		//scp the screens
		
		//todo after initial upload success - attempt to update and remove the word 'boobies' from the games description
	}

	static void BuildPlayer(BuildTargets target){
		string buildPath = Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName.Replace('\\','/') + "/buildPath";
		if(!Directory.Exists(buildPath))
			Directory.CreateDirectory(buildPath);

		BuildTarget buildTarget;
		string ext = "";

		switch(target){
			case BuildTargets.WINDOWS:
				buildPath += "/StandaloneWindows/";
				buildTarget = BuildTarget.StandaloneWindows;
				ext=".exe";
				break;
			case BuildTargets.LINUX:
				buildPath += "/StandaloneLinux64/";
				buildTarget = BuildTarget.StandaloneLinux64;
				ext=".x64";
				break;
			case BuildTargets.MAC:
				buildPath += "/StandaloneOSX/";
				buildTarget = BuildTarget.StandaloneOSX;
				break;
			default:
				return;
				break;
		}

		if(!Directory.Exists(buildPath))
			Directory.CreateDirectory(buildPath);

		FastLogger("@Building $"+Application.productName+"@ for target: $"+target.ToString());
		BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath+Application.productName+ext, buildTarget, BuildOptions.None);
	}

	[MenuItem("Uploader/Open Build Folder")]
	public static void OpenBuildFolder(){
		string buildPath = Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName.Replace('\\','/') + "/buildPath";
		if(!Directory.Exists(buildPath))
			Directory.CreateDirectory(buildPath);
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.FileName = "explorer";
		startInfo.Arguments = buildPath.Replace('/','\\');
		startInfo.UseShellExecute = false;
		startInfo.CreateNoWindow = true;
		System.Diagnostics.Process.Start(startInfo);
	}

	[MenuItem("Uploader/LogTest")]
	static void LogTest(){
		FastLogger("@Hello$");
	}

	static void FastLogger(string input, string color="#FF7500"){
		string log = input.Replace("@","<color="+color+">");
		log = log.Replace("$","</color>");
		Debug.Log(log);
	}

	[System.Serializable]
	public struct GameDataArray{
		public GameData [] games;
	}
	[System.Serializable]
	public struct GameData {
		public string title;
		public string header;
		public string [] screens;
		public string desc;
		public string id;
	}
}
