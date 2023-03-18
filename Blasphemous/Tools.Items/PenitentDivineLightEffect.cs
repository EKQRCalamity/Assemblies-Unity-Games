using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Tools.Items;

public class PenitentDivineLightEffect : ObjectEffect
{
	[SerializeField]
	private int beansCount = 4;

	[SerializeField]
	private float randMaxX = 10f;

	[SerializeField]
	private float randMaxY = 5f;

	public int DamageAmount = 1;

	private Penitent _owner;

	private BossAreaSummonAttack _areaSummonAttack;

	protected override bool OnApplyEffect()
	{
		Debug.Log("PENITENT DIVINE LIGHT EFFECT");
		_owner = Core.Logic.Penitent;
		_areaSummonAttack = _owner.GetComponentInChildren<PrayerUse>().divineLightPrayer;
		_areaSummonAttack.SetDamageStrength(_owner.Stats.PrayerStrengthMultiplier.Final);
		_areaSummonAttack.SummonAreas(Vector2.right);
		_areaSummonAttack.SummonAreas(Vector2.left);
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("SimpleHit");
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
