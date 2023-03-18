using System.Collections;
using Maikel.SteeringBehaviors;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class MoveToPointUsingAgent_EnemyAction : EnemyAction
{
	private Vector2 point;

	private Arrive arriveBehaviour;

	private AutonomousAgent agent;

	private float closeDistance;

	public IEnumerator WaitUntilClose(Transform t, Vector3 target, float distance = 0.01f)
	{
		while (Vector2.Distance(t.position, target) > distance)
		{
			yield return null;
		}
	}

	public EnemyAction StartAction(EnemyBehaviour e, AutonomousAgent _agent, Vector2 _target, float closeDistance = 2f)
	{
		point = _target;
		agent = _agent;
		arriveBehaviour = agent.GetComponent<Arrive>();
		if (arriveBehaviour == null)
		{
			Debug.LogError("Movement requires an Arrive component linked to the autonomous agent");
		}
		this.closeDistance = closeDistance;
		arriveBehaviour.target = point;
		agent.enabled = true;
		return StartAction(e);
	}

	protected override void DoOnStart()
	{
		base.DoOnStart();
	}

	protected override void DoOnStop()
	{
		base.DoOnStop();
		agent.enabled = false;
	}

	protected override IEnumerator BaseCoroutine()
	{
		yield return WaitUntilClose(owner.transform, point, closeDistance);
		Callback();
		yield return WaitUntilClose(owner.transform, point);
		agent.enabled = false;
		FinishAction();
	}
}
