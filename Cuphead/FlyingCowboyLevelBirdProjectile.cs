using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelBirdProjectile : BasicProjectile
{
	private static readonly int ExplosionLayer = 1;

	private static readonly int SmokeLayer = 2;

	private static readonly int ShadowLayer = 3;

	private static readonly float ShadowTriggerDistance = 260f;

	public static readonly float HighLandingPosition = -300f;

	public static readonly float LowLandingPosition = -340f;

	private Vector3 _direction;

	[SerializeField]
	private Transform shadowTransform;

	[SerializeField]
	private Transform spawnPoint;

	[SerializeField]
	private Transform smokeTransform;

	[SerializeField]
	private BasicProjectile shrapnelPrefab;

	private float landingPosition;

	private float gravity;

	private float shrapnelDelay;

	private float shrapnelSpeed;

	private float shrapnelSpreadAngle;

	private FlyingCowboyLevelCowboy cowgirl;

	protected override Vector3 Direction => _direction;

	public int shrapnelCount { get; set; }

	protected override void Start()
	{
		base.Start();
		StartCoroutine(landingPosition_cr());
		StartCoroutine(shrapnel_cr());
		StartCoroutine(shadow_cr());
	}

	public void Initialize(Vector2 initialVelocity, float gravity, float shrapnelDelay, float shrapnelSpeed, float shrapnelSpreadAngle, FlyingCowboyLevelCowboy cowgirl)
	{
		Speed = initialVelocity.magnitude;
		_direction = initialVelocity.normalized;
		this.gravity = gravity;
		this.shrapnelDelay = shrapnelDelay;
		this.shrapnelSpeed = shrapnelSpeed;
		this.shrapnelSpreadAngle = shrapnelSpreadAngle;
		this.cowgirl = cowgirl;
		landingPosition = HighLandingPosition;
	}

	protected override void FixedUpdate()
	{
		Vector3 vector = Direction * Speed;
		vector.y -= gravity * CupheadTime.FixedDelta;
		_direction = vector.normalized;
		Speed = vector.magnitude;
		base.FixedUpdate();
	}

	private IEnumerator landingPosition_cr()
	{
		while (base.transform.position.y > 0f)
		{
			yield return null;
		}
		if (cowgirl.onBottom && cowgirl.state == FlyingCowboyLevelCowboy.State.BeamAttack)
		{
			landingPosition = LowLandingPosition;
		}
	}

	private IEnumerator shadow_cr()
	{
		while (base.transform.position.y > landingPosition + ShadowTriggerDistance)
		{
			yield return null;
		}
		base.animator.Play("Land", ShadowLayer);
		base.animator.Update(0f);
		while (!base.animator.GetCurrentAnimatorStateInfo(ShadowLayer).IsName("Off"))
		{
			Vector3 position = shadowTransform.position;
			position.y = landingPosition;
			shadowTransform.position = position;
			yield return null;
		}
	}

	private IEnumerator shrapnel_cr()
	{
		while (base.transform.position.y > landingPosition)
		{
			yield return null;
		}
		Transform transform = base.transform;
		float? y = landingPosition;
		transform.SetPosition(null, y);
		move = false;
		float initialAngle = (180f - shrapnelSpreadAngle) * 0.5f;
		float angleInterval = shrapnelSpreadAngle / (float)(shrapnelCount - 1);
		for (int i = 0; i < shrapnelCount; i += 2)
		{
			shrapnelPrefab.Create(spawnPoint.position, initialAngle + angleInterval * (float)i, shrapnelSpeed);
		}
		SFX_COWGIRL_COWGIRL_P1_DynamiteExp();
		base.animator.Play("Bounce");
		base.animator.Play("A", ExplosionLayer);
		base.animator.Play("A", SmokeLayer);
		StartCoroutine(moveSmoke_cr("A"));
		base.animator.Play("Off", ShadowLayer);
		yield return CupheadTime.WaitForSeconds(this, shrapnelDelay);
		for (int j = 1; j < shrapnelCount; j += 2)
		{
			shrapnelPrefab.Create(spawnPoint.position, initialAngle + angleInterval * (float)j, shrapnelSpeed);
		}
		GetComponent<Collider2D>().enabled = false;
		base.animator.Play("Off");
		base.animator.Play("B", ExplosionLayer);
		base.animator.Play("B", SmokeLayer);
		StartCoroutine(moveSmoke_cr("B"));
	}

	private IEnumerator moveSmoke_cr(string animationName)
	{
		Vector3 initialPosition = smokeTransform.position;
		yield return base.animator.WaitForAnimationToStart(this, animationName, SmokeLayer);
		float speed = 0f;
		while (!base.animator.GetCurrentAnimatorStateInfo(SmokeLayer).IsName("Off"))
		{
			yield return null;
			speed += (float)CupheadTime.Delta * 1500f;
			Vector3 position = smokeTransform.position;
			position.x -= speed * (float)CupheadTime.Delta;
			smokeTransform.position = position;
		}
		smokeTransform.position = initialPosition;
	}

	private void animationEvent_ExplosionsFinished()
	{
		Object.Destroy(base.gameObject);
	}

	private void SFX_COWGIRL_COWGIRL_P1_DynamiteExp()
	{
		AudioManager.Play("sfx_DLC_Cowgirl_P1_DynamiteExp");
		emitAudioFromObject.Add("sfx_DLC_Cowgirl_P1_DynamiteExp");
	}
}
