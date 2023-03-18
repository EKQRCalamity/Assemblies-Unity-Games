using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.SpikesTrap;

[RequireComponent(typeof(Collider2D))]
public class StillSpikeTrap : MonoBehaviour
{
	private Collider2D TrapCollider;

	[FoldoutGroup("Overlap fixer settings", 0)]
	public ContactFilter2D overlapFilter;

	[FoldoutGroup("Overlap fixer settings", 0)]
	public Collider2D[] contacts;

	private void Awake()
	{
		TrapCollider = GetComponent<Collider2D>();
		contacts = new Collider2D[5];
	}

	private void Start()
	{
		SetNoSafePositionToOverlappedCollider();
	}

	private void SetNoSafePositionToOverlappedCollider()
	{
		int num = TrapCollider.OverlapCollider(overlapFilter, contacts);
		if (num <= 0)
		{
			return;
		}
		Collider2D[] array = contacts;
		foreach (Collider2D collider2D in array)
		{
			if ((bool)collider2D)
			{
				collider2D.gameObject.AddComponent<NoSafePosition>();
			}
		}
	}
}
