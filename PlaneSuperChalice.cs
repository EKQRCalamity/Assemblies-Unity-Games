using System.Collections;
using UnityEngine;

public class PlaneSuperChalice : AbstractPlaneSuper
{
	private const float ANALOG_THRESHOLD = 0.35f;

	private const float PADDING_TOP = 65f;

	private const float PADDING_BOTTOM = 35f;

	private const float PADDING_LEFT = 70f;

	private const float PADDING_RIGHT = 30f;

	private bool superHappening;

	private bool invulnerable;

	private float timer;

	private Vector2 accelDirection;

	private Vector2 moveDirection;

	private Vector2 _velocity;

	private DamageReceiver damageReceiver;

	private bool exploded;

	private bool missed;

	private Coroutine boomRoutine;

	[SerializeField]
	private Transform boom;

	private float curAngle;

	private float curSpeed;

	private Vector2 respawnPos;

	protected override void Start()
	{
		base.Start();
		boom.gameObject.SetActive(value: true);
		player.damageReceiver.OnDamageTaken += OnDamageTaken;
		player.stats.OnStoned += OnStoned;
	}

	private void FixedUpdate()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
		HandleInput();
		if (!exploded)
		{
			Move();
		}
		ClampPosition();
	}

	private void EndIntroAnimation()
	{
		SnapshotAudio();
		if (player != null)
		{
			player.UnpauseAll();
		}
		animHelper.IgnoreGlobal = false;
		PauseManager.Unpause();
		StartCoroutine(super_cr());
	}

	private void OnStoned()
	{
		exploded = true;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		exploded = true;
	}

	private IEnumerator super_cr()
	{
		player.damageReceiver.Vulnerable();
		respawnPos = base.transform.position;
		state = PlanePlayerWeaponManager.States.Super.Countdown;
		damageDealer = new DamageDealer(WeaponProperties.PlaneSuperChaliceSuperBomb.damage, WeaponProperties.PlaneSuperChaliceSuperBomb.damageRate, DamageDealer.DamageSource.Super, damagesPlayer: false, damagesEnemy: true, damagesOther: true);
		damageDealer.DamageMultiplier *= PlayerManager.DamageMultiplier;
		damageDealer.PlayerId = player.id;
		MeterScoreTracker tracker = new MeterScoreTracker(MeterScoreTracker.Type.Super);
		tracker.Add(damageDealer);
		curAngle = MathUtils.DirectionToAngle(Vector3.right);
		curSpeed = 0f;
		while (!exploded)
		{
			if (player != null)
			{
				player.transform.position = base.transform.position;
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
			yield return null;
		}
		respawnPos = base.transform.position;
		Fire();
		if (player != null)
		{
			player.PauseAll();
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
		base.animator.SetTrigger("Explode");
		AudioManager.Play("player_plane_bomb_explosion");
		AudioManager.Stop("player_plane_bomb_ticktock_loop");
	}

	protected override void Fire()
	{
		base.Fire();
	}

	private void PlayerReappear()
	{
		RestoreAudio();
		if (!(player == null))
		{
			player.motor.OnRevive(respawnPos);
			player.UnpauseAll();
			player.SetSpriteVisible(visibility: true);
			player.damageReceiver.Invulnerable(2f);
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

	private void HandleInput()
	{
		Trilean trilean = 0;
		Trilean trilean2 = 0;
		float num = 0f;
		if (player != null)
		{
			num = player.input.actions.GetAxis(1);
		}
		if (num > 0.35f || num < -0.35f)
		{
			trilean2 = num;
		}
		curAngle += (float)trilean2 * WeaponProperties.PlaneSuperChaliceSuperBomb.turnRate;
		curAngle = Mathf.Clamp(curAngle, 0f - WeaponProperties.PlaneSuperChaliceSuperBomb.maxAngle, WeaponProperties.PlaneSuperChaliceSuperBomb.maxAngle);
		curAngle *= WeaponProperties.PlaneSuperChaliceSuperBomb.angleDamp;
		accelDirection = MathUtils.AngleToDirection(curAngle);
		base.animator.SetInteger("Y", trilean2);
	}

	private void Move()
	{
		Vector2 vector = base.transform.position;
		moveDirection = accelDirection * curSpeed;
		curSpeed += WeaponProperties.PlaneSuperChaliceSuperBomb.accel * CupheadTime.FixedDelta;
		base.transform.AddPosition(moveDirection.x * CupheadTime.FixedDelta, moveDirection.y * CupheadTime.FixedDelta);
		Vector2 vector2 = base.transform.position;
		_velocity = (vector2 - vector) / CupheadTime.FixedDelta;
	}

	private void ClampPosition()
	{
		Vector2 vector = base.transform.position;
		vector.x = Mathf.Clamp(vector.x, Level.Current.Left, (float)Level.Current.Right - 30f);
		vector.y = Mathf.Clamp(vector.y, Level.Current.Ground, Level.Current.Ceiling);
		if ((Vector2)base.transform.position != vector)
		{
			exploded = true;
		}
	}

	private void CheckPosition()
	{
		Vector2 vector = base.transform.position;
		vector.x = Mathf.Clamp(vector.x, (float)Level.Current.Left - 350f, (float)Level.Current.Right + 150f);
		vector.y = Mathf.Clamp(vector.y, (float)Level.Current.Ground - 175f, (float)Level.Current.Ceiling + 325f);
		if ((Vector2)base.transform.position != vector)
		{
			missed = true;
		}
	}

	protected virtual void RestoreAudio(bool changePitch = true)
	{
		AudioManager.SnapshotReset(SceneLoader.SceneName, 2f);
		if (changePitch)
		{
			AudioManager.ChangeBGMPitch(1f, 2f);
		}
	}

	protected override void OnDestroy()
	{
		RestoreAudio();
		base.OnDestroy();
		if (player != null)
		{
			player.damageReceiver.OnDamageTaken -= OnDamageTaken;
			player.stats.OnStoned -= OnStoned;
		}
	}
}
