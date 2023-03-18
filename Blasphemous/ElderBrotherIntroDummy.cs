using System.Collections;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.ElderBrother;
using Gameplay.GameControllers.Enemies.MasterAnguish.Audio;
using Sirenix.OdinInspector;
using UnityEngine;

public class ElderBrotherIntroDummy : MonoBehaviour
{
	public Animator animator;

	public ElderBrotherBehaviour elderBro;

	public ElderBrotherAudio elderBroAudio;

	public void AnimEvent_ActivateBarrier()
	{
	}

	public void AnimEvent_DeactivateBarrier()
	{
	}

	[Button("TEST ANIMATION", ButtonSizes.Small)]
	public void TriggerIntro()
	{
		StartCoroutine(IntroDummyJump());
	}

	private void ShakeWave()
	{
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 0.7f, 0.3f, 2f);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.5f, Vector3.down * 1f, 12, 0.2f, 0f, default(Vector3), 0f);
	}

	private IEnumerator IntroDummyJump()
	{
		BigSmashPreparation();
		yield return new WaitForSeconds(1f);
		Smash();
		yield return new WaitForSeconds(0.3f);
		ShakeWave();
		yield return new WaitForSeconds(1f);
		BigSmashPreparation();
		yield return new WaitForSeconds(1f);
		Smash();
		yield return new WaitForSeconds(0.3f);
		ShakeWave();
		yield return new WaitForSeconds(1.6f);
		SetMidAir(midAir: true);
		yield return new WaitForSeconds(0.7f);
		base.transform.DOMoveY(base.transform.position.y + 12f, 0.8f).SetEase(Ease.OutCubic);
		yield return new WaitForSeconds(0.1f);
		elderBro.IntroJump();
	}

	public void BigSmashPreparation()
	{
		animator.SetTrigger("PREPARATION");
	}

	public void Smash()
	{
		animator.SetTrigger("SMASH");
	}

	public void SetMidAir(bool midAir)
	{
		animator.SetBool("MID-AIR", midAir);
	}

	public void PlayJump()
	{
		elderBroAudio.PlayDummyJump();
	}

	public void PlayAttack()
	{
		elderBroAudio.PlayAttack();
	}

	public void PlayAttackMove2()
	{
		elderBroAudio.PlayAttackMove2();
	}

	public void PlayAttackMove3()
	{
		elderBroAudio.PlayAttackMove3();
	}
}
