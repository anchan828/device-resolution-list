using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DeviceResolusionList
{
	public class GameViewSizeHelper
	{
		public enum GameViewSizeType
		{
			AspectRatio,
			FixedResolution
		}
	
		static string assemblyName = "UnityEditor.dll";
		static Type gameViewSizeType = Types.GetType ("UnityEditor.GameViewSizeType", assemblyName);
		static Type gameViewSize = Types.GetType ("UnityEditor.GameViewSize", assemblyName);
		static Type gameViewSizes = Types.GetType ("UnityEditor.ScriptableSingleton`1", assemblyName).MakeGenericType (Types.GetType ("UnityEditor.GameViewSizes", assemblyName));
	
	#region private Class
		private static GameViewSize _gameViewSize;
		public class GameViewSize
		{
			public GameViewSizeType type;
			public int width;
			public int height;
			public string baseText;
		}
	#endregion private Class
	#region public Method
		
		public static void AddCustomSize (GameViewSizeGroupType groupType, GameViewSize gameViewSize)
		{
			_gameViewSize = gameViewSize;
			object sizeType = Enum.Parse (gameViewSizeType, gameViewSize.type.ToString ());
		
			ConstructorInfo ctor = GameViewSizeHelper.gameViewSize.GetConstructor (new Type[] {
			gameViewSizeType,
			typeof(int),
			typeof(int),
			typeof(string)
		});
			object instance_gameViewSize = ctor.Invoke (new object[] {
			sizeType,
			gameViewSize.width,
			gameViewSize.height,
			gameViewSize.baseText
		});
		
			object instance_gameViewSizeGroup = GetGroup (groupType, instance);
		
			if (!Contains (instance_gameViewSizeGroup)) {
				AddCustomSize (instance_gameViewSizeGroup, instance_gameViewSize);
			}
		}
		
		public static void AddCustomSize (GameViewSizeGroupType groupType, GameViewSizeType type, int width, int height, string baseText)
		{
			AddCustomSize(groupType, new GameViewSize{type= type,width = width, height = height, baseText = baseText});
		}
	
		public static bool RemoveCustomSize (GameViewSizeGroupType groupType, GameViewSizeType type, int width, int height, string baseText)
		{
			_gameViewSize = new GameViewSize{type= type,width = width, height = height, baseText = baseText};
			return Remove (GetGroup (groupType, instance));
		}
	 	public static bool RemoveCustomSize (GameViewSizeGroupType groupType, GameViewSize gameViewSize){
			_gameViewSize = gameViewSize;
			return Remove (GetGroup (groupType, instance));
		}
		public static bool Contains (GameViewSizeGroupType groupType, GameViewSizeType type, int width, int height, string baseText)
		{
			_gameViewSize = new GameViewSize{type= type,width = width, height = height, baseText = baseText};
			return Contains (GetGroup (groupType, instance));
		}
	public static bool Contains (GameViewSizeGroupType groupType, GameViewSize gameViewSize)
		{
			_gameViewSize = gameViewSize;
			return Contains (GetGroup (groupType, instance));
		}
	#endregion public Method
	
		static bool Remove (object instance_gameViewSizeGroup)
		{
			int gameViewSizeLength = GetCustomCount (instance_gameViewSizeGroup);
			int totalCount = GetTotalCount (instance_gameViewSizeGroup);
			for (int i = totalCount - gameViewSizeLength; i < totalCount; i++) {
				object other_gameViewSize = GetGameViewSize (instance_gameViewSizeGroup, i);
				if (GameViewSize_Equals (_gameViewSize, other_gameViewSize)) {
					RemoveCustomSize (instance_gameViewSizeGroup, i);
					return true;
				}
			}
			return false;
		}
	
		static bool Contains (object instance_gameViewSizeGroup)
		{
			int gameViewSizeLength = GetCustomCount (instance_gameViewSizeGroup);
			int totalCount = GetTotalCount (instance_gameViewSizeGroup);
			for (int i = totalCount - gameViewSizeLength; i < totalCount; i++) {
				object other_gameViewSize = GetGameViewSize (instance_gameViewSizeGroup, i);
				if (GameViewSize_Equals (_gameViewSize, other_gameViewSize)) {
				
					return true;
				}
			}
			return false;
		}
	
		private static bool GameViewSize_Equals (GameViewSize a, object b)
		{
			int b_width = (int)GetGameSizeProperty (b, "width");
			int b_height = (int)GetGameSizeProperty (b, "height");
			string b_baseText = (string)GetGameSizeProperty (b, "baseText");
			GameViewSizeType b_sizeType = (GameViewSizeType)Enum.Parse (typeof(GameViewSizeType), GetGameSizeProperty (b, "sizeType").ToString ());
		
			return a.type == b_sizeType && a.width == b_width && a.height == b_height && a.baseText == b_baseText;
		}

		static object GetGameSizeProperty (object instance, string name)
		{
			return instance.GetType ().GetProperty (name).GetValue (instance, new object[0]);
		}
	
		static object m_instance;
	
		static object instance {
			get {
				if (m_instance == null) {
					PropertyInfo propertyInfo_gameViewSizes = gameViewSizes.GetProperty ("instance", BindingFlags.NonPublic | BindingFlags.Static);
					m_instance = propertyInfo_gameViewSizes.GetValue (null, new object[0]);
				}
				return m_instance;
			}
		}
	
		static object GetGroup (GameViewSizeGroupType groupType, object instance_gameViewSizes)
		{
			return instance_gameViewSizes.GetType ().GetMethod ("GetGroup", BindingFlags.Public | BindingFlags.Instance, null, new Type[] {
			groupType.GetType ()
		}, null).Invoke (instance_gameViewSizes, new object[] {
			groupType
		});
		}
	
		static object GetGameViewSize (object instance_gameViewSizeGroup, int i)
		{
			return instance_gameViewSizeGroup.GetType ().GetMethod ("GetGameViewSize", BindingFlags.Public | BindingFlags.Instance, null, new Type[] {
			typeof(int)
		}, null).Invoke (instance_gameViewSizeGroup, new object[] {
			i
		});
		}
	
		static int GetCustomCount (object instance_gameViewSizeGroup)
		{
			return (int)instance_gameViewSizeGroup.GetType ().GetMethod ("GetCustomCount", BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null).Invoke (instance_gameViewSizeGroup, new object[0]);
		}
	
		static int GetTotalCount (object instance_gameViewSizeGroup)
		{
			return (int)instance_gameViewSizeGroup.GetType ().GetMethod ("GetTotalCount", BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null).Invoke (instance_gameViewSizeGroup, new object[0]);
		}
	
		static void AddCustomSize (object instance_gameViewSizeGroup, object instance_gameViewSize)
		{
			instance_gameViewSizeGroup.GetType ().GetMethod ("AddCustomSize", BindingFlags.Public | BindingFlags.Instance, null, new Type[] {
			gameViewSize
		}, null).Invoke (instance_gameViewSizeGroup, new object[] {
			instance_gameViewSize
		});
		}
	
		static void RemoveCustomSize (object instance_gameViewSizeGroup, int index)
		{
			instance_gameViewSizeGroup.GetType ().GetMethod ("RemoveCustomSize", BindingFlags.Public | BindingFlags.Instance, null, new Type[] {
			typeof(int)
		}, null).Invoke (instance_gameViewSizeGroup, new object[] {
			index
		});
		}
	
	}
}