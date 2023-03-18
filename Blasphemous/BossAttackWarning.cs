using Framework.Managers;
using UnityEngine;

public class BossAttackWarning : MonoBehaviour
{
	public GameObject warningItem;

	public int poolSize = 1;

	private void Start()
	{
		if (warningItem != null)
		{
			PoolManager.Instance.CreatePool(warningItem, poolSize);
		}
	}

	public GameObject ShowWarning(Vector2 pos)
	{
		return PoolManager.Instance.ReuseObject(warningItem, pos, Quaternion.identity).GameObject;
	}
}
