using Framework.FrameworkCore;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class CliffSensor : MonoBehaviour
{
	[Range(0f, 3f)]
	public float cliffScopeDetection;

	private float currentDistanceToCliffLede;

	private Entity entity;

	private RaycastHit2D hitLeft;

	private RaycastHit2D hitRight;

	private EntityOrientation hitSide;

	private RaycastHit2D[] rayCasts;

	public LayerMask targetLayer;

	[Tooltip("Minimum distance to which it will be allowed to attack advancing.")]
	[Range(0f, 1f)]
	public float vanishingPointDistance;

	private void Awake()
	{
		entity = GetComponentInParent<Entity>();
	}

	private void Start()
	{
	}

	private void Update()
	{
		entity.Status.IsOnCliffLede = isOnCliffLede(rayCasts) && IsFacingCliffLede();
	}

	private void FixedUpdate()
	{
		if (IsNearbyCliff() && !entity.HasFlag("NEARBY_CLIFF"))
		{
			entity.SetFlag("NEARBY_CLIFF", active: true);
		}
		else if (!IsNearbyCliff() && entity.HasFlag("NEARBY_CLIFF"))
		{
			entity.SetFlag("NEARBY_CLIFF", active: false);
		}
	}

	private bool IsNearbyCliff()
	{
		hitRight = Physics2D.Raycast(base.transform.position, Vector2.right, cliffScopeDetection, targetLayer);
		hitLeft = Physics2D.Raycast(base.transform.position, -Vector2.right, cliffScopeDetection, targetLayer);
		rayCasts = new RaycastHit2D[2] { hitRight, hitLeft };
		Debug.DrawRay(base.transform.position, Vector2.right * cliffScopeDetection, Color.blue);
		Debug.DrawRay(base.transform.position, -Vector2.right * cliffScopeDetection, Color.red);
		if (hitRight.collider != null)
		{
			hitSide = EntityOrientation.Right;
		}
		if (hitLeft.collider != null)
		{
			hitSide = EntityOrientation.Left;
		}
		return (bool)hitLeft || (bool)hitRight;
	}

	private bool isOnCliffLede(RaycastHit2D[] rayCasts)
	{
		bool result = false;
		if (IsNearbyCliff())
		{
			for (byte b = 0; b < rayCasts.Length; b = (byte)(b + 1))
			{
				if (rayCasts[b].collider != null)
				{
					currentDistanceToCliffLede = rayCasts[b].distance;
					if (currentDistanceToCliffLede <= vanishingPointDistance)
					{
						result = true;
						break;
					}
				}
			}
		}
		return result;
	}

	public bool IsFacingCliffLede()
	{
		return entity.Status.Orientation == hitSide;
	}
}
