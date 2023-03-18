using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Actionables;

public class AshPlatform : MonoBehaviour, IActionable
{
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool firstPlatform;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private GameObject[] target;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[EventRef]
	private string appearSound;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[EventRef]
	private string disappearSound;

	public bool showing;

	private Tween currentTween;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private CollisionSensor collisionSensor;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private Collider2D collision;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private Animator animator;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private GameObject nextPlatformParticles;

	private MasterShaderEffects spriteEffects;

	public bool Locked { get; set; }

	private void Awake()
	{
		if (collision != null)
		{
			collision.enabled = false;
		}
		spriteEffects = GetComponentInChildren<MasterShaderEffects>();
	}

	private void Start()
	{
		PoolManager.Instance.CreatePool(nextPlatformParticles, target.Length);
		animator.SetBool("ENABLED", value: false);
		showing = false;
		collision.enabled = false;
	}

	private void OnDestroy()
	{
	}

	private IEnumerator DeactivateAfterSeconds(float seconds, float warningPercentage = 0.8f, Action callbackOnWarning = null)
	{
		float counter = 0f;
		bool warningActivated = false;
		while (counter < seconds)
		{
			counter += Time.deltaTime;
			yield return null;
			if (warningPercentage < counter / seconds && !warningActivated)
			{
				callbackOnWarning();
				warningActivated = true;
			}
		}
		HideAction();
	}

	private void DeactivationWarning()
	{
		spriteEffects.TriggerColorizeLerp(0f, 1f, 1f);
	}

	public void Show()
	{
		spriteEffects.DeactivateColorize();
		ShowAction();
	}

	public void Hide(float delay, float warningPercentage = 0.8f)
	{
		StopAllCoroutines();
		spriteEffects.DeactivateColorize();
		StartCoroutine(DeactivateAfterSeconds(delay, warningPercentage, DeactivationWarning));
	}

	private void ShowAction()
	{
		if (spriteRenderer.isVisible && !showing)
		{
			Core.Audio.PlaySfx(appearSound);
		}
		animator.SetBool("ENABLED", value: true);
		showing = true;
		collision.enabled = true;
	}

	private void HideAction()
	{
		if (spriteRenderer.isVisible && showing)
		{
			Core.Audio.PlaySfx(disappearSound);
		}
		animator.SetBool("ENABLED", value: false);
		showing = false;
		collision.enabled = false;
	}

	private void EffectToNextPlatform(AshPlatform p)
	{
		Vector3 position = base.transform.position;
		Vector3 position2 = p.transform.position;
		GameObject gameObject = PoolManager.Instance.ReuseObject(nextPlatformParticles, position, Quaternion.identity).GameObject;
		gameObject.transform.DOMove(position2, 0.2f).SetEase(Ease.OutCubic);
	}

	public List<GameObject> GetTargets()
	{
		return new List<GameObject>(target);
	}

	public void Use()
	{
		Show();
	}
}
