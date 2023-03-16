using System.Collections;
using UnityEngine;

public class SnowCultLevelIcePillar : AbstractProjectile
{
	private const float Y_POS_START = -430f;

	private const float Y_POS_END = -200f;

	private string typeString;

	private float timeToDelay;

	private float outTime;

	[SerializeField]
	private Effect warningSmoke;

	public virtual SnowCultLevelIcePillar Init(Vector3 pos, LevelProperties.SnowCult.IcePillar properties, bool typeToPlay, float timeToDelay)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = pos;
		typeString = ((!typeToPlay) ? "B" : "A");
		base.animator.Play("IceBlade_Start" + typeString);
		this.timeToDelay = timeToDelay;
		outTime = properties.outTime;
		Attack();
		return this;
	}

	private void Attack()
	{
		StartCoroutine(attack_cr());
	}

	private IEnumerator attack_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, timeToDelay);
		base.animator.SetTrigger("popUp");
		SFX_SNOWCULT_BladeStabfromGround();
	}

	private void WaitAndRetract()
	{
		StartCoroutine(waitandretract_cr());
	}

	private IEnumerator waitandretract_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, outTime);
		base.animator.SetTrigger("popDown");
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void Start()
	{
		base.Start();
	}

	private void WarningSmokeFX()
	{
		warningSmoke.Create(base.transform.position);
	}

	private void SFX_SNOWCULT_BladeStabfromGround()
	{
		AudioManager.Play("sfx_dlc_snowcult_p2_snowmonster_blade_stabfromground");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_snowmonster_blade_stabfromground");
	}
}
