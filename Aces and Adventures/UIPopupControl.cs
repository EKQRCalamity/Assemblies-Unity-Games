using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(UIPopupFitter))]
public class UIPopupControl : MonoBehaviour
{
	private static readonly List<string> CancelButtonNames = new List<string> { "Cancel", "Discard Changes" };

	public RectTransform window;

	[SerializeField]
	private Button _rayCastBlocker;

	public bool closeOnRayCastBlockerClicked = true;

	[Header("Title Bar")]
	[SerializeField]
	private RectTransform _titleBarContainer;

	[SerializeField]
	private TextMeshProUGUI _titleText;

	[SerializeField]
	private bool _displayCloseButtonOnTitleBar = true;

	[SerializeField]
	private Button _titleCloseButton;

	public UnityEvent onClose;

	public Action onCloseAction;

	public IEnumerator delayClose;

	[Header("Main Content")]
	[SerializeField]
	private RectTransform _mainContentContainer;

	[Header("Button Bar")]
	[SerializeField]
	private List<string> _buttons = new List<string>();

	[SerializeField]
	private RectTransform _buttonsContainer;

	public StringEvent onButtonClicked;

	public UnityEvent[] onButtonIndexClicked;

	public bool closeOnButtonBarClicked = true;

	private RectTransform _rt;

	private UIPopupFitter _fitter;

	private CanvasInputFocus _inputFocus;

	private Button _returnKeyButton;

	public Vector2? referenceResolution { get; set; }

	public RectTransform mainContentContainer => _mainContentContainer;

	private RectTransform rt => _rt ?? (_rt = GetComponent<RectTransform>());

	public UIPopupFitter fitter => _fitter ?? (_fitter = GetComponent<UIPopupFitter>());

	private bool _shouldDisplayTitleBar
	{
		get
		{
			if (!_displayCloseButtonOnTitleBar)
			{
				return !_titleText.text.IsNullOrEmpty();
			}
			return true;
		}
	}

	private bool _shouldDisplayButtonBar => _buttons.Count > 0;

	private void _UpdateButtons()
	{
		_buttonsContainer.gameObject.DestroyChildren();
		if (!_shouldDisplayButtonBar)
		{
			_buttonsContainer.gameObject.SetActive(value: false);
			return;
		}
		_buttonsContainer.gameObject.SetActive(value: true);
		for (int i = 0; i < _buttons.Count; i++)
		{
			string text2 = _buttons[i];
			GameObject gameObject = UIUtil.GetGameObject("UI/Button Fitted");
			Button componentInChildren = gameObject.GetComponentInChildren<Button>(includeInactive: true);
			TextMeshProUGUI text = gameObject.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
			text.text = text2;
			if (CancelButtonNames.Contains(text2))
			{
				componentInChildren.targetGraphic.color = componentInChildren.targetGraphic.color.SetAlpha(0.957f);
			}
			else if (_buttons.Count <= 2)
			{
				_returnKeyButton = componentInChildren;
			}
			componentInChildren.onClick.AddListener(delegate
			{
				onButtonClicked.Invoke(text.text);
				if (closeOnButtonBarClicked)
				{
					Close();
				}
			});
			if (onButtonIndexClicked.Length > i && onButtonIndexClicked[i] != null)
			{
				int buttonIndex = i;
				componentInChildren.onClick.AddListener(delegate
				{
					onButtonIndexClicked[buttonIndex].Invoke();
				});
			}
			gameObject.transform.SetParent(_buttonsContainer, worldPositionStays: false);
		}
	}

	private void _UpdateTitleBar()
	{
		_titleBarContainer.gameObject.SetActive(_shouldDisplayTitleBar);
		_titleCloseButton.gameObject.SetActive(_displayCloseButtonOnTitleBar);
	}

	private void _OnRayCastBlockerClicked()
	{
		if (closeOnRayCastBlockerClicked)
		{
			Close();
		}
	}

	private IEnumerator _DelayClose()
	{
		InputManager.SetEventSystemEnabled(this, enabled: false);
		if (delayClose != null)
		{
			while (delayClose.MoveNext())
			{
				InputManager.SetEventSystemEnabled(this, !_inputFocus.HasFocus());
				yield return null;
			}
		}
		InputManager.SetEventSystemEnabled(this, enabled: true);
	}

	public void SetBlocksAllRayCasts(bool blockAllRayCasts)
	{
		_rayCastBlocker.gameObject.SetActive(blockAllRayCasts);
	}

	public void SetRayCastBlockerColor(Color color)
	{
		if (_rayCastBlocker != null)
		{
			_rayCastBlocker.GetComponent<Image>().color = color;
		}
	}

	public void SetFitterData(RectTransform sizeReference, Vector2 size, RectTransform centerReference, Vector2 center, Vector2 pivot)
	{
		fitter.sizeReference = sizeReference;
		fitter.size = size;
		fitter.centerReference = centerReference ?? (fitter.parentCanvas.transform as RectTransform);
		if (_rayCastBlocker != null)
		{
			_rayCastBlocker.GetComponent<UIPopupFitter>().centerReference = fitter.centerReference;
		}
		fitter.center = center;
		window.pivot = pivot;
	}

	public void SetTitle(string title)
	{
		_titleText.text = title;
		_UpdateTitleBar();
	}

	public void SetTitleData(string title, bool displayCloseButton)
	{
		_titleText.text = title;
		_displayCloseButtonOnTitleBar = displayCloseButton;
		_UpdateTitleBar();
	}

	public void SetDisplayCloseButton(bool display)
	{
		_displayCloseButtonOnTitleBar = display;
		_UpdateTitleBar();
	}

	public void Close()
	{
		onClose.Invoke();
		if (onCloseAction != null)
		{
			onCloseAction();
		}
		if (delayClose == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Job.Process(_DelayClose(), Department.UI).Immediately().Do(delegate
		{
			UnityEngine.Object.Destroy(base.gameObject);
		});
	}

	public void SetMainContent(GameObject mainContent)
	{
		_mainContentContainer.gameObject.DestroyChildren();
		if ((bool)mainContent)
		{
			mainContent.transform.SetParent(_mainContentContainer, worldPositionStays: false);
		}
	}

	public bool AddButton(string label)
	{
		return AddButtons(new string[1] { label });
	}

	public bool RemoveButton(string label)
	{
		return RemoveButtons(new string[1] { label });
	}

	public bool AddButtons(IEnumerable<string> labels)
	{
		bool flag = false;
		foreach (string label in labels)
		{
			flag |= _buttons.AddUnique(label);
		}
		if (flag)
		{
			_UpdateButtons();
		}
		return flag;
	}

	public bool RemoveButtons(IEnumerable<string> labels)
	{
		bool flag = false;
		foreach (string label in labels)
		{
			flag |= _buttons.Remove(label);
		}
		if (flag)
		{
			_UpdateButtons();
		}
		return flag;
	}

	public void SetButtons(IEnumerable<string> labels)
	{
		_buttons = labels.ToList();
		if (_buttons.Count > 1)
		{
			foreach (string cancelButtonName in CancelButtonNames)
			{
				if (_buttons.Remove(cancelButtonName))
				{
					if (ProfileManager.options.game.ui.cancelButtonOnLeft)
					{
						_buttons.Insert(0, cancelButtonName);
					}
					else
					{
						_buttons.Add(cancelButtonName);
					}
					break;
				}
			}
		}
		_UpdateButtons();
	}

	public void ClearButtons()
	{
		if (_buttons.Count != 0)
		{
			_buttons.Clear();
			_UpdateButtons();
		}
	}

	public UIPopupControl SetButtonResponses(params KeyValuePair<string, Action>[] buttonResponses)
	{
		_buttons.Clear();
		for (int i = 0; i < buttonResponses.Length; i++)
		{
			string buttonName = buttonResponses[i].Key;
			_buttons.Add(buttonName);
			Action buttonResponse = buttonResponses[i].Value;
			if (buttonResponse != null)
			{
				onButtonClicked.AddListener(delegate(string s)
				{
					if (s == buttonName)
					{
						buttonResponse();
					}
				});
				continue;
			}
			onButtonClicked.AddListener(delegate(string s)
			{
				if (s == buttonName)
				{
					Close();
				}
			});
		}
		_UpdateButtons();
		return this;
	}

	public void SimulateButtonClick(string buttonName)
	{
		onButtonClicked.Invoke(buttonName);
		if (closeOnButtonBarClicked)
		{
			Close();
		}
	}

	private void Awake()
	{
		_UpdateButtons();
		_rayCastBlocker.onClick.AddListener(_OnRayCastBlockerClicked);
		_titleCloseButton.onClick.AddListener(Close);
		_inputFocus = GetComponent<CanvasInputFocus>();
		if ((bool)GetComponent<Canvas>())
		{
			GetComponent<Canvas>().overridePixelPerfect = true;
		}
	}

	private void Start()
	{
		if (referenceResolution.HasValue)
		{
			window.localScale = window.localScale.Multiply(GetComponentInParent<CanvasScaler>().referenceResolution.Multiply(referenceResolution.Value.Inverse()).Unproject(AxisType.Z, 1f));
		}
	}

	private void OnEnable()
	{
		InputManager.RequestInput(this);
	}

	private void OnDisable()
	{
		InputManager.ReleaseInput(this);
	}

	private void Update()
	{
		if (_inputFocus.HasFocus())
		{
			if (_displayCloseButtonOnTitleBar && InputManager.I[this][KeyAction.Pause, KeyAction.Back, KState.Clicked])
			{
				Close();
			}
			else if ((bool)_returnKeyButton && InputManager.I[KeyCode.Return][KState.Clicked])
			{
				_returnKeyButton.onClick.Invoke();
			}
		}
	}
}
