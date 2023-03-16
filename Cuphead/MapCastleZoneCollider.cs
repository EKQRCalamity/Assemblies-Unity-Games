using UnityEngine;

public class MapCastleZoneCollider : AbstractCollidableObject
{
	public delegate void MapCastleZoneCollision(MapCastleZoneCollider collider, GameObject other, CollisionPhase phase);

	[SerializeField]
	public MapCastleZones.Zone zone;

	[SerializeField]
	public Transform interactionPoint;

	[SerializeField]
	public bool enableLadderShadow = true;

	[SerializeField]
	public AbstractMapInteractiveEntity.PositionProperties returnPositions;

	public event MapCastleZoneCollision OnMapCastleZoneCollision;

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		Vector2 vector = interactionPoint.position;
		Gizmos.DrawWireSphere(vector, 0.2f);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(vector + returnPositions.singlePlayer, 0.2f);
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(vector + returnPositions.playerOne, Vector3.one * 0.2f);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(vector + returnPositions.playerTwo, Vector3.one * 0.2f);
	}

	protected override void OnCollision(GameObject hit, CollisionPhase phase)
	{
		base.OnCollision(hit, phase);
		if (hit.CompareTag("Player_Map") && (phase == CollisionPhase.Enter || phase == CollisionPhase.Exit) && this.OnMapCastleZoneCollision != null)
		{
			this.OnMapCastleZoneCollision(this, hit, phase);
		}
	}
}
