using UnityEngine;

public class GraveyardLevelSplitDevilBeam : AbstractProjectile
{
	private float xVelocity;

	[SerializeField]
	private Animator fireAnim;

	[SerializeField]
	private Animator lightAnim;

	[SerializeField]
	private SpriteRenderer[] fireRend;

	[SerializeField]
	private SpriteRenderer[] lightRend;

	[SerializeField]
	private SpriteRenderer fireFormDissipate;

	[SerializeField]
	private Effect bottomSmokeFX;

	private bool bottomSmokeFXTypeA = true;

	[SerializeField]
	private Effect midSmokeFX;

	[SerializeField]
	private Transform midSmokePos;

	[SerializeField]
	private GraveyardLevelSplitDevilBeamIgniteFX igniteFX;

	[SerializeField]
	private GraveyardLevelSplitDevilBeamTrailFX trailFX;

	[SerializeField]
	private float flameTrailSpacing = 128f;

	[SerializeField]
	private GameObject sparkleBeam;

	[SerializeField]
	private SpriteRenderer groundSpotlight;

	private bool onGround;

	private bool fireOn;

	[SerializeField]
	private Collider2D coll;

	private float warningTime;

	private float frameTimer;

	private float flameTrailDistanceTracker;

	private int flameTrailAnim;

	private bool forceFade;

	public GraveyardLevelSplitDevil devil { get; private set; }

	protected override void RandomizeVariant()
	{
	}

	public GraveyardLevelSplitDevilBeam Create(Vector3 pos, float xVelocity, float warningTime, GraveyardLevelSplitDevil devil)
	{
		GraveyardLevelSplitDevilBeam graveyardLevelSplitDevilBeam = base.Create(pos) as GraveyardLevelSplitDevilBeam;
		graveyardLevelSplitDevilBeam.xVelocity = xVelocity;
		graveyardLevelSplitDevilBeam.DestroyDistance = Level.Current.Width + 200;
		graveyardLevelSplitDevilBeam.devil = devil;
		graveyardLevelSplitDevilBeam.warningTime = warningTime;
		graveyardLevelSplitDevilBeam.fireOn = !graveyardLevelSplitDevilBeam.devil.isAngel;
		graveyardLevelSplitDevilBeam.coll.enabled = !graveyardLevelSplitDevilBeam.devil.isAngel;
		graveyardLevelSplitDevilBeam.UpdateFade(1f);
		if (graveyardLevelSplitDevilBeam.fireOn)
		{
			graveyardLevelSplitDevilBeam.fireAnim.Play("Form", 1, 0f);
			graveyardLevelSplitDevilBeam.fireAnim.Update(0f);
			Effect effect = igniteFX.Create(graveyardLevelSplitDevilBeam.transform.position, fireAnim);
			effect.transform.parent = graveyardLevelSplitDevilBeam.transform;
			AudioManager.Play("sfx_dlc_graveyard_beamchange_fireon");
			graveyardLevelSplitDevilBeam.emitAudioFromObject.Add("sfx_dlc_graveyard_beamchange_fireon");
		}
		else
		{
			SpriteRenderer[] array = graveyardLevelSplitDevilBeam.lightRend;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
			}
		}
		CupheadLevelCamera.Current.StartShake(4f);
		return graveyardLevelSplitDevilBeam;
	}

	protected override void Update()
	{
		base.Update();
		if (base.dead)
		{
			return;
		}
		if (devil.dead)
		{
			coll.enabled = false;
			forceFade = true;
		}
		if (warningTime <= 0f)
		{
			base.transform.AddPosition(xVelocity * (float)CupheadTime.Delta);
			if (fireAnim.GetBool("Smoke"))
			{
				flameTrailDistanceTracker += Mathf.Abs(xVelocity) * (float)CupheadTime.Delta;
			}
		}
		else
		{
			warningTime -= CupheadTime.Delta;
		}
		while (flameTrailDistanceTracker > flameTrailSpacing && !forceFade)
		{
			flameTrailDistanceTracker -= flameTrailSpacing;
			SpawnTrailFX();
		}
		if (Mathf.Abs(base.transform.position.x) < (float)((Mathf.Sign(base.transform.position.x) != Mathf.Sign(xVelocity)) ? 600 : 400) && !fireAnim.GetBool("Smoke"))
		{
			SpawnTrailFX();
		}
		if (Mathf.Abs(base.transform.position.x) < 550f && !onGround)
		{
			onGround = true;
			lightAnim.Play((!Rand.Bool()) ? "B" : "A", 1, 0f);
		}
		fireAnim.SetBool("Smoke", Mathf.Abs(base.transform.position.x) < (float)((Mathf.Sign(base.transform.position.x) != Mathf.Sign(xVelocity)) ? 600 : 400));
		coll.enabled = !devil.isAngel;
		if (fireOn != !devil.isAngel)
		{
			if (fireOn && !fireAnim.GetCurrentAnimatorStateInfo(1).IsName("Form") && fireAnim.GetBool("Smoke"))
			{
				Effect effect = bottomSmokeFX.Create(base.transform.position);
				if (bottomSmokeFXTypeA)
				{
					effect.Play();
				}
				bottomSmokeFXTypeA = !bottomSmokeFXTypeA;
				effect.transform.parent = base.transform;
				Effect effect2 = midSmokeFX.Create(midSmokePos.position);
				effect2.transform.localScale = midSmokePos.localScale;
				effect2.transform.parent = base.transform;
			}
			if (!fireOn)
			{
				AudioManager.Play("sfx_dlc_graveyard_beamchange_fireon");
				emitAudioFromObject.Add("sfx_dlc_graveyard_beamchange_fireon");
				Effect effect3 = igniteFX.Create(base.transform.position, fireAnim);
				effect3.transform.parent = base.transform;
			}
			fireAnim.Play((!fireOn) ? "Form" : "Dissipate", 1, 0f);
			fireAnim.Update(0f);
			fireOn = !devil.isAngel;
		}
		frameTimer += CupheadTime.Delta;
		while (frameTimer > 1f / 24f)
		{
			frameTimer -= 1f / 24f;
			UpdateFade(0.25f);
		}
	}

	private void UpdateFade(float amount)
	{
		SpriteRenderer[] array = fireRend;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Clamp(spriteRenderer.color.a + ((!(coll.enabled & !forceFade)) ? (0f - amount) : amount), 0f, 1f));
		}
		SpriteRenderer[] array2 = lightRend;
		foreach (SpriteRenderer spriteRenderer2 in array2)
		{
			spriteRenderer2.color = new Color(spriteRenderer2.color.r, spriteRenderer2.color.g, spriteRenderer2.color.b, Mathf.Clamp(spriteRenderer2.color.a + ((!coll.enabled && !forceFade) ? amount : (0f - amount)), 0f, 1f));
		}
		groundSpotlight.color = new Color(1f, 1f, 1f, Mathf.Clamp(groundSpotlight.color.a + ((coll.enabled || !(onGround & !forceFade)) ? (0f - amount) : amount), 0f, 1f));
	}

	private void SpawnTrailFX()
	{
		trailFX.Create(new Vector3(Mathf.Clamp(base.transform.position.x + flameTrailSpacing * Mathf.Sign(xVelocity), -550f, 550f), base.transform.position.y), new Vector3(0f - Mathf.Sign(xVelocity), 1f), this, flameTrailAnim);
		flameTrailAnim = (flameTrailAnim + 1) % 3;
	}

	private void LateUpdate()
	{
		fireRend[0].enabled = fireFormDissipate.sprite == null;
		sparkleBeam.transform.localPosition = new Vector3(0f, -1280 + (int)lightAnim.GetCurrentAnimatorStateInfo(0).normalizedTime % 8 * 280);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void Die()
	{
		Object.Destroy(base.gameObject);
	}
}
