using System.Collections;
using UnityEngine;

public class MapNPCChaliceFanB : AbstractMonoBehaviour
{
	private const int CHALICEFANBSTATE_INDEX = 25;

	[SerializeField]
	private SpriteRenderer[] lineSprites;

	[SerializeField]
	private SpriteRenderer blinkRend;

	[SerializeField]
	private MinMax blinkRange = new MinMax(3f, 5f);

	[SerializeField]
	private Animator campfire;

	private Levels undefeatedBoss = Levels.Test;

	private bool questComplete;

	private void Start()
	{
		if (PlayerData.Data.hasTalkedToChaliceFan)
		{
			Dialoguer.SetGlobalFloat(25, 1f);
		}
		AddDialoguerEvents();
		int num = 0;
		for (int i = 0; i < Level.chaliceLevels.Length; i++)
		{
			if (PlayerData.Data.GetLevelData(Level.chaliceLevels[i]).completedAsChaliceP1)
			{
				lineSprites[i].enabled = true;
			}
		}
		for (int j = 0; j < Level.chaliceLevels.Length; j++)
		{
			if (PlayerData.Data.GetLevelData(Level.chaliceLevels[j]).completedAsChaliceP1)
			{
				num++;
			}
			else if (undefeatedBoss == Levels.Test)
			{
				undefeatedBoss = Level.chaliceLevels[j];
			}
		}
		if (num == Level.chaliceLevels.Length)
		{
			Dialoguer.SetGlobalFloat(25, 2f);
			campfire.gameObject.SetActive(value: true);
			StartCoroutine(campfire_smoke_cr());
			questComplete = true;
		}
		else
		{
			SetBossRefText(undefeatedBoss);
		}
		StartCoroutine(blink_cr());
	}

	private void SetBossRefText(Levels level)
	{
		TranslationElement translationElement = Localization.Find(level.ToString() + "Reference");
		SpeechBubble.Instance.setBossRefText = translationElement.translation.text;
	}

	private void UpdateBossRef()
	{
		SetBossRefText(undefeatedBoss);
	}

	private void OnDestroy()
	{
		RemoveDialoguerEvents();
	}

	public void AddDialoguerEvents()
	{
		Localization.OnLanguageChangedEvent += UpdateBossRef;
		Dialoguer.events.onMessageEvent += OnDialoguerMessageEvent;
		Dialoguer.events.onEnded += OnDialogueEnded;
	}

	public void RemoveDialoguerEvents()
	{
		Localization.OnLanguageChangedEvent -= UpdateBossRef;
		Dialoguer.events.onMessageEvent -= OnDialoguerMessageEvent;
		Dialoguer.events.onEnded -= OnDialogueEnded;
	}

	private void OnDialoguerMessageEvent(string message, string metadata)
	{
		if (message == "MetChaliceFan")
		{
			PlayerData.Data.hasTalkedToChaliceFan = true;
			PlayerData.SaveCurrentFile();
			Dialoguer.SetGlobalFloat(25, 1f);
		}
	}

	private IEnumerator blink_cr()
	{
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, blinkRange.RandomFloat());
			while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f > 0.1f)
			{
				yield return null;
			}
			blinkRend.enabled = true;
			while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f < 0.9f)
			{
				yield return null;
			}
			blinkRend.enabled = false;
		}
	}

	private IEnumerator campfire_smoke_cr()
	{
		campfire.SetBool("SmokeL", value: true);
		yield return CupheadTime.WaitForSeconds(this, Random.Range(0.5f, 1f));
		campfire.SetBool("SmokeR", value: true);
		while (true)
		{
			if (!campfire.GetCurrentAnimatorStateInfo(1).IsName("None") || !campfire.GetCurrentAnimatorStateInfo(2).IsName("None"))
			{
				yield return CupheadTime.WaitForSeconds(this, Random.Range(1.5f, 3f));
			}
			campfire.SetBool("SmokeL", Rand.Bool());
			campfire.SetBool("SmokeR", Rand.Bool());
			if (campfire.GetCurrentAnimatorStateInfo(1).IsName("None"))
			{
				campfire.SetBool("SmokeL", value: true);
			}
			if (campfire.GetCurrentAnimatorStateInfo(2).IsName("None"))
			{
				campfire.SetBool("SmokeR", value: true);
			}
			yield return null;
		}
	}

	private void OnDialogueEnded()
	{
		if (questComplete && !PlayerData.Data.unlockedChaliceRecolor)
		{
			MapEventNotification.Current.ShowTooltipEvent(TooltipEvent.ChaliceFan);
			PlayerData.Data.unlockedChaliceRecolor = true;
			PlayerData.SaveCurrentFile();
			MapUI.Current.Refresh();
		}
	}
}
