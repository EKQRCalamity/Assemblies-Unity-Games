using System.Collections;
using UnityEngine;

public class GameStepAdventureTimer : GameStep
{
	protected override IEnumerator Update()
	{
		while (!base.finished)
		{
			yield return null;
		}
	}

	protected override void LateUpdate()
	{
		base.state.AdjustTotalTime(Time.deltaTime);
		GameStep activeStep = base.state.stack.activeStep;
		if (activeStep != null && activeStep.countsTowardsStrategyTime && base.state.encounterActive)
		{
			base.state.AdjustStrategyTime(Time.deltaTime);
		}
	}
}
