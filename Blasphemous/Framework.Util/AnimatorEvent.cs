using UnityEngine;

namespace Framework.Util;

public class AnimatorEvent : MonoBehaviour
{
	public delegate void AnimEvent(string id);

	public event AnimEvent OnEventLaunched;

	public void LaunchEvent(string eventParam)
	{
		if (this.OnEventLaunched != null)
		{
			this.OnEventLaunched(eventParam);
		}
	}
}
