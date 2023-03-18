using Com.LuisPedroFonseca.ProCamera2D;
using FMOD.Studio;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.UI.Widgets;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Interactables;

public class MiriamPortal : Interactable
{
	[BoxGroup("Design Settings", true, false, 0)]
	public float EnterDelay = 2f;

	[BoxGroup("Design Settings", true, false, 0)]
	public bool UseFade = true;

	[BoxGroup("Design Settings", true, false, 0)]
	public string InteractorAnimation = "OPEN_ENTER";

	[BoxGroup("Portal", true, false, 0)]
	public GameObject PortalPreRoot;

	[BoxGroup("Portal", true, false, 0)]
	public GameObject PortalOnRoot;

	[BoxGroup("Portal", true, false, 0)]
	public GameObject PortalOffRoot;

	[BoxGroup("Design Settings", true, false, 0)]
	[EventRef]
	public string PortalSound = string.Empty;

	[BoxGroup("Design Settings", true, false, 0)]
	[EventRef]
	public string portalAmbienceSound = "event:/SFX/Level/MiriamRoomPortal";

	private EventInstance _eventrefportalAmbienceSound;

	private string CurrentLevel = string.Empty;

	private bool IsPortalEnabled;

	private bool IsUsing;

	public override bool IsIgnoringPersistence()
	{
		return true;
	}

	public EntityOrientation Orientation()
	{
		return orientation;
	}

	protected override void OnAwake()
	{
		LevelManager.OnLevelLoaded += OnLevelLoaded;
	}

	protected override void OnDispose()
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		if (_eventrefportalAmbienceSound.isValid())
		{
			Core.Audio.StopEvent(ref _eventrefportalAmbienceSound);
		}
	}

	private void OnLevelLoaded(Framework.FrameworkCore.Level oldLevel, Framework.FrameworkCore.Level newLevel)
	{
		CurrentLevel = newLevel.LevelName;
		IsUsing = false;
		IsPortalEnabled = Core.Events.IsMiriamPortalEnabled(CurrentLevel);
		if (IsPortalEnabled)
		{
			Core.Audio.PlayEventNoCatalog(ref _eventrefportalAmbienceSound, portalAmbienceSound);
		}
		UpdatePortal();
	}

	private void UpdatePortal()
	{
		if (IsPortalEnabled)
		{
			PortalPreRoot.SetActive(value: false);
			PortalOnRoot.SetActive(value: true);
			PortalOffRoot.SetActive(value: false);
		}
		else if (Core.Events.IsMiriamQuestStarted)
		{
			PortalPreRoot.SetActive(value: false);
			PortalOnRoot.SetActive(value: false);
			PortalOffRoot.SetActive(value: true);
		}
		else
		{
			PortalPreRoot.SetActive(value: true);
			PortalOnRoot.SetActive(value: false);
			PortalOffRoot.SetActive(value: false);
		}
	}

	protected override void OnUpdate()
	{
		if (Core.Logic == null || !(Core.Logic.Penitent == null))
		{
			if (!IsUsing && IsPortalEnabled && base.PlayerInRange && base.InteractionTriggered && !base.Locked && !Core.Logic.Penitent.IsClimbingLadder)
			{
				UsePortal();
			}
			if (base.BeingUsed)
			{
				PlayerReposition();
			}
		}
	}

	protected override void InteractionStart()
	{
		ShowPlayer(show: false);
	}

	private void UsePortal()
	{
		IsUsing = true;
		interactorAnimator.SetTrigger(InteractorAnimation);
		Core.Audio.PlayOneShot(PortalSound);
		Core.Input.SetBlocker("DOOR", blocking: true);
		FadeWidget.OnFadeShowEnd += OnFadeShowEnd;
		FadeWidget.instance.Fade(toBlack: true, 0.2f, EnterDelay);
		Core.Logic.Penitent.Status.CastShadow = false;
		Core.Logic.Penitent.DamageArea.DamageAreaCollider.enabled = false;
		Core.Audio.StopEvent(ref _eventrefportalAmbienceSound);
	}

	private void OnFadeShowEnd()
	{
		ProCamera2D.Instance.HorizontalFollowSmoothness = 0f;
		ProCamera2D.Instance.VerticalFollowSmoothness = 0f;
		FadeWidget.OnFadeShowEnd -= OnFadeShowEnd;
		Core.Input.SetBlocker("DOOR", blocking: false);
		Core.Events.ActivateMiriamPortalAndTeleport(UseFade);
	}
}
