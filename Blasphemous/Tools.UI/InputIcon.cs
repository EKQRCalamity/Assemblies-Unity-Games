using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core.Surrogates;
using Framework.Managers;
using Rewired;
using RewiredConsts;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.UI;

[SelectionBase]
public class InputIcon : MonoBehaviour
{
	public enum AxisCheck
	{
		None,
		Positive,
		Negative
	}

	private const string GENERIC_ICON_LAYOUT_PATH = "Input/GenericIconLayout";

	private const string XBOX_ICON_LAYOUT_PATH = "Input/XBOXIconLayout";

	private const string PLAYSTATION_ICON_LAYOUT_PATH = "Input/PSIconLayout";

	private const string KEYBOARD_ICON_LAYOUT_PATH = "Input/KeyboardIconLayout";

	private static readonly Dictionary<string, InputIconLayout> CachedLayouts = new Dictionary<string, InputIconLayout>();

	[BoxGroup("Design Settings", true, false, 0)]
	[ActionIdProperty(typeof(RewiredConsts.Action))]
	[InfoBox("Define the icon layout in the config files placed at Assets/Design/Resources/Input", InfoMessageType.Info, null)]
	public int action;

	[BoxGroup("Design Settings", true, false, 0)]
	public AxisCheck axisCheck;

	[SerializeField]
	[FoldoutGroup("Attached References", false, 0)]
	[HideIf("InsideCanvas", true)]
	private SpriteRenderer gpIcon;

	[SerializeField]
	[FoldoutGroup("Attached References", false, 0)]
	[HideIf("InsideCanvas", true)]
	private TextMesh gpText;

	public bool isControlsRemappingInputIcon;

	[SerializeField]
	[FoldoutGroup("Attached References", false, 0)]
	[ShowIf("InsideCanvas", true)]
	private Image uiIcon;

	[SerializeField]
	[FoldoutGroup("Attached References", false, 0)]
	[ShowIf("InsideCanvas", true)]
	private Text uiText;

	private bool InsideCanvas
	{
		get
		{
			RectTransform component = GetComponent<RectTransform>();
			return component != null;
		}
	}

	private void Awake()
	{
		if (gpIcon != null)
		{
			gpIcon.sprite = null;
		}
		if (uiIcon != null)
		{
			uiIcon.sprite = null;
		}
	}

	private void Start()
	{
		Core.Input.JoystickPressed += ActiveInputChanged;
		Core.Input.KeyboardPressed += ActiveInputChanged;
		if (gpText != null)
		{
			gpText.GetComponent<Renderer>().sortingLayerName = "In-Game UI";
		}
		if (!isControlsRemappingInputIcon)
		{
			RefreshIcon();
		}
	}

	private void OnDestroy()
	{
		Core.Input.JoystickPressed -= ActiveInputChanged;
		Core.Input.KeyboardPressed -= ActiveInputChanged;
	}

	public void RefreshIcon()
	{
		if (!ReInput.isReady)
		{
			return;
		}
		if (action == -1)
		{
			RefreshBlankIcon();
			return;
		}
		Rewired.Player player = ReInput.players.GetPlayer(0);
		InputAction inputAction = ReInput.mapping.GetAction(action);
		ActionElementMap actionElementMap = null;
		if (inputAction != null)
		{
			AxisRange axisRange = ((axisCheck == AxisCheck.Positive) ? AxisRange.Positive : AxisRange.Negative);
			actionElementMap = Core.ControlRemapManager.FindLastElementMapByInputAction(inputAction, axisRange, Core.Input.ActiveController);
		}
		if (actionElementMap != null)
		{
			SetIconByButtonKey(actionElementMap.elementIdentifierName);
		}
	}

	private void RefreshBlankIcon()
	{
		InputIconLayout inputIconLayout = FindIconLayout();
		ButtonDescription buttonDescription = default(ButtonDescription);
		buttonDescription.text = string.Empty;
		buttonDescription.icon = inputIconLayout.defaultIcon;
		Sprite icon = buttonDescription.icon;
		string text = buttonDescription.text;
		if (!InsideCanvas)
		{
			if ((bool)gpText)
			{
				gpText.text = text;
			}
			if ((bool)gpIcon)
			{
				gpIcon.sprite = icon;
			}
			return;
		}
		if ((bool)uiIcon)
		{
			uiIcon.sprite = icon;
			uiIcon.SetNativeSize();
		}
		if ((bool)uiText)
		{
			uiText.text = text;
		}
	}

	public static ButtonDescription GetButtonDescriptionByButtonKey(string buttonName)
	{
		bool buttonsWithText;
		InputIconLayout inputIconLayout = FindIconLayout(out buttonsWithText);
		ButtonDescription result = default(ButtonDescription);
		ButtonDescription[] array = Array.FindAll(inputIconLayout.buttons, (ButtonDescription x) => string.Equals(x.button, buttonName, StringComparison.CurrentCultureIgnoreCase));
		if (array != null && array.Length > 0)
		{
			result = array[array.Length - 1];
			result.text = ((!(result.text == "-")) ? result.text : string.Empty);
		}
		else if (buttonsWithText)
		{
			result.text = buttonName;
			result.icon = inputIconLayout.defaultIcon;
		}
		else
		{
			result.text = ((buttonName.Length <= 1) ? buttonName : (buttonName[buttonName.Length - 1] + string.Empty));
			result.icon = inputIconLayout.defaultIcon;
		}
		return result;
	}

	public void SetIconByButtonKey(string buttonName)
	{
		bool buttonsWithText;
		InputIconLayout inputIconLayout = FindIconLayout(out buttonsWithText);
		ButtonDescription buttonDescriptionByButtonKey = GetButtonDescriptionByButtonKey(buttonName);
		Sprite icon = buttonDescriptionByButtonKey.icon;
		string text = buttonDescriptionByButtonKey.text;
		if (!InsideCanvas)
		{
			if ((bool)gpText)
			{
				gpText.text = text;
			}
			if ((bool)gpIcon)
			{
				gpIcon.sprite = icon;
			}
			return;
		}
		if ((bool)uiIcon)
		{
			uiIcon.sprite = icon;
			uiIcon.SetNativeSize();
		}
		if ((bool)uiText)
		{
			uiText.text = text;
			uiText.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 2f, uiText.transform.localPosition.z);
		}
	}

	public void Fade(float value, float time)
	{
		if (InsideCanvas)
		{
			Image[] componentsInChildren = GetComponentsInChildren<Image>();
			Text[] componentsInChildren2 = GetComponentsInChildren<Text>();
			componentsInChildren.ForEach(delegate(Image render)
			{
				render.DOFade(value, time);
			});
			componentsInChildren2.ForEach(delegate(Text text)
			{
				text.DOFade(value, time);
			});
			return;
		}
		SpriteRenderer[] componentsInChildren3 = GetComponentsInChildren<SpriteRenderer>();
		TextMesh[] componentsInChildren4 = GetComponentsInChildren<TextMesh>();
		componentsInChildren3.ForEach(delegate(SpriteRenderer render)
		{
			render.DOFade(value, time);
		});
		componentsInChildren4.ForEach(delegate(TextMesh text)
		{
			DOTween.To(() => text.color, delegate(ColorWrapper x)
			{
				text.color = x;
			}, new Color(1f, 1f, 1f, value), time);
		});
	}

	private static InputIconLayout FindIconLayout(out bool buttonsWithText)
	{
		buttonsWithText = false;
		JoystickType activeJoystickModel = Core.Input.ActiveJoystickModel;
		ControllerType activeControllerType = Core.Input.ActiveControllerType;
		string text = "Input/GenericIconLayout";
		if (activeControllerType == ControllerType.Keyboard)
		{
			buttonsWithText = true;
			text = "Input/KeyboardIconLayout";
		}
		else if (activeControllerType == ControllerType.Joystick && activeJoystickModel == JoystickType.PlayStation)
		{
			text = "Input/PSIconLayout";
		}
		else if (activeControllerType == ControllerType.Joystick && activeJoystickModel == JoystickType.XBOX)
		{
			text = "Input/XBOXIconLayout";
		}
		else if (activeControllerType == ControllerType.Joystick && activeJoystickModel == JoystickType.Generic)
		{
			text = "Input/GenericIconLayout";
		}
		if (!CachedLayouts.ContainsKey(text))
		{
			CachedLayouts[text] = Resources.Load<InputIconLayout>(text);
		}
		return CachedLayouts[text];
	}

	private InputIconLayout FindIconLayout()
	{
		JoystickType activeJoystickModel = Core.Input.ActiveJoystickModel;
		ControllerType activeControllerType = Core.Input.ActiveControllerType;
		string text = "Input/XBOXIconLayout";
		if (activeControllerType == ControllerType.Keyboard)
		{
			text = "Input/KeyboardIconLayout";
		}
		else if (activeControllerType == ControllerType.Joystick && activeJoystickModel == JoystickType.PlayStation)
		{
			text = "Input/PSIconLayout";
		}
		else if (activeControllerType == ControllerType.Joystick && activeJoystickModel == JoystickType.XBOX)
		{
			text = "Input/XBOXIconLayout";
		}
		else if (activeControllerType == ControllerType.Joystick && activeJoystickModel == JoystickType.Generic)
		{
			text = "Input/GenericIconLayout";
		}
		if (!CachedLayouts.ContainsKey(text))
		{
			CachedLayouts[text] = Resources.Load<InputIconLayout>(text);
		}
		return CachedLayouts[text];
	}

	private void ActiveInputChanged()
	{
		if (!isControlsRemappingInputIcon)
		{
			RefreshIcon();
		}
	}
}
