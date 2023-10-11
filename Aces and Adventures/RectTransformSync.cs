using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[ScriptOrder(1)]
public class RectTransformSync : MonoBehaviour
{
	[SerializeField]
	protected RectTransform _syncTo;

	[SerializeField]
	protected bool _syncActive = true;

	[SerializeField]
	protected bool _syncLayer = true;

	public bool clearSyncToOnDisable = true;

	public RectTransform syncTo
	{
		get
		{
			return _syncTo;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _syncTo, value))
			{
				_OnSyncToChange();
			}
		}
	}

	public bool syncActive
	{
		get
		{
			return _syncActive;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _syncActive, value) && _syncActive && (bool)_syncTo)
			{
				base.gameObject.SetActive(_syncTo.gameObject.activeSelf);
			}
		}
	}

	public bool syncLayer
	{
		get
		{
			return _syncLayer;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _syncLayer, value) && _syncLayer && (bool)_syncTo)
			{
				base.gameObject.layer = _syncTo.gameObject.layer;
			}
		}
	}

	private void _OnSyncToChange()
	{
		Update();
	}

	private void Update()
	{
		if (!syncTo)
		{
			return;
		}
		if (syncActive && !syncTo.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		if (syncLayer)
		{
			base.gameObject.layer = syncTo.gameObject.layer;
		}
		(base.transform as RectTransform).CopyRect3D(syncTo);
	}

	private void LateUpdate()
	{
		Update();
	}

	private void OnDisable()
	{
		if (clearSyncToOnDisable)
		{
			syncTo = null;
		}
	}

	public RectTransformSync SetData(RectTransform syncTo, bool syncActive = true, bool syncLayer = true)
	{
		this.syncLayer = syncLayer;
		this.syncTo = syncTo;
		this.syncActive = syncActive;
		return this;
	}
}
