using ProtoBuf;

[ProtoContract]
[UIField("Capture Value (Combatant)", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows taking value from a Dynamic Number and setting it as Action Context's captured event value.", category = "Combatant")]
public class CaptureValueAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField]
	[UIDeepValueChange]
	private DynamicNumber _value;

	public override bool hasEffectOnTarget => false;

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		int value = _value.GetValue(context);
		foreach (GameStep nextStep in context.gameState.stack.activeStep.GetNextSteps(GameStep.GroupType.Context))
		{
			if (nextStep is GameStepAAction gameStepAAction)
			{
				gameStepAAction.context = gameStepAAction.context.SetCapturedValue(value);
			}
		}
	}

	protected override string _ToStringUnique()
	{
		return $"Capture Value {_value}";
	}

	public int GetCapturedValue(ActionContext context)
	{
		return _value.GetValue(context);
	}
}
