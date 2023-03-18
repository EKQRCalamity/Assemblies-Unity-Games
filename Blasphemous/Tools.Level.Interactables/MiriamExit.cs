using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.UI.Widgets;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Interactables;

public class MiriamExit : Interactable
{
	[BoxGroup("Design Settings", true, false, 0)]
	public bool UseFade = true;

	[BoxGroup("Design Settings", true, false, 0)]
	public string InteractorAnimationName = string.Empty;

	[BoxGroup("Design Settings", true, false, 0)]
	public string InteractableAnimationTrigger = "TRIGGER_TAKE";

	[BoxGroup("Design Settings", true, false, 0)]
	public float InteractableAnimationDelay = 1f;

	[BoxGroup("Design Settings", true, false, 0)]
	public float FadeDelay = 1f;

	[BoxGroup("Design Settings", true, false, 0)]
	public float FadeTime = 0.6f;

	[BoxGroup("Design Settings", true, false, 0)]
	[EventRef]
	public string SharkSound = string.Empty;

	private bool IsUsing;

	public override bool IsIgnoringPersistence()
	{
		return true;
	}

	protected override void OnAwake()
	{
		LevelManager.OnLevelLoaded += OnLevelLoaded;
	}

	protected override void OnDispose()
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
	}

	private void OnLevelLoaded(Framework.FrameworkCore.Level oldLevel, Framework.FrameworkCore.Level newLevel)
	{
		IsUsing = false;
	}

	protected override void OnUpdate()
	{
		if (Core.Logic == null || !(Core.Logic.Penitent == null))
		{
			if (!IsUsing && base.PlayerInRange && base.InteractionTriggered && !base.Locked && !Core.Logic.Penitent.IsClimbingLadder)
			{
				UsePortal();
			}
			if (base.BeingUsed)
			{
				PlayerReposition();
			}
		}
	}

	protected override void InteractionEnd()
	{
		ShowPlayer(show: true);
	}

	private void UsePortal()
	{
		StartCoroutine(UseCourrutine());
	}

	private IEnumerator UseCourrutine()
	{
		if (RepositionBeforeInteract)
		{
			Core.Logic.Penitent.DrivePlayer.MoveToPosition(Waypoint.position, orientation);
			do
			{
				yield return null;
			}
			while (Core.Logic.Penitent.DrivePlayer.Casting);
		}
		IsUsing = true;
		ShowPlayer(show: false);
		Core.Input.SetBlocker("MIRIAM_PORTAL", blocking: true);
		Core.Logic.Penitent.Status.CastShadow = false;
		interactorAnimator.Play(InteractorAnimationName);
		yield return new WaitForSeconds(InteractableAnimationDelay);
		Core.Audio.PlayOneShot(SharkSound);
		interactableAnimator.SetTrigger(InteractableAnimationTrigger);
		yield return new WaitForSeconds(FadeDelay);
		Core.Audio.StopNamedSound("TeleportAltarMiriam");
		FadeWidget.OnFadeShowEnd += OnFadeShowEnd;
		FadeWidget.instance.Fade(toBlack: true, FadeTime);
	}

	private void OnFadeShowEnd()
	{
		ProCamera2D.Instance.HorizontalFollowSmoothness = 0f;
		ProCamera2D.Instance.VerticalFollowSmoothness = 0f;
		FadeWidget.OnFadeShowEnd -= OnFadeShowEnd;
		Core.Input.SetBlocker("MIRIAM_PORTAL", blocking: false);
		Core.Events.EndMiriamPortalAndReturn(UseFade);
	}
}
