using Gameplay.GameControllers.Entities;
using Rewired;
using UnityEngine;

namespace Framework.FrameworkCore;

public class Trait : MonoBehaviour
{
	public Entity EntityOwner { get; set; }

	protected Player RewiredInput { get; private set; }

	protected virtual void OnAwake()
	{
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnTraitEnable()
	{
	}

	protected virtual void OnTraitDisable()
	{
	}

	protected virtual void OnTraitDestroy()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnLateUpdate()
	{
	}

	protected virtual void OnFixedUpdate()
	{
	}

	private void Awake()
	{
		EntityOwner = GetComponentInParent<Entity>();
		if (EntityOwner != null)
		{
			OnAwake();
		}
	}

	private void Start()
	{
		if (EntityOwner == null)
		{
			Debug.LogError(string.Concat(GetType(), " has no Entity. Traits won't execute."));
		}
		if (EntityOwner != null)
		{
			OnStart();
		}
	}

	private void OnEnable()
	{
		if (EntityOwner != null)
		{
			OnTraitEnable();
		}
	}

	private void OnDisable()
	{
		if (EntityOwner != null)
		{
			OnTraitDisable();
		}
	}

	private void Update()
	{
		if (EntityOwner != null)
		{
			OnUpdate();
		}
	}

	private void LateUpdate()
	{
		if (EntityOwner != null)
		{
			OnLateUpdate();
		}
	}

	private void FixedUpdate()
	{
		if (EntityOwner != null)
		{
			OnFixedUpdate();
		}
	}
}
