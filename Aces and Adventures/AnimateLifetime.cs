using UnityEngine;
using UnityEngine.Events;

public class AnimateLifetime : MonoBehaviour
{
	public float lifetime;

	public OnCompleteAction onCompleteAction = OnCompleteAction.DeactivateGameObject;

	public UnityEvent onComplete;

	private float _lifetimeRemaining;

	private void OnEnable()
	{
		_lifetimeRemaining = lifetime;
	}

	private void Update()
	{
		float lifetimeRemaining = _lifetimeRemaining;
		_lifetimeRemaining -= Time.deltaTime;
		if (_lifetimeRemaining <= 0f && lifetimeRemaining > 0f)
		{
			if (onComplete != null)
			{
				onComplete.Invoke();
			}
			this.DoOnCompleteAction(onCompleteAction);
		}
	}

	public void Lifetime(float lifetime)
	{
		this.lifetime = lifetime;
	}
}
