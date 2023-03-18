using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Tools.Items;

public class StuntPrayerEffect : ObjectEffect
{
	[SerializeField]
	private GameObject attack;

	private Penitent _owner;

	private BossAreaSummonAttack _areaSummonAttack;

	protected override bool OnApplyEffect()
	{
		Debug.Log("STUNT PRAYER EFFECT");
		_owner = Core.Logic.Penitent;
		_areaSummonAttack = _owner.GetComponentInChildren<PrayerUse>().stuntPrayer;
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("SimpleHit");
		BossAreaSummonAttack areaSummonAttack = _areaSummonAttack;
		Vector3 position = _areaSummonAttack.transform.position;
		float final = _owner.Stats.PrayerStrengthMultiplier.Final;
		areaSummonAttack.SummonAreaOnPoint(position, 0f, final);
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
