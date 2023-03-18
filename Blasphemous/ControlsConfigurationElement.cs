using Framework.Managers;
using Sirenix.OdinInspector;
using Tools.UI;
using UnityEngine;
using UnityEngine.UI;

public class ControlsConfigurationElement : MonoBehaviour
{
	public enum CONTROLS_CONFIGURATION_ELEMENT_MODES
	{
		UNSELECTED,
		SELECTED,
		EDITING,
		CONFLICT_UNSELECTED,
		CONFLICT_SELECTED,
		GREYEDOUT
	}

	public Text actionText;

	public Text conflictText;

	public InputIcon inputIcon;

	public Image inputIconImage;

	public Image editModeIcon;

	public Image selectedModeIcon;

	[SerializeField]
	[FoldoutGroup("Colors", false, 0)]
	public Color textColorUnselected;

	[SerializeField]
	[FoldoutGroup("Colors", false, 0)]
	public Color textColorSelected;

	[SerializeField]
	[FoldoutGroup("Colors", false, 0)]
	public Color textColorConflict;

	[SerializeField]
	[FoldoutGroup("Colors", false, 0)]
	public Color textColorEditing;

	[SerializeField]
	[FoldoutGroup("Colors", false, 0)]
	public Color textColorGreyedOut;

	[SerializeField]
	[FoldoutGroup("Audio", false, 0)]
	public string OnMoveAudio;

	[SerializeField]
	[FoldoutGroup("Audio", false, 0)]
	public string OnClickAudio;

	private string currentActionName;

	private string currentButtonKey;

	private int currentActionElementMapId;

	private CONTROLS_CONFIGURATION_ELEMENT_MODES currentMode;

	private void ChangeMode(CONTROLS_CONFIGURATION_ELEMENT_MODES newMode)
	{
		ExitMode(currentMode);
		currentMode = newMode;
		EnterMode(currentMode);
	}

	private void OnEnterSelected()
	{
		actionText.color = textColorSelected;
		Core.Audio.PlayOneShot(OnMoveAudio);
		selectedModeIcon.enabled = true;
		conflictText.enabled = false;
	}

	private void OnEnterUnselected()
	{
		actionText.color = textColorUnselected;
		selectedModeIcon.enabled = false;
		editModeIcon.enabled = false;
		inputIconImage.enabled = true;
		conflictText.enabled = false;
	}

	private void OnEnterEditing()
	{
		actionText.color = textColorEditing;
		Core.Audio.PlayOneShot(OnClickAudio);
		editModeIcon.enabled = true;
		inputIconImage.enabled = false;
		inputIcon.SetIconByButtonKey(string.Empty);
		ControlRemapManager.InputMappedEvent += OnAssignedButtonChanged;
		Core.ControlRemapManager.StartListeningInput(currentActionElementMapId);
	}

	private void OnEnterConflictSelected()
	{
		actionText.color = textColorConflict;
		Core.Audio.PlayOneShot(OnMoveAudio);
		selectedModeIcon.enabled = true;
		conflictText.enabled = true;
	}

	private void OnEnterConflictUnselected()
	{
		actionText.color = textColorConflict;
		selectedModeIcon.enabled = false;
		editModeIcon.enabled = false;
		inputIconImage.enabled = true;
		conflictText.enabled = true;
	}

	private void OnEnterGreyedOut()
	{
		actionText.color = textColorGreyedOut;
	}

	private void OnExitSelected()
	{
	}

	private void OnExitUnselected()
	{
	}

	private void OnExitEditing()
	{
		Core.Audio.PlayOneShot(OnClickAudio);
		editModeIcon.enabled = false;
		inputIconImage.enabled = true;
		inputIcon.SetIconByButtonKey(currentButtonKey);
		Core.ControlRemapManager.StopListeningInput();
	}

	private void OnExitConflictSelected()
	{
	}

	private void OnExitConflictUnselected()
	{
	}

	private void OnExitGreyedOut()
	{
	}

	private void EnterMode(CONTROLS_CONFIGURATION_ELEMENT_MODES m)
	{
		switch (m)
		{
		case CONTROLS_CONFIGURATION_ELEMENT_MODES.UNSELECTED:
			OnEnterUnselected();
			break;
		case CONTROLS_CONFIGURATION_ELEMENT_MODES.SELECTED:
			OnEnterSelected();
			break;
		case CONTROLS_CONFIGURATION_ELEMENT_MODES.EDITING:
			OnEnterEditing();
			break;
		case CONTROLS_CONFIGURATION_ELEMENT_MODES.CONFLICT_UNSELECTED:
			OnEnterConflictUnselected();
			break;
		case CONTROLS_CONFIGURATION_ELEMENT_MODES.CONFLICT_SELECTED:
			OnEnterConflictSelected();
			break;
		case CONTROLS_CONFIGURATION_ELEMENT_MODES.GREYEDOUT:
			OnEnterGreyedOut();
			break;
		}
	}

	private void ExitMode(CONTROLS_CONFIGURATION_ELEMENT_MODES m)
	{
		switch (m)
		{
		case CONTROLS_CONFIGURATION_ELEMENT_MODES.UNSELECTED:
			OnExitUnselected();
			break;
		case CONTROLS_CONFIGURATION_ELEMENT_MODES.SELECTED:
			OnExitSelected();
			break;
		case CONTROLS_CONFIGURATION_ELEMENT_MODES.EDITING:
			OnExitEditing();
			break;
		case CONTROLS_CONFIGURATION_ELEMENT_MODES.CONFLICT_UNSELECTED:
			OnExitConflictUnselected();
			break;
		case CONTROLS_CONFIGURATION_ELEMENT_MODES.CONFLICT_SELECTED:
			OnExitConflictSelected();
			break;
		case CONTROLS_CONFIGURATION_ELEMENT_MODES.GREYEDOUT:
			OnExitGreyedOut();
			break;
		}
	}

	public void Init(string actionName, string defaultButton, int actionElementMapId)
	{
		currentActionName = actionName;
		currentActionElementMapId = actionElementMapId;
		actionText.text = Core.ControlRemapManager.LocalizeActionName(currentActionName);
		SetButton(defaultButton);
	}

	public int GetCurrentActionElementMapId()
	{
		return currentActionElementMapId;
	}

	public string GetCurrentButtonKey()
	{
		return currentButtonKey;
	}

	public void SetButton(string button)
	{
		currentButtonKey = button;
		inputIcon.isControlsRemappingInputIcon = true;
		inputIcon.SetIconByButtonKey(currentButtonKey);
	}

	public void OnAssignedButtonChanged(string newButton, int actionElementMapId)
	{
		currentActionElementMapId = actionElementMapId;
		SetButton(newButton);
		ChangeMode(CONTROLS_CONFIGURATION_ELEMENT_MODES.SELECTED);
	}

	public void OnElementUnselected()
	{
		if (currentMode == CONTROLS_CONFIGURATION_ELEMENT_MODES.CONFLICT_SELECTED)
		{
			ChangeMode(CONTROLS_CONFIGURATION_ELEMENT_MODES.CONFLICT_UNSELECTED);
		}
		else
		{
			ChangeMode(CONTROLS_CONFIGURATION_ELEMENT_MODES.UNSELECTED);
		}
	}

	public void OnElementSelected()
	{
		if (currentMode == CONTROLS_CONFIGURATION_ELEMENT_MODES.CONFLICT_UNSELECTED)
		{
			ChangeMode(CONTROLS_CONFIGURATION_ELEMENT_MODES.CONFLICT_SELECTED);
		}
		else
		{
			ChangeMode(CONTROLS_CONFIGURATION_ELEMENT_MODES.SELECTED);
		}
	}

	public void OnElementToogleGreyOut()
	{
		if (currentMode == CONTROLS_CONFIGURATION_ELEMENT_MODES.UNSELECTED)
		{
			ChangeMode(CONTROLS_CONFIGURATION_ELEMENT_MODES.GREYEDOUT);
		}
		else if (currentMode == CONTROLS_CONFIGURATION_ELEMENT_MODES.GREYEDOUT)
		{
			ChangeMode(CONTROLS_CONFIGURATION_ELEMENT_MODES.UNSELECTED);
		}
	}

	public void OnEditPressed()
	{
		ChangeMode(CONTROLS_CONFIGURATION_ELEMENT_MODES.EDITING);
	}

	public void OnElementMarkedAsConflicting()
	{
		if (currentMode == CONTROLS_CONFIGURATION_ELEMENT_MODES.UNSELECTED)
		{
			ChangeMode(CONTROLS_CONFIGURATION_ELEMENT_MODES.CONFLICT_UNSELECTED);
		}
		else if (currentMode == CONTROLS_CONFIGURATION_ELEMENT_MODES.SELECTED)
		{
			ChangeMode(CONTROLS_CONFIGURATION_ELEMENT_MODES.CONFLICT_SELECTED);
		}
	}

	public void OnElementUnmarkedAsConflicting()
	{
		if (currentMode == CONTROLS_CONFIGURATION_ELEMENT_MODES.CONFLICT_UNSELECTED)
		{
			ChangeMode(CONTROLS_CONFIGURATION_ELEMENT_MODES.UNSELECTED);
		}
		else if (currentMode == CONTROLS_CONFIGURATION_ELEMENT_MODES.CONFLICT_SELECTED)
		{
			ChangeMode(CONTROLS_CONFIGURATION_ELEMENT_MODES.SELECTED);
		}
	}
}
