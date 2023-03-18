using Framework.Managers;
using Framework.Pooling;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Prayers;

public class TearHarvestEffect : PoolObject
{
	public Vector2 Offset;

	private void LateUpdate()
	{
		Vector3 position = Core.Logic.Penitent.GetPosition() + ((!Core.Logic.Penitent.IsCrouched) ? ((Vector3)Offset) : Vector3.zero);
		base.transform.position = position;
	}

	public void Dispose()
	{
		Destroy();
	}
}
