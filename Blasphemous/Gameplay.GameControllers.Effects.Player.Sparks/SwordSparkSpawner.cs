using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Environment;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Sparks;

public class SwordSparkSpawner : Trait
{
	public BoxCollider2D AttackArea;

	private LevelEffectsStore _levelEffectsStore;

	public SwordSpark[] SwordSparks;

	private readonly List<SwordSpark> _swordSparkPool = new List<SwordSpark>();

	public SwordSpark.SwordSparkType CurrentSwordSparkSpawningType { get; set; }

	private void AttackAreaPositioning()
	{
		Bounds bounds = AttackArea.bounds;
		base.transform.position = bounds.center;
	}

	protected SwordSpark GetCurrentSwordSpark(SwordSpark.SwordSparkType currentSwordSparkType)
	{
		SwordSpark result = null;
		if (SwordSparks.Length > 0)
		{
			for (byte b = 0; b < SwordSparks.Length; b = (byte)(b + 1))
			{
				if (SwordSparks[b].sparkType == currentSwordSparkType)
				{
					result = SwordSparks[b];
					break;
				}
			}
		}
		return result;
	}

	protected SwordSpark GetCurrentSwordSparkFromPool(SwordSpark.SwordSparkType currentSwordSparkType)
	{
		SwordSpark swordSpark = null;
		if (_swordSparkPool.Count > 0)
		{
			for (int i = 0; i < _swordSparkPool.Count; i++)
			{
				if (_swordSparkPool[i].sparkType == currentSwordSparkType && !_swordSparkPool[i].gameObject.activeSelf)
				{
					swordSpark = _swordSparkPool[i];
					_swordSparkPool.Remove(swordSpark);
					break;
				}
			}
		}
		return swordSpark;
	}

	protected override void OnStart()
	{
		_levelEffectsStore = Core.Logic.CurrentLevelConfig.GetComponentInChildren<LevelEffectsStore>();
	}

	protected override void OnUpdate()
	{
		AttackAreaPositioning();
	}

	public SwordSpark GetSwordSpark(Vector3 position)
	{
		SwordSpark swordSpark = null;
		GameObject gameObject;
		if (_swordSparkPool.Count > 0)
		{
			swordSpark = GetCurrentSwordSparkFromPool(CurrentSwordSparkSpawningType);
			if ((bool)swordSpark)
			{
				gameObject = swordSpark.gameObject;
				gameObject.SetActive(value: true);
				gameObject.transform.position = position;
			}
			else
			{
				swordSpark = GetCurrentSwordSpark(CurrentSwordSparkSpawningType);
				gameObject = Object.Instantiate(swordSpark.gameObject, position, Quaternion.identity);
			}
		}
		else
		{
			swordSpark = GetCurrentSwordSpark(CurrentSwordSparkSpawningType);
			if (!swordSpark)
			{
				return null;
			}
			gameObject = Object.Instantiate(swordSpark.gameObject, position, Quaternion.identity);
		}
		gameObject.transform.parent = _levelEffectsStore.transform;
		return swordSpark;
	}

	public void StoreSwordSpark(SwordSpark swordSpark)
	{
		if (_swordSparkPool != null)
		{
			swordSpark.gameObject.SetActive(value: false);
			_swordSparkPool.Add(swordSpark);
		}
	}

	public void DrainSwordSparkPool()
	{
		if (_swordSparkPool.Count > 0)
		{
			_swordSparkPool.Clear();
		}
	}
}
