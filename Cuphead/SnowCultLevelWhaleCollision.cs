using UnityEngine;

public class SnowCultLevelWhaleCollision : AbstractCollidableObject
{
	[SerializeField]
	private SnowCultLevelWizard wiz;

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			wiz.PlayerHitByWhale(hit, phase);
		}
	}
}
