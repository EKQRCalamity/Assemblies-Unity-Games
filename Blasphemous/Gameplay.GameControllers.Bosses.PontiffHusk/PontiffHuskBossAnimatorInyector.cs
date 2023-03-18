using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffHusk;

public class PontiffHuskBossAnimatorInyector : EnemyAnimatorInyector
{
	private static readonly int B_Death = Animator.StringToHash("DEATH");

	private static readonly int B_Hide = Animator.StringToHash("HIDE");

	private static readonly int B_Charge = Animator.StringToHash("CHARGE");

	private static readonly int B_Shoot = Animator.StringToHash("SHOOT");

	private static readonly int B_AltShoot = Animator.StringToHash("ALT_SHOOT");

	private static readonly int B_Cast = Animator.StringToHash("CAST");

	private static readonly int B_Beam = Animator.StringToHash("BEAM");

	private static readonly int T_Turn = Animator.StringToHash("TURN");

	public Animator mainAnimator;

	public void PlayDeath()
	{
		if ((bool)base.EntityAnimator)
		{
			ResetAll();
			StopHide();
			SetDualBool(B_Death, value: true);
		}
	}

	public void StopDeath()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_Death, value: false);
		}
	}

	public void PlayCharge()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_Charge, value: true);
		}
	}

	public void StopCharge()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_Charge, value: false);
		}
	}

	public void PlayShoot()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_Shoot, value: true);
		}
	}

	public void StopShoot()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_Shoot, value: false);
		}
	}

	public void PlayAltShoot()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_AltShoot, value: true);
		}
	}

	public void StopAltShoot()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_AltShoot, value: false);
		}
	}

	public void PlayCast()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_Cast, value: true);
		}
	}

	public void StopCast()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_Cast, value: false);
		}
	}

	public void PlayBeam()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_Beam, value: true);
		}
	}

	public void StopBeam()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_Beam, value: false);
		}
	}

	public void PlayTurn()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_Charge, value: false);
			SetDualBool(B_Shoot, value: false);
			SetDualBool(B_AltShoot, value: false);
			SetDualTrigger(T_Turn);
		}
	}

	public bool IsTurnQeued()
	{
		return mainAnimator.GetBool(T_Turn);
	}

	public void PlayHide()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(T_Turn, value: false);
			SetDualBool(B_Hide, value: true);
		}
	}

	public void StopHide()
	{
		if ((bool)base.EntityAnimator)
		{
			SetDualBool(B_Hide, value: false);
		}
	}

	public bool GetHide()
	{
		if (!base.EntityAnimator)
		{
			return false;
		}
		return GetDualBool(B_Hide);
	}

	public void ResetAll()
	{
		SetDualBool(B_Death, value: false);
		SetDualBool(B_Charge, value: false);
		SetDualBool(B_Shoot, value: false);
		SetDualBool(B_AltShoot, value: false);
		SetDualBool(B_Cast, value: false);
		SetDualBool(B_Beam, value: false);
	}

	private void ResetDualTrigger(int name)
	{
		mainAnimator.ResetTrigger(name);
	}

	private void SetDualTrigger(int name)
	{
		mainAnimator.SetTrigger(name);
	}

	private void SetDualBool(int name, bool value)
	{
		mainAnimator.SetBool(name, value);
	}

	private bool GetDualBool(int name)
	{
		return mainAnimator.GetBool(name);
	}
}
