using Gameplay.GameControllers.Bosses.PietyMonster.IA;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Attack;

[RequireComponent(typeof(Collider2D))]
public class PietyFeetAttackArea : MonoBehaviour
{
	public LayerMask TargetLayerMask;

	public PietyMonsterBehaviour PietyMonsterBehaviour;

	private void Awake()
	{
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if ((TargetLayerMask.value & (1 << other.gameObject.layer)) > 0 && !PietyMonsterBehaviour.TargetOnRange)
		{
			PietyMonsterBehaviour.TargetOnRange = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if ((TargetLayerMask.value & (1 << other.gameObject.layer)) > 0 && PietyMonsterBehaviour.TargetOnRange)
		{
			PietyMonsterBehaviour.TargetOnRange = false;
		}
	}
}
