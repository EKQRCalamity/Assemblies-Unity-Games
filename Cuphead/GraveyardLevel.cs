using System.Collections;
using UnityEngine;

public class GraveyardLevel : Level
{
	private LevelProperties.Graveyard properties;

	[SerializeField]
	private GraveyardLevelSplitDevil[] splitDevil;

	private PatternString attackCounterString;

	private int attackCounter;

	private Mode originalMode;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortraitMain;

	[SerializeField]
	private string _bossQuoteMain;

	public override Levels CurrentLevel => Levels.Graveyard;

	public override Scenes CurrentScene => Scenes.scene_level_graveyard;

	public override Sprite BossPortrait => _bossPortraitMain;

	public override string BossQuote => _bossQuoteMain;

	protected override void PartialInit()
	{
		properties = LevelProperties.Graveyard.GetMode(base.mode);
		properties.OnStateChange += base.zHack_OnStateChanged;
		properties.OnBossDeath += base.zHack_OnWin;
		base.timeline = properties.CreateTimeline(base.mode);
		goalTimes = properties.goalTimes;
		properties.OnBossDamaged += base.timeline.DealDamage;
		base.PartialInit();
	}

	protected override void Awake()
	{
		originalMode = Level.CurrentMode;
		Level.SetCurrentMode(Mode.Normal);
		base.Awake();
		Level.IsGraveyard = true;
	}

	protected override void Start()
	{
		base.Start();
		for (int i = 0; i < splitDevil.Length; i++)
		{
			splitDevil[i].LevelInit(properties);
		}
		attackCounterString = new PatternString(properties.CurrentState.splitDevilBeam.attacksBeforeBeamString);
		attackCounter = attackCounterString.PopInt();
		AudioManager.PlayLoop("sfx_dlc_graveyard_amb_loop");
	}

	protected override void PlayAnnouncerReady()
	{
		AudioManager.Play("level_announcer_ready_ghostly");
	}

	protected override void PlayAnnouncerBegin()
	{
		AudioManager.Play("level_announcer_begin_ghostly");
	}

	public bool CheckForBeamAttack()
	{
		attackCounter--;
		if (attackCounter == -1)
		{
			attackCounter = attackCounterString.PopInt();
			return true;
		}
		return false;
	}

	protected override void OnLevelStart()
	{
		for (int i = 0; i < splitDevil.Length; i++)
		{
			splitDevil[i].NextPattern();
		}
	}

	protected override void OnWin()
	{
		base.OnWin();
		for (int i = 0; i < splitDevil.Length; i++)
		{
			splitDevil[i].Die();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortraitMain = null;
		Level.SetCurrentMode(originalMode);
		splitDevil = null;
	}

	private IEnumerator devilPattern_cr()
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
		LevelProperties.Graveyard.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}
}
