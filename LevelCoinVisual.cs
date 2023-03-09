using UnityEngine;

public class LevelCoinVisual : AbstractPausableComponent
{
	private void OnDeathAnimComplete()
	{
		Object.Destroy(base.gameObject);
	}
}
