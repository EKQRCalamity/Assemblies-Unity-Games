using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.Disclaimer;

public class DisclaimerText : MonoBehaviour
{
	private Text disclaimerTextComponent;

	private string disclaimerText;

	private void Awake()
	{
		disclaimerTextComponent = GetComponent<Text>();
		if ((bool)disclaimerTextComponent && !Debug.isDebugBuild)
		{
			disclaimerTextComponent.enabled = false;
		}
	}

	private void Start()
	{
		if (disclaimerTextComponent == null)
		{
			return;
		}
		disclaimerText = disclaimerTextComponent.text;
		if (string.IsNullOrEmpty(disclaimerText))
		{
			return;
		}
		try
		{
			string newValue = string.Format("{0} - {1}", "Steam", Application.platform);
			disclaimerText = disclaimerText.Replace("%1", "v." + Application.version);
			disclaimerText = disclaimerText.Replace("%2", newValue);
			disclaimerText = disclaimerText.Replace("%3", "October 2021");
			disclaimerTextComponent.text = disclaimerText;
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message);
		}
	}
}
