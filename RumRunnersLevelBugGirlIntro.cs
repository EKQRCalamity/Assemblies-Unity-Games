using UnityEngine;

public class RumRunnersLevelBugGirlIntro : MonoBehaviour
{
	[SerializeField]
	private RumRunnersLevelMobIntroAnimation introAnimation;

	private void OnEnable()
	{
		GetComponent<DamageReceiver>().OnDamageTaken += OnDamageTaken;
	}

	private void OnDisable()
	{
		GetComponent<DamageReceiver>().OnDamageTaken -= OnDamageTaken;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		introAnimation.bugGirlDamage += info.damage;
	}

	private void animationEvent_BugWalkBegin()
	{
		introAnimation.StartBugWalk();
	}

	private void animationEvent_BugTauntBegin()
	{
		introAnimation.StopBugWalk();
	}

	private void animationEvent_TauntBump()
	{
		introAnimation.BarrelExit();
	}
}
