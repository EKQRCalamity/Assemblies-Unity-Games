using System.Collections;
using UnityEngine;

public class MapNPCLostBarbershop : AbstractMapInteractiveEntity
{
	[SerializeField]
	private string triggerShow;

	[SerializeField]
	private string triggerHide;

	[SerializeField]
	private MapNPCBarbershop[] mapNPCBarbershops;

	[SerializeField]
	private int dialoguerVariableID = 10;

	private bool reunited;

	private bool FirstTimeFoundSFX;

	private SpriteRenderer spriteRenderer;

	[HideInInspector]
	public bool SkipDialogueEvent;

	private void Start()
	{
		AddDialoguerEvents();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RemoveDialoguerEvents();
	}

	public void AddDialoguerEvents()
	{
		Dialoguer.events.onMessageEvent += OnDialoguerMessageEvent;
		Dialoguer.events.onEnded += OnDialogueEndedHandler;
		Dialoguer.events.onInstantlyEnded += OnDialogueEndedHandler;
	}

	public void RemoveDialoguerEvents()
	{
		Dialoguer.events.onMessageEvent -= OnDialoguerMessageEvent;
		Dialoguer.events.onEnded -= OnDialogueEndedHandler;
		Dialoguer.events.onInstantlyEnded -= OnDialogueEndedHandler;
	}

	private void OnDialoguerMessageEvent(string message, string metadata)
	{
		if (!SkipDialogueEvent && message == "LostBarberFound")
		{
			GetComponent<MapDialogueInteraction>().disabledActivations = true;
			reunited = true;
		}
	}

	protected override void Activate()
	{
	}

	protected override MapUIInteractionDialogue Show(PlayerInput player)
	{
		FoundBarbershopSFX();
		base.animator.SetTrigger(triggerShow);
		return null;
	}

	private void OnDialogueEndedHandler()
	{
		if (!SkipDialogueEvent && reunited)
		{
			Dialoguer.SetGlobalFloat(dialoguerVariableID, 1f);
			PlayerData.SaveCurrentFile();
			StartCoroutine(found_cr());
		}
	}

	private IEnumerator found_cr()
	{
		base.animator.SetTrigger(triggerHide);
		yield return base.animator.WaitForAnimationToEnd(this, "anim_map_barbershop_outtro_d");
		playerCanWalkBehind = true;
		SetLayer(GetComponent<SpriteRenderer>());
		for (int i = 0; i < mapNPCBarbershops.Length; i++)
		{
			mapNPCBarbershops[i].NowFour();
			if (mapNPCBarbershops[i].mapDialogueInteraction == null)
			{
				continue;
			}
			for (int j = 0; j < mapNPCBarbershops[i].mapDialogueInteraction.dialogues.Length; j++)
			{
				if (!(mapNPCBarbershops[i].mapDialogueInteraction.dialogues[j] == null))
				{
					mapNPCBarbershops[i].mapDialogueInteraction.Hide(mapNPCBarbershops[i].mapDialogueInteraction.dialogues[j]);
				}
			}
		}
		Hide(null);
		while (CupheadMapCamera.Current != null && CupheadMapCamera.Current.IsCameraFarFromPlayer())
		{
			yield return null;
		}
		for (int k = 0; k < mapNPCBarbershops.Length; k++)
		{
			mapNPCBarbershops[k].CleanUp();
		}
	}

	private void FoundBarbershopSFX()
	{
		if (!FirstTimeFoundSFX)
		{
			AudioManager.Play("find_barbershop_member");
			FirstTimeFoundSFX = true;
		}
	}
}
