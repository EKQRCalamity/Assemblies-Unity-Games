using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.CliffGrabbers;

[RequireComponent(typeof(BoxCollider2D))]
public class CliffGrabActivator : MonoBehaviour
{
	public LayerMask NearPlatform;

	public float OffsetDetection = 0.5f;

	public float VerticalRayDistance = 4.25f;

	private Gameplay.GameControllers.Penitent.Penitent _player;

	private Vector2 RightBoundary { get; set; }

	private Vector2 LeftBoundary { get; set; }

	private BoxCollider2D BoxCollider { get; set; }

	private void Awake()
	{
		BoxCollider = GetComponentInChildren<BoxCollider2D>();
		RightBoundary = new Vector2(BoxCollider.bounds.max.x + OffsetDetection, BoxCollider.bounds.min.y);
		LeftBoundary = new Vector2(BoxCollider.bounds.min.x - OffsetDetection, BoxCollider.bounds.min.y);
	}

	private void Update()
	{
		if (!_player)
		{
			_player = Core.Logic.Penitent;
		}
		if ((bool)_player && !_player.IsGrabbingCliffLede)
		{
			bool flag = CheckRayCastHitsPlatform();
			BoxCollider.enabled = !flag;
		}
	}

	private bool CheckRayCastHitsPlatform()
	{
		RaycastHit2D raycastHit2D = Physics2D.Raycast(RightBoundary, -Vector2.up, VerticalRayDistance, NearPlatform);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(LeftBoundary, -Vector2.up, VerticalRayDistance, NearPlatform);
		return (bool)raycastHit2D || (bool)raycastHit2D2;
	}
}
