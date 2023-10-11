using System;
using System.Collections.Generic;
using UnityEngine;

public class ColliderEventHook : MonoBehaviour
{
	[Header("Filters")]
	public LayerMask layersToRespondTo = -1;

	[SerializeField]
	protected string _friendlyTypeNameFilter;

	public bool checkParentsForTypeFilter;

	[Range(0f, 1000f)]
	public int maxTriggerPerObject;

	public bool listenToCollisions = true;

	public bool listenToTriggerCollisions = true;

	[Header("Events")]
	[Header("Enter")]
	[SerializeField]
	protected CollisionEvent _OnEnter;

	[SerializeField]
	protected ColliderEvent _OnTriggerEnter;

	[Header("Stay")]
	public bool listenToCollisionStayEvents;

	[SerializeField]
	protected CollisionEvent _OnStay;

	[SerializeField]
	protected ColliderEvent _OnTriggerStay;

	[Header("Exit")]
	public bool listenToCollisionExitEvents;

	[SerializeField]
	protected CollisionEvent _OnExit;

	[SerializeField]
	protected ColliderEvent _OnTriggerExit;

	private Type _typeFilter;

	private Dictionary<GameObject, int> _triggerCounts;

	public CollisionEvent OnEnter => _OnEnter ?? (_OnEnter = new CollisionEvent());

	public CollisionEvent OnStay => _OnStay ?? (_OnStay = new CollisionEvent());

	public CollisionEvent OnExit => _OnExit ?? (_OnExit = new CollisionEvent());

	public ColliderEvent OnEnterTrigger => _OnTriggerEnter ?? (_OnTriggerEnter = new ColliderEvent());

	public ColliderEvent OnStayTrigger => _OnTriggerStay ?? (_OnTriggerStay = new ColliderEvent());

	public ColliderEvent OnExitTrigger => _OnTriggerExit ?? (_OnTriggerExit = new ColliderEvent());

	public string friendlyTypeNameFilter
	{
		get
		{
			return _friendlyTypeNameFilter;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _friendlyTypeNameFilter, value))
			{
				_OnFriendlyTypeNameFilterChange();
			}
		}
	}

	public Type typeFilter
	{
		get
		{
			return _typeFilter;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _typeFilter, value) && _typeFilter != null && !_typeFilter.IsSubclassOf(typeof(Component)))
			{
				_typeFilter = null;
			}
		}
	}

	protected Dictionary<GameObject, int> triggerCounts => _triggerCounts ?? (_triggerCounts = new Dictionary<GameObject, int>());

	private void _OnFriendlyTypeNameFilterChange()
	{
		typeFilter = (friendlyTypeNameFilter.IsNullOrEmpty() ? null : ReflectionUtil.GetTypeFromFriendlyName(friendlyTypeNameFilter));
	}

	private bool _ValidCollision(GameObject collidedWith, bool isEnter = false)
	{
		if (!layersToRespondTo.Match(collidedWith))
		{
			return false;
		}
		if (maxTriggerPerObject > 0)
		{
			if (isEnter)
			{
				triggerCounts.Increment(collidedWith);
			}
			if (triggerCounts.IncrementedCount(collidedWith) > maxTriggerPerObject)
			{
				return false;
			}
		}
		if (!(typeFilter == null) && !collidedWith.GetComponentInChildren(typeFilter))
		{
			if (checkParentsForTypeFilter)
			{
				return collidedWith.GetComponentInParent(typeFilter);
			}
			return false;
		}
		return true;
	}

	private void Awake()
	{
		_OnFriendlyTypeNameFilterChange();
	}

	private void LateUpdate()
	{
		if (_triggerCounts.IsNullOrEmpty())
		{
			return;
		}
		foreach (GameObject item in triggerCounts.EnumerateKeysSafe())
		{
			if (!item)
			{
				triggerCounts.Remove(item);
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (listenToCollisions && _ValidCollision(collision.gameObject, isEnter: true))
		{
			OnEnter.Invoke(collision);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (listenToCollisions && listenToCollisionStayEvents && _ValidCollision(collision.gameObject))
		{
			OnStay.Invoke(collision);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (listenToCollisions && listenToCollisionExitEvents && _ValidCollision(collision.gameObject))
		{
			OnExit.Invoke(collision);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (listenToTriggerCollisions && _ValidCollision(other.gameObject, isEnter: true))
		{
			OnEnterTrigger.Invoke(other);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (listenToTriggerCollisions && listenToCollisionStayEvents && _ValidCollision(other.gameObject))
		{
			OnStayTrigger.Invoke(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (listenToTriggerCollisions && listenToCollisionExitEvents && _ValidCollision(other.gameObject))
		{
			OnExitTrigger.Invoke(other);
		}
	}
}
