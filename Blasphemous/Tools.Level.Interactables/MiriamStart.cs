using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.UI.Widgets;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Interactables;

public class MiriamStart : Interactable
{
	[BoxGroup("Design Settings", true, false, 0)]
	public bool UseFade = true;

	[BoxGroup("Design Settings", true, false, 0)]
	public string InteractorAnimationTrigger = "OPEN_ENTER";

	[BoxGroup("Design Settings", true, false, 0)]
	public float FadeDelay = 1f;

	[BoxGroup("Design Settings", true, false, 0)]
	public float FadeTime = 0.6f;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	public EntityOrientation spawnOrientation;

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
		Core.Audio.PlaySfxOnCatalog("CHECKPOINT_KNEE_START", 0.65f);
		interactorAnimator.SetTrigger(InteractorAnimationTrigger);
		yield return new WaitForSeconds(FadeDelay);
		FadeWidget.OnFadeShowEnd += OnFadeShowEnd;
		FadeWidget.instance.Fade(toBlack: true, FadeTime);
	}

	private void OnFadeShowEnd()
	{
		ProCamera2D.Instance.HorizontalFollowSmoothness = 0f;
		ProCamera2D.Instance.VerticalFollowSmoothness = 0f;
		FadeWidget.OnFadeShowEnd -= OnFadeShowEnd;
		Core.Input.SetBlocker("MIRIAM_PORTAL", blocking: false);
		Core.Events.CancelMiriamPortalAndReturn(UseFade);
	}
}
