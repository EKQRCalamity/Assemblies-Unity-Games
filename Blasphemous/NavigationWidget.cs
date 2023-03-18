using Framework.Managers;
using Gameplay.UI.Widgets;
using Sirenix.OdinInspector;
using Tools.Level.Interactables;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NavigationWidget : UIWidget
{
	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private GameObject buttonPrefab;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private Transform list;

	public int ButtonFontSize = 10;

	private GameObject firstButton;

	private Button previousButton;

	private void Awake()
	{
		Show(b: false);
	}

	public void Show(bool b)
	{
		if (b)
		{
			GenerateList();
		}
		if ((bool)Core.Logic.CurrentLevelConfig)
		{
			Core.Logic.CurrentLevelConfig.TimeScale = ((!b) ? 1 : 0);
		}
		for (int i = 0; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).gameObject.SetActive(b);
		}
	}

	private void GenerateList()
	{
		PrieDieu[] array = Object.FindObjectsOfType<PrieDieu>();
		Door[] array2 = Object.FindObjectsOfType<Door>();
		firstButton = null;
		previousButton = null;
		if (array.Length > 0 || array2.Length > 0)
		{
			DestroyList();
		}
		for (int i = 0; i < array.Length; i++)
		{
			CreateButton(array[i].name, array[i].transform.position);
		}
		for (int j = 0; j < array2.Length; j++)
		{
			CreateButton(array2[j].name, array2[j].spawnPoint.position);
		}
		if ((bool)firstButton)
		{
			EventSystem.current.SetSelectedGameObject(firstButton);
			firstButton.GetComponent<Button>().OnSelect(new BaseEventData(EventSystem.current));
		}
	}

	private void DestroyList()
	{
		firstButton = null;
		for (int i = 0; i < list.childCount; i++)
		{
			Object.Destroy(list.GetChild(i).gameObject);
		}
	}

	private void CreateButton(string identificativeName, Vector3 position)
	{
		GameObject gameObject = Object.Instantiate(buttonPrefab);
		gameObject.transform.SetParent(list);
		gameObject.transform.localScale = Vector3.one;
		Text componentInChildren = gameObject.GetComponentInChildren<Text>();
		NavigationButton componentInChildren2 = gameObject.GetComponentInChildren<NavigationButton>();
		componentInChildren2.destination = position;
		componentInChildren.text = identificativeName;
		componentInChildren.fontSize = ButtonFontSize;
		Button componentInChildren3 = gameObject.GetComponentInChildren<Button>();
		if (!firstButton)
		{
			firstButton = gameObject;
		}
		if ((bool)previousButton)
		{
			Navigation navigation = previousButton.navigation;
			navigation.selectOnDown = componentInChildren3;
			previousButton.navigation = navigation;
			navigation = componentInChildren3.navigation;
			navigation.selectOnUp = previousButton;
			componentInChildren3.navigation = navigation;
		}
		previousButton = componentInChildren3;
	}
}
