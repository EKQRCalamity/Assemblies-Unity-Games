using UnityEngine;

namespace Tools.Level.Actionables;

[ExecuteInEditMode]
public class TriggerTrapManagerAutogenerator : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private bool executedFlag;

	private void Start()
	{
		if (!executedFlag)
		{
			TriggerTrapManager triggerTrapManager = Object.FindObjectOfType<TriggerTrapManager>();
			if (triggerTrapManager == null)
			{
				GameObject gameObject = new GameObject("----------TRIGGER TRAP MANAGER----------(auto-generated)");
				TriggerTrapManager triggerTrapManager2 = gameObject.AddComponent<TriggerTrapManager>();
				triggerTrapManager2.LinkToSceneTraps();
			}
			executedFlag = true;
		}
	}
}
