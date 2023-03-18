using Framework.FrameworkCore;
using UnityEngine;

namespace Gameplay.GameControllers.Environment;

[RequireComponent(typeof(BoxCollider2D))]
public class CliffLede : MonoBehaviour
{
	public enum CliffLedeSide
	{
		Left,
		Right
	}

	[Tooltip("The entity orientation allowed to grab the cliff lede hook.")]
	public CliffLedeSide cliffLedeSide;

	[Tooltip("Determines whether the cliff lede is climbable. True by default.")]
	public bool isClimbable = true;

	[Tooltip("Sprite representation of the cliff")]
	public SpriteRenderer gizmo;

	private EntityOrientation cliffLedeGrabSideAllowed;

	private CliffLedeRootTarget rootTarget;

	public EntityOrientation CliffLedeGrabSideAllowed => cliffLedeGrabSideAllowed;

	public CliffLedeRootTarget RootTarget => rootTarget;

	private void Awake()
	{
		cliffLedeGrabSideAllowed = ((cliffLedeSide == CliffLedeSide.Left) ? EntityOrientation.Left : EntityOrientation.Right);
	}

	private void Start()
	{
		rootTarget = GetComponentInChildren<CliffLedeRootTarget>();
	}

	private void OnValidate()
	{
		if (gizmo != null)
		{
			gizmo.flipX = cliffLedeSide == CliffLedeSide.Left;
		}
	}
}
