using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.PlayMaker.Action;

[ActionCategory("Blasphemous Action")]
public class CountObjectMarket : FsmStateAction
{
	public FsmInt TotalObj;

	private GameObject objetos;

	public override void Reset()
	{
		TotalObj = null;
	}

	public override void OnEnter()
	{
	}

	public override void OnUpdate()
	{
		TotalObj = GameObject.FindGameObjectsWithTag("MKTObj").Length;
	}

	public override void OnExit()
	{
	}
}
