using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.PietyMonster.Attack;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Effects.Player.Dust;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Animation;

public class PietyAnimatorBridge : MonoBehaviour
{
	public PietyMonster PietyMonster;

	private CameraManager _cameraManager;

	public bool AllowWalkCameraShake { get; set; }

	private void Start()
	{
		_cameraManager = Core.Logic.CameraManager;
	}

	public void StompCameraShake()
	{
		if (!(_cameraManager == null))
		{
			_cameraManager.ProCamera2DShake.ShakeUsingPreset("PietyStomp");
			SmashRumble();
		}
	}

	public void WalkCameraShake()
	{
		if (!(_cameraManager == null))
		{
			if (AllowWalkCameraShake)
			{
				_cameraManager.ProCamera2DShake.ShakeUsingPreset("PietyStep");
			}
			StepRumble();
		}
	}

	public void StepRumble()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Rumble.UsePreset("Step");
		}
	}

	public void SmashRumble()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Rumble.UsePreset("Smash");
		}
	}

	public void StompAttack()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.PietyBehaviour.StompAttack.CurrentWeaponAttack();
		}
	}

	public void ClawAttack()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.PietyBehaviour.ClawAttack.CurrentWeaponAttack();
		}
	}

	public void SmashAttack()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.PietyBehaviour.SmashAttack.CurrentWeaponAttack();
		}
	}

	public void GetStepDust()
	{
		if (AllowWalkCameraShake && PietyMonster.DustSpawner != null)
		{
			StepDustRoot stepDustRoot = PietyMonster.DustSpawner.StepDustRoot;
			Vector2 stepDustPosition = stepDustRoot.transform.position;
			Vector2 vector = stepDustRoot.transform.localPosition;
			stepDustPosition.x = ((PietyMonster.Status.Orientation != EntityOrientation.Left) ? stepDustPosition.x : (stepDustPosition.x - vector.x * 2f));
			PietyMonster.DustSpawner.CurrentStepDustSpawn = StepDust.StepDustType.Running;
			PietyMonster.DustSpawner.GetStepDust(stepDustPosition);
			PlayStepStomp();
		}
	}

	public void ReadyToAttack()
	{
	}

	public void StompAttackResizeDamageArea()
	{
		Vector2 attackDamageAreaOffset = PietyMonster.PietyBehaviour.StompAttack.AttackDamageAreaOffset;
		Vector2 attackDamageAreaSize = PietyMonster.PietyBehaviour.StompAttack.AttackDamageAreaSize;
		BoxCollider2D boxCollider2D = (BoxCollider2D)PietyMonster.DamageArea.DamageAreaCollider;
		boxCollider2D.offset = attackDamageAreaOffset;
		boxCollider2D.size = attackDamageAreaSize;
	}

	public void SetDefaultDamageArea()
	{
		BoxCollider2D boxCollider2D = (BoxCollider2D)PietyMonster.DamageArea.DamageAreaCollider;
		boxCollider2D.size = PietyMonster.PietyBehaviour.StompAttack.DefaultDamageAreaSize;
		boxCollider2D.offset = PietyMonster.PietyBehaviour.StompAttack.DefaultDamageAreaOffset;
	}

	public void LaunchRoots()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.PietyRootsManager.EnableNearestRoots();
		}
	}

	public void LaunchDominoRoots()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.PietyRootsManager.EnableDominoRoots();
		}
	}

	public void Spit()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.PietyBehaviour.SpitAttack.Spit();
			PietyMonster.PietyBehaviour.SpitAttack.Spitsthrown++;
			int currentSpitAmount = PietyMonster.PietyBehaviour.SpitAttack.CurrentSpitAmount;
			if (PietyMonster.PietyBehaviour.SpitAttack.Spitsthrown >= currentSpitAmount)
			{
				PietyMonster.PietyBehaviour.SpitAttack.Spitsthrown = 0;
				PietyMonster.PietyBehaviour.StompAttackCounter = 0;
				PietyMonster.AnimatorInyector.StopSpiting();
				PietyMonster.PietyBehaviour.Spiting = false;
			}
		}
	}

	public void DestroyBushes()
	{
		PietyBushManager pietyBushManager = Object.FindObjectOfType<PietyBushManager>();
		if (pietyBushManager != null)
		{
			pietyBushManager.DestroyBushes();
		}
	}

	public void PlayStep()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Audio.PlayWalk();
		}
	}

	public void PlayStepStomp()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Audio.PlayStepStomp();
		}
	}

	public void Turn()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Audio.PlayTurn();
		}
	}

	public void PlayStop()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Audio.PlayStop();
		}
	}

	public void PlaySlash()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Audio.PlaySlash();
		}
	}

	public void PlayStomp()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Audio.PlayStomp();
		}
	}

	public void PlayDeath()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Audio.PlayDead();
		}
	}

	public void PlayScream()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Audio.PlayScream();
		}
	}

	public void PlaySmash()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Audio.PlaySmash();
		}
	}

	public void PlayGetUp()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Audio.PlayGetUp();
		}
	}

	public void ReadyToSpit()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Audio.ReadyToSpit();
		}
	}

	public void PlaySpit()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Audio.Spit();
		}
	}

	public void PlayStompHit()
	{
		if (!(PietyMonster == null))
		{
			PietyMonster.Audio.PlayStompHit();
		}
	}
}
