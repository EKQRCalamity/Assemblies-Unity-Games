using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.DrownedCorpse.Animator;

public class DrownedCorpseAnimatorInjector : EnemyAnimatorInyector
{
	public const float VanishAfterRunAnimDuration = 0.75f;

	private DrownedCorpse _drownedCorpse;

	private static readonly int RunParam = UnityEngine.Animator.StringToHash("RUN");

	private static readonly int Vanish = UnityEngine.Animator.StringToHash("VANISH");

	private static readonly int Damage = UnityEngine.Animator.StringToHash("DAMAGE");

	protected override void OnStart()
	{
		base.OnStart();
		_drownedCorpse = (DrownedCorpse)OwnerEntity;
		AnimEvent_DisableDamageArea();
	}

	public void ShowHelmet()
	{
		if ((bool)_drownedCorpse)
		{
			_drownedCorpse.Helmet.gameObject.SetActive(value: true);
			_drownedCorpse.Helmet.transform.position = OwnerEntity.transform.position;
		}
	}

	public void HideHelmet()
	{
		if ((bool)_drownedCorpse)
		{
			_drownedCorpse.Helmet.gameObject.SetActive(value: false);
		}
	}

	public void Awake()
	{
		if ((bool)_drownedCorpse)
		{
			_drownedCorpse.Animator.SetBool(Vanish, value: false);
		}
	}

	public void Run()
	{
		if ((bool)_drownedCorpse)
		{
			_drownedCorpse.Animator.SetBool(RunParam, value: true);
		}
	}

	public void DontRun()
	{
		if ((bool)_drownedCorpse)
		{
			_drownedCorpse.Animator.SetBool(RunParam, value: false);
		}
	}

	public void VanishAfterRun()
	{
		if ((bool)_drownedCorpse)
		{
			_drownedCorpse.Animator.SetBool(Vanish, value: true);
		}
	}

	public void VanishAfterDamage()
	{
		if ((bool)_drownedCorpse)
		{
			_drownedCorpse.Animator.SetTrigger(Damage);
			_drownedCorpse.Animator.SetBool(Vanish, value: true);
		}
	}

	public void AnimEvent_EnableDamageArea()
	{
		if ((bool)_drownedCorpse)
		{
			_drownedCorpse.DamageArea.DamageAreaCollider.enabled = true;
		}
	}

	public void AnimEvent_DisableDamageArea()
	{
		if ((bool)_drownedCorpse)
		{
			_drownedCorpse.DamageArea.DamageAreaCollider.enabled = false;
		}
	}
}
