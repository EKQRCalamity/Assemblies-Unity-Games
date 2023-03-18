using System;
using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using Gameplay.GameControllers.Bosses.Amanecidas;
using Gameplay.GameControllers.Camera;
using Sirenix.OdinInspector;
using UnityEngine;

public class AmanecidaArena : MonoBehaviour
{
	public enum WEAPON_FIGHT_PHASE
	{
		FIRST,
		SECOND,
		THIRD
	}

	[Serializable]
	public struct GameObjectsToActivateByPhase
	{
		public WEAPON_FIGHT_PHASE phase;

		public List<GameObject> gameobjects;

		public bool useCameraInfluence;

		public Vector2 cameraInfluence;
	}

	public List<GameObjectsToActivateByPhase> gameobjectsToActivateEachPhase;

	public GameObject camBoundariesGo;

	public List<GameObject> decoParentsInOrder;

	public Rect battleBounds;

	public Vector2 boundsCenterOffset;

	private float deactivateSeconds = 0.8f;

	private float activateSeconds = 0.8f;

	public GameObject tweenParent;

	private GameObjectsToActivateByPhase currentPhaseConfig;

	[Button(ButtonSizes.Small)]
	public void TestInfluence()
	{
		if (currentPhaseConfig.useCameraInfluence)
		{
			ProCamera2D.Instance.ApplyInfluence(currentPhaseConfig.cameraInfluence);
		}
	}

	private void Update()
	{
		ApplyPhaseInfluence();
	}

	private void ApplyPhaseInfluence()
	{
		if (currentPhaseConfig.useCameraInfluence)
		{
			ProCamera2D.Instance.ApplyInfluence(currentPhaseConfig.cameraInfluence);
		}
	}

	public void ActivateArena(Amanecidas amanecida, Vector2 origin, bool onlySetBattleBounds = false, WEAPON_FIGHT_PHASE fightPhase = WEAPON_FIGHT_PHASE.FIRST)
	{
		if (fightPhase == WEAPON_FIGHT_PHASE.FIRST)
		{
			amanecida.Behaviour.battleBounds = battleBounds;
			amanecida.Behaviour.battleBounds.center = origin + boundsCenterOffset;
		}
		if (onlySetBattleBounds)
		{
			return;
		}
		GameObjectsToActivateByPhase gameObjectsToActivateByPhase = (currentPhaseConfig = gameobjectsToActivateEachPhase.Find((GameObjectsToActivateByPhase x) => x.phase == fightPhase));
		List<GameObject> gameobjects = gameObjectsToActivateByPhase.gameobjects;
		if (gameobjects != null)
		{
			foreach (GameObject item in gameobjects)
			{
				item.SetActive(value: true);
			}
		}
		if (amanecida.IsLaudes)
		{
			ActivateLaudesDeco(fightPhase);
			return;
		}
		CameraNumericBoundaries component = camBoundariesGo.GetComponent<CameraNumericBoundaries>();
		component.CenterKeepSize();
		component.SetBoundaries();
	}

	public void StartIntro()
	{
		if (tweenParent != null)
		{
			tweenParent.gameObject.SetActive(value: true);
			float num = 2f;
			float duration = 2f;
			tweenParent.transform.localPosition += Vector3.down * num;
			tweenParent.transform.DOLocalMoveY(tweenParent.transform.localPosition.y + num, duration);
		}
	}

	public void StartDeactivateArena()
	{
		StartCoroutine(DeactivateArena());
	}

	private IEnumerator DeactivateArena()
	{
		foreach (GameObjectsToActivateByPhase item in gameobjectsToActivateEachPhase)
		{
			WEAPON_FIGHT_PHASE phase = item.phase;
			DectivateLaudesDeco(phase);
		}
		yield return new WaitForSeconds(deactivateSeconds * 0.5f);
		foreach (GameObjectsToActivateByPhase item2 in gameobjectsToActivateEachPhase)
		{
			List<GameObject> gameobjects = item2.gameobjects;
			if (gameobjects == null)
			{
				continue;
			}
			foreach (GameObject item3 in gameobjects)
			{
				BoxCollider2D componentInChildren = item3.GetComponentInChildren<BoxCollider2D>();
				if (componentInChildren != null)
				{
					componentInChildren.enabled = false;
				}
			}
		}
		yield return new WaitForSeconds(deactivateSeconds * 0.5f);
		foreach (GameObjectsToActivateByPhase item4 in gameobjectsToActivateEachPhase)
		{
			List<GameObject> gameobjects2 = item4.gameobjects;
			if (gameobjects2 == null)
			{
				continue;
			}
			foreach (GameObject item5 in gameobjects2)
			{
				item5.SetActive(value: false);
			}
		}
		currentPhaseConfig = default(GameObjectsToActivateByPhase);
	}

	public void ActivateDeco(int index)
	{
		for (int i = 0; i < decoParentsInOrder.Count; i++)
		{
			decoParentsInOrder[i].SetActive(index == i);
		}
	}

	public void ActivateLaudesDeco(WEAPON_FIGHT_PHASE fightPhase)
	{
		for (int i = 0; i < decoParentsInOrder[(int)fightPhase].transform.childCount; i++)
		{
			LaudesPlatformController[] componentsInChildren = decoParentsInOrder[(int)fightPhase].GetComponentsInChildren<LaudesPlatformController>();
			LaudesPlatformController[] array = componentsInChildren;
			foreach (LaudesPlatformController laudesPlatformController in array)
			{
				laudesPlatformController.ShowAllPlatforms();
			}
		}
	}

	public void PlayParticles(WEAPON_FIGHT_PHASE fightPhase)
	{
		for (int i = 0; i < decoParentsInOrder[(int)fightPhase].transform.childCount; i++)
		{
			ParticleSystem[] componentsInChildren = decoParentsInOrder[(int)fightPhase].transform.GetChild(i).GetComponentsInChildren<ParticleSystem>();
			ParticleSystem[] array = componentsInChildren;
			foreach (ParticleSystem particleSystem in array)
			{
				particleSystem.Play();
			}
		}
	}

	public void DectivateLaudesDeco(WEAPON_FIGHT_PHASE fightPhase)
	{
		for (int i = 0; i < decoParentsInOrder[(int)fightPhase].transform.childCount; i++)
		{
			LaudesPlatformController[] componentsInChildren = decoParentsInOrder[(int)fightPhase].GetComponentsInChildren<LaudesPlatformController>();
			LaudesPlatformController[] array = componentsInChildren;
			foreach (LaudesPlatformController laudesPlatformController in array)
			{
				laudesPlatformController.HideAllPlatforms();
			}
		}
	}

	[Button("Editor Tool: Fill deco parents", ButtonSizes.Small)]
	public void FillDecoParents()
	{
		decoParentsInOrder.Clear();
		Transform transform = base.transform.Find("ARENA_DECO");
		if (transform != null)
		{
			foreach (Transform item in transform.transform)
			{
				decoParentsInOrder.Add(item.gameObject);
			}
			Debug.Log("Deco parents added");
		}
		else
		{
			Debug.LogError("Couldnt find child object << ARENA_DECO >>");
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere((Vector2)base.transform.position + boundsCenterOffset, 0.1f);
		Gizmos.DrawWireCube(battleBounds.center, battleBounds.size);
	}
}
