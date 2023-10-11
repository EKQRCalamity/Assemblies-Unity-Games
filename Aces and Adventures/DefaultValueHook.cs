using UnityEngine;

public class DefaultValueHook : MonoBehaviour
{
	public object defaultValue;

	public ObjectEvent OnDefaultValueRequest;

	public void RequestDefaultValue()
	{
		if (defaultValue != null)
		{
			OnDefaultValueRequest.Invoke(defaultValue);
		}
	}
}
