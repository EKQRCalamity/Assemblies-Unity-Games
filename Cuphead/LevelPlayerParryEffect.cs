using UnityEngine;

public class LevelPlayerParryEffect : AbstractParryEffect
{
	private const float CHALICE_X_OFFSET = 29.5f;

	private const float CHALICE_Y_OFFSET = 10f;

	private const float CHALICE_RADIUS = 80f;

	protected override bool IsHit => (player as LevelPlayerController).motor.IsHit;

	private LevelPlayerController levelPlayer => player as LevelPlayerController;

	protected override void SetPlayer(AbstractPlayerController player)
	{
		base.SetPlayer(player);
		if (player.stats.Loadout.charm == Charm.charm_chalice)
		{
			GetComponent<CircleCollider2D>().offset = new Vector2(29.5f, 10f);
			GetComponent<CircleCollider2D>().radius = 80f;
		}
		levelPlayer.motor.OnHitEvent += OnHitCancel;
		levelPlayer.motor.OnGroundedEvent += OnGroundedCancel;
		levelPlayer.motor.OnDashStartEvent += OnDashCancel;
		levelPlayer.weaponManager.OnExStart += OnWeaponCancel;
		levelPlayer.weaponManager.OnSuperStart += OnWeaponCancel;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		levelPlayer.motor.OnHitEvent -= OnHitCancel;
		levelPlayer.motor.OnGroundedEvent -= OnGroundedCancel;
		levelPlayer.motor.OnDashStartEvent -= OnDashCancel;
		levelPlayer.weaponManager.OnExStart -= OnWeaponCancel;
		levelPlayer.weaponManager.OnSuperStart -= OnWeaponCancel;
	}

	protected override void OnHitCancel()
	{
		base.OnHitCancel();
		levelPlayer.motor.OnParryHit();
	}

	private void OnDashCancel()
	{
		if (!didHitSomething && !(this == null))
		{
			Cancel();
		}
	}

	private void OnGroundedCancel()
	{
		if (!player.stats.isChalice && !didHitSomething && !(this == null))
		{
			Cancel();
		}
	}

	private void OnWeaponCancel()
	{
		if (!didHitSomething && !(this == null))
		{
			Cancel();
		}
	}

	protected override void Cancel()
	{
		base.Cancel();
		levelPlayer.animationController.ResumeNormanAnim();
	}

	protected override void CancelSwitch()
	{
		base.CancelSwitch();
		levelPlayer.motor.OnParryCanceled();
	}

	protected override void OnPaused()
	{
		base.OnPaused();
		levelPlayer.animationController.OnParryPause();
		levelPlayer.weaponManager.ParrySuccess();
	}

	protected override void OnUnpaused()
	{
		base.OnUnpaused();
		levelPlayer.animationController.ResumeNormanAnim();
		levelPlayer.motor.OnParryComplete();
	}

	protected override void OnSuccess()
	{
		base.OnSuccess();
		levelPlayer.weaponManager.ParrySuccess();
		levelPlayer.animationController.OnParrySuccess();
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		levelPlayer.motor.ResetChaliceDoubleJump();
		levelPlayer.animationController.OnParryAnimEnd();
	}
}
