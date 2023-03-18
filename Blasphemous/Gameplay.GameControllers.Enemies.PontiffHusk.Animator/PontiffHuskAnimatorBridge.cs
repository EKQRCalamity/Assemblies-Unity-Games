using UnityEngine;

namespace Gameplay.GameControllers.Enemies.PontiffHusk.Animator;

public class PontiffHuskAnimatorBridge : MonoBehaviour
{
	private PontiffHuskRanged _PontiffHuskRanged;

	private PontiffHuskMelee _PontiffHuskMelee;

	private void Start()
	{
		_PontiffHuskRanged = GetComponentInParent<PontiffHuskRanged>();
		_PontiffHuskMelee = GetComponentInParent<PontiffHuskMelee>();
	}

	public void PlayChargeAttack()
	{
		if (_PontiffHuskRanged != null)
		{
			_PontiffHuskRanged.Audio.ChargeAttack();
		}
		if (_PontiffHuskMelee != null)
		{
			_PontiffHuskMelee.Audio.ChargeAttack();
		}
	}
}
