using System.Collections;
using System.Collections.Generic;
using Framework.Managers;
using Rewired;
using UnityEngine;

public class ControlsMenuScreen : BaseMenuScreen
{
	public RectTransform contentTransform;

	public GameObject navigationButtonsRoot;

	public GameObject remappingLeyendButtonsRoot;

	public GameObject editLeyendButton;

	public GameObject acceptLeyendButton;

	public GameObject restoreDefaultsLeyendButton;

	public GameObject saveAndExitLeyendButton;

	public GameObject cancelLeyendButton;

	public GameObject leftStickDisclaimer;

	private Player rewiredPlayer;

	private CanvasGroup canvasGroup;

	private List<ControlsConfigurationElement> elements;

	private int index;

	private bool editing;

	private int framesSkipped;

	private bool initialized;

	private float delaySecondsForFastScroll = 0.5f;

	private int skippedFramesForFastScroll = 5;

	private int skippedFramesForSlowScroll = 10;

	private float axisThreshold = 0.3f;

	private int maxNumberOfRowsShown = 6;

	private int indexOfFirstElementShown = -1;

	private int indexOfLastElementShown = -1;

	private int currentOffset;

	private List<string> actionNames = new List<string>();

	public bool currentlyActive => base.gameObject.activeInHierarchy && canvasGroup != null && canvasGroup.alpha == 1f;

	public override void Open()
	{
		base.Open();
		base.gameObject.SetActive(value: true);
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 1f;
		elements = new List<ControlsConfigurationElement>();
		foreach (RectTransform item in contentTransform)
		{
			ControlsConfigurationElement component = item.GetComponent<ControlsConfigurationElement>();
			component.OnElementUnselected();
			elements.Add(component);
		}
		index = 0;
		indexOfFirstElementShown = 0;
		indexOfLastElementShown = maxNumberOfRowsShown - 1;
		currentOffset = 0;
		contentTransform.anchoredPosition = new Vector2(0f, 0f);
		Init();
		UpdateAllElements();
		elements[index].OnElementSelected();
		OnOpen();
	}

	private void Init()
	{
		if (!initialized)
		{
			rewiredPlayer = ReInput.players.GetPlayer(0);
			actionNames = Core.ControlRemapManager.GetAllActionNamesInOrder();
			initialized = true;
		}
	}

	private void UpdateAllElements()
	{
		UnmarkPreviousConflictingActions();
		bool flag = false;
		Dictionary<string, ActionElementMap> rebindeableAemsByActionName = Core.ControlRemapManager.GetRebindeableAemsByActionName(actionNames);
		if (rebindeableAemsByActionName.Keys.Count != elements.Count)
		{
			Debug.Log("ControlsMenuScreen: UpdateAllElements: The number of elements isn't equal to the number of actions!");
			flag = true;
		}
		if (!flag)
		{
			int num = 0;
			foreach (string key in rebindeableAemsByActionName.Keys)
			{
				if (rebindeableAemsByActionName[key] == null)
				{
					Debug.Log("ControlsMenuScreen: UpdateAllElements: it seems that action '" + key + "' wasn't present in a previous configuration of the game.");
					flag = true;
					break;
				}
				elements[num].Init(key, rebindeableAemsByActionName[key].elementIdentifierName, rebindeableAemsByActionName[key].id);
				num++;
			}
		}
		if (!flag)
		{
			return;
		}
		Core.ControlRemapManager.RestoreDefaultMaps();
		rebindeableAemsByActionName = Core.ControlRemapManager.GetRebindeableAemsByActionName(actionNames);
		int num2 = 0;
		foreach (string key2 in rebindeableAemsByActionName.Keys)
		{
			elements[num2].Init(key2, rebindeableAemsByActionName[key2].elementIdentifierName, rebindeableAemsByActionName[key2].id);
			num2++;
		}
	}

	public bool TryClose()
	{
		if (!editing)
		{
			if (Core.ControlRemapManager.CountConflictingActions() == 0)
			{
				Core.ControlRemapManager.WriteControlsSettingsToFile();
			}
			else
			{
				Core.ControlRemapManager.LoadKeyboardAndMouseControlsSettingsFromFile();
				if (Core.Input.ActiveController.type == ControllerType.Joystick)
				{
					Core.ControlRemapManager.LoadJoystickControlsSettingsFromFile((Joystick)Core.Input.ActiveController);
				}
			}
			Close();
		}
		return !editing;
	}

	public override void Close()
	{
		base.Close();
		canvasGroup.alpha = 0f;
		OnClose();
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		Core.Input.JoystickPressed += UpdateAllElements;
		Core.Input.KeyboardPressed += UpdateAllElements;
		navigationButtonsRoot.SetActive(value: false);
		remappingLeyendButtonsRoot.SetActive(value: true);
		saveAndExitLeyendButton.SetActive(value: true);
		cancelLeyendButton.SetActive(value: false);
		editLeyendButton.SetActive(value: true);
		acceptLeyendButton.SetActive(value: false);
		restoreDefaultsLeyendButton.SetActive(value: true);
		leftStickDisclaimer.SetActive(value: false);
	}

	private void Update()
	{
		if (!initialized)
		{
			return;
		}
		if (rewiredPlayer.GetButtonDown(50))
		{
			ProcessSubmitInput();
			return;
		}
		if (rewiredPlayer.GetButtonDown(52))
		{
			ProcessRestoreDefaultInput();
			return;
		}
		float axisRaw = rewiredPlayer.GetAxisRaw(49);
		if (Mathf.Abs(axisRaw) > axisThreshold)
		{
			ProcessScrollInput(axisRaw);
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ProcessSubmitInput();
		}
	}

	private void ProcessSubmitInput()
	{
		editing = !editing;
		for (int i = 0; i < elements.Count; i++)
		{
			if (i == index)
			{
				if (editing)
				{
					elements[i].OnEditPressed();
					ControlRemapManager.InputMappedEvent += ProcessAssignmentInput;
				}
				else
				{
					elements[i].OnElementSelected();
				}
			}
			else if (Core.Input.ActiveControllerType != ControllerType.Joystick || (Core.Input.ActiveControllerType == ControllerType.Joystick && editing))
			{
				elements[i].OnElementToogleGreyOut();
			}
		}
		editLeyendButton.SetActive(!editing);
		restoreDefaultsLeyendButton.SetActive(!editing);
		if (Core.Input.ActiveControllerType == ControllerType.Joystick)
		{
			acceptLeyendButton.SetActive(value: false);
			leftStickDisclaimer.SetActive(editing);
		}
		else
		{
			acceptLeyendButton.SetActive(editing);
			leftStickDisclaimer.SetActive(value: false);
		}
		if (!editing)
		{
			bool flag = Core.ControlRemapManager.CountConflictingActions() == 0;
			saveAndExitLeyendButton.SetActive(!flag);
			cancelLeyendButton.SetActive(flag);
		}
		else
		{
			saveAndExitLeyendButton.SetActive(value: false);
			cancelLeyendButton.SetActive(value: false);
		}
	}

	private void ProcessRestoreDefaultInput()
	{
		if (!editing)
		{
			Core.ControlRemapManager.RestoreDefaultMaps();
			UpdateAllElements();
			UnmarkPreviousConflictingActions();
			saveAndExitLeyendButton.SetActive(value: true);
			cancelLeyendButton.SetActive(value: false);
		}
	}

	private void ProcessScrollInput(float scrollAxis)
	{
		float axisRawPrev = rewiredPlayer.GetAxisRawPrev(49);
		if (axisRawPrev == 0f)
		{
			framesSkipped = 0;
			if (!editing)
			{
				CalculateNewIndex(scrollAxis);
			}
			return;
		}
		float axisTimeActive = rewiredPlayer.GetAxisTimeActive(49);
		int num = ((!(axisTimeActive > delaySecondsForFastScroll)) ? skippedFramesForSlowScroll : skippedFramesForFastScroll);
		framesSkipped++;
		if (framesSkipped % num == 0)
		{
			framesSkipped = 0;
			if (!editing)
			{
				CalculateNewIndex(scrollAxis);
			}
		}
	}

	private void CalculateNewIndex(float scrollAxis)
	{
		int num = index;
		num = ((!(scrollAxis > 0f)) ? (num + 1) : (num - 1));
		num = Mathf.Clamp(num, 0, elements.Count - 1);
		if (num != index)
		{
			UpdateIndexAndElements(num);
		}
	}

	private void ProcessAssignmentInput(string buttonName, int actionElementMapId)
	{
		for (int i = 0; i < elements.Count; i++)
		{
			if (i != index)
			{
				elements[i].OnElementToogleGreyOut();
			}
		}
		Dictionary<int, string> allCurrentConflictingButtonsByAemId = Core.ControlRemapManager.GetAllCurrentConflictingButtonsByAemId();
		bool flag = allCurrentConflictingButtonsByAemId.Keys.Count > 0;
		UnmarkPreviousConflictingActions();
		if (flag)
		{
			MarkCurrentConflictingActions(allCurrentConflictingButtonsByAemId);
		}
		saveAndExitLeyendButton.SetActive(!flag);
		cancelLeyendButton.SetActive(flag);
		editLeyendButton.SetActive(value: true);
		acceptLeyendButton.SetActive(value: false);
		restoreDefaultsLeyendButton.SetActive(value: true);
		leftStickDisclaimer.SetActive(value: false);
		StartCoroutine(ProcessAssignmentInputAtEndOfFrame(editing: false));
	}

	private IEnumerator ProcessAssignmentInputAtEndOfFrame(bool editing)
	{
		yield return new WaitForEndOfFrame();
		this.editing = editing;
	}

	private void MarkCurrentConflictingActions(Dictionary<int, string> currentConflictingActionsAndKeys)
	{
		foreach (int key in currentConflictingActionsAndKeys.Keys)
		{
			foreach (ControlsConfigurationElement element in elements)
			{
				if (element.GetCurrentActionElementMapId().Equals(key) && element.GetCurrentButtonKey().Equals(currentConflictingActionsAndKeys[key]))
				{
					element.OnElementMarkedAsConflicting();
				}
			}
		}
	}

	private void UnmarkPreviousConflictingActions()
	{
		foreach (ControlsConfigurationElement element in elements)
		{
			element.OnElementUnmarkedAsConflicting();
		}
	}

	private void UpdateIndexAndElements(int i)
	{
		elements[index].OnElementUnselected();
		index = i;
		elements[index].OnElementSelected();
		Vector2 anchoredPosition = elements[index].GetComponent<RectTransform>().anchoredPosition;
		float offsetByIndex = GetOffsetByIndex(i);
		contentTransform.anchoredPosition = new Vector2(0f, offsetByIndex);
	}

	private float GetOffsetByIndex(int index)
	{
		if (index > indexOfLastElementShown)
		{
			indexOfFirstElementShown++;
			indexOfLastElementShown++;
			currentOffset = (maxNumberOfRowsShown - index - 1) * -30;
		}
		else if (index < indexOfFirstElementShown)
		{
			indexOfFirstElementShown--;
			indexOfLastElementShown--;
			currentOffset = index * 30;
		}
		return currentOffset;
	}

	protected override void OnClose()
	{
		Core.Input.JoystickPressed -= UpdateAllElements;
		Core.Input.KeyboardPressed -= UpdateAllElements;
		navigationButtonsRoot.SetActive(value: true);
		remappingLeyendButtonsRoot.SetActive(value: false);
		base.gameObject.SetActive(value: false);
		base.OnClose();
	}
}
