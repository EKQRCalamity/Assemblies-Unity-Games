using System.Linq;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.GameControllers.Bosses.BossFight;

public class BossFightManager : MonoBehaviour
{
	public UnityEvent onStartBossFight;

	public UnityEvent onFinishBossFight;

	public const string State1 = "state1";

	public const string State2 = "state2";

	public const string State3 = "state3";

	public const string Ending = "Ending";

	public Enemy Boss;

	public BossPhase[] Phases;

	public bool BossCountsTowardsAC43 = true;

	public Transform SpawnPoint;

	public bool ShowLifeBar = true;

	private float numFlasksAtStartBossFight;

	private float numFlasksAtEndBossFight;

	private const int TOTAL_NUMBER_OF_BOSSES_FOR_AC43 = 11;

	public BossFightAudio Audio { get; set; }

	public BossFightMetrics Metrics { get; set; }

	public string CurrentBossPhaseId { get; private set; }

	public bool IsFightStarted { get; private set; }

	private void Awake()
	{
		CurrentBossPhaseId = "state1";
		Audio = GetComponent<BossFightAudio>();
		Metrics = GetComponent<BossFightMetrics>();
	}

	private void Start()
	{
		Init();
	}

	public void Init()
	{
		if ((bool)Boss)
		{
			SetCurrentActivePhase();
			Boss.OnDamaged += OnDamageBoss;
			Boss.OnDeath += OnDeathBoss;
		}
	}

	private void Update()
	{
		if (IsFightStarted)
		{
			Audio.SetBossTrackState(Boss.CurrentLife);
		}
	}

	private void OnDestroy()
	{
		if ((bool)Boss)
		{
			Boss.OnDamaged -= OnDamageBoss;
			Boss.OnDeath -= OnDeathBoss;
		}
	}

	public void EnableLifeBar(bool enableLifeBar = true)
	{
		if (enableLifeBar)
		{
			if (ShowLifeBar)
			{
				UIController.instance.ShowBossHealth(Boss);
			}
			Boss.UseHealthBar = false;
		}
		else
		{
			UIController.instance.HideBossHealth();
		}
	}

	public void StartBossFight()
	{
		StartBossFight(playBossTrack: true);
	}

	public void StartBossFight(bool playBossTrack = true)
	{
		EnableLifeBar();
		SetCurrentActivePhase();
		if (playBossTrack)
		{
			Audio.PlayBossTrack();
			Audio.SetBossTrackState(BossCurrentLifeNormalized());
		}
		Metrics.StartBossFight();
		IsFightStarted = true;
		numFlasksAtStartBossFight = Core.Logic.Penitent.Stats.Flask.Current;
		onStartBossFight.Invoke();
	}

	public void SetCurrentActivePhase()
	{
		BossPhase currentPhase = GetCurrentPhase();
		if (currentPhase == null)
		{
			return;
		}
		BossPhase[] phases = Phases;
		foreach (BossPhase bossPhase in phases)
		{
			bossPhase.IsActive = bossPhase.Id.Equals(currentPhase.Id);
			if (bossPhase.IsActive)
			{
				CurrentBossPhaseId = bossPhase.Id;
			}
		}
	}

	private BossPhase GetCurrentPhase()
	{
		if (Phases == null)
		{
			return null;
		}
		float currentBossLife = 100f;
		if (Boss.gameObject.activeInHierarchy)
		{
			currentBossLife = Boss.CurrentLife / Boss.Stats.Life.Base * 100f;
		}
		return Phases.FirstOrDefault((BossPhase phase) => currentBossLife >= phase.PhaseInterval.x && currentBossLife <= phase.PhaseInterval.y);
	}

	private float BossCurrentLifeNormalized()
	{
		return Boss.CurrentLife / Boss.Stats.Life.Base * 100f;
	}

	public Vector3 GetPenitentSpawnPoint()
	{
		return SpawnPoint.position;
	}

	public void SetPenitentOnSpawnPoint()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		if (penitent != null)
		{
			Vector2 vector = GetPenitentSpawnPoint();
			penitent.transform.position = vector;
		}
	}

	private void OnDamageBoss()
	{
		SetCurrentActivePhase();
	}

	private void OnDeathBoss()
	{
		EnableLifeBar(enableLifeBar: false);
		Metrics.EndBossFight();
		Audio.SetBossTrackState(BossCurrentLifeNormalized());
		Audio.SetBossEndingMusic(1f);
		IsFightStarted = false;
		onFinishBossFight.Invoke();
	}

	public void AddProgressToAC43()
	{
		if (BossCountsTowardsAC43)
		{
			numFlasksAtEndBossFight = Core.Logic.Penitent.Stats.Flask.Current;
			if (Core.Events.GetFlag(Core.LevelManager.currentLevel.LevelName + "_BOSSDEAD_AC43"))
			{
				Debug.Log("There was previously an increase to AC43 due to this boss!");
			}
			else if (numFlasksAtStartBossFight == numFlasksAtEndBossFight)
			{
				Core.AchievementsManager.Achievements["AC43"].AddProgress(9.090909f);
				Core.Events.SetFlag(Core.LevelManager.currentLevel.LevelName + "_BOSSDEAD_AC43", b: true);
			}
		}
	}
}
