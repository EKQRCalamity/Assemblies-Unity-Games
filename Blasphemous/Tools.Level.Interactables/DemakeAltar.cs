using System.Collections;
using FMOD.Studio;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.UI;
using Gameplay.UI.Widgets;
using Sirenix.OdinInspector;
using Tools.DataContainer;
using UnityEngine;

namespace Tools.Level.Interactables;

[SelectionBase]
public class DemakeAltar : Interactable
{
	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private string FirstDemakeScene;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private float Price = 2500f;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private string kneeStartId;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private string kneeEndId;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	private string insertCoinEvent;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private float kneeStartDelay;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private float kneeEndDelay;

	[SerializeField]
	[BoxGroup("Event Settings", true, false, 0)]
	[ReadOnly]
	private string OnAltarUse = "DEMAKE_ALTAR_ACTIVATED";

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	[Required]
	private SpriteRenderer entityRenderer;

	[FoldoutGroup("Intro cutscene", 0)]
	public CutsceneData demakeIntroCutscene;

	[FoldoutGroup("Intro cutscene", 0)]
	public bool mute;

	[FoldoutGroup("Intro cutscene", 0)]
	public float fadeTimeStart = 0.5f;

	[FoldoutGroup("Intro cutscene", 0)]
	public float fadeTimeEnd = 0.5f;

	[FoldoutGroup("Intro cutscene", 0)]
	public bool useBackground;

	private bool penitentKneeing;

	private int DialogResponse;

	private Framework.FrameworkCore.Level prevLevel;

	private EventInstance arcadeEventInstance;

	private void OnAnimationEvent(string id)
	{
		if (id == "KNEE_END")
		{
			penitentKneeing = false;
		}
	}

	protected override void OnPlayerReady()
	{
		CheckAnimationEvents();
	}

	protected override void ObjectEnable()
	{
		if (!(base.AnimatorEvent == null))
		{
			base.AnimatorEvent.OnEventLaunched += OnAnimationEvent;
			Core.Audio.PlayEventNoCatalog(ref arcadeEventInstance, "event:/SFX/DEMAKE/ArcadeMusic", base.transform.position);
		}
	}

	protected override void ObjectDisable()
	{
		if (!(base.AnimatorEvent == null))
		{
			base.AnimatorEvent.OnEventLaunched -= OnAnimationEvent;
			Core.Audio.StopEvent(ref arcadeEventInstance);
		}
	}

	protected override IEnumerator OnUse()
	{
		yield return ActivationLogic();
	}

	protected override void InteractionEnd()
	{
		Core.Logic.Penitent.SpriteRenderer.enabled = true;
		Core.Logic.Penitent.DamageArea.enabled = true;
	}

	protected override void OnUpdate()
	{
		if (!base.BeingUsed && base.PlayerInRange && base.InteractionTriggered)
		{
			Use();
		}
		if (base.BeingUsed)
		{
			PlayerReposition();
		}
	}

	protected override void PlayerReposition()
	{
		Core.Logic.Penitent.transform.position = interactorAnimator.transform.position;
	}

	private IEnumerator ActivationLogic()
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
		Core.Logic.Penitent.SpriteRenderer.enabled = false;
		Core.Logic.Penitent.DamageArea.enabled = false;
		Core.Audio.PlaySfxOnCatalog(kneeStartId, kneeStartDelay);
		if (entityRenderer != null)
		{
			entityRenderer.flipX = Core.Logic.Penitent.Status.Orientation == EntityOrientation.Left;
		}
		interactorAnimator.SetTrigger("KNEE_START");
		Core.Events.LaunchEvent(OnAltarUse, base.name);
		yield return DemakeAltarCourrutine();
	}

	private IEnumerator DemakeAltarCourrutine()
	{
		yield return StartConversationAndWait("DLG_QT_ALTARDEMAKE", (int)Price);
		if (DialogResponse == 0)
		{
			if (Core.Logic.Penitent.Stats.Purge.Current < Price)
			{
				yield return StartConversationAndWait("DLG_CONFESSOR_NOTENOUGHPURGE", 0);
				yield return new WaitForSeconds(0.5f);
				interactorAnimator.SetTrigger("KNEE_END");
				Core.Audio.PlaySfxOnCatalog(kneeEndId, kneeEndDelay);
				yield break;
			}
			Core.Audio.PlaySfx(insertCoinEvent);
			Core.Logic.Penitent.Stats.Purge.Current -= Price;
			Core.Persistence.SaveGame(fullSave: false);
			yield return new WaitForSeconds(1.5f);
			Core.Audio.Ambient.StopCurrent();
			Core.Audio.StopEvent(ref arcadeEventInstance);
			Core.Audio.PlayNamedSound("event:/Background Layer/DemakePressStartScreen", "DEMAKE_INTRO");
			FadeWidget.OnFadeShowEnd += OnFadeShowEnd;
			FadeWidget.instance.StartEasyFade(new Color(0f, 0f, 0f, 0f), Color.black, 1f, toBlack: true);
		}
		else
		{
			interactorAnimator.SetTrigger("KNEE_END");
			Core.Audio.PlaySfxOnCatalog(kneeEndId, kneeEndDelay);
		}
	}

	private void OnFadeShowEnd()
	{
		FadeWidget.OnFadeShowEnd -= OnFadeShowEnd;
		UIController.instance.ShowIntroDemakeWidget(OnPressStart);
	}

	private void OnPressStart()
	{
		Core.DemakeManager.StartDemakeRun();
	}

	private IEnumerator ShowMessage(string Id, float time)
	{
		Core.Dialog.ShowMessage(Id, 0, string.Empty, time);
		yield return new WaitForSecondsRealtime(time);
	}

	private IEnumerator StartConversationAndWait(string Id, int purge)
	{
		int noResponse = (DialogResponse = -10);
		Core.Dialog.OnDialogFinished += DialogEnded;
		Core.Logic.Penitent.Animator.SetBool("IS_DIALOGUE_MODE", value: true);
		Core.Dialog.StartConversation(Id, modal: true, useOnlyLast: false, hideWidget: true, purge);
		while (DialogResponse == noResponse)
		{
			yield return null;
		}
		yield return null;
	}

	private void DialogEnded(string id, int response)
	{
		Core.Dialog.OnDialogFinished -= DialogEnded;
		Core.Logic.Penitent.Animator.SetBool("IS_DIALOGUE_MODE", value: false);
		DialogResponse = response;
	}
}
