using UnityEngine;

public class PointerGroup3D : MonoBehaviour
{
	public enum EnableState : byte
	{
		Ignore,
		Disabled,
		Enabled
	}

	[SerializeField]
	protected bool _masterEnabled = true;

	[SerializeField]
	protected EnableState _oversEnabled;

	[SerializeField]
	protected EnableState _clicksEnabled;

	[SerializeField]
	protected EnableState _dragsEnabled;

	[SerializeField]
	protected EnableState _dropsEnabled;

	[SerializeField]
	protected EnableState _scrollEnabled = EnableState.Enabled;

	[SerializeField]
	protected bool _affectNon3DPointerComponents;

	[SerializeField]
	protected bool _updateStatesEveryFrame = true;

	public bool masterEnabled
	{
		get
		{
			return _masterEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _masterEnabled, value))
			{
				_UpdateEnableStates();
			}
		}
	}

	public EnableState oversEnabled
	{
		get
		{
			return _oversEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _oversEnabled, value))
			{
				_UpdateEnableStates();
			}
		}
	}

	public EnableState clicksEnabled
	{
		get
		{
			return _clicksEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _clicksEnabled, value))
			{
				_UpdateEnableStates();
			}
		}
	}

	public EnableState dragsEnabled
	{
		get
		{
			return _dragsEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _dragsEnabled, value))
			{
				_UpdateEnableStates();
			}
		}
	}

	public EnableState dropsEnabled
	{
		get
		{
			return _dropsEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _dropsEnabled, value))
			{
				_UpdateEnableStates();
			}
		}
	}

	public EnableState scrollEnabled
	{
		get
		{
			return _scrollEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _scrollEnabled, value))
			{
				_UpdateEnableStates();
			}
		}
	}

	public bool affectNon3DPointerComponents
	{
		get
		{
			return _affectNon3DPointerComponents;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _affectNon3DPointerComponents, value))
			{
				_UpdateEnableStates();
			}
		}
	}

	private void _SetEnableState<T>(EnableState enableState) where T : MonoBehaviour
	{
		if (enableState == EnableState.Ignore)
		{
			return;
		}
		foreach (T item in base.gameObject.GetComponentsInChildrenPooled<T>(includeInactive: true))
		{
			item.enabled = masterEnabled && enableState == EnableState.Enabled;
		}
	}

	private void _UpdateEnableStates()
	{
		if (base.enabled)
		{
			_SetEnableState<PointerOver3D>(oversEnabled);
			_SetEnableState<PointerClick3D>(clicksEnabled);
			_SetEnableState<PointerDrag3D>(dragsEnabled);
			_SetEnableState<PointerDrop3D>(dropsEnabled);
			_SetEnableState<PointerScroll3D>(scrollEnabled);
			if (affectNon3DPointerComponents)
			{
				_SetEnableState<PointerOverTrigger>(oversEnabled);
				_SetEnableState<PointerClickTrigger>(clicksEnabled);
				_SetEnableState<DragTrigger>(dragsEnabled);
			}
		}
	}

	private void OnEnable()
	{
		_UpdateEnableStates();
	}

	private void Start()
	{
		_UpdateEnableStates();
	}

	private void LateUpdate()
	{
		if (_updateStatesEveryFrame)
		{
			_UpdateEnableStates();
		}
	}

	public void SetOversEnabled(bool enabled)
	{
		oversEnabled = ((!enabled) ? EnableState.Disabled : EnableState.Enabled);
	}

	public void SetClicksEnabled(bool enabled)
	{
		clicksEnabled = ((!enabled) ? EnableState.Disabled : EnableState.Enabled);
	}

	public void SetDragsEnabled(bool enabled)
	{
		dragsEnabled = ((!enabled) ? EnableState.Disabled : EnableState.Enabled);
	}

	public void SetDropsEnabled(bool enabled)
	{
		dropsEnabled = ((!enabled) ? EnableState.Disabled : EnableState.Enabled);
	}

	public void SetScrollEnabled(bool enabled)
	{
		scrollEnabled = ((!enabled) ? EnableState.Disabled : EnableState.Enabled);
	}

	public PointerGroup3D SetData(bool master = true, bool affectNon3dComponents = true, bool overs = true, bool clicks = true, bool drags = true, bool drops = true, bool scroll = true)
	{
		masterEnabled = master;
		affectNon3DPointerComponents = affectNon3dComponents;
		SetOversEnabled(overs);
		SetClicksEnabled(clicks);
		SetDragsEnabled(drags);
		SetDropsEnabled(drops);
		SetScrollEnabled(scroll);
		return this;
	}
}
