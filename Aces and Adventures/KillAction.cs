using ProtoBuf;

[ProtoContract]
[UIField("Kill", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class KillAction : ACombatantAction
{
	public override bool dealsDamage => true;

	public static void Kill(ActionContext context, ACombatant combatant, AAction action = null)
	{
		if (combatant.HP.value > 0)
		{
			combatant.HP.value = 0;
			context.gameState.stack.Queue(new GameStepDeath(context, action));
		}
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		Kill(context, combatant, this);
	}

	protected override string _ToStringUnique()
	{
		return "Kill";
	}

	public override int GetPotentialDamage(ActionContext context)
	{
		ACombatant aCombatant = context.GetTarget<ACombatant>();
		if (aCombatant == null || (int)aCombatant.HP <= 0)
		{
			return 0;
		}
		return aCombatant.HP;
	}
}
