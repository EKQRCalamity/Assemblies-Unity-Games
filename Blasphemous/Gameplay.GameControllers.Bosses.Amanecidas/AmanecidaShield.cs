using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Amanecidas;

public class AmanecidaShield : MonoBehaviour
{
	private struct FragmentSortingData
	{
		public AmanecidasShieldFragment fr;

		public float d;
	}

	public float timeToShow = 0.1f;

	public float timeToHide = 0.6f;

	public ParticleSystem brokenShieldParticles;

	public ParticleSystem brokenFragmentParticles;

	public ParticleSystem recoverShieldParticles;

	public float alphaHidden;

	public float alphaShowing = 0.4f;

	public Color originColor;

	public Color damagedColor;

	public float fadeDuration = 1f;

	public float secondaryDuration = 1f;

	public SpriteRenderer mainSprite;

	public SpriteRenderer secondarySprite;

	public Animator shieldAnimator;

	public ParticleSystem stencilFragmentsParticles;

	public float radius = 3f;

	public List<Transform> shieldFragmentTransforms;

	public Amanecidas amanecidas;

	private List<AmanecidasShieldFragment> shieldFragments;

	private Coroutine currentCoroutine;

	private int fragmentsToDestroy = 8;

	private void Awake()
	{
		shieldFragments = new List<AmanecidasShieldFragment>();
		foreach (Transform shieldFragmentTransform in shieldFragmentTransforms)
		{
			shieldFragments.Add(shieldFragmentTransform.GetComponentInChildren<AmanecidasShieldFragment>());
		}
	}

	public void SetDamagePercentage(float percentage)
	{
		CheckFragmentsToBreak(percentage);
	}

	public void Death()
	{
		StopAllCoroutines();
		BreakShield();
	}

	public void FlashShieldFromPenitent()
	{
		Vector2 vector = Core.Logic.Penitent.transform.position;
		Vector2 vector2 = vector - (Vector2)base.transform.position + Vector2.up;
		Vector2 p = (Vector2)base.transform.position + vector2.normalized * radius;
		StopAllCoroutines();
		StartCoroutine(FlashAllFragmentsRoutine(p, 0.3f));
	}

	public void FlashShieldFromDown()
	{
		StopAllCoroutines();
		Vector2 p = (Vector2)base.transform.position + Vector2.down * 4f;
		StartCoroutine(FlashAllFragmentsRoutine(p, 0.2f));
	}

	public void SetAlpha(SpriteRenderer spr, float a)
	{
		spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, a);
	}

	public void BreakShield()
	{
		base.transform.DOKill();
		brokenFragmentParticles.transform.position = base.transform.position;
		brokenFragmentParticles.Emit(50);
		foreach (AmanecidasShieldFragment shieldFragment in shieldFragments)
		{
			switch (shieldFragment.currentState)
			{
			case AmanecidasShieldFragment.AMA_SHIELD_FRAGMENT_STATES.SHIELD:
				shieldFragment.BreakFromShield((shieldFragment.transform.position - base.transform.position).normalized * 8f);
				break;
			case AmanecidasShieldFragment.AMA_SHIELD_FRAGMENT_STATES.RISING:
				shieldFragment.FadeOut();
				break;
			}
		}
	}

	private IEnumerator RecoverShieldCoroutine(float secondsRising = 1f, float secondsWait = 1f, float maxSecondsToTransform = 1f, float timeToPunchTransform = 1f, Action callback = null)
	{
		float rising = secondsRising / (float)shieldFragments.Count;
		foreach (AmanecidasShieldFragment item in shieldFragments)
		{
			Vector2 p = new Vector2(base.transform.position.x + (float)UnityEngine.Random.Range(-6, 6), base.transform.position.y);
			item.RaiseFromGround(p, rising);
			yield return new WaitForSeconds(rising);
		}
		yield return new WaitForSeconds(secondsWait);
		foreach (AmanecidasShieldFragment shieldFragment in shieldFragments)
		{
			shieldFragment.GoToShieldTransform(UnityEngine.Random.Range(0.1f, maxSecondsToTransform));
		}
		yield return new WaitForSeconds(maxSecondsToTransform);
		SlowPunchShieldTransforms(timeToPunchTransform);
		callback?.Invoke();
	}

	public void SlowPunchShieldTransforms(float totalTime)
	{
		float num = 3f;
		foreach (AmanecidasShieldFragment shieldFragment in shieldFragments)
		{
			Vector2 vector = shieldFragment.shieldTransform.localPosition;
			Vector2 vector2 = vector + vector.normalized * num;
			Debug.DrawLine(shieldFragment.shieldTransform.TransformPoint(vector), shieldFragment.shieldTransform.TransformPoint(vector), Color.red, 10f);
			Sequence sequence = DOTween.Sequence();
			sequence.Append(shieldFragment.shieldTransform.DOLocalMove(vector2, totalTime * 0.8f).SetEase(Ease.InOutCubic));
			sequence.Append(shieldFragment.shieldTransform.DOLocalMove(vector, totalTime * 0.2f).SetEase(Ease.InCubic));
			sequence.Play();
		}
	}

	public void StartToRecoverShield(float secondsRising = 1f, float secondsWait = 1f, float maxSecondsToTransform = 1f, float timeToPunchTransform = 1f, Action callback = null)
	{
		currentCoroutine = StartCoroutine(RecoverShieldCoroutine(secondsRising, secondsWait, maxSecondsToTransform, timeToPunchTransform, callback));
	}

	public void CheckFragmentsToBreak(float currentPercentage)
	{
		List<AmanecidasShieldFragment> list = shieldFragments.Where((AmanecidasShieldFragment x) => x.currentState == AmanecidasShieldFragment.AMA_SHIELD_FRAGMENT_STATES.BROKEN).ToList();
		float num = (float)list.Count / (float)fragmentsToDestroy;
		if (1f - num > currentPercentage)
		{
			DestroyFragment();
		}
	}

	private void DestroyFragment()
	{
		List<AmanecidasShieldFragment> list = shieldFragments.Where((AmanecidasShieldFragment x) => x.currentState == AmanecidasShieldFragment.AMA_SHIELD_FRAGMENT_STATES.SHIELD).ToList();
		if (list.Count > 0)
		{
			AmanecidasShieldFragment amanecidasShieldFragment = list[UnityEngine.Random.Range(0, list.Count)];
			amanecidasShieldFragment.BreakFromShield(amanecidasShieldFragment.transform.position - base.transform.position);
			brokenFragmentParticles.transform.position = amanecidasShieldFragment.transform.position;
			brokenFragmentParticles.Emit(30);
		}
	}

	public void ShakeWave(Vector2 pos)
	{
		Core.Logic.ScreenFreeze.Freeze(0.1f, 0.15f);
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 0.5f, 0.3f, 2f);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.5f, Vector3.down * 0.5f, 12, 0.2f, 0.01f, default(Vector3), 0.01f, ignoreTimeScale: true);
	}

	private IEnumerator FlashAllFragmentsRoutine(Vector2 p, float totalSeconds)
	{
		List<AmanecidasShieldFragment> fragmentsInShield = shieldFragments.Where((AmanecidasShieldFragment x) => x.currentState == AmanecidasShieldFragment.AMA_SHIELD_FRAGMENT_STATES.SHIELD).ToList();
		for (int j = 0; j < fragmentsInShield.Count; j++)
		{
			fragmentsInShield[j].FadeIn(0.1f);
		}
		if (fragmentsInShield.Count > 0)
		{
			List<AmanecidasShieldFragment> byDistance = SortFragmentsByDistance(p, fragmentsInShield);
			float secondsBetweenFlash = totalSeconds / (float)byDistance.Count;
			if (amanecidas.AnimatorInyector.IsOut())
			{
				secondsBetweenFlash = 0.01f;
			}
			for (int i = 0; i < byDistance.Count; i++)
			{
				byDistance[i].Flash();
				yield return new WaitForSeconds(secondsBetweenFlash);
			}
		}
		yield return new WaitForSeconds(0.15f);
		float fadeOutTime = 0.4f;
		if (amanecidas.AnimatorInyector.IsOut())
		{
			fadeOutTime = 0.05f;
		}
		for (int k = 0; k < fragmentsInShield.Count; k++)
		{
			fragmentsInShield[k].FadeOut(fadeOutTime);
		}
	}

	public List<AmanecidasShieldFragment> SortFragmentsByDistance(Vector2 point, List<AmanecidasShieldFragment> eligibleFragments)
	{
		List<FragmentSortingData> list = new List<FragmentSortingData>();
		for (int i = 0; i < eligibleFragments.Count; i++)
		{
			FragmentSortingData fragmentSortingData = default(FragmentSortingData);
			fragmentSortingData.d = Vector2.Distance(point, eligibleFragments[i].transform.position);
			fragmentSortingData.fr = eligibleFragments[i];
			FragmentSortingData item = fragmentSortingData;
			list.Add(item);
		}
		return (from x in list
			orderby x.d
			select x.fr).ToList();
	}

	public void InterruptShieldRecharge()
	{
		if (currentCoroutine != null)
		{
			StopCoroutine(currentCoroutine);
		}
		foreach (AmanecidasShieldFragment shieldFragment in shieldFragments)
		{
			shieldFragment.FadeOut();
		}
	}
}
