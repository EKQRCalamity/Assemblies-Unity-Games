using System.Collections;
using Rewired;
using UnityEngine;

public class MapGraveyardHandler : MapDialogueInteraction
{
	[SerializeField]
	private GameObject graveFire;

	[SerializeField]
	private MapGraveyardGrave[] grave;

	[SerializeField]
	private float pressDurationToReEnable = 1f;

	[SerializeField]
	private GameObject ghostPrefab;

	[SerializeField]
	private Animator beamAnimator;

	private int[] puzzleOrder;

	private int entryCount;

	private int correctCount;

	private float currentDuration;

	private Animator[] extantGhosts;

	public bool canReenter { get; private set; }

	protected override void Start()
	{
		extantGhosts = new Animator[2];
		if (!PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, Charm.charm_curse) && !PlayerData.Data.IsUnlocked(PlayerId.PlayerTwo, Charm.charm_curse))
		{
			MapGraveyardGrave[] array = grave;
			foreach (MapGraveyardGrave mapGraveyardGrave in array)
			{
				mapGraveyardGrave.SetInteractable(value: false);
			}
			base.gameObject.SetActive(value: false);
			return;
		}
		base.Start();
		puzzleOrder = PlayerData.Data.curseCharmPuzzleOrder;
		AddDialoguerEvents();
		if (!PlayerData.Data.curseCharmPuzzleComplete)
		{
			ResetGraves();
			return;
		}
		if (!PlayerData.Data.GetLevelData(Levels.Graveyard).completed)
		{
			showBeam();
		}
		grave[5].SetInteractable(value: true);
	}

	protected override MapUIInteractionDialogue Show(PlayerInput player)
	{
		if (!PlayerData.Data.GetLevelData(Levels.Graveyard).completed)
		{
			return base.Show(player);
		}
		return null;
	}

	public void AddDialoguerEvents()
	{
		Dialoguer.events.onMessageEvent += OnDialoguerMessageEvent;
	}

	public void RemoveDialoguerEvents()
	{
		Dialoguer.events.onMessageEvent -= OnDialoguerMessageEvent;
	}

	private void OnDialoguerMessageEvent(string message, string metadata)
	{
		if (metadata == "LOADGRAVEYARD")
		{
			StartCoroutine(load_fight_cr());
		}
	}

	private IEnumerator load_fight_cr()
	{
		PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: false, promptBeforeJoin: false);
		SetPlayerReturnPos();
		Map.Current.CurrentState = Map.State.Graveyard;
		if (Map.Current.players[0] != null)
		{
			Map.Current.players[0].animator.SetTrigger("Sleep");
		}
		if (Map.Current.players[1] != null)
		{
			Map.Current.players[1].animator.SetTrigger("Sleep");
		}
		yield return new WaitForSeconds(1f);
		SceneLoader.LoadScene(Scenes.scene_level_graveyard, SceneLoader.Transition.Blur, SceneLoader.Transition.Blur, SceneLoader.Icon.HourglassBroken);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RemoveDialoguerEvents();
	}

	private void showBeam()
	{
		beamAnimator.Play("Aura", 1, 0f);
		beamAnimator.Play("Start", 0, 0f);
		beamAnimator.Update(0f);
	}

	public void ActivatedGrave(int index, int playerNum, Vector3 ghostPos)
	{
		if (!PlayerData.Data.curseCharmPuzzleComplete)
		{
			if (index < 0 || entryCount >= 3)
			{
				return;
			}
			Animator component = Object.Instantiate(ghostPrefab, ghostPos, Quaternion.identity).GetComponent<Animator>();
			SFX_GRAVEYARD_Interact(entryCount);
			if (index == puzzleOrder[entryCount])
			{
				correctCount++;
			}
			entryCount++;
			if (entryCount == puzzleOrder.Length)
			{
				if (correctCount == entryCount)
				{
					component.Play("Yes");
					SFX_GRAVEYARD_Positive();
					showBeam();
					PlayerData.Data.curseCharmPuzzleComplete = true;
					PlayerData.SaveCurrentFile();
				}
				else
				{
					component.Play("No");
					SFX_GRAVEYARD_Negative();
					StartCoroutine(reset_cr());
				}
				extantGhosts[0].SetTrigger("EngageEnd");
				extantGhosts[1].SetTrigger("EngageEnd");
			}
			else
			{
				component.Play("EngageStart");
				extantGhosts[entryCount - 1] = component;
			}
		}
		else if ((index == -1 && !PlayerData.Data.GetLevelData(Levels.Graveyard).completed) || canReenter)
		{
			StartSpeechBubble();
		}
	}

	private void UpdateReenterCodeActive()
	{
		switch (interactor)
		{
		default:
			if (PlayerWithinDistance(0))
			{
				Player actions4 = Map.Current.players[0].input.actions;
				if (actions4.GetButton(11) && actions4.GetButton(12))
				{
					currentDuration += CupheadTime.Delta;
				}
				else
				{
					currentDuration = 0f;
				}
			}
			break;
		case Interactor.Mugman:
			if (PlayerWithinDistance(1))
			{
				Player actions3 = Map.Current.players[1].input.actions;
				if (actions3.GetButton(11) && actions3.GetButton(12))
				{
					currentDuration += CupheadTime.Delta;
				}
				else
				{
					currentDuration = 0f;
				}
			}
			break;
		case Interactor.Either:
		{
			bool flag = false;
			if (PlayerWithinDistance(0))
			{
				Player actions = Map.Current.players[0].input.actions;
				if (actions.GetButton(11) && actions.GetButton(12))
				{
					currentDuration += CupheadTime.Delta;
					flag = true;
				}
			}
			if (PlayerWithinDistance(1))
			{
				Player actions2 = Map.Current.players[1].input.actions;
				if (actions2.GetButton(11) && actions2.GetButton(12))
				{
					currentDuration += CupheadTime.Delta;
					flag = true;
				}
			}
			if (!flag)
			{
				currentDuration = 0f;
			}
			break;
		}
		case Interactor.Both:
			if (Map.Current.players[0] == null || Map.Current.players[1] == null)
			{
				canReenter = false;
			}
			if (!PlayerWithinDistance(0) || !PlayerWithinDistance(1))
			{
				break;
			}
			if (Map.Current.players[0].input.actions.GetButton(13))
			{
				if (Map.Current.players[1].input.actions.GetButton(13))
				{
					currentDuration += CupheadTime.Delta;
				}
				else
				{
					currentDuration = 0f;
				}
			}
			else
			{
				currentDuration = 0f;
			}
			break;
		}
		if (currentDuration >= pressDurationToReEnable && !canReenter && PlayerData.Data.GetLevelData(Levels.Graveyard).completed)
		{
			SFX_GRAVEYARD_Positive();
			showBeam();
			canReenter = true;
		}
	}

	private void ResetGraves()
	{
		MapGraveyardGrave[] array = grave;
		foreach (MapGraveyardGrave mapGraveyardGrave in array)
		{
			mapGraveyardGrave.SetInteractable(value: true);
		}
		entryCount = 0;
		correctCount = 0;
	}

	private IEnumerator reset_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 2f);
		ResetGraves();
	}

	protected override void Update()
	{
		UpdateReenterCodeActive();
	}

	private void SFX_GRAVEYARD_Activate()
	{
		AudioManager.Play("sfx_dlc_worldmap_graveyard_activate");
	}

	private void SFX_GRAVEYARD_Interact(int i)
	{
		AudioManager.Play("sfx_dlc_worldmap_graveyard_interact_" + (i + 1));
	}

	private void SFX_GRAVEYARD_Negative()
	{
		AudioManager.Play("sfx_dlc_worldmap_graveyard_negative");
	}

	private void SFX_GRAVEYARD_Positive()
	{
		AudioManager.Play("sfx_dlc_worldmap_graveyard_positive");
	}
}
