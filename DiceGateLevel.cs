using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceGateLevel : Level
{
	private LevelProperties.DiceGate properties;

	[SerializeField]
	private AbstractLevelInteractiveEntity toNextWorld;

	[SerializeField]
	private AbstractLevelInteractiveEntity toPrevWorld;

	public AbstractUIInteractionDialogue.Properties world2PrevProperties;

	public AbstractUIInteractionDialogue.Properties world2NextProperties;

	[SerializeField]
	private GameObject kingDice;

	[SerializeField]
	private List<GameObject> chalkboardCrosses;

	[SerializeField]
	private DialogueInteractionPoint dialogueInteractionPoint;

	[SerializeField]
	private DialoguerDialogues dialogueWorld2;

	[SerializeField]
	private string completeLevelAnimationTrigger;

	[SerializeField]
	private GameObject world1Background;

	[SerializeField]
	private GameObject world2Background;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	public override Levels CurrentLevel => Levels.DiceGate;

	public override Scenes CurrentScene => Scenes.scene_level_dice_gate;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.DiceGate.GetMode(base.mode);
		properties.OnStateChange += base.zHack_OnStateChanged;
		properties.OnBossDeath += base.zHack_OnWin;
		base.timeline = properties.CreateTimeline(base.mode);
		goalTimes = properties.goalTimes;
		properties.OnBossDamaged += base.timeline.DealDamage;
		base.PartialInit();
	}

	protected override void Start()
	{
		base.Start();
		SceneLoader.OnLoaderCompleteEvent += SetMusic;
		if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_1)
		{
			world1Background.SetActive(value: true);
			if (PlayerData.Data.CheckLevelCompleted(Levels.Veggies))
			{
				chalkboardCrosses[0].SetActive(value: true);
			}
			if (PlayerData.Data.CheckLevelCompleted(Levels.Slime))
			{
				chalkboardCrosses[1].SetActive(value: true);
			}
			if (PlayerData.Data.CheckLevelCompleted(Levels.Frogs))
			{
				chalkboardCrosses[2].SetActive(value: true);
			}
			if (PlayerData.Data.CheckLevelCompleted(Levels.FlyingBlimp))
			{
				chalkboardCrosses[3].SetActive(value: true);
			}
			if (PlayerData.Data.CheckLevelCompleted(Levels.Flower))
			{
				chalkboardCrosses[4].SetActive(value: true);
			}
			if (PlayerData.Data.CheckLevelsCompleted(Level.world1BossLevels))
			{
				dialogueInteractionPoint.animationTriggerOnEnd = completeLevelAnimationTrigger;
				OpenWay();
			}
			else
			{
				CloseWay();
			}
		}
		else if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_2)
		{
			world2Background.SetActive(value: true);
			toPrevWorld.dialogueProperties = world2PrevProperties;
			toNextWorld.dialogueProperties = world2NextProperties;
			if (PlayerData.Data.CheckLevelCompleted(Levels.Baroness))
			{
				chalkboardCrosses[0].SetActive(value: true);
			}
			if (PlayerData.Data.CheckLevelCompleted(Levels.FlyingGenie))
			{
				chalkboardCrosses[1].SetActive(value: true);
			}
			if (PlayerData.Data.CheckLevelCompleted(Levels.Clown))
			{
				chalkboardCrosses[2].SetActive(value: true);
			}
			if (PlayerData.Data.CheckLevelCompleted(Levels.FlyingBird))
			{
				chalkboardCrosses[3].SetActive(value: true);
			}
			if (PlayerData.Data.CheckLevelCompleted(Levels.Dragon))
			{
				chalkboardCrosses[4].SetActive(value: true);
			}
			dialogueInteractionPoint.dialogueInteraction = dialogueWorld2;
			if (PlayerData.Data.CheckLevelsCompleted(Level.world2BossLevels))
			{
				dialogueInteractionPoint.animationTriggerOnEnd = completeLevelAnimationTrigger;
				OpenWay();
			}
			else
			{
				CloseWay();
			}
		}
		else
		{
			Debug.LogError("SOMETHING BAD HAPPENED");
		}
	}

	private void SetMusic()
	{
		AudioManager.PlayBGMPlaylistManually(goThroughPlaylistAfter: true);
	}

	protected override void OnLevelStart()
	{
	}

	protected override void OnDestroy()
	{
		SceneLoader.OnLoaderCompleteEvent -= SetMusic;
		base.OnDestroy();
		_bossPortrait = null;
	}

	private void CloseWay()
	{
		toNextWorld.enabled = false;
		kingDice.SetActive(value: true);
		if (PlayerData.Data.CurrentMapData.hasVisitedDieHouse)
		{
			if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_1)
			{
				Dialoguer.SetGlobalFloat(16, 1f);
			}
			PlayerData.SaveCurrentFile();
		}
	}

	private void OpenWay()
	{
		toNextWorld.enabled = true;
		if (PlayerData.Data.CurrentMapData.hasKingDiceDisappeared)
		{
			kingDice.SetActive(value: false);
		}
		if (PlayerData.Data.CurrentMap == Scenes.scene_map_world_1)
		{
			Dialoguer.SetGlobalFloat(16, 2f);
		}
		else
		{
			Dialoguer.SetGlobalFloat(17, 1f);
		}
		PlayerData.SaveCurrentFile();
	}

	private IEnumerator dicegatePattern_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		while (true)
		{
			yield return StartCoroutine(nextPattern_cr());
			yield return null;
		}
	}

	private IEnumerator nextPattern_cr()
	{
		LevelProperties.DiceGate.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
