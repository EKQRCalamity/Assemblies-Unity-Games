using System;
using UnityEngine;

public class TutorialLevel : Level
{
	[Serializable]
	public class Prefabs
	{
	}

	private LevelProperties.Tutorial properties;

	[SerializeField]
	private Transform background;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	[SerializeField]
	private PlayerDeathEffect[] playerGoBackToHouseEffects;

	public override Levels CurrentLevel => Levels.Tutorial;

	public override Scenes CurrentScene => Scenes.scene_level_tutorial;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.Tutorial.GetMode(base.mode);
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
		background.SetParent(UnityEngine.Camera.main.transform);
		background.ResetLocalTransforms();
	}

	protected override void OnLevelStart()
	{
	}

	public void GoBackToHouse()
	{
		AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		playerGoBackToHouseEffects[0].gameObject.SetActive(value: true);
		playerGoBackToHouseEffects[0].transform.position = player.transform.position;
		player.gameObject.SetActive(value: false);
		playerGoBackToHouseEffects[0].animator.SetTrigger("OnStartTutorial");
		player = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		if (player != null)
		{
			playerGoBackToHouseEffects[1].gameObject.SetActive(value: true);
			playerGoBackToHouseEffects[1].transform.position = player.transform.position;
			player.gameObject.SetActive(value: false);
			playerGoBackToHouseEffects[1].animator.SetTrigger("OnStartTutorial");
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		for (int i = 0; i < playerGoBackToHouseEffects.Length; i++)
		{
			playerGoBackToHouseEffects[i].Clean();
		}
		playerGoBackToHouseEffects = null;
	}
}
