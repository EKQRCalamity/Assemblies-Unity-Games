using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirplaneLevelLeader : LevelProperties.Airplane.Entity
{
	private const float PAW_MOVE_X = 750f;

	private const float BULLET_SPAWN_X = 380f;

	[Header("Spawn Positions")]
	[SerializeField]
	private Transform[] laserPositions;

	[SerializeField]
	private Transform rocketSpawnLeft;

	[SerializeField]
	private Transform rocketSpawnRight;

	[SerializeField]
	private Transform yellowPosSideways;

	[SerializeField]
	private Transform redPosSideways;

	[SerializeField]
	private Transform flashRootLeft;

	[SerializeField]
	private Transform flashRootRight;

	[SerializeField]
	private Transform leftDogBowlSpawn;

	[SerializeField]
	private Transform rightDogBowlSpawn;

	[SerializeField]
	private AnimationClip buildLaserAni;

	[Header("Prefabs")]
	[SerializeField]
	private AirplaneLevelRocket rocketPrefab;

	[SerializeField]
	private AirplaneLevelDropBullet yellowBullet;

	[SerializeField]
	private AirplaneLevelDropBullet redBullet;

	[SerializeField]
	private Animator[] laserAnimator;

	[SerializeField]
	private Effect flashEffect;

	private List<int> lasersToShoot;

	private List<int> lasersNextToShoot;

	private bool[] laserOut = new bool[5];

	private float[] laserDeathTime = new float[5]
	{
		0.2f,
		0.6f,
		0.8f,
		4f / 15f,
		0.4f
	};

	private PatternString bulletDelayString;

	private PatternString bulletColorString;

	private int laserPositionStringsMainIndex;

	private bool isDead;

	private DamageReceiver damageReceiver;

	[SerializeField]
	private LevelBossDeathExploder rotatedExploder;

	[SerializeField]
	private LevelBossDeathExploder pawRightExploder;

	[SerializeField]
	private LevelBossDeathExploder pawLeftExploder;

	[SerializeField]
	private List<Animator> deathPuffs;

	public bool IsAttacking { get; private set; }

	public bool camRotatedHorizontally { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	protected override void OnDestroy()
	{
		damageReceiver.OnDamageTaken -= OnDamageTaken;
		base.OnDestroy();
		WORKAROUND_NullifyFields();
	}

	public override void LevelInit(LevelProperties.Airplane properties)
	{
		base.LevelInit(properties);
		bulletDelayString = new PatternString(properties.CurrentState.dropshot.bulletDelayStrings);
		bulletColorString = new PatternString(properties.CurrentState.dropshot.bulletColorString);
		laserPositionStringsMainIndex = Random.Range(0, properties.CurrentState.laser.laserPositionStrings.Length);
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (!((AirplaneLevel)Level.Current).Rotating)
		{
			base.properties.DealDamage(info.damage);
			if (base.properties.CurrentHealth <= 0f && !isDead)
			{
				StopAllCoroutines();
				StartCoroutine(death_cr());
			}
		}
	}

	public void StartLeader()
	{
		base.animator.Play("Intro");
	}

	public void RotateCamera()
	{
		camRotatedHorizontally = !camRotatedHorizontally;
	}

	private void AniEvent_PawGrab()
	{
		CupheadLevelCamera.Current.Shake(30f, 0.8f);
		((AirplaneLevel)Level.Current).MoveBoundsIn();
		((AirplaneLevel)Level.Current).BlurBGCamera();
	}

	private void AniEvent_StartButtonPush()
	{
		if (base.animator.GetCurrentAnimatorStateInfo(3).IsName("Push_Wait"))
		{
			base.animator.Play("Push_Start", 3, 0f);
			base.animator.Update(0f);
			AudioManager.Play("sfx_dlc_dogfight_leadervocal_buttonbashbegin");
			emitAudioFromObject.Add("sfx_dlc_dogfight_leadervocal_buttonbashbegin");
		}
	}

	private void LateUpdate()
	{
		if (base.animator.GetCurrentAnimatorStateInfo(3).IsName("Push") && base.animator.GetCurrentAnimatorStateInfo(0).IsName("Sideways_Idle") && base.animator.GetCurrentAnimatorStateInfo(3).normalizedTime != base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime)
		{
			base.animator.Play("Push", 3, base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
			base.animator.Update(0f);
		}
	}

	public void StartDropshot()
	{
		StartCoroutine(drop_shot_cr());
	}

	private IEnumerator drop_shot_cr()
	{
		IsAttacking = true;
		LevelProperties.Airplane.Dropshot p = base.properties.CurrentState.dropshot;
		bulletDelayString.SetSubStringIndex(0);
		AirplaneLevelDropBullet[] bullets = new AirplaneLevelDropBullet[bulletDelayString.SubStringLength()];
		bool onLeft = true;
		AudioManager.PlayLoop("sfx_dlc_dogfight_p3_leader_buttonpresses_loop");
		for (int i = 0; i < bullets.Length; i++)
		{
			yield return CupheadTime.WaitForSeconds(this, bulletDelayString.PopFloat());
			bool isRed = bulletColorString.PopLetter() == 'R';
			AirplaneLevelDropBullet bullet = ((!isRed) ? yellowBullet : redBullet);
			Vector3 pos = Vector3.zero;
			pos.x = ((!onLeft) ? 380f : (-380f));
			pos.y = ((!isRed) ? yellowPosSideways.position.y : redPosSideways.position.y);
			Transform startPos = ((!onLeft) ? rightDogBowlSpawn : leftDogBowlSpawn);
			AirplaneLevelDropBullet b = bullet.Spawn();
			b.Init(pos, startPos.position, p.bulletDropSpeed, p.bulletShootSpeed, onLeft, camRotatedHorizontally);
			bullets[i] = b;
			AudioManager.Play("sfx_dlc_dogfight_p3_dogcopter_dogbowl_fire");
			Transform flashPos = ((!onLeft) ? flashRootRight : flashRootLeft);
			Effect flash = flashEffect.Create(flashPos.position, flashPos.localScale);
			flash.transform.rotation = flashPos.rotation;
			onLeft = !onLeft;
		}
		base.animator.SetTrigger("EndButtonPush");
		AudioManager.FadeSFXVolume("sfx_dlc_dogfight_p3_leader_buttonpresses_loop", 0f, 0.25f);
		bool stillAttacking = true;
		while (stillAttacking)
		{
			bool bulletsAlive = false;
			AirplaneLevelDropBullet[] array = bullets;
			foreach (AirplaneLevelDropBullet airplaneLevelDropBullet in array)
			{
				if (airplaneLevelDropBullet.isMoving)
				{
					bulletsAlive = true;
				}
			}
			if (!bulletsAlive)
			{
				stillAttacking = false;
				break;
			}
			yield return null;
		}
		IsAttacking = false;
		yield return null;
	}

	public void OpenPawHoles()
	{
		for (int i = 0; i < laserAnimator.Length; i++)
		{
			laserAnimator[i].Play("SecretOpen");
		}
	}

	public void StartLaser()
	{
		StartCoroutine(laser_main_cr());
	}

	private List<int> GetLasersToShoot(string[] lasers)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < lasers.Length; i++)
		{
			list.Add(lasers[i][0] - 65);
		}
		return list;
	}

	private IEnumerator laser_main_cr()
	{
		IsAttacking = true;
		LevelProperties.Airplane.Laser p = base.properties.CurrentState.laser;
		string[] laserPositionStrings = p.laserPositionStrings[laserPositionStringsMainIndex].Split(',');
		for (int i = 0; i < laserPositionStrings.Length; i++)
		{
			string[] lasers = laserPositionStrings[i].Split(':');
			lasersToShoot = GetLasersToShoot(lasers);
			if (i + 1 < laserPositionStrings.Length)
			{
				string[] lasers2 = laserPositionStrings[i + 1].Split(':');
				lasersNextToShoot = GetLasersToShoot(lasers2);
			}
			else
			{
				lasersNextToShoot = new List<int>();
			}
			StartCoroutine(fire_lasers_cr(lasersToShoot, lasersNextToShoot, i));
			yield return CupheadTime.WaitForSeconds(this, buildLaserAni.length + p.laserHesitation + p.warningTime + p.laserDuration + p.laserDelay);
		}
		yield return CupheadTime.WaitForSeconds(this, buildLaserAni.length);
		laserPositionStringsMainIndex = (laserPositionStringsMainIndex + 1) % p.laserPositionStrings.Length;
		IsAttacking = false;
		yield return null;
	}

	private IEnumerator fire_lasers_cr(List<int> lasers, List<int> lasersNext, int round)
	{
		LevelProperties.Airplane.Laser p = base.properties.CurrentState.laser;
		for (int i = 0; i < lasers.Count; i++)
		{
			if (!laserOut[lasers[i]])
			{
				laserAnimator[lasers[i]].Play("In");
			}
			laserOut[lasers[i]] = true;
		}
		AudioManager.Play("sfx_dlc_dogfight_p3_dogcopter_laser_buildout");
		yield return CupheadTime.WaitForSeconds(this, buildLaserAni.length);
		yield return CupheadTime.WaitForSeconds(this, p.laserHesitation);
		for (int j = 0; j < lasers.Count; j++)
		{
			laserAnimator[lasers[j]].Play("WarningStart");
		}
		AudioManager.Play("sfx_dlc_dogfight_p3_dogcopter_laser_prefire_warning");
		yield return CupheadTime.WaitForSeconds(this, p.warningTime);
		for (int k = 0; k < lasers.Count; k++)
		{
			laserAnimator[lasers[k]].Play("FireStart");
		}
		AudioManager.Play("sfx_dlc_dogfight_p3_dogcopter_laser_fire");
		yield return CupheadTime.WaitForSeconds(this, p.laserDuration);
		bool puttingAtLeastOneLaserAway = false;
		for (int l = 0; l < lasers.Count; l++)
		{
			laserOut[lasers[l]] = lasersNext.Contains(lasers[l]) && !p.forceHide;
			laserAnimator[lasers[l]].SetBool("StayOut", laserOut[lasers[l]]);
			laserAnimator[lasers[l]].Play("End");
			if (!laserOut[lasers[l]])
			{
				puttingAtLeastOneLaserAway = true;
			}
		}
		if (puttingAtLeastOneLaserAway)
		{
			AudioManager.Play("sfx_dlc_dogfight_p3_dogcopter_laser_unbuild");
		}
	}

	private IEnumerator rocket_cr()
	{
		LevelProperties.Airplane.Rocket p = base.properties.CurrentState.rocket;
		int delayMainIndex = Random.Range(0, p.attackDelayString.Length);
		string[] delayString = p.attackDelayString[delayMainIndex].Split(',');
		int delayIndex = Random.Range(0, delayString.Length);
		int dirMainIndex = Random.Range(0, p.attackOrderString.Length);
		string[] dirString2 = p.attackOrderString[dirMainIndex].Split(',');
		int dirIndex = Random.Range(0, dirString2.Length);
		while (true)
		{
			delayString = p.attackDelayString[delayMainIndex].Split(',');
			dirString2 = p.attackOrderString[dirMainIndex].Split(',');
			int delay = 0;
			Parser.IntTryParse(delayString[delayIndex], out delay);
			yield return CupheadTime.WaitForSeconds(this, delay);
			Vector3 position = ((dirString2[dirIndex][0] != 'R') ? rocketSpawnLeft.position : rocketSpawnRight.position);
			rocketPrefab.Create(PlayerManager.GetNext(), position, p.homingSpeed, p.homingRotation, p.homingHP, p.homingTime);
			if (dirIndex < dirString2.Length - 1)
			{
				dirIndex++;
			}
			else
			{
				dirMainIndex = (dirMainIndex + 1) % p.attackOrderString.Length;
			}
			if (delayIndex < delayString.Length - 1)
			{
				delayIndex++;
			}
			else
			{
				delayMainIndex = (delayMainIndex + 1) % p.attackDelayString.Length;
			}
			yield return null;
		}
	}

	private IEnumerator death_cr()
	{
		isDead = true;
		GetComponent<BoxCollider2D>().enabled = false;
		rotatedExploder.enabled = camRotatedHorizontally;
		pawRightExploder.enabled = !camRotatedHorizontally;
		pawLeftExploder.enabled = !camRotatedHorizontally;
		base.animator.Play(camRotatedHorizontally ? "Copter_Death_Closeup" : "Copter_Death", base.animator.GetLayerIndex("Death"));
		if (!camRotatedHorizontally)
		{
			base.animator.Play("Off");
			base.animator.Play("Blades", base.animator.GetLayerIndex("DeathBlades"));
		}
		else
		{
			base.animator.Play("Death_Closeup", 3);
			base.animator.Play("SidewaysTears", 4);
		}
		AudioManager.Play("sfx_dlc_dogfight_leadervocal_death");
		base.animator.Update(0f);
		StartCoroutine(activate_death_puffs_cr());
		if (camRotatedHorizontally)
		{
			yield break;
		}
		for (int i = 0; i < laserAnimator.Length; i++)
		{
			if (!laserAnimator[i].GetCurrentAnimatorStateInfo(0).IsName("Off"))
			{
				laserAnimator[i].Play((i != 2) ? "Out" : "Dead", 0, laserDeathTime[i]);
			}
			else if (i == 2)
			{
				laserAnimator[i].Play("SecretOpen");
			}
		}
		while (PauseManager.state == PauseManager.State.Paused)
		{
			yield return null;
		}
		for (int j = 0; j < laserAnimator.Length; j++)
		{
			laserAnimator[j].GetComponent<AnimationHelper>().Speed = 1.25f;
		}
		while (!laserAnimator[2].GetCurrentAnimatorStateInfo(0).IsName("HoldOpen"))
		{
			yield return null;
		}
		((AirplaneLevel)Level.Current).LeaderDeath();
	}

	private IEnumerator activate_death_puffs_cr()
	{
		while (deathPuffs.Count > 0)
		{
			int i = Random.Range(0, deathPuffs.Count);
			deathPuffs[i].gameObject.SetActive(value: true);
			if (camRotatedHorizontally)
			{
				deathPuffs[i].Play("Sideways");
				deathPuffs[i].Update(0f);
			}
			deathPuffs.RemoveAt(i);
			yield return CupheadTime.WaitForSeconds(this, 1f / 6f);
		}
	}

	private void AnimationEvent_SFX_DOGFIGHT_P3_Dogcopter_ScreenRotateChomp()
	{
		AudioManager.Play("sfx_dlc_dogfight_p3_dogcopter_screenrotate_chomp");
	}

	private void AnimationEvent_SFX_DOGFIGHT_P3_Dogcopter_ScreenRotate()
	{
		AudioManager.Play("sfx_DLC_Dogfight_P3_DogCopter_ScreenRotate");
	}

	private void AnimationEvent_SFX_DOGFIGHT_P3_Dogcopter_GrabScreen()
	{
		AudioManager.Play("sfx_dlc_dogfight_p3_dogcopter_settle_grabscreen");
	}

	private void AnimationEvent_SFX_DOGFIGHT_P3_Dogcopter_Intro()
	{
		AudioManager.Play("sfx_dlc_dogfight_p3_dogcopter_intro");
	}

	private void AnimationEvent_SFX_DOGFIGHT_P3_Dogcopter_Intro2()
	{
		AudioManager.Play("sfx_dlc_dogfight_p3_dogcopter_intro2");
	}

	private void AnimationEvent_SFX_DOGFIGHT_P3_LeaderVocalEnd()
	{
		AudioManager.Play("sfx_dlc_dogfight_leadervocal_command");
	}

	private void WORKAROUND_NullifyFields()
	{
		laserPositions = null;
		rocketSpawnLeft = null;
		rocketSpawnRight = null;
		yellowPosSideways = null;
		redPosSideways = null;
		flashRootLeft = null;
		flashRootRight = null;
		leftDogBowlSpawn = null;
		rightDogBowlSpawn = null;
		buildLaserAni = null;
		rocketPrefab = null;
		yellowBullet = null;
		redBullet = null;
		laserAnimator = null;
		flashEffect = null;
		lasersToShoot = null;
		lasersNextToShoot = null;
		laserOut = null;
		laserDeathTime = null;
		bulletDelayString = null;
		bulletColorString = null;
		rotatedExploder = null;
		pawRightExploder = null;
		pawLeftExploder = null;
		deathPuffs = null;
	}
}
