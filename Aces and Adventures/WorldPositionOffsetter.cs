using UnityEngine;

[ScriptOrder(1000)]
public class WorldPositionOffsetter : MonoBehaviour
{
	[SerializeField]
	protected Vector3 _worldPositionOffset;

	private Vector3 _defaultOrigin;

	public Vector3 worldPositionOffset
	{
		get
		{
			return _worldPositionOffset;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _worldPositionOffset, value))
			{
				_OnWorldPositionOffsetChange();
			}
		}
	}

	public float x
	{
		get
		{
			return _worldPositionOffset.x;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _worldPositionOffset.x, value))
			{
				_OnWorldPositionOffsetChange();
			}
		}
	}

	public float y
	{
		get
		{
			return _worldPositionOffset.y;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _worldPositionOffset.y, value))
			{
				_OnWorldPositionOffsetChange();
			}
		}
	}

	public float z
	{
		get
		{
			return _worldPositionOffset.z;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _worldPositionOffset.z, value))
			{
				_OnWorldPositionOffsetChange();
			}
		}
	}

	private void _OnWorldPositionOffsetChange()
	{
		base.enabled = true;
	}

	private void OnTransformParentChanged()
	{
		if (!base.transform.parent)
		{
			_defaultOrigin = base.transform.position;
		}
	}

	private void LateUpdate()
	{
		base.transform.position = (base.transform.parent ? base.transform.parent.TransformPoint(Vector3.zero) : _defaultOrigin) + worldPositionOffset;
		if (_worldPositionOffset == Vector3.zero)
		{
			base.enabled = false;
		}
	}
}
