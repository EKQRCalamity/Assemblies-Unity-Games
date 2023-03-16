using System.Collections;
using UnityEngine;

public class OldManLevelSpikeFloor : AbstractCollidableObject
{
	public enum SpikeState
	{
		Idle,
		Spiked,
		Gnomed
	}

	private const float SPIKE_TRIGGER_RANGE = 50f;

	private const float MIN_DISTANCE_TO_STAY_SPIKED = 75f;

	[Header("Death FX")]
	[SerializeField]
	private Effect deathPuff;

	[SerializeField]
	private SpriteDeathParts[] deathParts;

	[Header("Prefabs")]
	[SerializeField]
	private Transform shootRoot;

	[SerializeField]
	private BasicProjectile gnomeProjectile;

	[SerializeField]
	private BasicProjectile gnomePinkProjectile;

	public SpikeState spikeState;

	private LevelProperties.OldMan.Spikes spikeProperties;

	private LevelProperties.OldMan.Turret gnomeProperties;

	private PatternString gnomeShootPatternString;

	private PatternString gnomePinkPatternString;

	private float hp;

	private DamageReceiver damageReceiver;

	private float shootAngle;

	private Coroutine spikeCR;

	private Coroutine gnomeCR;

	[SerializeField]
	private bool dontShootLeft;

	[SerializeField]
	private bool dontShootRight;

	[SerializeField]
	private SpriteRenderer gnomeRenderer;

	[SerializeField]
	private SpriteRenderer tuftRenderer;

	[SerializeField]
	private SpriteRenderer shootFXRenderer;

	private int id;

	private bool exit;

	private bool deathTimeOut;

	protected override void Awake()
	{
		base.Awake();
		damageReceiver = GetComponentInChildren<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	protected override void OnDestroy()
	{
		damageReceiver.OnDamageTaken -= OnDamageTaken;
		base.OnDestroy();
		WORKAROUND_NullifyFields();
	}

	public void SetID(int i)
	{
		id = i;
		base.animator.SetInteger("Variant", i % 4);
		gnomeRenderer.flipX = i % 8 > 3;
		tuftRenderer.flipX = gnomeRenderer.flipX;
	}

	private string AnimSuffix()
	{
		return (id % 4) switch
		{
			0 => "A", 
			1 => "B", 
			2 => "C", 
			_ => "D", 
		};
	}

	private string PopStartSuffix()
	{
		return (id % 4) switch
		{
			0 => "A_C", 
			1 => "B", 
			2 => "A_C", 
			_ => "D", 
		};
	}

	private string PopWarningSuffix()
	{
		return (id % 4) switch
		{
			0 => "A_C", 
			1 => "B_D", 
			2 => "A_C", 
			_ => "B_D", 
		};
	}

	private void Update()
	{
		if (spikeState != SpikeState.Gnomed && spikeState != SpikeState.Spiked && !deathTimeOut && MinDistanceToPlayer(base.transform.position) < 50f)
		{
			ChangeState(SpikeState.Spiked);
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (spikeState == SpikeState.Gnomed)
		{
			hp -= info.damage;
			if (hp <= 0f)
			{
				ChangeState(SpikeState.Idle);
				Level.Current.RegisterMinionKilled();
				Dead();
			}
		}
	}

	public void SetProperties(LevelProperties.OldMan properties)
	{
		spikeProperties = properties.CurrentState.spikes;
		gnomeProperties = properties.CurrentState.turret;
		gnomeShootPatternString = new PatternString(gnomeProperties.attackString);
		gnomePinkPatternString = new PatternString(gnomeProperties.pinkShotString);
		ChangeState(SpikeState.Idle);
	}

	public void SpawnGnome()
	{
		ChangeState(SpikeState.Gnomed);
	}

	private void ChangeState(SpikeState state)
	{
		if (exit || (spikeState != 0 && state != 0))
		{
			return;
		}
		if (gnomeCR != null)
		{
			StopCoroutine(gnomeCR);
		}
		if (spikeCR != null)
		{
			StopCoroutine(spikeCR);
		}
		base.animator.ResetTrigger("OnPimple");
		base.animator.ResetTrigger("OnPop");
		base.animator.ResetTrigger("OnWarning");
		base.animator.SetBool("IsAttacking", value: false);
		spikeState = state;
		switch (state)
		{
		case SpikeState.Gnomed:
			gnomeCR = StartCoroutine(gnome_up_cr());
			break;
		case SpikeState.Idle:
			if (!base.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle_" + AnimSuffix()))
			{
				StartCoroutine(restart_idle_cr());
			}
			break;
		case SpikeState.Spiked:
			spikeCR = StartCoroutine(spike_up_cr());
			break;
		}
	}

	private IEnumerator restart_idle_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, Random.Range(0.5f, 1f));
		base.animator.Play("Restart_Idle_" + PopStartSuffix());
		deathTimeOut = false;
	}

	private float MinDistanceToPlayer(Vector3 pos)
	{
		float num = float.MaxValue;
		AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		AbstractPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		if (player != null)
		{
			float num2 = Vector3.SqrMagnitude(pos - player.transform.position);
			if (num2 < num)
			{
				num = num2;
			}
		}
		if (player2 != null)
		{
			float num3 = Vector3.SqrMagnitude(pos - player2.transform.position);
			if (num3 < num)
			{
				num = num3;
			}
		}
		return Mathf.Sqrt(num);
	}

	private IEnumerator gnome_up_cr()
	{
		hp = gnomeProperties.hp;
		base.animator.SetTrigger("OnPimple");
		yield return base.animator.WaitForAnimationToEnd(this, "Pop_Start_" + PopStartSuffix());
		float t3 = 0f;
		while (t3 < gnomeProperties.appearWarning && !exit)
		{
			t3 += (float)CupheadTime.Delta;
			yield return null;
		}
		yield return null;
		t3 = 0f;
		while (t3 < gnomeProperties.spawnSecondaryBuffer && MinDistanceToPlayer(base.transform.position) < gnomeProperties.spawnDistanceCheck && !exit)
		{
			t3 += (float)CupheadTime.Delta;
			yield return null;
		}
		base.animator.SetTrigger("OnPop");
		yield return null;
		while (spikeState == SpikeState.Gnomed)
		{
			t3 = 0f;
			while (t3 < gnomeProperties.shotDelay && !exit)
			{
				t3 += (float)CupheadTime.Delta;
				yield return null;
			}
			base.animator.SetBool("IsAttacking", value: true);
			shootAngle = gnomeShootPatternString.PopFloat();
			if (shootAngle != 0f && ((shootAngle <= 180f && dontShootLeft) || (shootAngle > 180f && dontShootRight)))
			{
				shootAngle = 360f - shootAngle;
			}
			base.animator.SetBool("Diagonal", shootAngle != 0f);
			t3 = 0f;
			while (t3 < gnomeProperties.warningDuration && !exit)
			{
				t3 += (float)CupheadTime.Delta;
				yield return null;
			}
			yield return null;
			if (!exit)
			{
				base.transform.localScale = new Vector3((!((shootAngle > 180f) ^ gnomeRenderer.flipX)) ? 1 : (-1), 1f);
			}
			base.animator.SetBool("IsAttacking", value: false);
			yield return new WaitForEndOfFrame();
		}
		yield return null;
	}

	private float MinPlayerDistance()
	{
		float num = float.MaxValue;
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (!(allPlayer == null) && !(allPlayer.transform.position.y > base.transform.position.y + 200f))
			{
				float num2 = Mathf.Abs(base.transform.position.x - allPlayer.transform.position.x);
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	private IEnumerator spike_up_cr()
	{
		if (!((OldManLevel)Level.Current).playedFirstSpikeSound)
		{
			SFX_OMM_Gnome_SpikeRaiseFirst();
			((OldManLevel)Level.Current).playedFirstSpikeSound = true;
		}
		base.transform.GetChild(0).gameObject.tag = "EnemyProjectile";
		base.animator.SetTrigger("OnWarning");
		yield return base.animator.WaitForAnimationToEnd(this, "Warning_Start_" + AnimSuffix());
		float t2 = 0f;
		while (t2 < spikeProperties.warningDuration && !exit)
		{
			t2 += (float)CupheadTime.Delta;
			yield return null;
		}
		base.animator.SetBool("IsAttacking", value: true);
		t2 = 0f;
		while (t2 < spikeProperties.attackDuration && !exit)
		{
			t2 += (float)CupheadTime.Delta;
			yield return null;
		}
		while (MinPlayerDistance() < 75f && !exit)
		{
			yield return null;
		}
		yield return null;
		base.animator.SetBool("IsAttacking", value: false);
		yield return base.animator.WaitForAnimationToStart(this, "Idle_" + AnimSuffix());
		ChangeState(SpikeState.Idle);
		base.transform.GetChild(0).gameObject.tag = "Enemy";
		yield return null;
	}

	public void Dead()
	{
		Vector3 position = new Vector3(shootRoot.position.x, shootRoot.position.y - 100f);
		deathPuff.Create(position);
		base.animator.Play("None");
		for (int i = 0; i < deathParts.Length; i++)
		{
			if (i != 0 || Random.Range(0, 10) == 0)
			{
				deathParts[i].CreatePart(position);
			}
		}
		deathTimeOut = true;
		AudioManager.Play("sfx_dlc_omm_gnome_death");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_death");
	}

	public void Exit()
	{
		StartCoroutine(exit_cr());
	}

	private IEnumerator exit_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, Random.Range(0f, 1f));
		exit = true;
		base.animator.SetBool("Dead", value: true);
		yield return base.animator.WaitForAnimationToStart(this, "None");
		Object.Destroy(base.gameObject);
	}

	private void AniEvent_ShootProjectile()
	{
		BasicProjectile basicProjectile = ((gnomePinkPatternString.PopLetter() != 'P') ? gnomeProjectile : gnomePinkProjectile);
		if (shootAngle == 0f)
		{
			basicProjectile.Create(shootRoot.position, shootAngle, gnomeProperties.shotSpeed);
			shootFXRenderer.transform.eulerAngles = Vector3.zero;
			shootFXRenderer.transform.localPosition = Vector3.up * 18f;
		}
		else
		{
			basicProjectile.Create(shootRoot.position + Vector3.right * 40f * Mathf.Sign(shootAngle - 180f), shootAngle, gnomeProperties.shotSpeed);
			shootFXRenderer.transform.eulerAngles = new Vector3(0f, 0f, 40f * Mathf.Sign(shootAngle) * (float)((!(shootAngle > 180f)) ? 1 : (-1)));
			shootFXRenderer.transform.localPosition = new Vector3(30.5f * (float)((!gnomeRenderer.flipX) ? 1 : (-1)), 33f);
		}
		AudioManager.Play("sfx_dlc_omm_gnome_shoot_projectile");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_shoot_projectile");
	}

	private void SFX_OMM_Gnome_SpikeRaiseFirst()
	{
		AudioManager.Play("sfx_dlc_omm_gnome_spike_raisefirst");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_spike_raisefirst");
	}

	private void AnimationEvent_SFX_OMM_Gnome_SpikeRaise()
	{
		AudioManager.Play("sfx_dlc_omm_gnome_spike_raise");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_spike_raise");
	}

	private void AnimationEvent_SFX_OMM_Gnome_SpikeRetract()
	{
		AudioManager.Play("sfx_dlc_omm_gnome_spike_retract");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_spike_retract");
	}

	private void AnimationEvent_SFX_OMM_Gnome_BeardAnticipation()
	{
		AudioManager.Play("sfx_dlc_omm_gnome_beard_anticipation");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_beard_anticipation");
	}

	private void AnimationEvent_SFX_OMM_Gnome_BeardPopup()
	{
		AudioManager.Play("sfx_dlc_omm_gnome_beard_popup");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_beard_popup");
	}

	private void WORKAROUND_NullifyFields()
	{
		deathPuff = null;
		deathParts = null;
		shootRoot = null;
		gnomeProjectile = null;
		gnomePinkProjectile = null;
		gnomeShootPatternString = null;
		gnomePinkPatternString = null;
		spikeCR = null;
		gnomeCR = null;
		gnomeRenderer = null;
		tuftRenderer = null;
		shootFXRenderer = null;
	}
}
