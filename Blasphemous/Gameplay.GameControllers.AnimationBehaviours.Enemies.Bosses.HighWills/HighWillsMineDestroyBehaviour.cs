using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.HighWills;

public class HighWillsMineDestroyBehaviour : StateMachineBehaviour
{
	public enum MineState
	{
		EXPLODING,
		BEING_DESTROYED
	}

	[EnumToggleButtons]
	public MineState State;

	[ShowIf("ShowExplosionGO", true)]
	public GameObject ExplosionGO;

	private bool ShowExplosionGO => State == MineState.EXPLODING;

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		switch (State)
		{
		case MineState.EXPLODING:
			PoolManager.Instance.ReuseObject(ExplosionGO, animator.gameObject.transform.position, Quaternion.identity, createPoolIfNeeded: true);
			break;
		}
		animator.gameObject.SetActive(value: false);
	}
}
