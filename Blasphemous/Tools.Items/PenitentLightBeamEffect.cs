using System.Collections;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Tools.Items;

public class PenitentLightBeamEffect : ObjectEffect
{
	private Penitent _owner;

	private BossAreaSummonAttack _areaSummonAttack;

	public Material penitentBlueTintMaterial;

	private Material oldMat;

	public int DamageAmount = 1;

	protected override bool OnApplyEffect()
	{
		Debug.Log("PENITENT VERTICAL BEAM EFFECT");
		_owner = Core.Logic.Penitent;
		_areaSummonAttack = _owner.GetComponentInChildren<PrayerUse>().lightBeamPrayer;
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("SimpleHit");
		BossAreaSummonAttack areaSummonAttack = _areaSummonAttack;
		Vector3 position = _areaSummonAttack.transform.position;
		float final = _owner.Stats.PrayerStrengthMultiplier.Final;
		GameObject gameObject = areaSummonAttack.SummonAreaOnPoint(position, 0f, final);
		gameObject.GetComponent<BossSpawnedAreaAttack>().SetDamage(DamageAmount);
		StartCoroutine(VerticalBeamCoroutine());
		return base.OnApplyEffect();
	}

	private IEnumerator VerticalBeamCoroutine()
	{
		yield return new WaitForSeconds(0.4f);
		PushPlayerColor();
		yield return new WaitForSeconds(0.8f);
		PopPlayerColor();
	}

	private void PushPlayerColor()
	{
		oldMat = _owner.SpriteRenderer.material;
		_owner.SpriteRenderer.material = penitentBlueTintMaterial;
	}

	private void PopPlayerColor()
	{
		_owner.SpriteRenderer.material = oldMat;
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
