using System;
using UnityEngine;

public class ForestPlatformingLevelAcornMaker : PlatformingLevelShootingEnemy
{
	private const float ON_SCREEN_SOUND_PADDING = 100f;

	[SerializeField]
	private Effect explosion;

	[SerializeField]
	private Transform gruntRoot;

	[SerializeField]
	private SpriteRenderer gruntSprite;

	[SerializeField]
	private ForestPlatformingLevelAcorn acornPrefab;

	[SerializeField]
	private Transform spawnRoot;

	private bool isDying;

	public Action killAcorns;

	protected override void Shoot()
	{
		ForestPlatformingLevelAcorn.Direction direction = ForestPlatformingLevelAcorn.Direction.Left;
		direction = ((!(_target.transform.position.x < base.transform.position.x)) ? ForestPlatformingLevelAcorn.Direction.Right : ForestPlatformingLevelAcorn.Direction.Left);
		acornPrefab.Spawn(this, spawnRoot.transform.position, direction, moveUpFirst: true);
	}

	protected override void Die()
	{
		if (!isDying)
		{
			if (killAcorns != null)
			{
				killAcorns();
			}
			base.animator.SetTrigger("Death");
			Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			isDying = true;
			explosion.Create(gruntRoot.transform.position);
			gruntSprite.enabled = false;
		}
		else
		{
			base.Die();
		}
	}

	private void PlayGruntSFX()
	{
		if (CupheadLevelCamera.Current.ContainsPoint(base.transform.position, new Vector2(100f, 1000f)))
		{
			AudioManager.Play("level_acorn_maker_grunt");
			emitAudioFromObject.Add("level_acorn_maker_grunt");
		}
	}

	private void PlayIdleSFX()
	{
		if (CupheadLevelCamera.Current.ContainsPoint(base.transform.position, new Vector2(100f, 1000f)))
		{
			AudioManager.Play("level_acorn_maker_idle");
			emitAudioFromObject.Add("level_acorn_maker_idle");
		}
	}

	private void PlayDeathSFX()
	{
		AudioManager.Play("level_acorn_maker_death");
		emitAudioFromObject.Add("level_acorn_maker_death");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		acornPrefab = null;
		explosion = null;
	}
}
