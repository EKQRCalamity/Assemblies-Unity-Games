using System;
using Framework.FrameworkCore;
using HutongGames.PlayMaker;
using NodeCanvas.BehaviourTrees;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Starts a behaviour tree.")]
public class BehaviourStart : FsmStateAction
{
	public FsmGameObject behaviour;

	public override void OnEnter()
	{
		try
		{
			BehaviourTreeOwner component = behaviour.Value.GetComponent<BehaviourTreeOwner>();
			component.StartBehaviour();
			Finish();
		}
		catch (NullReferenceException ex)
		{
			Framework.FrameworkCore.Log.Error("Playmaker", "BehaviourStop has received a non behaviour object." + ex.ToString());
		}
	}
}
