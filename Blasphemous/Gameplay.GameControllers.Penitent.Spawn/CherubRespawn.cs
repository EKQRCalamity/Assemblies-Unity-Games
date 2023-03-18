using System;
using FMOD.Studio;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Pooling;
using Framework.Util;
using Gameplay.GameControllers.Effects.Entity.BlobShadow;
using Gameplay.UI.Widgets;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Spawn;

public class CherubRespawn : PoolObject
{
	private UnityEngine.Animator _animator;

	private SpriteRenderer _rootRenderer;

	private SpriteRenderer _cherubsRenderer;

	private Penitent _penitent;

	private BlobShadow _penitentShadow;

	[SerializeField]
	[TutorialId]
	private string TutorialFirstDead;

	[SerializeField]
	[TutorialId]
	private string TutorialSecondDead;

	[EventRef]
	public string CherubRespawnFx;

	private EventInstance _soundInstance;

	public bool PenitentShadowVisible { get; private set; }

	private void Awake()
	{
		_animator = GetComponent<UnityEngine.Animator>();
		_rootRenderer = GetComponent<SpriteRenderer>();
		_cherubsRenderer = GetComponentInChildren<SpriteRenderer>();
	}

	private void Start()
	{
		FadeWidget.OnFadeHidedEnd += OnFadeIn;
		LogicManager.GoToMainMenu = (Core.SimpleEvent)Delegate.Combine(LogicManager.GoToMainMenu, new Core.SimpleEvent(GoToMainMenu));
	}

	private void OnDestroy()
	{
		FadeWidget.OnFadeHidedEnd -= OnFadeIn;
	}

	private void Update()
	{
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		if (_penitentShadow == null && (bool)_penitent)
		{
			_penitentShadow = _penitent.Shadow;
		}
		if (!PenitentShadowVisible && (bool)_penitentShadow)
		{
			_penitentShadow.gameObject.SetActive(value: false);
		}
	}

	private void OnFadeIn()
	{
		if (!(_animator == null) && !Core.Logic.IsMenuScene())
		{
			base.transform.position = Core.Logic.Penitent.transform.position;
			_animator.SetTrigger("APPEAR");
			_rootRenderer.flipX = ((Core.Logic.Penitent.GetOrientation() != 0) ? true : false);
			_cherubsRenderer.flipX = ((Core.Logic.Penitent.GetOrientation() != 0) ? true : false);
			PlayRespawnSound();
		}
	}

	private void GoToMainMenu()
	{
		LogicManager.GoToMainMenu = (Core.SimpleEvent)Delegate.Remove(LogicManager.GoToMainMenu, new Core.SimpleEvent(GoToMainMenu));
		FadeWidget.OnFadeHidedEnd -= OnFadeIn;
		StopRespawnSound();
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		SetPlayerVisible(visible: false);
	}

	public void SetPlayerVisible(bool visible = true)
	{
		_penitent = UnityEngine.Object.FindObjectOfType<Penitent>();
		if (!(_penitent == null))
		{
			_penitent.SpriteRenderer.enabled = visible;
			PenitentShadowVisible = false;
		}
	}

	public void Dispose()
	{
		_penitent = null;
		_penitentShadow = null;
		Core.Events.SetFlag("CHERUB_RESPAWN", b: false);
		SetPlayerVisible();
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		string text = string.Empty;
		if (!Core.TutorialManager.IsTutorialUnlocked(TutorialFirstDead))
		{
			text = TutorialFirstDead;
		}
		else if (!Core.TutorialManager.IsTutorialUnlocked(TutorialSecondDead))
		{
			text = TutorialSecondDead;
		}
		if (text != string.Empty)
		{
			Singleton<Core>.Instance.StartCoroutine(Core.TutorialManager.ShowTutorial(text));
		}
		Destroy();
	}

	public void SetShadowVisible()
	{
		if (!(_penitentShadow == null))
		{
			PenitentShadowVisible = true;
			_penitentShadow.gameObject.SetActive(value: true);
		}
	}

	private void PlayRespawnSound()
	{
		StopRespawnSound();
		if (!_soundInstance.isValid())
		{
			_soundInstance = Core.Audio.CreateEvent(CherubRespawnFx);
			_soundInstance.start();
		}
	}

	private void StopRespawnSound()
	{
		if (_soundInstance.isValid())
		{
			_soundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_soundInstance.release();
			_soundInstance = default(EventInstance);
		}
	}
}
