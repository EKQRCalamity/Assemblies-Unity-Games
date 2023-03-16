using System.Collections;
using UnityEngine;

public class PlatformingLevelAutoscrollObject : AbstractCollidableObject
{
	[SerializeField]
	private Transform endPosition;

	protected float lockDistance = 600f;

	protected float endDelay = 1f;

	protected bool checkToLock;

	protected bool isLocked;

	protected bool isMoving { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		isMoving = false;
		isLocked = false;
	}

	protected virtual void Start()
	{
		if (checkToLock)
		{
			StartCoroutine(check_to_lock_cr());
		}
	}

	protected virtual void Update()
	{
		if (isMoving && base.transform.position.x > endPosition.transform.position.x)
		{
			StartEndingAutoscroll();
			isMoving = false;
		}
	}

	protected virtual IEnumerator check_to_lock_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.1f);
		float dist = PlayerManager.Center.x - base.transform.position.x;
		while (dist < 0f - lockDistance)
		{
			dist = PlayerManager.Center.x - base.transform.position.x;
			yield return null;
		}
		CupheadLevelCamera.Current.LockCamera(lockCamera: true);
		isLocked = true;
		yield return null;
	}

	protected virtual void StartAutoscroll()
	{
		isMoving = true;
		isLocked = false;
		CupheadLevelCamera.Current.LockCamera(lockCamera: false);
		CupheadLevelCamera.Current.SetAutoScroll(isScrolling: true);
	}

	protected virtual void StartEndingAutoscroll()
	{
		StartCoroutine(end_autoscroll());
	}

	protected virtual void EndAutoscroll()
	{
	}

	private IEnumerator end_autoscroll()
	{
		CupheadLevelCamera.Current.SetAutoScroll(isScrolling: false);
		EndAutoscroll();
		yield return null;
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		if (endPosition.transform != null)
		{
			Gizmos.DrawLine(new Vector3(endPosition.transform.position.x, endPosition.transform.position.y + 1500f), new Vector3(endPosition.transform.position.x, endPosition.transform.position.y - 1500f));
		}
	}
}
