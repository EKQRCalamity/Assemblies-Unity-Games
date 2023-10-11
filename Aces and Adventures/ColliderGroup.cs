using UnityEngine;

public class ColliderGroup : MonoBehaviour
{
	[SerializeField]
	protected bool _collidersEnabled = true;

	public bool collidersEnabled
	{
		get
		{
			return _collidersEnabled;
		}
		set
		{
			_collidersEnabled = value;
		}
	}

	private void _UpdateColliderEnable()
	{
		foreach (Collider item in base.gameObject.GetComponentsInChildrenPooled<Collider>(includeInactive: true))
		{
			item.enabled = collidersEnabled;
		}
	}

	private void OnEnable()
	{
		_UpdateColliderEnable();
	}

	private void LateUpdate()
	{
		_UpdateColliderEnable();
	}
}
