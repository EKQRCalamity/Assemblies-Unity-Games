using UnityEngine;

public class PointerState3DTracker : MonoBehaviour
{
	[SerializeField]
	private PointerStateEvent _OnStateChange;

	private PointerStateFlags _state = PointerStateFlags.None;

	public PointerStateEvent OnStateChange => _OnStateChange ?? (_OnStateChange = new PointerStateEvent());

	private PointerStateFlags state
	{
		get
		{
			return _state;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _state, value))
			{
				OnStateChange.Invoke(EnumUtil<PointerStateFlags>.ConvertFromFlag<PointerState>(EnumUtil.MaxActiveFlag(value)));
			}
		}
	}

	private void Awake()
	{
		_RegisterPointerOver(GetComponent<PointerOver3D>());
		_RegisterPointerClick(GetComponent<PointerClick3D>());
	}

	private void OnEnable()
	{
		state = PointerStateFlags.None;
	}

	private void LateUpdate()
	{
		if (EnumUtil.HasFlag(state, PointerStateFlags.Pressed) && InputManager.I[KeyCode.Mouse0][KState.Up])
		{
			state = EnumUtil<PointerStateFlags>.Subtract(state, PointerStateFlags.Pressed);
		}
	}

	private void _RegisterPointerOver(PointerOver3D pointerOver)
	{
		if ((bool)pointerOver)
		{
			pointerOver.OnEnter.AddListener(delegate
			{
				state = EnumUtil<PointerStateFlags>.Add(state, PointerStateFlags.Over);
			});
			pointerOver.OnExit.AddListener(delegate
			{
				state = EnumUtil<PointerStateFlags>.Subtract(state, PointerStateFlags.Over);
			});
		}
	}

	private void _RegisterPointerClick(PointerClick3D pointerClick)
	{
		if ((bool)pointerClick)
		{
			pointerClick.OnDown.AddListener(delegate
			{
				state = EnumUtil<PointerStateFlags>.Add(state, PointerStateFlags.Pressed);
			});
			pointerClick.OnUp.AddListener(delegate
			{
				state = EnumUtil<PointerStateFlags>.Subtract(state, PointerStateFlags.Pressed);
			});
		}
	}
}
