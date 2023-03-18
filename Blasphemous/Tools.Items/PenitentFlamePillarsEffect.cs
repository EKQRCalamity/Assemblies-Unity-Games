using Framework.FrameworkCore;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Tools.Items;

public class PenitentFlamePillarsEffect : ObjectEffect
{
	private Penitent _owner;

	private BossAreaSummonAttack _areaSummonAttack;

	protected override bool OnApplyEffect()
	{
		Debug.Log("PENITENT FLAME PILLARS EFFECT");
		_owner = Core.Logic.Penitent;
		_areaSummonAttack = _owner.GetComponentInChildren<PrayerUse>().flamePillarsPrayer;
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("SimpleHit");
		Vector2 vector = Vector2.right * ((_owner.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
		_areaSummonAttack.totalAreas = 8;
		_areaSummonAttack.SummonAreas(vector);
		return base.OnApplyEffect();
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	private void Update()
	{
	}

	protected override void OnRemoveEffect()
	{
		base.OnRemoveEffect();
	}
}
