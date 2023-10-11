using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

[ProtoContract]
[RequireComponent(typeof(ToggleGroup))]
public class TabGroup : MonoBehaviour
{
	public Dictionary<GameObject, GameObject> tabs = new Dictionary<GameObject, GameObject>();

	[HideInInspector]
	private Dictionary<GameObject, TabGroupElement> elements = new Dictionary<GameObject, TabGroupElement>();

	[HideInInspector]
	private ToggleGroup toggleGroup;

	protected void Awake()
	{
		toggleGroup = GetComponent<ToggleGroup>();
	}

	private void Start()
	{
		foreach (GameObject key in tabs.Keys)
		{
			ConnectTab(key);
			elements[key].OnValueChanged(key.GetComponent<Toggle>().isOn);
		}
		GameObject gameObject = tabs.Keys.FirstOrDefault();
		if ((bool)gameObject)
		{
			gameObject.GetComponent<Toggle>().isOn = true;
		}
	}

	private void ConnectTab(GameObject tabButton)
	{
		CanvasGroup orAddComponent = tabs[tabButton].GetOrAddComponent<CanvasGroup>();
		Toggle orAddComponent2 = tabButton.GetOrAddComponent<Toggle>();
		TabGroupElement tabGroupElement = new TabGroupElement(orAddComponent);
		elements.Add(tabButton, tabGroupElement);
		orAddComponent2.group = toggleGroup;
		orAddComponent2.onValueChanged.AddListener(tabGroupElement.OnValueChanged);
	}

	private void DisconnectTab(GameObject tabButton)
	{
		Toggle orAddComponent = tabButton.GetOrAddComponent<Toggle>();
		TabGroupElement @object = elements[tabButton];
		elements.Remove(tabButton);
		orAddComponent.group = null;
		orAddComponent.onValueChanged.RemoveListener(@object.OnValueChanged);
	}

	public void AddTab(GameObject toggle, GameObject tabContent)
	{
		if (!tabs.ContainsKey(toggle))
		{
			tabs.Add(toggle, tabContent);
			ConnectTab(toggle);
		}
	}

	public void RemoveTab(GameObject toggle)
	{
		if (tabs.ContainsKey(toggle))
		{
			DisconnectTab(toggle);
			tabs.Remove(toggle);
		}
	}

	public void ClearTabs()
	{
		List<GameObject> list = tabs.Keys.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			RemoveTab(list[i]);
		}
	}
}
