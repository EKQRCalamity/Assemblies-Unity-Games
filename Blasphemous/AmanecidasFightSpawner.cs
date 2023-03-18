using System;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Amanecidas;
using Gameplay.GameControllers.Bosses.BossFight;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;

public class AmanecidasFightSpawner : MonoBehaviour
{
	public enum AMANECIDAS_FIGHTS
	{
		LANCE,
		AXE,
		FALCATA,
		BOW,
		LAUDES
	}

	[Serializable]
	public struct AmanecidasFightData
	{
		public AMANECIDAS_FIGHTS fightType;

		public List<GameObject> prefabsToInstantiate;

		public Transform spawnPoint;

		[SerializeField]
		public LocalizedString displayName;
	}

	private const string AXE_FLAG = "SANTOS_AMANECIDA_AXE_ACTIVATED";

	private const string BOW_FLAG = "SANTOS_AMANECIDA_BOW_ACTIVATED";

	private const string FALCATA_FLAG = "SANTOS_AMANECIDA_FALCATA_ACTIVATED";

	private const string LANCE_FLAG = "SANTOS_AMANECIDA_LANCE_ACTIVATED";

	public GameObject bossPrefab;

	public GameObject currentBoss;

	public BossFightManager bossMgr;

	public AMANECIDAS_FIGHTS TEST_AmanecidaFightType;

	public bool useFlagsForFightType = true;

	private Amanecidas amanecida;

	public int amanecidaFight;

	private AmanecidaArena arena;

	private LaudesArena laudesArena;

	public static AmanecidasFightSpawner Instance;

	public List<AmanecidasFightData> fightData;

	private void Awake()
	{
		bossMgr = GetComponent<BossFightManager>();
		Instance = this;
		LevelManager.OnLevelLoaded += OnLevelLoaded;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		SpawnBoss();
		bossMgr.Boss = currentBoss.GetComponent<Enemy>();
		bossMgr.Init();
	}

	private void Update()
	{
	}

	public Vector3 GetPenitentSpawnPoint()
	{
		AMANECIDAS_FIGHTS fightType = GetCurrentFightFromFlags();
		return fightData.Find((AmanecidasFightData x) => x.fightType == fightType).spawnPoint.position;
	}

	public void SetPenitentOnSpawnPoint()
	{
		Penitent penitent = Core.Logic.Penitent;
		if (penitent != null)
		{
			Vector2 vector = GetPenitentSpawnPoint();
			penitent.transform.position = vector;
		}
	}

	public void SpawnBoss()
	{
		currentBoss = UnityEngine.Object.Instantiate(bossPrefab);
		amanecida = currentBoss.GetComponent<Amanecidas>();
		amanecida.transform.position = base.transform.position + Vector3.up * 25f;
		AMANECIDAS_FIGHTS fightType = GetCurrentFightFromFlags();
		Debug.Log("Current fight " + fightType);
		amanecida.SetupFight(fightType);
		GameObject gameObject = new GameObject("AMANECIDA_FIGHT_STUFF");
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		AmanecidasFightData amanecidasFightData = fightData.Find((AmanecidasFightData x) => x.fightType == fightType);
		amanecida.displayName = amanecidasFightData.displayName;
		if (amanecidasFightData.prefabsToInstantiate == null)
		{
			return;
		}
		foreach (GameObject item in amanecidasFightData.prefabsToInstantiate)
		{
			if (!(item != null))
			{
				continue;
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(item);
			gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
			gameObject2.transform.localPosition = Vector3.zero;
			CameraNumericBoundaries componentInChildren = gameObject2.GetComponentInChildren<CameraNumericBoundaries>();
			if (componentInChildren != null)
			{
				componentInChildren.CenterKeepSize();
				componentInChildren.SetBoundaries();
			}
			arena = gameObject2.GetComponent<AmanecidaArena>();
			if (arena != null)
			{
				arena.ActivateArena(amanecida, base.transform.position);
				arena.ActivateDeco(amanecidaFight);
				continue;
			}
			laudesArena = gameObject2.GetComponent<LaudesArena>();
			if (laudesArena != null)
			{
				amanecida.SetLaudesArena(laudesArena, base.transform.position, onlySetBoundaries: true);
			}
			else
			{
				Debug.LogError("404: Arena not found!");
			}
		}
	}

	[Button("TEST START FIGHT", ButtonSizes.Large)]
	public void StartAmanecidaFight()
	{
		bossMgr.StartBossFight();
		amanecida.Behaviour.StartCombat();
		if (laudesArena != null)
		{
			amanecida.SetLaudesArena(laudesArena, base.transform.position, onlySetBoundaries: false);
		}
	}

	[Button("TEST START INTRO", ButtonSizes.Large)]
	public void StartIntro()
	{
		amanecida.Behaviour.StartIntro();
		if (arena != null)
		{
			arena.StartIntro();
		}
		else if (laudesArena != null)
		{
			laudesArena.StartIntro(amanecida.Behaviour.currentWeapon);
		}
		else
		{
			Debug.LogError("Arena component not found!");
		}
	}

	private AMANECIDAS_FIGHTS GetCurrentFightFromFlags()
	{
		if (!useFlagsForFightType)
		{
			return TEST_AmanecidaFightType;
		}
		if (Core.Events.GetFlag("SANTOS_LAUDES_ACTIVATED"))
		{
			return AMANECIDAS_FIGHTS.LAUDES;
		}
		if (Core.Events.GetFlag("SANTOS_AMANECIDA_AXE_ACTIVATED"))
		{
			return AMANECIDAS_FIGHTS.AXE;
		}
		if (Core.Events.GetFlag("SANTOS_AMANECIDA_FALCATA_ACTIVATED"))
		{
			return AMANECIDAS_FIGHTS.FALCATA;
		}
		if (Core.Events.GetFlag("SANTOS_AMANECIDA_LANCE_ACTIVATED"))
		{
			return AMANECIDAS_FIGHTS.LANCE;
		}
		if (Core.Events.GetFlag("SANTOS_AMANECIDA_BOW_ACTIVATED"))
		{
			return AMANECIDAS_FIGHTS.BOW;
		}
		return TEST_AmanecidaFightType;
	}
}
