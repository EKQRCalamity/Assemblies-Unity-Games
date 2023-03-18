using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Interactables;

public class ActivateIfDLCInstalled : MonoBehaviour
{
	[HideIf("activateOnlyIfDLC2", true)]
	public bool activateOnlyIfDLC3 = true;

	[HideIf("activateOnlyIfDLC3", true)]
	public bool activateOnlyIfDLC2;

	public void Awake()
	{
		if (activateOnlyIfDLC3 && activateOnlyIfDLC2)
		{
			Debug.LogError("ActivateIfDLCInstalled: both activateOnlyIfDLC3 and activateOnlyIfDLC2 shouldn't be true!");
		}
		else if (!activateOnlyIfDLC3 && !activateOnlyIfDLC2)
		{
			Debug.LogError("ActivateIfDLCInstalled: both activateOnlyIfDLC3 and activateOnlyIfDLC2 shouldn't be false!");
		}
		if (activateOnlyIfDLC3)
		{
			base.gameObject.SetActive(value: true);
		}
		else if (activateOnlyIfDLC2)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
