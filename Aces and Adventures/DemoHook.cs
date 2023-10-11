using UnityEngine;

public class DemoHook : MonoBehaviour
{
	public BoolEvent onIsDemoChange;

	private void Awake()
	{
		onIsDemoChange?.Invoke(IOUtil.IsDemo);
	}
}
