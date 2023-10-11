using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField("Wait Action", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Use to add a wait between ability actions.", category = "Specialized")]
public class WaitAction : AAction
{
	private static readonly Target.Combatant.Self TARGET_SELF = new Target.Combatant.Self();

	[ProtoMember(1)]
	[UIField(min = 0.01f, max = 1f)]
	[DefaultValue(0.25f)]
	private float _wait = 0.25f;

	public override Target target => TARGET_SELF;

	protected override void _Tick(ActionContext context)
	{
		context.gameState.stack.Push(new GameStepWait(_wait, null, canSkip: false));
	}

	protected override string _ToStringUnique()
	{
		return $"Wait {_wait} second(s)";
	}

	protected override string _GetTargetString()
	{
		return "";
	}
}
