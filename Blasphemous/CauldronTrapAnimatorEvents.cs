using UnityEngine;

public class CauldronTrapAnimatorEvents : MonoBehaviour
{
	public CauldronTrap ownerTrap;

	public void TriggerLiquid()
	{
		ownerTrap.StartFall();
	}

	public void StopLiquid()
	{
		ownerTrap.StopFall();
	}
}
