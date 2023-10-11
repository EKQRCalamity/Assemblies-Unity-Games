using TMPro;
using UnityEngine;

public class KeybindInputView : MonoBehaviour
{
	[Header("Events")]
	[SerializeField]
	protected KeyCodeEvent _OnKeyCodeChange;

	[SerializeField]
	protected BoolEvent _OnIsBoundChange;

	[SerializeField]
	protected BoolEvent _OnIsListeningChange;

	[SerializeField]
	protected StringEvent _OnMessageChange;

	private KeyCode? _keyCode;

	private bool _listening;

	private bool _messageDirty;

	public KeyCodeEvent OnKeyCodeChange => _OnKeyCodeChange ?? (_OnKeyCodeChange = new KeyCodeEvent());

	public BoolEvent OnIsBoundChange => _OnIsBoundChange ?? (_OnIsBoundChange = new BoolEvent());

	public BoolEvent OnIsListeningChange => _OnIsListeningChange ?? (_OnIsListeningChange = new BoolEvent());

	public StringEvent OnMessageChange => _OnMessageChange ?? (_OnMessageChange = new StringEvent());

	public KeyCode? keyCode
	{
		get
		{
			return _keyCode;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _keyCode, value))
			{
				_OnKeyCodeValueChange();
			}
		}
	}

	public bool listening
	{
		get
		{
			return _listening;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _listening, value))
			{
				_OnIsListeningValueChange();
			}
		}
	}

	private bool _canUnbind
	{
		get
		{
			if (keyCode.HasValue)
			{
				return !listening;
			}
			return false;
		}
	}

	private void _OnKeyCodeValueChange()
	{
		OnKeyCodeChange.Invoke(keyCode);
		_messageDirty = true;
	}

	private void _OnIsListeningValueChange()
	{
		OnIsListeningChange.Invoke(_listening);
		InputManager.RegisterInput(_listening, this);
		if (_listening)
		{
			InputManager.I.OnKeyClicked += _OnKeyClicked;
		}
		else if ((bool)InputManager.I)
		{
			InputManager.I.OnKeyClicked -= _OnKeyClicked;
		}
		_messageDirty = true;
	}

	private void _UpdateMessage()
	{
		_messageDirty = false;
		if (listening)
		{
			OnMessageChange.Invoke(MessageData.UIPopupButton.PressDesiredKey.GetButton().Localize());
		}
		else if (keyCode.HasValue)
		{
			OnMessageChange.Invoke(EnumUtil.FriendlyName(keyCode.Value));
		}
		else
		{
			OnMessageChange.Invoke(MessageData.UIPopupButton.LeftClickToBeginBinding.GetButton().Localize());
		}
		GetComponent<TooltipVisibility>().enabled = _canUnbind;
		OnIsBoundChange.Invoke(_canUnbind);
	}

	private void _OnKeyClicked(KeyCode clickedKeyCode)
	{
		if (ProfileOptions.ControlOptions.ReservedKeyCodes.Contains(clickedKeyCode))
		{
			string title = MessageData.UIPopupTitle.ReservedKey.GetTitle().Localize();
			GameObject mainContent = UIUtil.CreateMessageBox(MessageData.UIPopupMessage.ReservedKey.GetMessage().SetArguments(EnumUtil.FriendlyName(clickedKeyCode)).Localize());
			Transform parent = base.transform;
			UIUtil.CreatePopup(title, mainContent, null, null, null, null, null, null, true, true, null, null, null, parent, null, null);
		}
		else if (keyCode != clickedKeyCode && ProfileManager.controls.keyBinds[clickedKeyCode] != null)
		{
			string title2 = MessageData.UIPopupTitle.KeyAlreadyBound.GetTitle().Localize();
			GameObject mainContent2 = UIUtil.CreateMessageBox(MessageData.UIPopupMessage.KeyAlreadyBound.GetMessage().SetArguments(EnumUtil.FriendlyName(clickedKeyCode), EnumUtil.FriendlyName(ProfileManager.controls.keyBinds[clickedKeyCode].action)).Localize());
			Transform parent = base.transform;
			UIUtil.CreatePopup(title2, mainContent2, null, null, null, null, null, null, true, true, null, null, null, parent, null, null);
		}
		else
		{
			keyCode = clickedKeyCode;
			listening = false;
		}
	}

	private void Awake()
	{
		TooltipCreator.CreateTextTooltip(base.transform, MessageData.UIPopupMessage.RightClickToUnbind.GetMessage().Localize(), beginShowTimer: false, 0.2f, backgroundEnabled: true, TextAlignmentOptions.Center, 0f, 12f, TooltipDirection.Vertical);
		if (!keyCode.HasValue)
		{
			_messageDirty = true;
		}
	}

	private void OnDisable()
	{
		listening = false;
	}

	private void LateUpdate()
	{
		if (_messageDirty)
		{
			_UpdateMessage();
		}
	}

	public void Unbind()
	{
		if (_canUnbind)
		{
			keyCode = null;
		}
	}
}
