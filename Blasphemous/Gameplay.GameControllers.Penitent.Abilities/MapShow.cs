using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class MapShow : Trait
{
	[SerializeField]
	private float updateTime = 0.2f;

	[SerializeField]
	private float minMovementToDig = 5f;

	[SerializeField]
	private Vector3 sizeBound;

	private bool forceUpdate;

	private float currentWaitTime;

	private Vector3 lastDigPos;

	protected override void OnAwake()
	{
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		forceUpdate = true;
		lastDigPos = Vector3.zero;
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
	}

	public void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		forceUpdate = true;
		lastDigPos = Vector3.zero;
	}

	protected override void OnUpdate()
	{
		bool flag = forceUpdate;
		if (!flag)
		{
			currentWaitTime += Time.deltaTime;
			if (currentWaitTime > updateTime)
			{
				Vector3 position = base.EntityOwner.transform.position;
				Vector3 vector = lastDigPos - position;
				currentWaitTime = 0f;
				flag = vector.sqrMagnitude >= minMovementToDig * minMovementToDig;
			}
		}
		if (flag)
		{
			currentWaitTime = 0f;
			lastDigPos = base.EntityOwner.transform.position;
			Core.NewMapManager.RevealCellInPosition(lastDigPos);
			forceUpdate = false;
		}
	}
}
