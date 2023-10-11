using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField("Heal", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant", order = 2u)]
public class HealAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIDeepValueChange]
	private DynamicNumber _healAmount;

	[ProtoMember(2)]
	[UIField(tooltip = "Can bring combatant back from 0 HP or lower.")]
	[DefaultValue(true)]
	private bool _canRevive = true;

	public static void Heal(AAction action, ActionContext context, int healAmount, bool allowHealingOverMaxHP = false)
	{
		context.gameState.Heal(context, context.gameState.ProcessHealAmount(context, action, healAmount, out var actorOverride), action, allowHealingOverMaxHP, actorOverride);
	}

	public override bool ShouldTick(ActionContext context)
	{
		if (base.ShouldTick(context))
		{
			AEntity actorOverride;
			if (_healAmount.GetValue(context) <= 0 || !(context.target is ACombatant aCombatant) || aCombatant.HPMissing <= 0 || (int)aCombatant.stats.health <= 0 || (!_canRevive && (int)aCombatant.HP < 1))
			{
				return context.gameState.ProcessHealAmount(context, this, _healAmount.GetValue(context), out actorOverride, suppressReactions: true) < 0;
			}
			return true;
		}
		return false;
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		Heal(this, context, _healAmount.GetValue(context));
	}

	protected override string _ToStringUnique()
	{
		return "Heal";
	}

	protected override string _ToStringAfterTarget()
	{
		return $" for {_healAmount}" + _canRevive.ToText("", " (!Revive)".SizeIfNotEmpty());
	}
}
