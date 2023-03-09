using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDialogueInteraction : AbstractMapInteractiveEntity
{
	[Serializable]
	public struct speechBubblePositionLanguage
	{
		public Localization.Languages languageApplied;

		public Vector2 speechBubblePosition;
	}

	[Serializable]
	public struct DEBUG_DialoguerCondition
	{
		public int ConditionId;

		public float Values;
	}

	[SerializeField]
	private SpeechBubble speechBubblePrefab;

	[SerializeField]
	private Vector2 speechBubblePosition;

	[SerializeField]
	private speechBubblePositionLanguage[] speechBubblePositions;

	[SerializeField]
	private Vector2 panCameraToPosition;

	[SerializeField]
	private int maxLines = -1;

	[SerializeField]
	private bool tailOnTheLeft;

	[SerializeField]
	private bool hideTail;

	[SerializeField]
	private bool expandOnTheRight;

	protected SpeechBubble speechBubble;

	public DialoguerDialogues dialogueInteraction;

	private Coroutine cutsceneCoroutine;

	[HideInInspector]
	public bool disabledActivations;

	[Header("DEBUG")]
	public List<DEBUG_DialoguerCondition> DebugDialogerCondition = new List<DEBUG_DialoguerCondition>();

	private bool IsDialogueEnded;

	public bool currentlySpeaking;

	protected virtual void Start()
	{
		if (speechBubble == null)
		{
			Vector3 vector = base.transform.position;
			if (speechBubblePositions != null)
			{
				vector = ApplyCustonMargin(vector);
			}
			else
			{
				vector.x += speechBubblePosition.x;
				vector.y += speechBubblePosition.y;
			}
			speechBubble = SpeechBubble.Instance;
			if (speechBubble == null)
			{
				speechBubble = UnityEngine.Object.Instantiate(speechBubblePrefab.gameObject, vector, Quaternion.identity, MapUI.Current.sceneCanvas.transform).GetComponent<SpeechBubble>();
			}
		}
		Dialoguer.events.onEnded += OnDialogueEndedHandler;
		Dialoguer.events.onInstantlyEnded += OnDialogueEndedHandler;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Dialoguer.events.onEnded -= OnDialogueEndedHandler;
		Dialoguer.events.onInstantlyEnded -= OnDialogueEndedHandler;
	}

	private void OnDialogueEndedHandler()
	{
		IsDialogueEnded = true;
		Check();
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		Vector3 vector = base.transform.position;
		if (speechBubblePositions != null)
		{
			vector = ApplyCustonMargin(vector);
		}
		else
		{
			vector.x += speechBubblePosition.x;
			vector.y += speechBubblePosition.y;
		}
		Gizmos.DrawWireSphere(vector, interactionDistance * 0.5f);
		vector = base.transform.position;
		vector.x += panCameraToPosition.x;
		vector.y += panCameraToPosition.y;
		Gizmos.DrawWireSphere(vector, interactionDistance * 0.5f);
	}

	protected override void Activate(MapPlayerController player)
	{
		if (dialogues[(int)player.id] != null && dialogues[(int)player.id].transform.localScale.x == 1f && MapBasicStartUI.Current.CurrentState == AbstractMapSceneStartUI.State.Inactive && MapDifficultySelectStartUI.Current.CurrentState == AbstractMapSceneStartUI.State.Inactive && MapConfirmStartUI.Current.CurrentState == AbstractMapSceneStartUI.State.Inactive && (Map.Current == null || Map.Current.CurrentState != Map.State.Graveyard))
		{
			base.Activate(player);
			StartSpeechBubble();
		}
	}

	protected virtual void StartSpeechBubble()
	{
		if (speechBubble.displayState == SpeechBubble.DisplayState.Hidden)
		{
			Vector3 vector = base.transform.position;
			if (speechBubblePositions != null)
			{
				vector = ApplyCustonMargin(vector);
			}
			else
			{
				vector.x += speechBubblePosition.x;
				vector.y += speechBubblePosition.y;
			}
			speechBubble.basePosition = vector;
			vector = base.transform.position;
			vector.x += panCameraToPosition.x;
			vector.y += panCameraToPosition.y;
			speechBubble.panPosition = vector;
			speechBubble.maxLines = maxLines;
			speechBubble.tailOnTheLeft = tailOnTheLeft;
			speechBubble.expandOnTheRight = expandOnTheRight;
			speechBubble.hideTail = hideTail;
			if (cutsceneCoroutine != null)
			{
				StopCoroutine(cutsceneCoroutine);
			}
			cutsceneCoroutine = StartCoroutine(CutScene_cr());
		}
	}

	protected override void Check()
	{
		if (!disabledActivations)
		{
			base.Check();
		}
	}

	private IEnumerator CutScene_cr()
	{
		if (speechBubble.displayState != 0)
		{
			yield break;
		}
		for (int i = 0; i < Map.Current.players.Length; i++)
		{
			if (!(Map.Current.players[i] == null))
			{
				Map.Current.players[i].Disable();
			}
		}
		yield return null;
		currentlySpeaking = true;
		Dialoguer.StartDialogue(dialogueInteraction);
		DialoguerEvents.EndedHandler afterDialogue = null;
		afterDialogue = delegate
		{
			Dialoguer.events.onEnded -= afterDialogue;
			StartCoroutine(reactivate_input_cr());
		};
		Dialoguer.events.onEnded += afterDialogue;
	}

	private IEnumerator reactivate_input_cr()
	{
		if (CupheadMapCamera.Current != null)
		{
			CupheadMapCamera.Current.SetActiveCollider(active: false);
		}
		while (CupheadMapCamera.Current != null && CupheadMapCamera.Current.IsCameraFarFromPlayer())
		{
			yield return null;
		}
		if (CupheadMapCamera.Current != null)
		{
			CupheadMapCamera.Current.SetActiveCollider(active: true);
		}
		for (int i = 0; i < Map.Current.players.Length; i++)
		{
			if (!(Map.Current.players[i] == null))
			{
				Map.Current.players[i].Enable();
			}
		}
		currentlySpeaking = false;
	}

	private Vector3 ApplyCustonMargin(Vector3 pos)
	{
		int num = 0;
		bool flag = false;
		while (!flag && num < speechBubblePositions.Length)
		{
			flag = speechBubblePositions[num].languageApplied == Localization.language;
			num = ((!flag) ? (num + 1) : num);
		}
		if (flag)
		{
			speechBubblePositionLanguage speechBubblePositionLanguage = speechBubblePositions[num];
			pos.x += speechBubblePositionLanguage.speechBubblePosition.x;
			pos.y += speechBubblePositionLanguage.speechBubblePosition.y;
		}
		else
		{
			pos.x += speechBubblePosition.x;
			pos.y += speechBubblePosition.y;
		}
		return pos;
	}

	public IEnumerator DebugStartDialogue()
	{
		yield return StartCoroutine(debugActivate_input_cr());
	}

	private IEnumerator debugActivate_input_cr()
	{
		bool DoneLanguage = false;
		int index = 0;
		WaitForSeconds Wait = new WaitForSeconds(0.3f);
		Localization.Languages startedLanguage = Localization.language;
		int ConditionIndex = 0;
		bool DoneVariables = false;
		yield return Wait;
		yield return Wait;
		while (!DoneLanguage)
		{
			DoneVariables = false;
			ConditionIndex = 0;
			while (!DoneVariables)
			{
				if (DebugDialogerCondition.Count > 0)
				{
					Dialoguer.SetGlobalFloat(DebugDialogerCondition[ConditionIndex].ConditionId, DebugDialogerCondition[ConditionIndex].Values);
				}
				IsDialogueEnded = false;
				index = 0;
				yield return Wait;
				yield return Wait;
				Activate(Map.Current.players[0]);
				yield return Wait;
				while (!IsDialogueEnded)
				{
					string ConditionKey = string.Empty;
					if (DebugDialogerCondition.Count > 0)
					{
						ConditionKey = "_" + DebugDialogerCondition[ConditionIndex].ConditionId + "_" + DebugDialogerCondition[ConditionIndex].Values;
					}
					ScreenshotHandler.TakeScreenshot_Static(ScreenshotHandler.cameraType.Map, "LOC_Screenshot", string.Concat(dialogueInteraction.ToString(), ConditionKey, "_", Localization.language, "_", index++));
					yield return Wait;
					Dialoguer.ContinueDialogue();
					yield return Wait;
				}
				ConditionIndex++;
				if (ConditionIndex >= DebugDialogerCondition.Count)
				{
					DoneVariables = true;
				}
			}
			yield return null;
			yield return null;
			yield return null;
			if (Localization.language == startedLanguage)
			{
				DoneLanguage = true;
			}
		}
	}
}
