using System.Collections;
using UnityEngine;

public class PlaneSuperBomb : AbstractPlaneSuper
{
	private bool earlyExplosion;

	private Coroutine boomRoutine;

	[SerializeField]
	private Transform boom;

	[SerializeField]
	private Transform boomMM;

	protected override void StartSuper()
	{
		base.StartSuper();
		player.damageReceiver.OnDamageTaken += OnDamageTaken;
		player.stats.OnStoned += OnStoned;
		if ((player.id == PlayerId.PlayerOne && !PlayerManager.player1IsMugman) || (player.id == PlayerId.PlayerTwo && PlayerManager.player1IsMugman))
		{
			boom.gameObject.SetActive(value: true);
		}
		else
		{
			boomMM.gameObject.SetActive(value: true);
		}
	}

	private IEnumerator super_cr()
	{
		float t = 0f;
		damageDealer = new DamageDealer(WeaponProperties.PlaneSuperBomb.damage, WeaponProperties.PlaneSuperBomb.damageRate, DamageDealer.DamageSource.Super, damagesPlayer: false, damagesEnemy: true, damagesOther: true);
		damageDealer.DamageMultiplier *= PlayerManager.DamageMultiplier;
		damageDealer.PlayerId = player.id;
		MeterScoreTracker tracker = new MeterScoreTracker(MeterScoreTracker.Type.Super);
		tracker.Add(damageDealer);
		while (t < WeaponProperties.PlaneSuperBomb.countdownTime && !earlyExplosion)
		{
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		Fire();
		if (player != null)
		{
			player.PauseAll();
			player.SetSpriteVisible(visibility: false);
			base.transform.position = player.transform.position;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
		base.animator.SetTrigger("Explode");
		AudioManager.Stop("player_plane_bomb_ticktock_loop");
		AudioManager.Play("player_plane_bomb_explosion");
	}

	private void OnStoned()
	{
		earlyExplosion = true;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		earlyExplosion = true;
	}

	private void EndIntroAnimation()
	{
		StartCountdown();
		AudioManager.PlayLoop("player_plane_bomb_ticktock_loop");
		StartCoroutine(super_cr());
	}

	private void PlayerReappear()
	{
		if (player != null)
		{
			player.UnpauseAll();
			player.SetSpriteVisible(visibility: true);
		}
	}

	private void Die()
	{
		Object.Destroy(base.gameObject);
	}

	private void StartBoomScale()
	{
		boomRoutine = StartCoroutine(boomScale_cr());
	}

	private IEnumerator boomScale_cr()
	{
		float t = 0f;
		float frameTime = 1f / 24f;
		float scale = 1f;
		while (true)
		{
			t += (float)CupheadTime.Delta;
			while (t > frameTime)
			{
				t -= frameTime;
				scale *= 1.15f;
				boom.SetScale(scale, scale);
				boomMM.SetScale(scale, scale);
			}
			yield return null;
		}
	}

	public void Pause()
	{
		if (boomRoutine != null)
		{
			StopCoroutine(boomRoutine);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (player != null)
		{
			player.damageReceiver.OnDamageTaken -= OnDamageTaken;
			player.stats.OnStoned -= OnStoned;
		}
	}

	private void PlaneSuperBombLaughAudio()
	{
		AudioManager.Play("player_plane_bomb_laugh");
	}
}
