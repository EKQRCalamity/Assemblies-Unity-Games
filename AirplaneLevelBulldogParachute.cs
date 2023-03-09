using System.Collections;
using UnityEngine;

public class AirplaneLevelBulldogParachute : LevelProperties.Airplane.Entity
{
	private float[] shotYPos = new float[3] { 100f, -50f, -200f };

	[SerializeField]
	private Transform spawnRoot1;

	[SerializeField]
	private Transform spawnRoot2;

	[SerializeField]
	private AirplaneLevelBoomerang boomerang;

	[SerializeField]
	private AirplaneLevelBoomerang boomerangPink;

	private PatternString pinkString;

	private DamageDealer damageDealer;

	[SerializeField]
	private AirplaneLevelBulldogPlane main;

	[SerializeField]
	private Animator mainAnimator;

	private int count;

	[SerializeField]
	private GameObject collider;

	public bool isMoving { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
		pinkString = new PatternString(base.properties.CurrentState.parachute.pinkString);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		WORKAROUND_NullifyFields();
	}

	public override void LevelInit(LevelProperties.Airplane properties)
	{
		base.LevelInit(properties);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	public void StartDescent(Vector2 pos, float scale)
	{
		isMoving = true;
		base.transform.position = pos;
		base.transform.localScale = new Vector3(scale, 1f);
		count = 0;
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		base.animator.Play("Drop");
		base.animator.Update(0f);
		yield return base.animator.WaitForAnimationToEnd(this, "Drop");
		isMoving = false;
	}

	public void EarlyExit()
	{
		StopAllCoroutines();
		StartCoroutine(early_exit_cr());
		if (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.41379312f)
		{
			base.transform.position = new Vector3(base.transform.position.x, collider.transform.localPosition.y + 400f);
			base.animator.Play("Drop", 0, 76f / 87f);
			base.animator.Update(0f);
		}
	}

	private IEnumerator early_exit_cr()
	{
		if (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.41379312f)
		{
			base.animator.Play("Drop", 0, 76f / 87f);
		}
		yield return base.animator.WaitForAnimationToStart(this, "None");
		isMoving = false;
		if (main.isDead && (bool)mainAnimator)
		{
			mainAnimator.SetBool("InParachuteATK", value: false);
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		GetComponent<HitFlash>().StopAllCoroutinesWithoutSettingScale();
		GetComponent<SpriteRenderer>().color = Color.black;
	}

	private void AniEvent_Shoot()
	{
		LevelProperties.Airplane.Parachute parachute = base.properties.CurrentState.parachute;
		Vector3 vector = new Vector3((count != 1) ? spawnRoot1.position.x : spawnRoot2.position.x, shotYPos[count]);
		float delay = ((count == 0) ? parachute.shotAReturnDelay.RandomFloat() : ((count != 1) ? parachute.shotCReturnDelay.RandomFloat() : parachute.shotBReturnDelay.RandomFloat()));
		if (pinkString.PopLetter() == 'P')
		{
			boomerangPink.Create(vector, parachute.speedForward, parachute.easeDistanceForward, parachute.speedReturn, parachute.easeDistanceReturn, delay, base.transform.localScale.x > 0f, 1);
		}
		else
		{
			boomerang.Create(vector, parachute.speedForward, parachute.easeDistanceForward, parachute.speedReturn, parachute.easeDistanceReturn, delay, base.transform.localScale.x > 0f, 1);
		}
		count++;
		AudioManager.Play("sfx_dlc_dogfight_p1_bulldog_bicepflex");
		emitAudioFromObject.Add("sfx_dlc_dogfight_p1_bulldog_bicepflex");
		AudioManager.Play("sfx_dlc_dogfight_dogflexhugovocal");
		emitAudioFromObject.Add("sfx_dlc_dogfight_dogflexhugovocal");
	}

	private void AniEvent_SFX_BulldogPlane_ParachuteEnd()
	{
		AudioManager.Play("sfx_DLC_Dogfight_P1_Bulldog_SpringsUp");
	}

	private void WORKAROUND_NullifyFields()
	{
		shotYPos = null;
		spawnRoot1 = null;
		spawnRoot2 = null;
		boomerang = null;
		boomerangPink = null;
		pinkString = null;
		damageDealer = null;
		main = null;
		mainAnimator = null;
		collider = null;
	}
}
