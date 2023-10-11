using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleLink : MonoBehaviour
{
	public List<Toggle> links = new List<Toggle>();

	private Toggle toggle;

	private void Awake()
	{
		toggle = GetComponent<Toggle>();
	}

	private void Start()
	{
		toggle.onValueChanged.AddListener(SelfValueChanged);
		for (int i = 0; i < links.Count; i++)
		{
			ConnectLink(links[i]);
		}
	}

	private void ConnectLink(Toggle link)
	{
		link.onValueChanged.AddListener(LinkValueChanged);
	}

	private void DisconnectLink(Toggle link)
	{
		link.onValueChanged.RemoveListener(LinkValueChanged);
	}

	private void LinkValueChanged(bool isOn)
	{
		if (isOn)
		{
			toggle.isOn = false;
		}
	}

	private void SelfValueChanged(bool isOn)
	{
		if (isOn)
		{
			for (int i = 0; i < links.Count; i++)
			{
				links[i].isOn = false;
			}
		}
	}

	public void AddLink(Toggle link)
	{
		if (!links.Contains(link))
		{
			links.Add(link);
			ConnectLink(link);
		}
	}

	public void RemoveLink(Toggle link)
	{
		if (links.Contains(link))
		{
			DisconnectLink(link);
			links.Remove(link);
		}
	}

	public void ClearLinks()
	{
		for (int num = links.Count - 1; num >= 0; num--)
		{
			RemoveLink(links[num]);
		}
	}
}
