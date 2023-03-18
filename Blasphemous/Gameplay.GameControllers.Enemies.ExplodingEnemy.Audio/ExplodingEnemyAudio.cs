using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.ExplodingEnemy.Audio;

public class ExplodingEnemyAudio : EntityAudio
{
	private const string AppearEventKey = "ExplodingEnemyAppear";

	private const string WalkEventKey = "ExplodingEnemyWalk";

	private const string DeathEventKey = "ExplodingEnemyDeath";

	private const string ExplodingEventKey = "ExplodingEnemyExplode";

	public void Appear()
	{
		bool flag = false;
		if (UnityEngine.Camera.main != null)
		{
			Vector3 vector = UnityEngine.Camera.main.WorldToViewportPoint(Owner.transform.position);
			vector.x = Mathf.Clamp01(vector.x);
			vector.y = Mathf.Clamp01(vector.y);
			flag = vector.x > 0f && vector.x < 1f && vector.y > 0f && vector.y < 1f;
		}
		if (flag)
		{
			PlayOneShotEvent("ExplodingEnemyAppear", FxSoundCategory.Motion);
		}
	}

	public void PlayDeath()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("ExplodingEnemyDeath", FxSoundCategory.Damage);
		}
	}

	public void PlayDeathExplosion()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("ExplodingEnemyExplode", FxSoundCategory.Damage);
		}
	}

	public void PlayWalk()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("ExplodingEnemyWalk", FxSoundCategory.Motion);
		}
	}
}
