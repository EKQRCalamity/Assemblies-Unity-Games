using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.U2D;

public class DEBUG_AssetPrinter : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnGUI()
	{
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
		gUIStyle.alignment = TextAnchor.UpperLeft;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Load Operations: " + AssetBundleLoader.loadCounter);
		IList list = AssetBundleLoader.DEBUG_LoadedAssetBundles();
		stringBuilder.AppendFormat("=== AssetBundles ({0}) ===\n", list.Count);
		foreach (string item in list)
		{
			stringBuilder.AppendLine(item);
		}
		IEnumerable<AssetBundle> allLoadedAssetBundles = AssetBundle.GetAllLoadedAssetBundles();
		int num = 0;
		foreach (AssetBundle allLoadedAssetBundle in AssetBundle.GetAllLoadedAssetBundles())
		{
			num++;
		}
		stringBuilder.AppendFormat("=== System AssetBundles ({0}) ===\n", num);
		foreach (AssetBundle allLoadedAssetBundle2 in AssetBundle.GetAllLoadedAssetBundles())
		{
			stringBuilder.AppendLine(allLoadedAssetBundle2.name);
		}
		GUI.Box(new Rect(0f, 0f, 400f, Screen.height), stringBuilder.ToString());
		stringBuilder.Length = 0;
		list = AssetLoader<SpriteAtlas>.DEBUG_GetLoadedAssets();
		stringBuilder.AppendFormat("=== Cached SpriteAtlases ({0}) ===\n", list.Count);
		foreach (string item2 in list)
		{
			stringBuilder.AppendLine(item2);
		}
		list = AssetLoader<AudioClip>.DEBUG_GetLoadedAssets();
		stringBuilder.AppendFormat("=== Cached Music ({0}) ===\n", list.Count);
		foreach (string item3 in list)
		{
			stringBuilder.AppendLine(item3);
		}
		list = AssetLoader<Texture2D[]>.DEBUG_GetLoadedAssets();
		stringBuilder.AppendFormat("=== Cached Textures ({0}) ===\n", list.Count);
		foreach (string item4 in list)
		{
			stringBuilder.AppendLine(item4);
		}
		GUI.Box(new Rect(400f, 0f, 400f, Screen.height), stringBuilder.ToString());
		stringBuilder.Length = 0;
		list = Resources.FindObjectsOfTypeAll<SpriteAtlas>();
		stringBuilder.AppendFormat("=== System SpriteAtlases ({0}) ===\n", list.Count);
		foreach (object item5 in list)
		{
			stringBuilder.AppendLine(((SpriteAtlas)item5).name);
		}
		GUI.Box(new Rect(800f, 0f, 400f, Screen.height), stringBuilder.ToString());
	}
}
