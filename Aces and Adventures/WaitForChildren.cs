using System.Linq;
using UnityEngine;

public class WaitForChildren : MonoBehaviour
{
	[SerializeField]
	protected WaitForChildrenType _waitForChildren = WaitForChildrenType.ImmediateChildren;

	[SerializeField]
	protected WaitForSpecificChildrenType _waitForSpecificChildren = WaitForSpecificChildrenType.AudioVisual;

	private Transform[] _children;

	public WaitForChildrenType waitForChildren
	{
		get
		{
			return _waitForChildren;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _waitForChildren, value))
			{
				_SetDirty();
			}
		}
	}

	public WaitForSpecificChildrenType waitForSpecificChildren
	{
		get
		{
			return _waitForSpecificChildren;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _waitForSpecificChildren, value))
			{
				_SetDirty();
			}
		}
	}

	protected Transform[] children => _children ?? (_children = (from t in _waitForChildren.Children(base.transform)
		where _waitForSpecificChildren.IsValid(t.gameObject)
		select t).ToArray());

	public bool childrenFinished
	{
		get
		{
			if (waitForChildren == WaitForChildrenType.None)
			{
				return true;
			}
			bool flag = base.transform.GetWorldScale().Clamp() == Vector3.zero;
			Transform[] array = children;
			foreach (Transform transform in array)
			{
				if (transform.gameObject.activeInHierarchy && (!flag || (bool)transform.GetComponent<ParticleSystem>()))
				{
					return false;
				}
			}
			return true;
		}
	}

	private void _SetDirty()
	{
		_children = null;
	}

	public WaitForChildren SetData(WaitForChildrenType type, WaitForSpecificChildrenType specificType)
	{
		waitForChildren = type;
		waitForSpecificChildren = specificType;
		return this;
	}
}
