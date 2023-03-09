using System.Collections;
using UnityEngine;

public class PlayerSuperChaliceShield : AbstractPlayerSuper
{
	[SerializeField]
	private Vector3 shadowOffset;

	[SerializeField]
	private GameObject shieldHeartPrefab;

	private GameObject shieldHeart;

	[SerializeField]
	private Transform shieldHeartSpawnPos;

	private PlayerSuperChaliceShieldHeart shieldHeartScript;

	protected override void Awake()
	{
		base.Awake();
		base.tag = "Untagged";
	}

	protected override void StartSuper()
	{
		player.weaponManager.OnSuperStart -= player.motor.StartSuper;
		if (player.motor.Grounded)
		{
			player.weaponManager.OnSuperEnd -= player.motor.OnSuperEnd;
		}
		base.StartSuper();
		AudioManager.Play("player_super_chalice_shield");
		StartCoroutine(super_cr());
		Level.ScoringData.superMeterUsed += 5;
	}

	private void CreateHeart()
	{
		shieldHeart = Object.Instantiate(shieldHeartPrefab);
		shieldHeart.transform.position = shieldHeartSpawnPos.position;
		shieldHeartScript = shieldHeart.GetComponent<PlayerSuperChaliceShieldHeart>();
		shieldHeartScript.player = player.transform;
		player.stats.SetChaliceShield(chaliceShield: true);
		player.damageReceiver.Invulnerable(0.1f);
	}

	private void LetPlayerMove()
	{
		Fire();
		EndSuper();
	}

	private IEnumerator super_cr()
	{
		if (!player.motor.Grounded)
		{
			base.animator.Play("SuperAir");
		}
		while ((bool)player && !player.stats.ChaliceShieldOn)
		{
			yield return null;
		}
		while ((bool)player && player.stats.ChaliceShieldOn)
		{
			yield return null;
		}
		shieldHeartScript.Destroy();
		if ((bool)player)
		{
			player.damageReceiver.OnRevive(Vector3.zero);
		}
		Object.Destroy(base.gameObject);
	}
}
