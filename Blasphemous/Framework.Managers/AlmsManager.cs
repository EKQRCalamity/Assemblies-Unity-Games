using System;
using Framework.FrameworkCore;
using Tools.DataContainer;
using UnityEngine;

namespace Framework.Managers;

public class AlmsManager : GameSystem, PersistentInterface
{
	[Serializable]
	public class AlmsPersistenceData : PersistentManager.PersistentData
	{
		public int tears;

		public AlmsPersistenceData()
			: base("ID_ALMS")
		{
		}
	}

	private int _currentTears;

	private const string ALMS_RESOURCE_CONFIG = "AlmsConfig";

	private const string PERSITENT_ID = "ID_ALMS";

	public int TearsGiven
	{
		get
		{
			return _currentTears;
		}
		private set
		{
			_currentTears = value;
			CurentTier = 0;
			foreach (int tears in Config.GetTearsList())
			{
				if (_currentTears < tears)
				{
					break;
				}
				CurentTier++;
			}
		}
	}

	public int CurentTier { get; private set; }

	public AlmsConfigData Config { get; private set; }

	public override void Start()
	{
		Config = Resources.Load<AlmsConfigData>("AlmsConfig");
		ResetAll();
	}

	public override void AllInitialized()
	{
		Core.Persistence.AddPersistentManager(this);
	}

	public bool IsMaxTier()
	{
		return CurentTier >= Config.TearsToUnlock.Length;
	}

	public bool CanConsumeTears(int tears)
	{
		return Core.Logic.Penitent.Stats.Purge.Current >= (float)tears;
	}

	public bool ConsumeTears(int tears)
	{
		int curentTier = CurentTier;
		if (CanConsumeTears(tears))
		{
			Core.Logic.Penitent.Stats.Purge.Current -= tears;
			TearsGiven += tears;
		}
		bool flag = curentTier != CurentTier;
		if (flag && curentTier < 7 && CurentTier >= 7)
		{
			Core.ColorPaletteManager.UnlockAlmsColorPalette();
		}
		return flag;
	}

	public int GetPrieDieuLevel()
	{
		int result = 1;
		if (CurentTier >= Config.Level3PrieDieus)
		{
			result = 3;
		}
		else if (CurentTier >= Config.Level2PrieDieus)
		{
			result = 2;
		}
		return result;
	}

	public int GetAltarLevel()
	{
		return CurentTier + 1;
	}

	public void DEBUG_SetTearsGiven(int tears)
	{
		TearsGiven = tears;
	}

	private void ResetAll()
	{
		TearsGiven = 0;
	}

	public string GetPersistenID()
	{
		return "ID_ALMS";
	}

	public void ResetPersistence()
	{
		ResetAll();
	}

	public int GetOrder()
	{
		return 0;
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		AlmsPersistenceData almsPersistenceData = new AlmsPersistenceData();
		almsPersistenceData.tears = TearsGiven;
		return almsPersistenceData;
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		AlmsPersistenceData almsPersistenceData = (AlmsPersistenceData)data;
		TearsGiven = almsPersistenceData.tears;
	}

	public override void OnGUI()
	{
		DebugResetLine();
		DebugDrawTextLine("AlmsManager -------------------------------------");
		DebugDrawTextLine("TearsGiven: " + TearsGiven);
		DebugDrawTextLine("Tier: " + CurentTier);
		DebugDrawTextLine("Num Tiers: " + Config.TearsToUnlock.Length + "  IsMax:" + IsMaxTier());
		DebugDrawTextLine("PrieDieu Level: " + GetPrieDieuLevel());
		DebugDrawTextLine("Altar Level: " + GetAltarLevel());
		DebugDrawTextLine("Levels:");
		int num = 1;
		foreach (int tears in Config.GetTearsList())
		{
			string text = "Tier " + num + "  " + tears;
			if (num == CurentTier)
			{
				text = "--->" + text;
			}
			DebugDrawTextLine(text);
			num++;
		}
	}
}
