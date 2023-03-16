using System;
using System.Collections;
using UnityEngine;

public class ChessBishopLevelCandle : AbstractCollidableObject
{
	[SerializeField]
	private Transform blowoutRoot;

	[SerializeField]
	private GameObject smoke;

	[SerializeField]
	private GameObject vanquishFX;

	[SerializeField]
	private GameObject vanquishSpark;

	[SerializeField]
	private GameObject shadow;

	[SerializeField]
	private float staggerLoopTime;

	[SerializeField]
	private float floatAmplitude;

	[SerializeField]
	private bool offsetFloat;

	private float easeToFloat = 1f;

	private Vector3 basePos;

	private Vector3 shadowPos;

	private Vector3 introPos;

	private float distToBlowout;

	private AbstractPlayerController player1;

	private AbstractPlayerController player2;

	private Vector3 lastPlayer1Position = Vector3.zero;

	private Vector3 lastPlayer2Position = Vector3.zero;

	private float stepTimer;

	[SerializeField]
	private GameObject glow;

	[SerializeField]
	private ChessBishopLevelIntroCandle introCandle;

	private bool introPlaying = true;

	[SerializeField]
	private bool isLastIntro;

	[SerializeField]
	private float introOvershoot;

	public bool isLit { get; private set; }

	public void Init(float distToBlowout)
	{
		glow.SetActive(value: false);
		this.distToBlowout = distToBlowout;
		basePos = base.transform.position;
		shadowPos = shadow.transform.position;
		StartCoroutine(intro_cr());
	}

	private float EaseOvershoot(float start, float end, float value, float overshoot)
	{
		float num = Mathf.Lerp(start, end, value);
		return num + Mathf.Sin(value * (float)Math.PI) * ((end - start) * overshoot);
	}

	private IEnumerator intro_cr()
	{
		introPos = introCandle.transform.position;
		yield return null;
		while (!introCandle.moving)
		{
			yield return null;
		}
		float t2 = 0f;
		while (t2 < 1f)
		{
			introCandle.transform.position = introPos + Vector3.up * 800f * EaseUtils.EaseOutSine(0f, 1f, Mathf.InverseLerp(0f, 1f, t2));
			t2 += 1f / 24f;
			yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
		}
		base.transform.position = basePos + Vector3.up * 800f;
		base.animator.Play("IntroToIdle");
		t2 = 0f;
		while (t2 < 1f)
		{
			base.transform.position = basePos + (Vector3.up * 800f * EaseOvershoot(1f, 0f, t2, introOvershoot) + floatAmplitude * Vector3.up);
			t2 += 1f / 24f;
			yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
		}
		introPlaying = false;
	}

	private bool PlayerInRange()
	{
		if ((bool)player1 && !player1.IsDead)
		{
			float num = Vector3.SqrMagnitude(blowoutRoot.position - player1.center);
			if (num < distToBlowout * distToBlowout && player1.center != lastPlayer1Position)
			{
				return true;
			}
		}
		if ((bool)player2 && !player2.IsDead)
		{
			float num2 = Vector3.SqrMagnitude(blowoutRoot.position - player2.center);
			if (num2 < distToBlowout * distToBlowout && player2.center != lastPlayer2Position)
			{
				return true;
			}
		}
		return false;
	}

	private void Update()
	{
		player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		if (PlayerInRange())
		{
			if (isLit)
			{
				StopAllCoroutines();
				StartCoroutine(light_out_cr());
			}
			else if (base.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
			{
				base.animator.Play("Walkby", 0, 0f);
			}
		}
		if ((bool)player1 && !player1.IsDead)
		{
			lastPlayer1Position = player1.center;
		}
		if ((bool)player2 && !player2.IsDead)
		{
			lastPlayer2Position = player2.center;
		}
		stepTimer += CupheadTime.Delta;
		while (stepTimer > 1f / 24f)
		{
			Step();
			stepTimer -= 1f / 24f;
		}
	}

	private void Step()
	{
		shadow.transform.position = shadowPos;
		if (!introPlaying)
		{
			base.transform.position = basePos;
			if (base.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || base.animator.GetCurrentAnimatorStateInfo(0).IsName("Lit"))
			{
				easeToFloat = Mathf.Clamp(0f, 1f, easeToFloat + 1f / 24f);
				float num = ((!offsetFloat) ? base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime : (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.25f));
				base.transform.position += Vector3.up * Mathf.Cos(num * (float)Math.PI * 2f) * floatAmplitude * easeToFloat;
			}
			else
			{
				easeToFloat = 0f;
			}
		}
	}

	public void LightUp()
	{
		isLit = true;
		StopAllCoroutines();
		StartCoroutine(light_up_cr());
	}

	private IEnumerator light_up_cr()
	{
		base.animator.Play((!Rand.Bool()) ? "ReigniteB" : "ReigniteA", 1);
		glow.SetActive(value: true);
		yield return base.animator.WaitForAnimationToStart(this, "None", 1);
		base.animator.Play("Lit", 0, base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
		SFX_KOG_Bishop_CandlesLightUp();
	}

	private IEnumerator light_out_cr()
	{
		isLit = false;
		glow.SetActive(value: false);
		base.animator.Play("Stagger");
		SFX_KOG_Bishop_CandleSnuff();
		smoke.transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(-5, 5));
		vanquishFX.transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(0, 360));
		vanquishSpark.transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(0, 360));
		base.animator.Play((!Rand.Bool()) ? "SmokeB" : "SmokeA", 2);
		yield return CupheadTime.WaitForSeconds(this, staggerLoopTime);
		base.animator.SetTrigger("EndStaggerLoop");
	}

	private void SFX_KOG_Bishop_CandlesLightUp()
	{
		AudioManager.Play("sfx_dlc_kog_bishop_candleslightup");
		emitAudioFromObject.Add("sfx_dlc_kog_bishop_candleslightup");
	}

	private void SFX_KOG_Bishop_CandleSnuff()
	{
		AudioManager.Play("sfx_dlc_kog_bishop_candlesnuff");
		emitAudioFromObject.Add("sfx_dlc_kog_bishop_candlesnuff");
	}
}
