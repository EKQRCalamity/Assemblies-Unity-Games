using System;
using System.Collections.Generic;
using System.Linq;
using Framework.FrameworkCore;
using Framework.FrameworkCore.Attributes;
using Framework.Penitences;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using Gameplay.GameControllers.Penitent.Damage;
using Gameplay.UI.Others.UIGameLogic;
using UnityEngine;

namespace Framework.Managers;

public class PenitenceManager : GameSystem, PersistentInterface
{
	public delegate void PenitenceDelegate(IPenitence current, List<IPenitence> completed);

	[Serializable]
	public class PenitencePersistenceData : PersistentManager.PersistentData
	{
		public IPenitence currentPenitence;

		public List<IPenitence> allPenitences = new List<IPenitence>();

		public PenitencePersistenceData()
			: base("ID_PENITENCE")
		{
		}
	}

	private IPenitence currentPenitence;

	private List<IPenitence> allPenitences;

	public bool UseStocksOfHealth;

	public const float HealthPerStock = 30f;

	private FlaskRegenerationBalance flaskRegenerationBalance;

	private const string FLASK_REGENERATION_BALANCE_CHART = "PE02/FlaskRegenerationBalance";

	private float regenFactor;

	private float lifeAccum;

	private bool isLevelLoaded;

	private bool isRegenActive;

	private int lastFlaskLevel;

	private float regenerationPerSecond;

	public bool UseFervourFlasks;

	private const string PERSITENT_ID = "ID_PENITENCE";

	private static int FlaskUpgradeLevel => (int)(Core.Logic.Penitent.Stats.FlaskHealth.PermanetBonus / Core.Logic.Penitent.Stats.FlaskHealthUpgrade) + 1;

	public static event PenitenceDelegate OnPenitenceChanged;

	public override void Initialize()
	{
		ResetPenitencesList();
		regenFactor = 0f;
		flaskRegenerationBalance = Resources.Load<FlaskRegenerationBalance>("PE02/FlaskRegenerationBalance");
		if (!flaskRegenerationBalance)
		{
			Debug.LogErrorFormat("Can't find flask regeneration balance chart at {0}", "PE02/FlaskRegenerationBalance");
		}
	}

	public override void AllInitialized()
	{
		Core.Persistence.AddPersistentManager(this);
		LevelManager.OnBeforeLevelLoad += OnBeforeLevelLoad;
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		PenitentDamageArea.OnDamagedGlobal = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Combine(PenitentDamageArea.OnDamagedGlobal, new PenitentDamageArea.PlayerDamagedEvent(OnPenitentHit));
	}

	public override void Dispose()
	{
		LevelManager.OnBeforeLevelLoad -= OnBeforeLevelLoad;
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		PenitentDamageArea.OnDamagedGlobal = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Remove(PenitentDamageArea.OnDamagedGlobal, new PenitentDamageArea.PlayerDamagedEvent(OnPenitentHit));
	}

	public override void Update()
	{
		if (Mathf.Approximately(regenFactor, 0f) || !isLevelLoaded || !Core.ready || Core.Logic.Penitent == null)
		{
			return;
		}
		Life life = Core.Logic.Penitent.Stats.Life;
		if (isRegenActive)
		{
			RestoreHealth();
			if (life.Current >= life.CurrentMax)
			{
				isRegenActive = false;
			}
			return;
		}
		Healing componentInChildren = Core.Logic.Penitent.GetComponentInChildren<Healing>();
		if (componentInChildren.IsHealing && life.Current < life.CurrentMax)
		{
			isRegenActive = true;
			lifeAccum = 0f;
		}
	}

	public void ActivatePenitence(string id)
	{
		IPenitence penitence = allPenitences.Find((IPenitence p) => p.Id.Equals(id));
		if (penitence != null)
		{
			ActivatePenitence(penitence);
		}
		else
		{
			Debug.LogError("ActivatePenitence: Penitence with id '" + id + "' does not exist!");
		}
	}

	public void ActivatePE01()
	{
		ActivatePenitence(allPenitences.Find((IPenitence p) => p is PenitencePE01));
	}

	public void ActivatePE02()
	{
		ActivatePenitence(allPenitences.Find((IPenitence p) => p is PenitencePE02));
	}

	public void ActivatePE03()
	{
		ActivatePenitence(allPenitences.Find((IPenitence p) => p is PenitencePE03));
	}

	public bool CheckPenitenceExists(string id)
	{
		return allPenitences.Exists((IPenitence p) => p.Id.Equals(id));
	}

	public void DeactivateCurrentPenitence()
	{
		if (currentPenitence != null)
		{
			currentPenitence.Deactivate();
			currentPenitence = null;
			SendEvent();
		}
	}

	public void MarkCurrentPenitenceAsAbandoned()
	{
		currentPenitence.Abandoned = true;
		DeactivateCurrentPenitence();
		SendEvent();
	}

	public void MarkCurrentPenitenceAsCompleted()
	{
		currentPenitence.Completed = true;
		SendEvent();
	}

	public IPenitence GetCurrentPenitence()
	{
		return currentPenitence;
	}

	public List<IPenitence> GetAllPenitences()
	{
		return allPenitences;
	}

	public List<IPenitence> GetAllCompletedPenitences()
	{
		return allPenitences.FindAll((IPenitence p) => p.Completed);
	}

	public List<IPenitence> GetAllAbandonedPenitences()
	{
		return allPenitences.FindAll((IPenitence p) => p.Abandoned);
	}

	public float GetPercentageCompletition()
	{
		return (float)allPenitences.Count((IPenitence p) => p.Completed) * GameConstants.PercentageValues[PersistentManager.PercentageType.Penitence_NgPlus];
	}

	public void MarkPenitenceAsCompleted(string id)
	{
		IPenitence penitence = allPenitences.Find((IPenitence x) => x.Id == id);
		if (penitence != null)
		{
			penitence.Completed = true;
		}
		else
		{
			Debug.LogError("MarkPenitenceAsCompleted: penitence with id: " + id + " could not be found!");
		}
	}

	public void AddFlasksPassiveHealthRegen(float regenFactor)
	{
		this.regenFactor += regenFactor;
		if (this.regenFactor < 0f)
		{
			this.regenFactor = 0f;
		}
	}

	public void ResetRegeneration()
	{
		if (isRegenActive)
		{
			PlayerHealthPE02.FillAmount = 0f;
			lifeAccum = 0f;
		}
	}

	private void OnBeforeLevelLoad(Level oldLevel, Level newLevel)
	{
		isLevelLoaded = false;
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		isLevelLoaded = true;
	}

	private void OnPenitentHit(Penitent penitent, Hit hit)
	{
		if (isRegenActive)
		{
			isRegenActive = false;
			PlayerHealthPE02.FillAmount = 0f;
			lifeAccum = 0f;
		}
	}

	private void ActivatePenitence(IPenitence penitence)
	{
		if (currentPenitence != null)
		{
			currentPenitence.Deactivate();
		}
		currentPenitence = penitence;
		currentPenitence.Activate();
		SendEvent();
	}

	private void ResetPenitencesList()
	{
		allPenitences = new List<IPenitence>
		{
			new PenitencePE01(),
			new PenitencePE02(),
			new PenitencePE03()
		};
		SendEvent();
	}

	private void SendEvent()
	{
		if (PenitenceManager.OnPenitenceChanged != null)
		{
			PenitenceManager.OnPenitenceChanged(GetCurrentPenitence(), GetAllCompletedPenitences());
		}
	}

	private void RestoreHealth()
	{
		if (Core.PenitenceManager.UseStocksOfHealth)
		{
			if (lastFlaskLevel != FlaskUpgradeLevel)
			{
				regenerationPerSecond = flaskRegenerationBalance.GetTimeByFlaskLevel(FlaskUpgradeLevel);
				lastFlaskLevel = FlaskUpgradeLevel;
			}
			lifeAccum += Time.deltaTime * (30f / (regenerationPerSecond / regenFactor));
			float num = 30f;
			float num2 = Core.Logic.Penitent.Stats.Life.Current % num;
			if (num2 > 0f)
			{
				num = num2;
			}
			PlayerHealthPE02.FillAmount = (float)(int)(lifeAccum / num / (1f / 11f)) / 11f;
			if (lifeAccum >= num)
			{
				Core.Logic.Penitent.Stats.Life.Current += num;
				lifeAccum = 0f;
				PlayerHealthPE02.FillAmount = 1f;
			}
		}
		else
		{
			Core.Logic.Penitent.Stats.Life.Current += Time.deltaTime * regenFactor * (float)FlaskUpgradeLevel;
		}
	}

	public override void OnGUI()
	{
		DebugResetLine();
		DebugDrawTextLine("Penitence -------------------------------------");
		DebugDrawTextLine("List:");
		foreach (IPenitence allPenitence in allPenitences)
		{
			string text = string.Empty;
			if (allPenitence == currentPenitence)
			{
				text = "CURRENT -->";
			}
			DebugDrawTextLine(text + allPenitence.Id + ": Abandoned:" + allPenitence.Abandoned + " Completed:" + allPenitence.Completed);
		}
		if (regenFactor > 0f)
		{
			DebugDrawTextLine(string.Empty);
			DebugDrawTextLine("Health Regeneration:");
			DebugDrawTextLine("IsRegenActive:" + isRegenActive);
			DebugDrawTextLine("UseStocksOfHealth:" + UseStocksOfHealth);
			DebugDrawTextLine("IsLevelLoaded:" + isLevelLoaded);
			DebugDrawTextLine("RegenFactor:" + regenFactor);
			DebugDrawTextLine("Flask Level:" + FlaskUpgradeLevel);
			DebugDrawTextLine("Time for flask level " + FlaskUpgradeLevel + ": " + regenerationPerSecond);
			DebugDrawTextLine("Final time for flask level and RegenFactor: " + regenerationPerSecond / regenFactor);
			DebugDrawTextLine("LifeAccum:" + lifeAccum);
		}
	}

	public int GetOrder()
	{
		return 10;
	}

	public string GetPersistenID()
	{
		return "ID_PENITENCE";
	}

	public void ResetPersistence()
	{
		if (currentPenitence != null)
		{
			DeactivateCurrentPenitence();
		}
		ResetPenitencesList();
		regenFactor = 0f;
		lifeAccum = 0f;
		isLevelLoaded = false;
		isRegenActive = false;
		UseFervourFlasks = false;
		UseStocksOfHealth = false;
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		PenitencePersistenceData penitencePersistenceData = new PenitencePersistenceData();
		penitencePersistenceData.currentPenitence = currentPenitence;
		penitencePersistenceData.allPenitences = allPenitences;
		return penitencePersistenceData;
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		PenitencePersistenceData penitencePersistenceData = (PenitencePersistenceData)data;
		if (penitencePersistenceData.currentPenitence != null)
		{
			ActivatePenitence(penitencePersistenceData.currentPenitence);
		}
		allPenitences = penitencePersistenceData.allPenitences;
		for (int i = 0; i < 3; i++)
		{
			if (allPenitences[i] == null)
			{
				switch (i)
				{
				case 0:
					allPenitences[i] = new PenitencePE01();
					break;
				case 1:
					allPenitences[i] = new PenitencePE02();
					break;
				case 2:
					allPenitences[i] = new PenitencePE03();
					break;
				default:
					Debug.LogError("Invalid Penitence Index: " + i);
					break;
				}
			}
		}
	}
}
