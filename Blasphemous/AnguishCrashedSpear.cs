using DG.Tweening;
using Tools.Level.Actionables;
using UnityEngine;

public class AnguishCrashedSpear : MonoBehaviour
{
	public TriggerBasedTrap trap;

	public BreakableObject breakable;

	private void Start()
	{
		trap.OnUsedEvent += OnUsed;
	}

	private void OnUsed(TriggerBasedTrap obj)
	{
		obj.OnUsedEvent -= OnUsed;
		GetComponentInChildren<BreakableDamageArea>().DamageAreaCollider.enabled = false;
		obj.transform.DOPunchPosition(Vector3.up * 0.2f, obj.lastDelay, 20).OnComplete(delegate
		{
			breakable.Use();
		});
	}
}
