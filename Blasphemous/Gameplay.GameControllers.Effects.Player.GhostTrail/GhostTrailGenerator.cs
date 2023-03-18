using System;
using System.Collections;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Penitent;
using Tools.Level.Layout;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.GhostTrail;

[RequireComponent(typeof(SpriteRenderer))]
public class GhostTrailGenerator : Trait
{
	private readonly List<SpriteRenderer> _trailParts = new List<SpriteRenderer>();

	private LevelInitializer _currentLevel;

	public Material customMaterial;

	[Tooltip("The alpha decrease factor in every step")]
	[Range(0f, 1f)]
	public float AlphaStep = 0.1f;

	[Tooltip("The alpha value of the ghost trail SpriteRenderer color when it is spawned")]
	[Range(0f, 1f)]
	public float InitialGhostTrailAlpha = 0.6f;

	[Tooltip("The time rate step for invoking ghost trail")]
	[Range(0f, 1f)]
	public float TimeStep;

	[Tooltip("The color of the ghost trail sprite renderer")]
	public Color TrailColor;

	[Tooltip("Should the rotation of the sprite be followed?")]
	public bool followRotation;

	[Tooltip("Should the scale of the entity be followed?")]
	public bool followScale;

	[Tooltip("Should the orientation of the entity be followed?")]
	public bool followOrientation = true;

	[SerializeField]
	public bool EnableGhostTrail;

	private bool _startGenerateTrail;

	public static bool AreGhostTrailsAllowed = true;

	public void DoEnableTrail()
	{
		EnableGhostTrail = true;
	}

	public void DoDisableTrail()
	{
		EnableGhostTrail = false;
	}

	protected override void OnStart()
	{
		base.OnStart();
		_currentLevel = Core.Logic.CurrentLevelConfig;
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		AreGhostTrailsAllowed = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!AreGhostTrailsAllowed && EnableGhostTrail)
		{
			EnableGhostTrail = false;
		}
		if (EnableGhostTrail)
		{
			if (!_startGenerateTrail)
			{
				_startGenerateTrail = true;
				InvokeRepeating("GetGhostTrail", 0f, TimeStep);
				Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
				penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(OnPenitentDead));
			}
		}
		else if (_startGenerateTrail)
		{
			_startGenerateTrail = !_startGenerateTrail;
			CancelInvoke();
			Gameplay.GameControllers.Penitent.Penitent penitent2 = Core.Logic.Penitent;
			penitent2.OnDead = (Core.SimpleEvent)Delegate.Remove(penitent2.OnDead, new Core.SimpleEvent(OnPenitentDead));
		}
	}

	private void OnPenitentDead()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Remove(penitent.OnDead, new Core.SimpleEvent(OnPenitentDead));
		EnableGhostTrail = false;
	}

	private void GetGhostTrail()
	{
		if (!EnableGhostTrail)
		{
			return;
		}
		SpriteRenderer spriteRenderer;
		if (_trailParts.Count > 0)
		{
			spriteRenderer = _trailParts[_trailParts.Count - 1];
			if (!spriteRenderer)
			{
				return;
			}
			_trailParts.Remove(spriteRenderer);
			spriteRenderer.gameObject.SetActive(value: true);
			spriteRenderer.gameObject.transform.position = base.transform.position;
			if (followRotation)
			{
				spriteRenderer.gameObject.transform.rotation = base.transform.rotation;
			}
			if (followScale)
			{
				spriteRenderer.transform.localScale = base.transform.localScale;
			}
			spriteRenderer.sprite = GetComponent<SpriteRenderer>().sprite;
			Color trailColor = TrailColor;
			if (trailColor.a < 1f)
			{
				trailColor.a = 1f;
			}
			spriteRenderer.color = trailColor;
		}
		else
		{
			GameObject gameObject = new GameObject();
			gameObject.layer = LayerMask.NameToLayer("Penitent");
			GameObject gameObject2 = gameObject;
			spriteRenderer = gameObject2.AddComponent<SpriteRenderer>();
			if ((bool)customMaterial)
			{
				spriteRenderer.material = customMaterial;
			}
			spriteRenderer.sprite = GetComponent<SpriteRenderer>().sprite;
			spriteRenderer.color = TrailColor;
			gameObject2.transform.position = base.transform.position;
			if (followRotation)
			{
				gameObject2.transform.rotation = base.transform.rotation;
			}
			gameObject2.transform.localScale = base.transform.localScale;
			gameObject2.transform.parent = _currentLevel.LevelEffectsStore.transform;
		}
		if (followOrientation)
		{
			spriteRenderer.flipX = base.EntityOwner.Status.Orientation == EntityOrientation.Left;
		}
		Singleton<Core>.Instance.StartCoroutine(FadeTrailPart(spriteRenderer));
	}

	private void StoreGhostTrail(SpriteRenderer ghostTrail)
	{
		if (ghostTrail.gameObject.activeSelf)
		{
			_trailParts.Add(ghostTrail);
			ghostTrail.gameObject.SetActive(value: false);
		}
	}

	public void DrainGhostTrailPool()
	{
		if (_trailParts.Count > 0)
		{
			_trailParts.Clear();
		}
	}

	private IEnumerator FadeTrailPart(SpriteRenderer trailPartRenderer)
	{
		Color color = trailPartRenderer.color;
		color.a = InitialGhostTrailAlpha;
		WaitForEndOfFrame delay = new WaitForEndOfFrame();
		while (color.a >= 0f)
		{
			if (!trailPartRenderer)
			{
				yield break;
			}
			trailPartRenderer.color = color;
			color.a -= AlphaStep;
			yield return delay;
		}
		if ((bool)trailPartRenderer)
		{
			color.a = 0f;
			trailPartRenderer.color = color;
			StoreGhostTrail(trailPartRenderer);
		}
	}
}
