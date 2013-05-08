using UnityEngine;
using System.IO;
using UnityEditor;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace DeviceResolusionList
{
	public class DeviceResolutionListWindow : EditorWindow
	{
		private List<DeviceResolution> devices = new List<DeviceResolution> ();
		private const string assetName = "DevicesResolutionList";
		private Vector2 scroll = new Vector2 ();
		private bool guiVisible = false;
		private static DeviceResolutionListWindow _window;
		private static DeviceResolutionListWindow window{
			get{
				if(_window == null){
					_window = GetWindow<DeviceResolutionListWindow> ();
				}
				return _window;
			}
			set{
				_window = value;	
			}
		}
		enum LoadType
		{
			None,
			Csv
		}
	
	#region deviceStyle
		private static GUIStyle _deviceStyle;

		private static GUIStyle deviceStyle {
			get {
				if (_deviceStyle == null) {
					_deviceStyle = new GUIStyle (EditorStyles.label);
					_deviceStyle.wordWrap = true;
				}

				return _deviceStyle;
			}
		}
	#endregion 
	#region titleStyle

		private static GUIStyle _titleStyle;
	
		private static GUIStyle titleStyle {
			get {
				if (_titleStyle == null) {
					_titleStyle = new GUIStyle (EditorStyles.boldLabel);
					_titleStyle.alignment = TextAnchor.MiddleCenter;
				}
			
				return _titleStyle;
			}
		}
	#endregion 
	
	#region EditorWindow Methods


		[MenuItem("Window/DeviceList")]
		static void Open ()
		{
			GetWindow<DeviceResolutionListWindow> ().maxSize = new Vector2 (580, 9999);
		}
	
		void OnEnable ()
		{
			window = GetWindow<DeviceResolutionListWindow> ();
			window.ShowNotification (new GUIContent ("Loading..."));
			LoadType loadType = LoadType.None;
			string loadPath = "";

			string[] paths = AssetDatabase.GetAllAssetPaths ();
			foreach (string path in paths) {
				if (Path.GetFileName (path) == assetName + ".csv") {
					loadPath = path; 
					loadType = LoadType.Csv;
					continue;
				}
			}
			if (loadType == LoadType.Csv) {
				LoadDeviceResolusionsFromCsv (loadPath);
			} else {
				Debug.LogError ("Couldn't load device list.");
			}
		
			guiVisible = true;
			window.RemoveNotification ();
		}

		void OnDisable ()
		{
			window = null;
		}
	#endregion
	#region GUI Methods
	
		void OnGUI ()
		{
			if (!guiVisible)
				return;
			DrawTitle ();
			scroll = GUILayout.BeginScrollView (scroll);
				
			foreach (DeviceResolution device in devices) {
				DrawDevice (device);
			}
			GUILayout.EndScrollView ();
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("All")) {
				EnableAll (true);
			}
			if (GUILayout.Button ("None")) {
				EnableAll (false);
			}
			if (GUILayout.Button ("Cancel")) {
				Close ();
			}
			if (GUILayout.Button ("Update")) {
				UpdateGameViewSizes ();
			}
			EditorGUILayout.EndHorizontal ();
		
		}
	
		void DrawTitle ()
		{
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Space (15);
			GUILayout.Label ("キャリア", titleStyle, GUILayout.Width (60));
			GUILayout.Label ("メーカー", titleStyle, GUILayout.Width (100));
			GUILayout.Label ("端末名", titleStyle, GUILayout.Width (200));
			GUILayout.Label ("横", titleStyle, GUILayout.Width (40));
			GUILayout.Label ("縦", titleStyle, GUILayout.Width (40));
			GUILayout.Label ("発売日", titleStyle, GUILayout.Width (70));
			EditorGUILayout.EndHorizontal ();
		}
	
		static void DrawDevice (DeviceResolution device)
		{
			if (device == null) {
				return;
			}
			EditorGUILayout.BeginHorizontal ();
			device.enable = GUILayout.Toggle (device.enable, "", GUILayout.Width (15));
			GUILayout.Label (device.career, deviceStyle, GUILayout.Width (60));
			GUILayout.Label (device.maker, deviceStyle, GUILayout.Width (100));
			GUILayout.Label (device.deviceName, deviceStyle, GUILayout.Width (200));
			GUILayout.Label (device.width.ToString (), deviceStyle, GUILayout.Width (40));
			GUILayout.Label (device.height.ToString (), deviceStyle, GUILayout.Width (40));
			GUILayout.Label (device.releaseDate, deviceStyle, GUILayout.Width (70));
			EditorGUILayout.EndHorizontal ();
		}
	#endregion
	
	#region Button Actions
	
		void EnableAll (bool enable)
		{
			devices.ForEach (device => device.enable = enable);
		}

		void UpdateGameViewSizes ()
		{
			devices.ForEach (device => {
				GameViewSizeGroupType type = GameViewSizeGroupType.Android;
				if (device.deviceName.ToLower ().Contains ("iphone") || device.deviceName.ToLower ().Contains ("ipad")) {
					type = GameViewSizeGroupType.iOS;
				}
				if (device.enable) {
					GameViewSizeHelper.AddCustomSize (type, GameViewSizeHelper.GameViewSizeType.FixedResolution, device.width, device.height, device.deviceName);
				} else {
					GameViewSizeHelper.RemoveCustomSize (type, GameViewSizeHelper.GameViewSizeType.FixedResolution, device.width, device.height, device.deviceName);
				}
			});
			window.ShowNotification (new GUIContent ("Updated"));
		}

	#endregion
	
	#region CSV
	
	
	
		void LoadDeviceResolusionsFromCsv (string path)
		{
			string[,] splitCsv = SplitCsv (File.ReadAllText (path));
			for (int y = 1; y < splitCsv.GetUpperBound (1); y++) {
				DeviceResolution device = CreateInstance<DeviceResolution> ();
				device.career = splitCsv [0, y];
				device.maker = splitCsv [1, y];
				device.deviceName = splitCsv [2, y];
				device.width = int.Parse (splitCsv [3, y]);
				device.height = int.Parse (splitCsv [4, y]);
				device.releaseDate = splitCsv [5, y];
			
				GameViewSizeGroupType type = GameViewSizeGroupType.Android;
				if (device.deviceName.ToLower ().Contains ("iphone") || device.deviceName.ToLower ().Contains ("ipad")) {
					type = GameViewSizeGroupType.iOS;
				}
				device.enable = GameViewSizeHelper.Contains (type, GameViewSizeHelper.GameViewSizeType.FixedResolution, device.width, device.height, device.deviceName);
				devices.Add (device);
			}
		}

		private string[,] SplitCsv (string csvText)
		{
			string[] lines = csvText.Split ("\n" [0]); 
		
			int width = 0; 
			for (int i = 0; i < lines.Length; i++) {
				string[] row = SplitCsvLine (lines [i]); 
				width = Mathf.Max (width, row.Length); 
			}
		
			string[,] outputGrid = new string[width + 1, lines.Length + 1]; 
			for (int y = 0; y < lines.Length; y++) {
				string[] row = SplitCsvLine (lines [y]); 
				for (int x = 0; x < row.Length; x++) {
					outputGrid [x, y] = row [x]; 
				
					outputGrid [x, y] = outputGrid [x, y].Replace ("\"\"", "\"");
				}
			}
		
			return outputGrid; 
		}

		private string[] SplitCsvLine (string line)
		{
			List<string> list = new List<string> ();
			MatchCollection matches = Regex.Matches (line, @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)", RegexOptions.ExplicitCapture);
			
			foreach (Match match in matches) {
				list.Add (match.Groups [1].Value);
			}
			return list.ToArray ();
		}
	#endregion
	}
}
