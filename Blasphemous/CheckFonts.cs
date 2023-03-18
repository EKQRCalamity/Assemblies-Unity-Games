using System.Collections.Generic;
using I2.Loc;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckFonts : MonoBehaviour
{
	private class data
	{
		public int total;

		public List<string> names = new List<string>();

		public List<string> prefabs = new List<string>();
	}

	public Font goodFont;

	public TMP_FontAsset goodFontPro;

	public List<Font> fontsToChange;

	public bool showNames;

	public bool changePro;

	[Button(ButtonSizes.Medium)]
	public void CheckChild()
	{
		Text[] componentsInChildren = GetComponentsInChildren<Text>(includeInactive: true);
		CheckInternal(componentsInChildren);
	}

	[Button(ButtonSizes.Medium)]
	public void CheckAll()
	{
		Text[] foundObjects = Resources.FindObjectsOfTypeAll<Text>();
		CheckInternal(foundObjects);
	}

	[Button(ButtonSizes.Medium)]
	public void CheckChildTextMesh()
	{
		TextMeshProUGUI[] componentsInChildren = GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		CheckInternalPro(componentsInChildren);
	}

	[Button(ButtonSizes.Medium)]
	public void CheckAllTextMesh()
	{
		TextMeshProUGUI[] foundObjects = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
		CheckInternalPro(foundObjects);
	}

	[Button(ButtonSizes.Medium)]
	public void CheckLocalizationChild()
	{
		Text[] componentsInChildren = GetComponentsInChildren<Text>(includeInactive: true);
		CheckLocalizationInternal(componentsInChildren);
	}

	[Button(ButtonSizes.Medium)]
	public void CheckLocalizationAll()
	{
		Text[] foundObjects = Resources.FindObjectsOfTypeAll<Text>();
		CheckLocalizationInternal(foundObjects);
	}

	public void CheckInternalPro(TextMeshProUGUI[] foundObjects)
	{
		Dictionary<string, data> dictionary = new Dictionary<string, data>();
		int num = 0;
		foreach (TextMeshProUGUI textMeshProUGUI in foundObjects)
		{
			if (textMeshProUGUI.font == goodFontPro)
			{
				continue;
			}
			if (changePro)
			{
				textMeshProUGUI.font = goodFontPro;
				textMeshProUGUI.material = goodFontPro.material;
				num++;
				continue;
			}
			if (!dictionary.ContainsKey(textMeshProUGUI.font.name))
			{
				dictionary[textMeshProUGUI.font.name] = new data();
			}
			dictionary[textMeshProUGUI.font.name].total++;
			dictionary[textMeshProUGUI.font.name].names.Add(GetGameObjectPath(textMeshProUGUI.transform));
		}
	}

	public void CheckInternal(Text[] foundObjects)
	{
		Dictionary<string, data> dictionary = new Dictionary<string, data>();
		int num = 0;
		foreach (Text text in foundObjects)
		{
			if (text.font == goodFont)
			{
				continue;
			}
			if (fontsToChange.Contains(text.font))
			{
				text.font = goodFont;
				text.material = goodFont.material;
				num++;
				continue;
			}
			if (!dictionary.ContainsKey(text.font.name))
			{
				dictionary[text.font.name] = new data();
			}
			dictionary[text.font.name].total++;
			dictionary[text.font.name].names.Add(GetGameObjectPath(text.transform));
		}
		Debug.Log("********************************************");
		Debug.Log("***  FONTS CHANGEG:" + num);
		Debug.Log("***  UNKNOW");
		foreach (KeyValuePair<string, data> item in dictionary)
		{
			Debug.Log(item.Key + ": " + item.Value.total);
			if (showNames && item.Value.names.Count > 0)
			{
				foreach (string name in item.Value.names)
				{
					Debug.Log("    -" + name);
				}
			}
			if (item.Value.prefabs.Count <= 0)
			{
				continue;
			}
			Debug.Log("--- Prefabs");
			foreach (string prefab in item.Value.prefabs)
			{
				Debug.Log("    -" + prefab);
			}
		}
	}

	public void CheckLocalizationInternal(Text[] foundObjects)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (Text text in foundObjects)
		{
			string gameObjectPath = GetGameObjectPath(text.transform);
			Localize component = text.gameObject.GetComponent<Localize>();
			if (!component)
			{
				list.Add(gameObjectPath);
			}
			else if (component.SecondaryTerm == null || !component.SecondaryTerm.StartsWith("UI/FONT"))
			{
				list2.Add(gameObjectPath + " -- TEXT: " + text.text + "  SECOND:" + component.SecondaryTerm);
			}
		}
		Debug.Log("********************************************");
		Debug.Log("***  NO LOCALIZATION:" + list.Count);
		foreach (string item in list)
		{
			Debug.Log(item);
		}
		Debug.Log("********************************************");
		Debug.Log("***  ERROR SECONDARY:" + list2.Count);
		foreach (string item2 in list2)
		{
			Debug.Log(item2);
		}
	}

	private string GetGameObjectPath(Transform transform)
	{
		string text = transform.name;
		while (transform.parent != null)
		{
			transform = transform.parent;
			text = transform.name + "/" + text;
		}
		return text;
	}
}
