using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Environment;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Dust;

public class StepDustSpawner : MonoBehaviour
{
	private LevelEffectsStore levelEffectsStore;

	public Gameplay.GameControllers.Entities.Entity Owner;

	public StepDust[] stepsDust;

	private const int dustPoolSize = 3;

	public StepDustRoot StepDustRoot { get; set; }

	public bool EntityRaiseDust { get; set; }

	public StepDust.StepDustType CurrentStepDustSpawn { get; set; }

	private void Start()
	{
		levelEffectsStore = Core.Logic.CurrentLevelConfig.LevelEffectsStore;
		CreateDustPool();
		if (Owner == null)
		{
			Debug.LogError("StepDust Spanwer needs an owner!");
		}
		else
		{
			StepDustRoot = Owner.GetComponentInChildren<StepDustRoot>();
		}
	}

	private void CreateDustPool()
	{
		for (byte b = 0; b < stepsDust.Length; b = (byte)(b + 1))
		{
			PoolManager.Instance.CreatePool(stepsDust[b].gameObject, 3);
		}
	}

	private void OnDestroy()
	{
	}

	public StepDust GetStepDust(Vector2 stepDustPosition)
	{
		if (Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE))
		{
			return null;
		}
		GameObject stepDustfromProperties = GetStepDustfromProperties(CurrentStepDustSpawn);
		StepDust component = PoolManager.Instance.ReuseObject(stepDustfromProperties, stepDustPosition, Quaternion.identity).GameObject.GetComponent<StepDust>();
		component.Owner = Owner;
		component.transform.parent = levelEffectsStore.transform;
		component.SetSpriteOrientation(Owner.Status.Orientation);
		return component;
	}

	public void StoreStepDust(StepDust stepDust)
	{
		if ((bool)stepDust)
		{
			stepDust.gameObject.SetActive(value: false);
		}
	}

	private GameObject GetStepDustfromProperties(StepDust.StepDustType stepDustType)
	{
		StepDust stepDust = null;
		for (byte b = 0; b < stepsDust.Length; b = (byte)(b + 1))
		{
			if (stepsDust[b].stepDustType == stepDustType)
			{
				stepDust = stepsDust[b];
			}
		}
		return stepDust.gameObject;
	}
}
