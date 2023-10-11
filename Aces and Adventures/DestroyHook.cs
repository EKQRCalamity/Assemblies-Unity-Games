using UnityEngine;
using UnityEngine.Events;

public class DestroyHook : MonoBehaviour
{
	public UnityEvent OnDestroyed;

	private void OnDestroy()
	{
		OnDestroyed?.Invoke();
	}

	public void AddOnDestroyedListener(UnityAction action)
	{
		UnityEventExtensions.AddListener(ref OnDestroyed, action);
	}

	public void DestroySelf()
	{
		if ((bool)base.gameObject)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
