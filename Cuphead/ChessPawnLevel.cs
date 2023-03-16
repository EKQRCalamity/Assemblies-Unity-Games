using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPawnLevel : ChessLevel
{
	private LevelProperties.ChessPawn properties;

	private const float WAIT_TO_RUN_DIST = 200f;

	private const float SPACING = 180f;

	private const float LEFTMOST_X = -622f;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortraitMain;

	[SerializeField]
	private string _bossQuoteMain;

	[SerializeField]
	private ChessPawnLevelPawn pawn;

	private ChessPawnLevelPawn[] pawns;

	private bool dead;

	public override Levels CurrentLevel => Levels.ChessPawn;

	public override Scenes CurrentScene => Scenes.scene_level_chess_pawn;

	public override Sprite BossPortrait => _bossPortraitMain;

	public override string BossQuote => _bossQuoteMain;

	protected override void PartialInit()
	{
		properties = LevelProperties.ChessPawn.GetMode(base.mode);
		properties.OnStateChange += base.zHack_OnStateChanged;
		properties.OnBossDeath += base.zHack_OnWin;
		base.timeline = properties.CreateTimeline(base.mode);
		goalTimes = properties.goalTimes;
		properties.OnBossDamaged += base.timeline.DealDamage;
		base.PartialInit();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_bossPortraitMain = null;
		base.OnIntroEvent -= onIntroEvent;
		pawn = null;
		pawns = null;
	}

	protected override void Start()
	{
		Level.IsChessBoss = true;
		base.Start();
		base.OnIntroEvent += onIntroEvent;
		int num = (int)properties.TotalHealth / 10;
		pawns = new ChessPawnLevelPawn[num];
		for (int i = 0; i < num; i++)
		{
			pawns[i] = pawn.Init(this);
			pawns[i].transform.position = GetPosition(i) + new Vector3(0f, 300f);
			pawns[i].SetIndex(i);
		}
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(main_cr());
	}

	public void TakeDamage()
	{
		properties.DealDamage(10f);
		if (properties.CurrentHealth <= 0f)
		{
			die();
		}
	}

	private void onIntroEvent()
	{
		for (int i = 0; i < pawns.Length; i++)
		{
			pawns[i].StartIntro();
			SFX_KOG_PAWN_IntroJeers();
		}
	}

	private IEnumerator main_cr()
	{
		LevelProperties.ChessPawn.Pawn p = properties.CurrentState.pawn;
		PatternString pawnAttackDelay = new PatternString(p.pawnAttackDelayString);
		PatternString pawnDirection = new PatternString(p.pawnDirectionString);
		PatternString pawnOrder = new PatternString(p.pawnOrderString);
		while (true)
		{
			bool pink = pawnOrder.PopLetter() == 'P';
			List<int> availableList = new List<int>();
			for (int j = 0; j < pawns.Length; j++)
			{
				if (pink == pawns[j].CanParry && !pawns[j].inUse)
				{
					availableList.Add(j);
				}
			}
			if (availableList.Count > 0)
			{
				float dir2 = 0f;
				switch (pawnDirection.PopLetter())
				{
				case 'L':
					dir2 = -1f;
					break;
				case 'D':
					dir2 = 0f;
					break;
				case 'R':
					dir2 = 1f;
					break;
				}
				int i = Random.Range(0, availableList.Count);
				if (Mathf.Abs(GetPosition(pawns[availableList[i]].currentIndex).x + dir2 * p.pawnDropDistance) > 800f)
				{
					dir2 = 0f;
				}
				dir2 *= p.pawnDropDistance;
				pawns[availableList[i]].Attack(p.pawnWarningTime, dir2, p.pawnDropSpeed, p.pawnRunHesitation, p.pawnRunSpeed, p.pawnReturnDelay);
				yield return CupheadTime.WaitForSeconds(this, pawnAttackDelay.PopFloat() - p.pawnDelayReduction * (float)damageCount());
			}
			yield return null;
		}
	}

	private void die()
	{
		if (!dead)
		{
			dead = true;
			SFX_KOG_PAWN_BeatLevelHarp();
			ChessPawnLevelPawn[] array = pawns;
			foreach (ChessPawnLevelPawn chessPawnLevelPawn in array)
			{
				chessPawnLevelPawn.Death();
			}
		}
	}

	private int damageCount()
	{
		return (int)(properties.TotalHealth - properties.CurrentHealth) / 10;
	}

	public int GetReturnIndex()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < pawns.Length; i++)
		{
			list.Add(i);
		}
		for (int j = 0; j < pawns.Length; j++)
		{
			list.Remove(pawns[j].currentIndex);
		}
		return list[Random.Range(0, list.Count)];
	}

	public Vector3 GetPosition(int index)
	{
		return new Vector3(-622f + (float)index * 180f, 340f);
	}

	public bool ClearToRun(float testDir, Vector3 pos)
	{
		bool result = true;
		for (int i = 0; i < pawns.Length; i++)
		{
			if (pawns[i].speed != 0f && Mathf.Sign(pawns[i].speed) == testDir && Vector3.Distance(pawns[i].transform.position, pos) < 200f)
			{
				result = false;
			}
		}
		return result;
	}

	private void SFX_KOG_PAWN_IntroJeers()
	{
		AudioManager.Play("sfx_DLC_KOG_Pawn_IntroJeers");
	}

	private void SFX_KOG_PAWN_BeatLevelHarp()
	{
		AudioManager.Play("sfx_dlc_kog_pawn_beatlevelharp");
	}
}
