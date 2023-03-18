using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Tools.DataContainer;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MapGenerator : SerializedMonoBehaviour
{
	private List<Transform> domains = new List<Transform>();

	private Dictionary<string, List<Transform>> domainZones = new Dictionary<string, List<Transform>>();

	private Camera currentCamera;

	private const int resWidth = 640;

	private const int resHeight = 360;

	private string MAP_RESOURCE_CONFIG = "Maps/MapData";

	private MapData currentData;

	[BoxGroup("Config", true, false, 0)]
	public string domainRegExp = "D[0-9][0-9] - (?<name>[a-zA-Z0-9_ ()]*)";

	[BoxGroup("Config", true, false, 0)]
	public Transform rootMap;

	[BoxGroup("Config", true, false, 0)]
	public Camera orthCamera;

	private bool hasError;

	[ShowIf("hasError", true)]
	[BoxGroup("Config", true, false, 0)]
	[ShowInInspector]
	[ReadOnly]
	[LabelText("")]
	[Multiline]
	private string errorDescription;

	[BoxGroup("Generate", true, false, 0)]
	[ShowInInspector]
	[ReadOnly]
	[LabelText("Number of domains")]
	private int numDomains;

	[BoxGroup("Generate", true, false, 0)]
	[ShowInInspector]
	[ReadOnly]
	[LabelText("Number of zones")]
	private int numZones;

	[HorizontalGroup("Domain", 0f, 0, 0, 0)]
	[ShowIf("CanSave", true)]
	[ValueDropdown("GetDomains")]
	private string currentDomain;

	[HorizontalGroup("Zone", 0f, 0, 0, 0)]
	[ShowIf("CanSave", true)]
	[ValueDropdown("GetZones")]
	private string currentZone;

	[BoxGroup("Config", true, false, 0)]
	[Button(ButtonSizes.Medium)]
	public void Refresh()
	{
		hasError = false;
		errorDescription = string.Empty;
		numDomains = 0;
		numZones = 0;
		domainZones.Clear();
		domains.Clear();
		currentCamera = orthCamera;
		if (!currentCamera)
		{
			currentCamera = GetComponent<Camera>();
		}
		if (!currentCamera)
		{
			hasError = true;
			errorDescription = "Select a camera or attach to gameobject with camera";
			return;
		}
		Regex regex = new Regex(domainRegExp);
		List<Transform> list = (rootMap ? rootMap.Cast<Transform>().ToList() : (from gObj in base.gameObject.scene.GetRootGameObjects()
			select gObj.transform).ToList());
		if (list.Count <= 0)
		{
			hasError = true;
			errorDescription = "The root object hasn't any child with this regex (use null if you want to use scene root)";
			return;
		}
		foreach (Transform item2 in list)
		{
			string text = item2.name;
			Match match = regex.Match(text);
			if (!match.Success)
			{
				continue;
			}
			domains.Add(item2);
			domainZones[text] = new List<Transform>();
			numDomains++;
			foreach (Transform item3 in item2)
			{
				domainZones[text].Add(item3);
				numZones++;
			}
		}
	}

	private IList<ValueDropdownItem<string>> GetDomains()
	{
		ValueDropdownList<string> valueDropdownList = new ValueDropdownList<string>();
		foreach (string key in domainZones.Keys)
		{
			valueDropdownList.Add(key, key);
		}
		return valueDropdownList;
	}

	[HorizontalGroup("Domain", 0f, 0, 0, 0)]
	[ShowIf("CanSave", true)]
	[Button(ButtonSizes.Small)]
	public void SaveDomain()
	{
		_SaveDomainMaps(currentDomain);
	}

	private IList<ValueDropdownItem<string>> GetZones()
	{
		ValueDropdownList<string> valueDropdownList = new ValueDropdownList<string>();
		if (currentDomain != null && domainZones.ContainsKey(currentDomain))
		{
			foreach (Transform item in domainZones[currentDomain])
			{
				valueDropdownList.Add(item.name, item.name);
			}
			return valueDropdownList;
		}
		return valueDropdownList;
	}

	[HorizontalGroup("Zone", 0f, 0, 0, 0)]
	[ShowIf("CanSave", true)]
	[Button(ButtonSizes.Small)]
	public void SaveZone()
	{
		_SaveZoneMap(currentDomain, currentZone);
	}

	[BoxGroup("Generate", true, false, 0)]
	[ShowIf("CanSave", true)]
	[Button(ButtonSizes.Large)]
	public void SaveAllMaps()
	{
		PrepareScene();
		foreach (List<Transform> item in domainZones.Values.ToList())
		{
			foreach (Transform item2 in item)
			{
				_ExportMap(item2);
			}
		}
		RestoreScene();
	}

	[BoxGroup("Generate", true, false, 0)]
	[ShowIf("CanSave", true)]
	[Button(ButtonSizes.Large)]
	public void CheckAllMaps()
	{
		foreach (List<Transform> item in domainZones.Values.ToList())
		{
			foreach (Transform item2 in item)
			{
				SetCameraAndCheckMap(item2);
			}
		}
	}

	private bool CanSave()
	{
		return numZones != 0;
	}

	[BoxGroup("Config", true, false, 0)]
	[ShowIf("InCanvas", true)]
	[Button(ButtonSizes.Medium)]
	public void TestInUI()
	{
		MapData mapData = Resources.Load<MapData>(MAP_RESOURCE_CONFIG);
		GameObject gameObject = new GameObject("MAP", typeof(RectTransform));
		gameObject.transform.SetParent(base.transform);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.localRotation = Quaternion.identity;
		rectTransform.localScale = Vector3.one;
		rectTransform.localPosition = Vector3.zero;
		foreach (KeyValuePair<string, MapData.MapItem> datum in mapData.data)
		{
			float num = (float)datum.Value.height / (datum.Value.cameraSize * 2f);
			GameObject gameObject2 = new GameObject(datum.Key, typeof(RectTransform));
			gameObject2.transform.SetParent(rectTransform);
			RectTransform rectTransform2 = (RectTransform)gameObject2.transform;
			rectTransform2.localRotation = Quaternion.identity;
			rectTransform2.localScale = Vector3.one;
			rectTransform2.localPosition = new Vector3(datum.Value.position.x * num, datum.Value.position.y * num, 0f);
			rectTransform2.sizeDelta = new Vector2(datum.Value.width, datum.Value.height);
			Image image = gameObject2.AddComponent<Image>();
			Sprite sprite = (Sprite)Resources.Load(datum.Value.mapImage, typeof(Sprite));
			image.sprite = sprite;
		}
	}

	private bool InCanvas()
	{
		return GetComponent<Canvas>() != null;
	}

	private void _SaveDomainMaps(string domain)
	{
		if (!domainZones.ContainsKey(domain))
		{
			return;
		}
		PrepareScene();
		foreach (Transform item in domainZones[domain])
		{
			_ExportMap(item);
		}
		RestoreScene();
	}

	private void _SaveZoneMap(string domain, string zone)
	{
		if (!domainZones.ContainsKey(domain))
		{
			return;
		}
		PrepareScene();
		foreach (Transform item in domainZones[domain])
		{
			if (item.name == zone)
			{
				_ExportMap(item);
			}
		}
		RestoreScene();
	}

	private bool SetCameraAndCheckMap(Transform tr)
	{
		Bounds maxBounds = GetMaxBounds(tr);
		Vector3 localPosition = new Vector3(maxBounds.center.x, maxBounds.center.y, -100f);
		currentCamera.transform.localPosition = localPosition;
		bool flag = IsFullyVisible(maxBounds);
		if (!flag)
		{
			Debug.Log("*** Error in map DOMAIN:" + tr.parent.name + " ZONE: " + tr.name);
		}
		return flag;
	}

	private void _AddToHideAllObjectsByTag(List<GameObject> list, Transform parent, string tag)
	{
		foreach (Transform item in parent)
		{
			if (item.tag == tag)
			{
				item.gameObject.SetActive(value: false);
				list.Add(item.gameObject);
			}
			else if (item.childCount > 0)
			{
				_AddToHideAllObjectsByTag(list, item, tag);
			}
		}
	}

	private void _ExportMap(Transform tr)
	{
		SetCameraAndCheckMap(tr);
		List<GameObject> list = new List<GameObject>();
		tr.parent.gameObject.SetActive(value: true);
		foreach (Transform item in tr.parent)
		{
			item.gameObject.SetActive(item == tr);
			list.Add(item.gameObject);
		}
		Dictionary<SpriteRenderer, Color> dictionary = new Dictionary<SpriteRenderer, Color>();
		SpriteRenderer[] componentsInChildren = tr.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			dictionary[spriteRenderer] = spriteRenderer.color;
			spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
		}
		TextMesh[] componentsInChildren2 = tr.GetComponentsInChildren<TextMesh>();
		foreach (TextMesh textMesh in componentsInChildren2)
		{
			if (textMesh.gameObject.activeSelf)
			{
				list.Add(textMesh.gameObject);
				textMesh.gameObject.SetActive(value: false);
			}
		}
		_AddToHideAllObjectsByTag(list, tr, "EditorOnly");
		Debug.Log("-- Exporting " + tr.name);
		string domain = tr.parent.name.Substring(0, 3);
		string zone = tr.name.Substring(0, 3);
		string mapImage = SaveScreenShot(domain, zone);
		if ((bool)currentData)
		{
			MapData.MapItem mapItem = new MapData.MapItem();
			mapItem.position = new Vector3(currentCamera.transform.position.x, currentCamera.transform.position.y, 0f);
			mapItem.domain = domain;
			mapItem.zone = zone;
			mapItem.mapImage = mapImage;
			mapItem.height = 360;
			mapItem.width = 640;
			mapItem.cameraSize = currentCamera.orthographicSize;
			mapItem.cells = new List<Bounds>();
			SpriteRenderer[] componentsInChildren3 = tr.GetComponentsInChildren<SpriteRenderer>();
			SpriteRenderer[] array = componentsInChildren3;
			foreach (Renderer renderer in array)
			{
				if (!renderer.gameObject.CompareTag("EditorOnly"))
				{
					mapItem.cells.Add(new Bounds(renderer.bounds.center, renderer.bounds.size));
				}
			}
			currentData.data[mapItem.GetKey()] = mapItem;
		}
		foreach (KeyValuePair<SpriteRenderer, Color> item2 in dictionary)
		{
			item2.Key.color = item2.Value;
		}
		list.ForEach(delegate(GameObject obj)
		{
			obj.SetActive(value: true);
		});
		tr.parent.gameObject.SetActive(value: false);
	}

	private void PrepareScene()
	{
		foreach (Transform domain in domains)
		{
			domain.gameObject.SetActive(value: false);
		}
		currentData = Resources.Load<MapData>(MAP_RESOURCE_CONFIG);
		if (!currentData)
		{
			currentData = ScriptableObject.CreateInstance<MapData>();
		}
		currentData.data = new Dictionary<string, MapData.MapItem>();
	}

	private void RestoreScene()
	{
		foreach (Transform domain in domains)
		{
			domain.gameObject.SetActive(value: true);
		}
	}

	private Bounds GetMaxBounds(Transform obj)
	{
		bool flag = true;
		Bounds result = default(Bounds);
		SpriteRenderer[] componentsInChildren = obj.GetComponentsInChildren<SpriteRenderer>();
		if (componentsInChildren.Length > 0)
		{
			SpriteRenderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				if (!renderer.gameObject.CompareTag("EditorOnly"))
				{
					if (flag)
					{
						result = new Bounds(componentsInChildren[0].bounds.center, componentsInChildren[0].bounds.size);
						flag = false;
					}
					else
					{
						result.Encapsulate(renderer.bounds);
					}
				}
			}
		}
		else
		{
			result = new Bounds(obj.position, Vector3.zero);
		}
		return result;
	}

	public void SetImportSetting()
	{
	}

	public bool IsFullyVisible(Bounds bounds)
	{
		Vector3 size = bounds.size;
		Vector3 min = bounds.min;
		Plane[] array = GeometryUtility.CalculateFrustumPlanes(currentCamera);
		List<Vector3> list = new List<Vector3>(8);
		list.Add(min);
		list.Add(min + new Vector3(0f, 0f, size.z));
		list.Add(min + new Vector3(size.x, 0f, size.z));
		list.Add(min + new Vector3(size.x, 0f, 0f));
		List<Vector3> list2 = list;
		for (int i = 0; i < 4; i++)
		{
			list2.Add(list2[i] + size.y * Vector3.up);
		}
		for (int j = 0; j < array.Length; j++)
		{
			for (int k = 0; k < list2.Count; k++)
			{
				if (!array[j].GetSide(list2[k]))
				{
					return false;
				}
			}
		}
		return true;
	}

	private string SaveScreenShot(string domain, string zone)
	{
		RenderTexture renderTexture = new RenderTexture(640, 360, 32, RenderTextureFormat.ARGB32);
		currentCamera.targetTexture = renderTexture;
		Texture2D texture2D = new Texture2D(640, 360, TextureFormat.ARGB32, mipmap: false);
		currentCamera.Render();
		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, 640f, 360f), 0, 0);
		currentCamera.targetTexture = null;
		RenderTexture.active = null;
		Object.DestroyImmediate(renderTexture);
		return EditorSaveImageToDisk(texture2D, $"{domain}_{zone}");
	}

	private string SaveMask(Transform tr, string domain, string zone)
	{
		Texture2D texture2D = new Texture2D(640, 360, TextureFormat.Alpha8, mipmap: false);
		byte element = 0;
		texture2D.LoadRawTextureData(Enumerable.Repeat(element, 230400).ToArray());
		texture2D.Apply();
		return EditorSaveImageToDisk(texture2D, $"{domain}_{zone}_Mask");
	}

	private string EditorSaveImageToDisk(Texture2D screenShot, string name)
	{
		byte[] bytes = screenShot.EncodeToPNG();
		string text = "Maps\\" + name;
		string text2 = Application.dataPath + "\\Resources\\" + text + ".png";
		if (!Directory.Exists(Path.GetDirectoryName(text2)))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(text2));
		}
		Debug.Log(text2);
		File.WriteAllBytes(text2, bytes);
		return text;
	}
}
