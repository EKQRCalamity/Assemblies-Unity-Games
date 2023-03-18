using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory(ActionCategory.GameObject)]
[Tooltip("Finds any GameObject(s) with a Name that contains a particular set of String and stores the count of them in an FSM Int.")]
public class FindGameObjectsResourcesWithNameContaining : FsmStateAction
{
	[Tooltip("Find any GameObject(s) with a Name containing this string and Count the number of them.")]
	public FsmString[] withNameContaining;

	[Tooltip("If this bool is set to True then the String search is case insensitive.")]
	[VariableType(VariableType.Bool)]
	public FsmBool caseInsensitive;

	[UIHint(UIHint.Variable)]
	[Tooltip("Store the result in an FSM Int of the Count of the found GameObject(s).")]
	public FsmInt storeCount;

	[UIHint(UIHint.Variable)]
	[Tooltip("Store the result in a GameObject variable.")]
	[VariableType(VariableType.GameObject)]
	public FsmArray storeResults;

	private string name;

	public override void Reset()
	{
		withNameContaining = new FsmString[1];
		storeCount = null;
		storeResults = null;
	}

	public override void OnEnter()
	{
		Find();
		Finish();
	}

	private void Find()
	{
		StringComparison _compare = ((!caseInsensitive.Value) ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase);
		List<GameObject> list = new List<GameObject>();
		FsmString[] array = withNameContaining;
		foreach (FsmString _containString in array)
		{
			if (!string.IsNullOrEmpty(_containString.Value))
			{
				list.AddRange((from GameObject g in Resources.FindObjectsOfTypeAll(typeof(GameObject))
					where g.name.IndexOf(_containString.Value, _compare) >= 0
					select g).ToList());
			}
		}
		storeCount.Value = list.Count;
		storeResults.RawValue = list.ToArray();
	}

	public override string ErrorCheck()
	{
		if (withNameContaining.Length == 0)
		{
			return "Please Specify at least one String to be found within GameObject(s') Name(s).";
		}
		if (storeCount.IsNone && storeResults.IsNone)
		{
			return "Please use either storeCount or storeResults else this action is not necessary";
		}
		return string.Empty;
	}
}
