using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ATransformToTarget))]
public class ATransformToTargetHook : MonoBehaviour
{
	public enum ResetType : byte
	{
		Enable,
		Awake
	}

	[SerializeField]
	protected UnityEvent _onTargetReached;

	public ResetType resetOn;

	private ATransformToTarget _transformToTarget;

	public UnityEvent onTargetReached => _onTargetReached ?? (_onTargetReached = new UnityEvent());

	public ATransformToTarget transformToTarget => this.CacheComponent(ref _transformToTarget);

	private void Awake()
	{
		if (resetOn == ResetType.Awake)
		{
			transformToTarget.onTargetReached += onTargetReached.Invoke;
		}
	}

	private void OnEnable()
	{
		if (resetOn == ResetType.Enable)
		{
			transformToTarget.onTargetReached += onTargetReached.Invoke;
		}
	}
}
