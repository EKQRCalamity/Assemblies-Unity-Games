using UnityEngine;

public class SnowCultLevelTable : AbstractPausableComponent
{
	[SerializeField]
	private SnowCultLevelWizard wiz;

	private Vector3 vel;

	private bool chase;

	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private float accel = 1f;

	[SerializeField]
	private float decelOnDeactivate = 1f;

	[SerializeField]
	private float maxVel = 200f;

	[SerializeField]
	private float maxDistance = 20f;

	private float outroTimer;

	private Vector3 lastPos;

	public void Intro(Vector3 startVel)
	{
		base.transform.parent = null;
		base.transform.position = wiz.transform.position;
		vel = startVel / CupheadTime.FixedDelta;
		chase = true;
		base.animator.Play("Intro");
		SFX_SNOWCULT_WizardTableCrystalballLoop();
	}

	public void Outro()
	{
		chase = false;
		vel = (base.transform.position - lastPos) / CupheadTime.FixedDelta;
		outroTimer = 1f / 3f;
	}

	private void FixedUpdate()
	{
		if (!rend.enabled)
		{
			return;
		}
		lastPos = base.transform.position;
		base.transform.position += vel * CupheadTime.FixedDelta;
		if (chase)
		{
			vel += (wiz.transform.position - base.transform.position).normalized * accel * CupheadTime.FixedDelta;
			if (vel.magnitude > maxVel)
			{
				vel = vel.normalized * maxVel;
			}
			if (Vector2.Distance(wiz.transform.position, base.transform.position) > maxDistance)
			{
				base.transform.position = wiz.transform.position + (base.transform.position - wiz.transform.position).normalized * maxDistance;
			}
		}
		else
		{
			vel -= vel * decelOnDeactivate * CupheadTime.FixedDelta;
		}
		if (outroTimer > 0f)
		{
			outroTimer -= CupheadTime.FixedDelta;
			if (outroTimer <= 0f)
			{
				base.animator.Play("Outro");
				SFX_SNOWCULT_WizardTableDisappear();
			}
		}
	}

	private void AnimationEvent_SFX_SNOWCULT_WizardTableAppear()
	{
		AudioManager.Play("sfx_dlc_snowcult_p1_wizard_crystalball_appear");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p1_wizard_crystalball_appear");
	}

	private void SFX_SNOWCULT_WizardTableDisappear()
	{
		AudioManager.Stop("sfx_dlc_snowcult_p1_wizard_crystalball_loop");
		AudioManager.Play("sfx_dlc_snowcult_p1_wizard_crystalball_disappear");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p1_wizard_crystalball_disappear");
	}

	private void SFX_SNOWCULT_WizardTableCrystalballLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_snowcult_p1_wizard_crystalball_loop");
		AudioManager.FadeSFXVolume("sfx_dlc_snowcult_p1_wizard_crystalball_loop", 0.15f, 1f);
		emitAudioFromObject.Add("sfx_dlc_snowcult_p1_wizard_crystalball_loop");
	}
}
