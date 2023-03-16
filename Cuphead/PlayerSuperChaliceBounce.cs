using System;
using System.Collections;
using UnityEngine;

public class PlayerSuperChaliceBounce : AbstractPlayerSuper
{
	[NonSerialized]
	public bool LAUNCHED_VERSION = WeaponProperties.LevelSuperChaliceBounce.launchedVersion;

	private float DAMAGE = WeaponProperties.LevelSuperChaliceBounce.damage;

	private float DAMAGE_RATE = WeaponProperties.LevelSuperChaliceBounce.damageRate;

	private float DURATION = WeaponProperties.LevelSuperChaliceBounce.duration;

	[SerializeField]
	private PlayerSuperChaliceBounceBall ball;

	public float timer;

	protected override void Start()
	{
		base.Start();
	}

	protected override void StartSuper()
	{
		base.StartSuper();
		StartCoroutine(super_cr());
	}

	private IEnumerator super_cr()
	{
		float duration = DURATION;
		timer = duration;
		Fire();
		if (LAUNCHED_VERSION)
		{
			yield return new WaitForEndOfFrame();
			player.animationController.EnableSpriteRenderer();
		}
		while (timer > 0f && !interrupted)
		{
			timer -= CupheadTime.FixedDelta;
			yield return null;
		}
		if (!LAUNCHED_VERSION)
		{
			EndSuper();
			player.transform.position = ball.transform.position;
		}
		CleanUp();
	}

	public void CleanUp()
	{
		UnityEngine.Object.Destroy(ball.gameObject);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected override void Fire()
	{
		PauseManager.Unpause();
		if (!LAUNCHED_VERSION)
		{
			player.PauseAll();
			AnimationHelper component = GetComponent<AnimationHelper>();
			component.IgnoreGlobal = false;
		}
		else
		{
			EndSuper();
			player.stats.OnSuperEnd();
		}
		ball = ball.Create(base.transform.position + Vector3.up * 100f) as PlayerSuperChaliceBounceBall;
		ball.player = player;
		ball.PlayerId = player.id;
		ball.velocity.x = (int)player.motor.MoveDirection.x * 500;
		ball.Damage = DAMAGE;
		ball.DamageRate = DAMAGE_RATE;
		ball.super = this;
	}
}
