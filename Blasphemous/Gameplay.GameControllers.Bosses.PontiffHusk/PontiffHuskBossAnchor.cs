using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffHusk;

public class PontiffHuskBossAnchor : MonoBehaviour
{
	public Transform ReferenceTransform;

	public bool FollowsParentsX = true;

	public bool FollowsParentsY;

	[HideInInspector]
	public float FixedX;

	[HideInInspector]
	public float FixedY;

	private void Start()
	{
		FixedX = base.gameObject.transform.position.x;
		FixedY = base.gameObject.transform.position.y;
	}

	private void LateUpdate()
	{
		Vector2 vector = new Vector2(base.gameObject.transform.position.x, base.gameObject.transform.position.y);
		if (!FollowsParentsX)
		{
			vector.x = FixedX;
		}
		else
		{
			vector.x = ReferenceTransform.position.x;
		}
		if (!FollowsParentsY)
		{
			vector.y = FixedY;
		}
		else
		{
			vector.y = ReferenceTransform.position.y;
		}
		base.gameObject.transform.position = vector;
	}
}
