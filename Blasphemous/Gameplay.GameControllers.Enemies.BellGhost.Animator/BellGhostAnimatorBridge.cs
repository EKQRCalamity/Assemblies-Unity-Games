using UnityEngine;

namespace Gameplay.GameControllers.Enemies.BellGhost.Animator;

public class BellGhostAnimatorBridge : MonoBehaviour
{
	private BellGhost _bellGhost;

	private void Start()
	{
		_bellGhost = GetComponentInParent<BellGhost>();
	}

	public void PlayBrokenBell()
	{
		_bellGhost.Audio.BrokenBell();
	}

	public void Ramming()
	{
		_bellGhost.Behaviour.Ramming();
	}

	public void PlayChargeAttack()
	{
		_bellGhost.Audio.ChargeAttack();
	}
}
