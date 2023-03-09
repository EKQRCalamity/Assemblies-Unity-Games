using System;
using System.Collections;
using UnityEngine;

public class OldManLevelPuppetBall : AbstractProjectile
{
	private const float GROUND_Y_OFFSET = -10f;

	private const float HIT_GROUND_OFFSET = 20f;

	private LevelProperties.OldMan.Hands properties;

	private Vector3 startPos;

	private Vector3 endPos;

	private Vector3 platformPos;

	private float size = 50f;

	[SerializeField]
	private float shadowRange = 100f;

	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private SpriteRenderer shadowRend;

	[SerializeField]
	private Sprite[] shadowSprites;

	[SerializeField]
	private Effect coinPrefab;

	[SerializeField]
	private Effect featherPrefab;

	private bool puppetDead;

	public bool readyToCatch { get; private set; }

	public bool isMoving { get; private set; }

	public virtual OldManLevelPuppetBall Init(Vector3 startPos, Vector3 platformPos, Vector3 endPos, LevelProperties.OldMan.Hands properties)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = startPos;
		base.transform.localScale = new Vector3(Mathf.Sign(endPos.x - startPos.x), 1f);
		this.startPos = startPos;
		this.endPos = endPos;
		this.platformPos = platformPos + Vector3.up * -10f;
		this.properties = properties;
		Move();
		base.animator.Play("Idle", 0, 0.7647059f);
		return this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		WORKAROUND_NullifyFields();
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		SFX_OMM_P2_DamagePlayerCheer();
	}

	private void Move()
	{
		isMoving = true;
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		readyToCatch = false;
		float STRAIGHT_BOUNCE_CUTOFF = 0.66f;
		YieldInstruction wait = new WaitForFixedUpdate();
		float percentage = Mathf.Abs(startPos.x - platformPos.x) / Mathf.Abs(startPos.x - endPos.x);
		float newX = base.transform.position.x;
		float newY = base.transform.position.y;
		float direction = Mathf.Sign(endPos.x - startPos.x);
		float xTotalDist = Mathf.Abs(startPos.x - platformPos.x);
		float yRad = base.transform.position.y - (platformPos.y + size);
		while (base.transform.position.y > platformPos.y + size + 20f)
		{
			newX += direction * CupheadTime.FixedDelta * properties.ballSpeed * ((!puppetDead) ? 1f : 0f);
			float xDist = Mathf.Abs(newX - startPos.x);
			TransformExtensions.SetPosition(y: (!(percentage < 1f - STRAIGHT_BOUNCE_CUTOFF)) ? (platformPos.y + size + yRad * Mathf.Cos(xDist / xTotalDist * ((float)Math.PI / 2f))) : Mathf.Lerp(startPos.y, platformPos.y + size, xDist / xTotalDist), transform: base.transform, x: newX);
			yield return wait;
		}
		base.transform.SetPosition(platformPos.x, platformPos.y + size);
		newX = base.transform.position.x;
		xTotalDist = Mathf.Abs(platformPos.x - endPos.x);
		yRad = endPos.y - base.transform.position.y;
		base.animator.SetTrigger("OnBounce");
		yield return base.animator.WaitForAnimationToEnd(this, "Bounce");
		while (Mathf.Sign(endPos.x - base.transform.position.x) == direction || puppetDead)
		{
			newX += direction * CupheadTime.FixedDelta * properties.ballSpeed * ((!puppetDead) ? 1f : 0f);
			float xDist = Mathf.Abs(newX - platformPos.x);
			TransformExtensions.SetPosition(y: (!(percentage > STRAIGHT_BOUNCE_CUTOFF)) ? (platformPos.y + size + yRad * Mathf.Sin(xDist / xTotalDist * ((float)Math.PI / 2f))) : Mathf.Lerp(platformPos.y + size, endPos.y, xDist / xTotalDist), transform: base.transform, x: newX);
			if (!readyToCatch && xDist / xTotalDist >= 0.9f && !puppetDead)
			{
				readyToCatch = true;
			}
			if (puppetDead && (base.transform.position.x > 1140f || base.transform.position.x < -1140f))
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			yield return wait;
		}
	}

	private void LateUpdate()
	{
		shadowRend.transform.position = new Vector3(base.transform.position.x, platformPos.y + size);
		if (base.transform.position.y < platformPos.y + size + shadowRange)
		{
			float num = Mathf.Lerp(shadowSprites.Length - 1, 0f, Mathf.InverseLerp(platformPos.y + size, platformPos.y + size + shadowRange, base.transform.position.y));
			shadowRend.sprite = shadowSprites[(int)num];
		}
	}

	public void GetCaught()
	{
		isMoving = false;
		this.Recycle();
	}

	public void Explode()
	{
		puppetDead = true;
		shadowRend.enabled = false;
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		component.sortingLayerName = "Effects";
		component.sortingOrder = 100;
		base.animator.Play("Explode");
		for (int i = 0; i < 12; i++)
		{
			coinPrefab.Create(base.transform.position + (Vector3)MathUtils.AngleToDirection(UnityEngine.Random.Range(0, 360) * UnityEngine.Random.Range(0, 50)));
		}
		for (int j = 0; j < 15; j++)
		{
			featherPrefab.Create(base.transform.position + (Vector3)MathUtils.AngleToDirection(UnityEngine.Random.Range(0, 360) * UnityEngine.Random.Range(0, 50)));
		}
	}

	private void SFX_OMM_P2_DamagePlayerCheer()
	{
		AudioManager.Play("sfx_dlc_omm_p2_puppet_ball_damageplayercheer");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_puppet_ball_damageplayercheer");
	}

	private void AnimationEvent_SFX_OMM_P2_PuppetBallBounce()
	{
		AudioManager.Play("sfx_dlc_omm_p2_puppet_ball_bounce");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_puppet_ball_bounce");
	}

	private void AnimationEvent_ExplodeEnd()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void WORKAROUND_NullifyFields()
	{
		sprite = null;
		shadowRend = null;
		shadowSprites = null;
		coinPrefab = null;
		featherPrefab = null;
	}
}
