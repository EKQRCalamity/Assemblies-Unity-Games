using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class AutoConnectLinks : MonoBehaviour
{
	[Button(ButtonSizes.Small)]
	public void ConnectAllJoints()
	{
		List<CharacterJoint> list = new List<CharacterJoint>(GetComponentsInChildren<CharacterJoint>());
		foreach (CharacterJoint item in list)
		{
			if (item.transform.parent != null)
			{
				item.connectedBody = item.transform.parent.GetComponent<Rigidbody>();
			}
		}
	}
}
