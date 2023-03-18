using Framework.FrameworkCore;
using Framework.Util;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class EntityMotionChecker : Trait
{
	public LayerMask BlockLayerMask;

	public LayerMask PatrolBlockLayerMask;

	private RaycastHit2D[] _bottomHits;

	private RaycastHit2D[] _forwardsHits;

	private RaycastHit2D[] _patrolHits;

	[Header("Sensors variables")]
	[SerializeField]
	private float xOffset;

	[SerializeField]
	private float yOffset;

	[Space]
	[SerializeField]
	private bool useDifferentOffsetForGroundSensors;

	[SerializeField]
	private float xOffsetGround;

	[SerializeField]
	private float yOffsetGround;

	[Header("Detection Range")]
	[Tooltip("The length of the block detection raycast")]
	[Range(0f, 1f)]
	public float RangeBlockDetection = 0.5f;

	[Tooltip("The length og the ground detection raycast")]
	[Range(0f, 10f)]
	public float RangeGroundDetection = 2f;

	public bool HitsFloor;

	public bool HitsBlock;

	public bool HitsPatrolBlock;

	protected override void OnStart()
	{
		base.OnStart();
		_bottomHits = new RaycastHit2D[1];
		_forwardsHits = new RaycastHit2D[1];
		_patrolHits = new RaycastHit2D[2];
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		int num = ((base.EntityOwner.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
		Vector2 vector = (Vector2)base.transform.position + (float)num * (Vector2)base.transform.right * xOffset + (Vector2)base.transform.up * yOffset;
		Vector2 vector2 = ((!useDifferentOffsetForGroundSensors) ? vector : ((Vector2)base.transform.position + (float)num * (Vector2)base.transform.right * xOffsetGround + (Vector2)base.transform.up * yOffsetGround));
		HitsFloor = Physics2D.LinecastNonAlloc(vector2, vector2 - (Vector2)base.transform.up * RangeGroundDetection, _bottomHits, BlockLayerMask) > 0;
		Color color = ((!HitsFloor) ? Color.yellow : Color.green);
		Debug.DrawLine(vector2, vector2 - (Vector2)base.transform.up * RangeGroundDetection, color);
		HitsBlock = Physics2D.LinecastNonAlloc(vector, vector + (float)num * (Vector2)base.transform.right * RangeBlockDetection, _forwardsHits, BlockLayerMask) > 0;
		int num2 = Physics2D.LinecastNonAlloc(vector, vector + (float)num * (Vector2)base.transform.right * RangeBlockDetection, _patrolHits, PatrolBlockLayerMask);
		HitsPatrolBlock = num2 > 0;
		if (num2 == 1)
		{
			Enemy componentInParent = _patrolHits[0].collider.GetComponentInParent<Enemy>();
			if (componentInParent != null && componentInParent == base.EntityOwner)
			{
				HitsPatrolBlock = false;
			}
		}
		color = ((!HitsBlock) ? Color.yellow : Color.green);
		Debug.DrawLine(vector, vector + (float)num * (Vector2)base.transform.right * RangeBlockDetection, color);
		color = ((!HitsPatrolBlock) ? Color.grey : Color.cyan);
		Debug.DrawLine(vector + Vector2.up * 0.05f, vector + Vector2.up * 0.05f + (float)num * (Vector2)base.transform.right * RangeBlockDetection, color);
	}

	public bool HitsBlockInPosition(Vector2 pos, Vector2 dir, float range, out Vector2 hitPoint, bool show = false)
	{
		hitPoint = Vector2.zero;
		Vector2 vector = pos + dir * range;
		bool flag = Physics2D.LinecastNonAlloc(pos, vector, _forwardsHits, BlockLayerMask) > 0;
		if (show)
		{
			Color color = ((!flag) ? Color.grey : Color.cyan);
			Debug.DrawLine(pos, vector, color, 0.5f);
		}
		if (flag)
		{
			hitPoint = _forwardsHits[0].point;
		}
		return flag;
	}

	public bool HitsFloorInPosition(Vector2 pos, float range, out Vector2 hitPoint, bool show = false)
	{
		hitPoint = Vector2.zero;
		bool flag = Physics2D.LinecastNonAlloc(pos, pos - (Vector2)base.transform.up * range, _bottomHits, BlockLayerMask) > 0;
		if (show)
		{
			Color color = ((!flag) ? Color.grey : Color.cyan);
			Debug.DrawLine(pos, pos - (Vector2)base.transform.up * range, color, 0.5f);
		}
		if (flag)
		{
			hitPoint = _bottomHits[0].point;
		}
		return flag;
	}

	public void SnapToGround(Transform t, float distance, float skin = 0.05f)
	{
		int num = ((base.EntityOwner.Status.Orientation == EntityOrientation.Right) ? 1 : (-1));
		Vector2 vector = (Vector2)base.transform.position + (float)num * (Vector2)base.transform.right * xOffset + (Vector2)base.transform.up * yOffset;
		if (Physics2D.LinecastNonAlloc(vector, vector - (Vector2)base.transform.up * distance, _bottomHits, BlockLayerMask) > 0)
		{
			GameplayUtils.DrawDebugCross(base.transform.position, Color.magenta, 10f);
			Debug.DrawLine(vector, _bottomHits[0].point, Color.green, 10f);
			t.position += (Vector3)_bottomHits[0].point - (Vector3)vector;
			t.position += base.transform.up * skin;
		}
		else
		{
			Debug.DrawLine(vector, vector - (Vector2)base.transform.up * distance, Color.red, 10f);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere((Vector2)base.transform.position + (Vector2)base.transform.TransformDirection(new Vector2(xOffset, yOffset)), 0.05f);
		Vector2 vector = (Vector2)base.transform.position + 1f * (Vector2)base.transform.right * xOffset + (Vector2)base.transform.up * yOffset;
		Vector2 vector2 = ((!useDifferentOffsetForGroundSensors) ? vector : ((Vector2)base.transform.position + 1f * (Vector2)base.transform.right * xOffsetGround + (Vector2)base.transform.up * yOffsetGround));
		Gizmos.DrawLine(vector2, vector2 - (Vector2)base.transform.up * RangeGroundDetection);
		Gizmos.DrawLine(vector, vector + (Vector2)base.transform.right * RangeBlockDetection);
	}
}
