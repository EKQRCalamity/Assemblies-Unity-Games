using System.Collections;
using UnityEngine;

public class OldManLevelDwarf : AbstractProjectile
{
	private const float START_POS_Y = -430f;

	private const float JUMP_POS_Y = -325f;

	private const float LAND_POS_Y = -314f;

	private const float LAND_OFFSET = 25f;

	private const float SHADOW_OFFSET_START = 40f;

	private const float SHADOW_OFFSET_END = 60f;

	[Header("Death FX")]
	[SerializeField]
	private Effect deathPuff;

	[SerializeField]
	private SpriteDeathParts[] deathParts;

	[Header("Beard FX")]
	[SerializeField]
	private Effect beardPopA;

	[SerializeField]
	private Effect beardPopB;

	[SerializeField]
	private Effect beardHealA;

	[SerializeField]
	private Effect beardHealB;

	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private Collider2D coll;

	private DamageReceiver damageReceiver;

	private Vector3 startPos;

	private Vector3 vel;

	private float gravity;

	private float health;

	private float apexTime;

	private float bulletSpeed;

	private float apexheight;

	private float currentArcTime;

	private float warningTime;

	private bool typeA;

	private bool parryable;

	private string colorString;

	private bool isBlue = true;

	private Effect beardPop;

	[SerializeField]
	private float shadowRange = 100f;

	[SerializeField]
	private SpriteRenderer shadowRend;

	[SerializeField]
	private Sprite[] shadowSprites;

	private bool groundShadow;

	[SerializeField]
	private OldManLevelBeardController beardController;

	[SerializeField]
	private int rufflePos;

	public bool inPlace { get; private set; }

	protected override void OnDieDistance()
	{
	}

	protected override void OnDieLifetime()
	{
	}

	protected override void Start()
	{
		base.Start();
		startPos = base.transform.position;
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		damageReceiver.enabled = false;
		inPlace = true;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public override void OnParry(AbstractPlayerController player)
	{
		Death(parried: true);
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		health -= info.damage;
		if (health < 0f)
		{
			Level.Current.RegisterMinionKilled();
			Death();
		}
	}

	private IEnumerator move_up_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		float speed = 200f;
		rend.sortingLayerID = SortingLayer.NameToID("Default");
		rend.sortingOrder = 2;
		while (base.transform.position.y < -430f)
		{
			base.transform.AddPosition(0f, speed * CupheadTime.FixedDelta);
			yield return wait;
		}
		beardController.CueRuffle(rufflePos);
		base.animator.SetTrigger("Continue");
		yield return AnimatorExtensions.WaitForAnimationToEnd(name: "Trans" + ((!typeA) ? "_B" : "_A") + colorString, animator: base.animator, parent: this);
		yield return null;
	}

	public override void SetParryable(bool parryable)
	{
		base.SetParryable(parryable);
	}

	public void ShootInArc(float apexHeight, float timeToApex, float health, bool typeA, bool parryable, float warningTime)
	{
		apexheight = apexHeight;
		apexTime = timeToApex;
		this.health = health;
		this.typeA = typeA;
		inPlace = false;
		damageReceiver.enabled = true;
		this.parryable = parryable;
		this.warningTime = warningTime;
		if (parryable)
		{
			colorString = "_Pink";
		}
		else
		{
			colorString = ((!isBlue) ? "_Teal" : string.Empty);
			isBlue = !isBlue;
		}
		SetParryable(parryable: false);
		StartCoroutine(arc_cr());
	}

	private void CalculateArc()
	{
		float num = apexheight;
		float num2 = apexTime * apexTime;
		float num3 = -2f * num / num2;
		float num4 = 2f * num / apexTime;
		float num5 = num4 * num4;
		Vector3 position = base.transform.position;
		Vector3 position2 = PlayerManager.GetNext().transform.position;
		float num6 = position2.x - position.x;
		float num7 = position2.y - position.y;
		float num8 = num5 + 2f * num3 * num7;
		if (num8 < 0f)
		{
			num8 = 0f;
		}
		float a = (0f - num4 + Mathf.Sqrt(num8)) / num3;
		float b = (0f - num4 - Mathf.Sqrt(num8)) / num3;
		float num9 = Mathf.Max(a, b);
		vel.x = num6 / num9;
		vel.y = num4;
		gravity = num3;
	}

	private IEnumerator arc_cr()
	{
		bool finishedArcing = false;
		inPlace = false;
		string typeString = ((!typeA) ? "_B" : "_A");
		base.animator.Play("Climb" + typeString + colorString);
		GetComponent<SpriteRenderer>().sortingOrder = 2;
		yield return StartCoroutine(move_up_cr());
		Effect beardPopPrefab = ((!typeA) ? beardPopB : beardPopA);
		beardPop = beardPopPrefab.Create(new Vector3(base.transform.position.x, -335f));
		yield return null;
		yield return beardPop.animator.WaitForAnimationToStart(this, "Pimple_Warning");
		AudioManager.Play("sfx_dlc_omm_gnome_groundstretch");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_groundstretch");
		yield return CupheadTime.WaitForSeconds(this, warningTime);
		CalculateArc();
		SetParryable(parryable);
		coll.enabled = true;
		base.transform.position = new Vector3(base.transform.position.x, -325f);
		base.transform.localScale = new Vector3(Mathf.Sign(vel.x), 1f);
		base.animator.Play("Spin" + typeString + colorString);
		rend.sortingLayerID = SortingLayer.NameToID("Player");
		rend.sortingOrder = 50;
		AudioManager.Play("sfx_dlc_omm_gnome_groundstretchpop");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_groundstretchpop");
		AudioManager.Play("sfx_dlc_omm_gnome_somersault_voice");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_somersault_voice");
		AudioManager.Play("sfx_dlc_omm_gnome_somersault");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_somersault");
		beardPop.animator.Play("Pimple_End");
		groundShadow = true;
		currentArcTime = 0f;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (!finishedArcing)
		{
			vel += new Vector3(0f, gravity * CupheadTime.FixedDelta);
			base.transform.Translate(vel * CupheadTime.FixedDelta);
			if (rend.sortingOrder == 50 && vel.y < 0f)
			{
				rend.sortingLayerID = SortingLayer.NameToID("Enemies");
				rend.sortingOrder = 4;
			}
			if (vel.y < 0f && base.transform.position.y < -289f)
			{
				finishedArcing = true;
				break;
			}
			currentArcTime += CupheadTime.FixedDelta;
			yield return wait;
		}
		groundShadow = false;
		Vector3 pos = new Vector3(base.transform.position.x, -289f);
		Effect beardHealPrefab = ((!typeA) ? beardHealB : beardHealA);
		beardHealPrefab.Create(pos + Vector3.down * 25f);
		rend.sortingLayerID = SortingLayer.NameToID("Default");
		rend.sortingOrder = 5;
		vel.x = 0f;
		vel.y *= 0.5f;
		float t = 0f;
		while (t < 1f / 24f && base.transform.position.y > -334f)
		{
			t += CupheadTime.FixedDelta;
			base.transform.Translate(vel * CupheadTime.FixedDelta);
			yield return wait;
		}
		Respawn();
		yield return null;
	}

	public void Death(bool parried = false)
	{
		if ((bool)beardPop)
		{
			Object.Destroy(beardPop.gameObject);
		}
		if (base.transform.position.y > startPos.y)
		{
			deathPuff.Create(base.transform.position);
		}
		if (!parried)
		{
			for (int i = 0; i < deathParts.Length; i++)
			{
				if (i != 0 || Random.Range(0, 10) == 0)
				{
					SpriteDeathParts spriteDeathParts = deathParts[i].CreatePart(base.transform.position);
					if (i != 0)
					{
						spriteDeathParts.animator.Play(colorString);
					}
				}
			}
			AudioManager.Play("sfx_dlc_omm_gnome_popper_death");
			emitAudioFromObject.Add("sfx_dlc_omm_gnome_popper_death");
		}
		AudioManager.Stop("sfx_dlc_omm_gnome_somersault");
		AudioManager.Stop("sfx_dlc_omm_gnome_somersault_voice");
		groundShadow = false;
		Respawn();
	}

	private void Respawn()
	{
		StopAllCoroutines();
		damageReceiver.enabled = false;
		base.transform.position = startPos;
		inPlace = true;
		coll.enabled = false;
	}

	private void LateUpdate()
	{
		if (groundShadow)
		{
			shadowRend.sortingOrder = 5;
			shadowRend.transform.position = new Vector3(base.transform.position.x, -314f + Mathf.Lerp(40f, 60f, currentArcTime / (apexTime * 2f)));
			if (base.transform.position.y < -314f + shadowRange)
			{
				float num = Mathf.Lerp(0f, shadowSprites.Length - 4, Mathf.InverseLerp(-314f, -314f + shadowRange, base.transform.position.y));
				shadowRend.sprite = shadowSprites[(int)num];
			}
			else
			{
				shadowRend.sprite = shadowSprites[shadowSprites.Length - 3 + (int)(currentArcTime * 24f) % 3];
			}
		}
		else
		{
			shadowRend.sortingOrder = 1;
			shadowRend.transform.localPosition = Vector3.zero;
		}
	}
}
