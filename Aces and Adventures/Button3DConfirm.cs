using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Button3DConfirm : Button3D
{
	[Header("Confirm================================================================================================")]
	public bool unconfirmOnPointerExit = true;

	[SerializeField]
	protected Transform _confirm;

	[SerializeField]
	protected Button3DSoundPack _confirmSoundPack;

	[SerializeField]
	protected ToggleAnimator3DSoundPack _toggleSoundPack;

	[SerializeField]
	protected UnityEvent _OnConfirmClick;

	private Transform _confirmSlot;

	private bool _inConfirmState;

	private Button3DSoundPack _defaultSoundPack;

	private TransformTarget _confirmVelocity;

	private Quaternion _confirmTargetOriginalRotation;

	private PooledAudioPlayer _audioPlayer;

	private bool _cursorOver;

	private Transform confirm
	{
		get
		{
			if (!_confirm)
			{
				return _confirm = _rest;
			}
			return _confirm;
		}
	}

	private Button3DSoundPack confirmSoundPack
	{
		get
		{
			Button3DSoundPack button3DSoundPack = _confirmSoundPack;
			if ((object)button3DSoundPack == null)
			{
				Button3DSoundPack obj = base.soundPack ?? ScriptableObject.CreateInstance<Button3DSoundPack>();
				Button3DSoundPack button3DSoundPack2 = obj;
				_confirmSoundPack = obj;
				button3DSoundPack = button3DSoundPack2;
			}
			return button3DSoundPack;
		}
	}

	private ToggleAnimator3DSoundPack toggleSoundPack => this.CacheScriptObject(ref _toggleSoundPack);

	public bool inConfirmState
	{
		get
		{
			return _inConfirmState;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _inConfirmState, value))
			{
				_OnConfirmStateChange();
			}
		}
	}

	public UnityEvent OnConfirmClick => _OnConfirmClick ?? (_OnConfirmClick = new UnityEvent());

	private Transform _confirmTargetTransform
	{
		get
		{
			if (!inConfirmState)
			{
				return _rest;
			}
			return confirm;
		}
	}

	private PooledAudioPlayer audioPlayer
	{
		get
		{
			if (!_audioPlayer)
			{
				return _audioPlayer = base.gameObject.GetOrAddComponent<PooledAudioPlayer>();
			}
			return _audioPlayer;
		}
	}

	private void _OnConfirmStateChange()
	{
		if (inConfirmState)
		{
			_defaultSoundPack = base.soundPack;
		}
		base.soundPack = (inConfirmState ? confirmSoundPack : _defaultSoundPack);
		_confirmVelocity = _confirmVelocity.ResetRotationData();
		_confirmTargetOriginalRotation = _confirmSlot.localRotation;
		audioPlayer.Play(toggleSoundPack.GetAudioPack(inConfirmState));
	}

	private void _OnConfirmEnterHandler(PointerEventData eventData)
	{
		_cursorOver = true;
	}

	private void _OnConfirmExitHandler(PointerEventData eventData)
	{
		_cursorOver = false;
	}

	private void _OnConfirmClickHandler(PointerEventData eventData)
	{
		if (inConfirmState)
		{
			OnConfirmClick.Invoke();
		}
		else
		{
			inConfirmState = true;
		}
	}

	protected override void _RegisterEvents()
	{
		base._RegisterEvents();
		base.pointerOver3D.OnEnter.AddListener(_OnConfirmEnterHandler);
		base.pointerOver3D.OnExit.AddListener(_OnConfirmExitHandler);
		base.pointerClick3D.OnClick.AddListener(_OnConfirmClickHandler);
	}

	protected override void _UnregisterEvents()
	{
		base._UnregisterEvents();
		base.pointerOver3D.OnEnter.RemoveListener(_OnConfirmEnterHandler);
		base.pointerOver3D.OnExit.RemoveListener(_OnConfirmExitHandler);
		base.pointerClick3D.OnClick.RemoveListener(_OnConfirmClickHandler);
	}

	protected override void Awake()
	{
		base.Awake();
		confirm.SetParent(base.slot.parent, worldPositionStays: true);
		_confirmSlot = new GameObject("Confirm Slot").transform;
		_confirmSlot.SetParent(base.slot, worldPositionStays: false);
		base.transform.SetParent(_confirmSlot, worldPositionStays: true);
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (_AnimateTransform(_confirmSlot, ref _confirmVelocity, _confirmTargetOriginalRotation, _confirmTargetTransform) && !_cursorOver && unconfirmOnPointerExit)
		{
			inConfirmState = false;
		}
	}
}
