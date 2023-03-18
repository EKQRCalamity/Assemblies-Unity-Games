using System.Linq;
using Gameplay.GameControllers.Entities;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.Playmaker2.Action;

[ActionCategory(ActionCategory.Array)]
[HutongGames.PlayMaker.Tooltip("Find all active GameObjects with a specific name and store them in an array.")]
public class ArrayFindGameObjectsByName : FsmStateAction
{
	[RequiredField]
	[UIHint(UIHint.Variable)]
	[HutongGames.PlayMaker.Tooltip("The Array Variable to use.")]
	public FsmArray array;

	[HutongGames.PlayMaker.Tooltip("the name")]
	public FsmString ObjectName;

	public override void Reset()
	{
		array = null;
		ObjectName = new FsmString
		{
			UseVariable = true
		};
	}

	public override void OnEnter()
	{
		FindGoByName();
		Finish();
	}

	public void FindGoByName()
	{
		if (!string.IsNullOrEmpty(ObjectName.Value) && ObjectName.Value.Length > 1)
		{
			array.Values = (from go in Object.FindObjectsOfType<GameObject>()
				where go.name.Contains(ObjectName.Value) && !go.GetComponentInChildren<Enemy>().Status.Dead
				select go).ToArray();
		}
	}
}
