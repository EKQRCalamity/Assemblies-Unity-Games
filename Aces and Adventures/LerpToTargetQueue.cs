using System.Collections.Generic;
using UnityEngine;

public class LerpToTargetQueue : MonoBehaviour
{
	[Header("Targets")]
	public Transform lerpFrom;

	public Transform lerpTo;

	[Header("Time")]
	public bool useScaledTime;

	public bool tickQueueTimeWhileEmpty = true;

	[Range(0f, 10f)]
	public float queueTime;

	[Range(0.01f, 100f)]
	public float duration = 1f;

	[Header("Curves")]
	public bool positionEnabled = true;

	public AnimationCurve positionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public bool rotationEnabled = true;

	public AnimationCurve rotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public bool scaleEnabled = true;

	public AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Header("Events")]
	public OnCompleteAction onLerpFinishedAction;

	[SerializeField]
	protected TransformEvent _onLerpFinished;

	private Queue<Transform> _queue;

	private float _queueTimeRemaining;

	private Queue<Transform> queue => _queue ?? (_queue = new Queue<Transform>());

	public TransformEvent onLerpFinished => _onLerpFinished ?? (_onLerpFinished = new TransformEvent());

	private bool _BeginLerping(Transform t)
	{
		LerpToTarget lerpToTarget = t.GetComponent<LerpToTarget>();
		if ((bool)lerpToTarget)
		{
			return false;
		}
		lerpToTarget = t.gameObject.AddComponent<LerpToTarget>().SetData(lerpFrom, lerpTo, duration, useScaledTime, positionCurve, rotationCurve, scaleCurve, positionEnabled, rotationEnabled, scaleEnabled);
		lerpToTarget.onTargetReached += delegate
		{
			lerpToTarget.DoOnCompleteAction(onLerpFinishedAction);
			Object.Destroy(lerpToTarget);
			if ((bool)t)
			{
				onLerpFinished.Invoke(t);
			}
		};
		return true;
	}

	private void OnEnable()
	{
		_queueTimeRemaining = tickQueueTimeWhileEmpty.ToFloat(0f, queueTime);
	}

	private void Update()
	{
		if (!tickQueueTimeWhileEmpty && queue.Count == 0)
		{
			return;
		}
		if (_queueTimeRemaining > 0f)
		{
			_queueTimeRemaining -= GameUtil.GetDeltaTime(useScaledTime);
		}
		while (_queueTimeRemaining <= 0f && queue.Count > 0)
		{
			if (_BeginLerping(queue.Dequeue()))
			{
				_queueTimeRemaining += queueTime;
			}
		}
	}

	public void Add(Transform t)
	{
		t.CopyFrom(lerpFrom);
		queue.Enqueue(t);
	}
}
