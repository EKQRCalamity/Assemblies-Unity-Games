using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class WickerWurmTailAttack : MonoBehaviour
{
	public BlindBabyPoints.WickerWurmPathConfig pathConfigShow;

	public BlindBabyPoints.WickerWurmPathConfig pathConfig;

	public SplineFollower pathfollower;

	public BodyChainLink tailEnd;

	public BodyChainMaster tailMaster;

	private float retractDelay;

	public float showTailCounter;

	public void ShowTail(Vector2 point, bool rightSide, float delay)
	{
		pathfollower.spline = pathConfigShow.spline;
		tailMaster.chainMode = BodyChainMaster.CHAIN_UPDATE_MODES.NORMAL;
		if ((bool)pathfollower && (bool)pathfollower.spline)
		{
			pathfollower.spline.transform.localScale = new Vector3(rightSide ? 1 : (-1), pathfollower.spline.transform.localScale.y, 1f);
			pathfollower.currentCounter = 0f;
			pathfollower.spline.gameObject.transform.position = point;
			pathfollower.movementCurve = pathConfig.curve;
			pathfollower.duration = pathConfig.duration;
			pathfollower.followActivated = true;
			pathfollower.OnMovingToNextPoint += OnNextPointOnSight;
			retractDelay = delay;
		}
	}

	public void TailAttack(Vector2 point, bool rightSide, float delay)
	{
		pathfollower.spline = pathConfig.spline;
		tailMaster.chainMode = BodyChainMaster.CHAIN_UPDATE_MODES.NORMAL;
		pathfollower.spline.transform.localScale = new Vector3(rightSide ? 1 : (-1), pathfollower.spline.transform.localScale.y, 1f);
		pathfollower.currentCounter = 0f;
		pathfollower.spline.gameObject.transform.position = point;
		pathfollower.movementCurve = pathConfig.curve;
		pathfollower.duration = pathConfig.duration;
		pathfollower.followActivated = true;
		pathfollower.OnMovementCompleted += OnTailAttackCompleted;
		pathfollower.OnMovingToNextPoint += OnNextPointOnSight;
		retractDelay = delay;
	}

	private void OnNextPointOnSight(Vector2 obj)
	{
		Vector2 vector = obj - (Vector2)tailMaster.transform.position;
		tailMaster.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(vector.y, vector.x) * 57.29578f);
	}

	[Button("Debug_TestShow", ButtonSizes.Small)]
	public void TestShow()
	{
		ShowTail(base.transform.position, rightSide: true, 0f);
	}

	[Button("Debug_TestAttac", ButtonSizes.Small)]
	public void TestAttack()
	{
		TailAttack(base.transform.position, rightSide: true, 0f);
	}

	private void OnTailAttackCompleted()
	{
		pathfollower.OnMovementCompleted -= OnTailAttackCompleted;
		pathfollower.OnMovingToNextPoint -= OnNextPointOnSight;
		StartCoroutine(RetractTail());
	}

	private IEnumerator RetractTail()
	{
		yield return new WaitForSeconds(retractDelay);
		tailMaster.chainMode = BodyChainMaster.CHAIN_UPDATE_MODES.BACKWARDS;
		tailEnd.transform.DOMoveY(tailEnd.transform.position.y - 20f * pathConfig.spline.transform.localScale.y, 2f).SetEase(Ease.InOutQuad);
	}
}
