using System.Collections;
using UnityEngine;

public class MountainPlatformingLevelElevatorHandler : AbstractPausableComponent
{
	[SerializeField]
	private MountainPlatformingLevelMudmanSpawner mudmanSpawner;

	[SerializeField]
	private Transform topBackground;

	[SerializeField]
	private Transform bottomBackground;

	[SerializeField]
	private float speed;

	[SerializeField]
	private GameObject scrollingObject;

	[SerializeField]
	private Transform triggerPoint;

	[SerializeField]
	private Transform triggerPoint2;

	[SerializeField]
	private Transform pointA;

	[SerializeField]
	private GameObject invisibleWall;

	[SerializeField]
	private ScrollingBackgroundElevator cloudSprite;

	[SerializeField]
	private ScrollingBackgroundElevator foregroundSprite;

	[SerializeField]
	private ScrollingBackgroundElevator backgroundSprite;

	[SerializeField]
	private ScrollingBackgroundElevator[] midgroundSprites;

	[SerializeField]
	private float time;

	private float easeingTime;

	private Coroutine cameraLockRoutine;

	public static bool elevatorIsMoving { get; private set; }

	private void Start()
	{
		elevatorIsMoving = false;
		StartCoroutine(wait_cr());
		cameraLockRoutine = StartCoroutine(lock_camera_cr());
		invisibleWall.SetActive(value: false);
		PlatformingLevelParallax[] componentsInChildren = bottomBackground.GetComponentsInChildren<PlatformingLevelParallax>();
		foreach (PlatformingLevelParallax platformingLevelParallax in componentsInChildren)
		{
			platformingLevelParallax.enabled = false;
		}
	}

	private IEnumerator lock_camera_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		AbstractPlayerController player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		AbstractPlayerController player3 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		while (true)
		{
			player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
			player3 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
			CupheadLevelCamera.Current.LockCamera(CupheadLevelCamera.Current.transform.position.x > triggerPoint.transform.position.x);
			if (player3 != null)
			{
				if (!player3.IsDead && !player1.IsDead)
				{
					if (player1.transform.position.x < triggerPoint.transform.position.x && player3.transform.position.x < triggerPoint.transform.position.x)
					{
						CupheadLevelCamera.Current.LockCamera(lockCamera: false);
					}
				}
				else if (player3.IsDead)
				{
					if (player1.transform.position.x < triggerPoint.transform.position.x)
					{
						CupheadLevelCamera.Current.LockCamera(lockCamera: false);
					}
				}
				else if (player1.IsDead && player3.transform.position.x < triggerPoint.transform.position.x)
				{
					CupheadLevelCamera.Current.LockCamera(lockCamera: false);
				}
			}
			else if (player1.transform.position.x < triggerPoint.transform.position.x)
			{
				CupheadLevelCamera.Current.LockCamera(lockCamera: false);
			}
			yield return null;
		}
	}

	private IEnumerator wait_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		AbstractPlayerController player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		AbstractPlayerController player3 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		while (true)
		{
			player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
			player3 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
			if (player3 != null)
			{
				if (!player3.IsDead && !player1.IsDead)
				{
					if ((player1.transform.position.x > triggerPoint.transform.position.x && player3.transform.position.x > triggerPoint2.transform.position.x) || (player1.transform.position.x > triggerPoint2.transform.position.x && player3.transform.position.x > triggerPoint.transform.position.x))
					{
						break;
					}
				}
				else if (player3.IsDead)
				{
					if (player1.transform.position.x > triggerPoint.transform.position.x)
					{
						break;
					}
				}
				else if (player1.IsDead && player3.transform.position.x > triggerPoint.transform.position.x)
				{
					break;
				}
			}
			else if (player1.transform.position.x > triggerPoint.transform.position.x)
			{
				break;
			}
			yield return null;
		}
		StopCoroutine(cameraLockRoutine);
		CupheadLevelCamera.Current.LockCamera(lockCamera: true);
		invisibleWall.SetActive(value: true);
		AudioManager.Play("castle_lift_start");
		CupheadLevelCamera.Current.Shake(10f, 1f);
		yield return CupheadTime.WaitForSeconds(this, 0.9f);
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		float t = 0f;
		elevatorIsMoving = true;
		Vector3 startPos = scrollingObject.transform.position;
		Vector3 pos = scrollingObject.transform.position;
		Vector3 dir = pointA.transform.position - scrollingObject.transform.position;
		Vector3 middle = scrollingObject.transform.position + dir.normalized * (Vector3.Distance(scrollingObject.transform.position, pointA.transform.position) / 2f);
		ScrollingBackgroundElevator[] array = midgroundSprites;
		foreach (ScrollingBackgroundElevator scrollingBackgroundElevator in array)
		{
			scrollingBackgroundElevator.SetUp(MathUtils.AngleToDirection(-45f), speed);
		}
		backgroundSprite.SetUp(MathUtils.AngleToDirection(-45f), speed - speed / 4f);
		foregroundSprite.SetUp(MathUtils.AngleToDirection(-45f), speed + speed / 4f);
		cloudSprite.SetUp(MathUtils.AngleToDirection(-45f), speed + speed / 4f);
		PlatformingLevelParallax[] componentsInChildren = topBackground.GetComponentsInChildren<PlatformingLevelParallax>();
		foreach (PlatformingLevelParallax platformingLevelParallax in componentsInChildren)
		{
			platformingLevelParallax.enabled = false;
		}
		bottomBackground.parent = scrollingObject.transform;
		mudmanSpawner.SpawnMudmen();
		AudioManager.PlayLoop("castle_lift_loop");
		while (scrollingObject.transform.position.y < middle.y)
		{
			pos -= (Vector3)MathUtils.AngleToDirection(-45f) * speed * CupheadTime.FixedDelta;
			scrollingObject.transform.position = pos;
			yield return wait;
		}
		while (t < time)
		{
			t += CupheadTime.FixedDelta;
			yield return null;
		}
		Vector3 midPos = scrollingObject.transform.position;
		float endTime = Vector3.Distance(startPos, pointA.position) / speed;
		t = 0f;
		ScrollingBackgroundElevator[] array2 = midgroundSprites;
		foreach (ScrollingBackgroundElevator scrollingBackgroundElevator2 in array2)
		{
			scrollingBackgroundElevator2.EaseoutSpeed(endTime);
		}
		cloudSprite.EaseoutSpeed(endTime);
		foregroundSprite.EaseoutSpeed(endTime);
		backgroundSprite.EaseoutSpeed(endTime);
		StartCoroutine(easeTime(endTime));
		cloudSprite.ending = true;
		while (easeingTime < endTime)
		{
			pos -= (Vector3)MathUtils.AngleToDirection(-45f) * speed * CupheadTime.FixedDelta;
			scrollingObject.transform.position = pos;
			yield return wait;
		}
		AudioManager.Stop("castle_lift_loop");
		AudioManager.Play("castle_lift_end");
		CupheadLevelCamera.Current.Shake(10f, 0.5f);
		PlatformingLevelParallax[] componentsInChildren2 = bottomBackground.GetComponentsInChildren<PlatformingLevelParallax>();
		foreach (PlatformingLevelParallax platformingLevelParallax2 in componentsInChildren2)
		{
			platformingLevelParallax2.enabled = true;
			platformingLevelParallax2.UpdateBasePosition();
		}
		ScrollingBackgroundElevator[] array3 = midgroundSprites;
		foreach (ScrollingBackgroundElevator scrollingBackgroundElevator3 in array3)
		{
			scrollingBackgroundElevator3.ending = true;
		}
		backgroundSprite.ending = true;
		elevatorIsMoving = false;
		if (PlayerManager.GetFirst().transform.position.x < CupheadLevelCamera.Current.transform.position.x)
		{
			CupheadLevelCamera.Current.OffsetCamera(cameraOffset: true, leftOffset: true);
		}
		else
		{
			CupheadLevelCamera.Current.OffsetCamera(cameraOffset: true, leftOffset: false);
		}
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		CupheadLevelCamera.Current.OffsetCamera(cameraOffset: false, leftOffset: false);
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		CupheadLevelCamera.Current.LockCamera(lockCamera: false);
		yield return null;
	}

	private IEnumerator easeTime(float time)
	{
		float startSpeed = speed;
		easeingTime = 0f;
		while (easeingTime < time)
		{
			easeingTime += CupheadTime.Delta;
			speed = Mathf.Lerp(startSpeed, 0f, easeingTime / time);
			yield return null;
		}
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.color = new Color(0f, 0f, 1f, 1f);
		Gizmos.DrawLine(triggerPoint.transform.position, new Vector3(triggerPoint.transform.position.x, 5000f, 0f));
		Gizmos.DrawLine(triggerPoint2.transform.position, new Vector3(triggerPoint2.transform.position.x, 5000f, 0f));
		Gizmos.color = new Color(0f, 1f, 0f, 1f);
		Gizmos.DrawWireSphere(pointA.transform.position, 100f);
		Gizmos.DrawLine(pointA.transform.position, scrollingObject.transform.position);
	}
}
