using Framework.FrameworkCore;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

[RequireComponent(typeof(Collider2D))]
public class DamageAreaSwapper : Trait
{
	private DamageArea _entityDamageArea;

	private bool _damageAreaIsSet;

	private bool _damageAreaColliderIsSet;

	protected override void OnStart()
	{
		base.OnStart();
		_entityDamageArea = base.EntityOwner.EntityDamageArea;
		_damageAreaIsSet = _entityDamageArea != null;
		if (_damageAreaIsSet)
		{
			_damageAreaColliderIsSet = _entityDamageArea.DamageAreaCollider != null;
		}
		if (!_damageAreaColliderIsSet)
		{
			Debug.LogError("Damage Area Collider must be set in Damage Area");
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		SetDamageAreaColliderOrientation();
	}

	private void SetDamageAreaColliderOrientation()
	{
		if (_damageAreaColliderIsSet)
		{
			EntityOrientation orientation = base.EntityOwner.Status.Orientation;
			Vector3 localScale = base.transform.localScale;
			localScale.x = ((orientation != EntityOrientation.Left) ? 1 : (-1));
			base.transform.localScale = localScale;
		}
	}
}
