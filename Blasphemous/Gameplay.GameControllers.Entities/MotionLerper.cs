using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class MotionLerper : MonoBehaviour
{
	public Core.SimpleEvent OnLerpStart;

	public Core.SimpleEvent OnLerpStop;

	[Header("Motion Params")]
	[Tooltip("The transform used to move the game object")]
	[SerializeField]
	private Transform motionObject;

	[Tooltip("The time taken to move from the start to finish positions during Lerp")]
	[Range(0.01f, 10f)]
	public float TimeTakenDuringLerp = 1f;

	[Tooltip("How far the object should move when along the X axis when the lerper is fired")]
	public float distanceToMove = 10f;

	[Tooltip("The behaviour of the lerp acceleration")]
	public AnimationCurve speedCurve;

	private Vector3 startPosition;

	private Vector3 endPosition;

	private float timeStartedLerping;

	public bool IsLerping { get; private set; }

	public void StartLerping(Vector3 dir)
	{
		if (OnLerpStart != null)
		{
			OnLerpStart();
		}
		timeStartedLerping = Time.time;
		Vector3 vector = (startPosition = new Vector3(motionObject.position.x, motionObject.position.y, motionObject.position.z));
		endPosition = vector + dir * distanceToMove;
		if (!IsLerping)
		{
			IsLerping = true;
		}
	}

	public void StopLerping()
	{
		if (IsLerping)
		{
			IsLerping = !IsLerping;
		}
		if (OnLerpStop != null)
		{
			OnLerpStop();
		}
	}

	private void Update()
	{
		if (!IsLerping)
		{
			return;
		}
		float num = Time.time - timeStartedLerping;
		float num2 = num / TimeTakenDuringLerp;
		motionObject.position = Vector3.Lerp(startPosition, endPosition, speedCurve.Evaluate(num2));
		if (num2 >= 1f)
		{
			IsLerping = false;
			if (OnLerpStop != null)
			{
				OnLerpStop();
			}
		}
	}
}
