using Framework.Managers;
using Framework.Util;
using Gameplay.UI.Others.Screen;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.UI.Widgets;

public class EndScreenWidget : UIWidget
{
	public static EndScreenWidget instance;

	public const string THE_LAST_DOOR_URL = "https://thelastdoor.com/";

	private InputField emailInputField;

	private EndScreenBody endScreenBody;

	[SerializeField]
	protected GameObject congratsText;

	[SerializeField]
	protected GameObject endMenuFirstSelected;

	private void Awake()
	{
		instance = this;
		emailInputField = GetComponentInChildren<InputField>(includeInactive: true);
		endScreenBody = GetComponentInChildren<EndScreenBody>(includeInactive: true);
	}

	public void LoadMainMenu()
	{
		Core.Logic.LoadMenuScene();
	}

	private string getInputText()
	{
		string result = string.Empty;
		if (emailInputField != null)
		{
			result = emailInputField.text;
		}
		return result;
	}

	private void emptyInputText()
	{
		if (emailInputField != null && !string.IsNullOrEmpty(emailInputField.text))
		{
			emailInputField.text = string.Empty;
		}
	}

	public void StoreEmail()
	{
		string inputText = getInputText();
		if (!string.IsNullOrEmpty(inputText))
		{
			HandleTextFile.WriteString(inputText);
			emptyInputText();
		}
	}

	public void VisitBlasphemousKickstarterPage()
	{
		openURL("https://thelastdoor.com/");
	}

	private void openURL(string url)
	{
		try
		{
			Application.OpenURL(url);
		}
		catch (UnityException ex)
		{
			Debug.LogError(ex.Message);
		}
	}

	public void EnableEndSceneBody()
	{
		if (congratsText != null && !congratsText.gameObject.activeSelf)
		{
			congratsText.gameObject.SetActive(value: true);
		}
		if (!endScreenBody.gameObject.activeSelf)
		{
			endScreenBody.gameObject.SetActive(value: true);
			setEndMenuButtonsSelectables();
		}
	}

	private void setEndMenuButtonsSelectables()
	{
		if (endMenuFirstSelected != null)
		{
			EventSystem.current.SetSelectedGameObject(endMenuFirstSelected);
		}
	}
}
