using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Sends metric data to the cloud")]
public class SendMetric : FsmStateAction
{
	public FsmString metricId;

	public FsmString metricDefinition;

	public override void OnEnter()
	{
		Core.Metrics.CustomEvent(metricId.Value, metricDefinition.Value);
		Finish();
	}
}
