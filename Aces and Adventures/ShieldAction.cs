using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField("Shield", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class ShieldAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIDeepValueChange]
	private DynamicNumber _shieldAmount;

	public override bool ShouldTick(ActionContext context)
	{
		if (base.ShouldTick(context))
		{
			return _shieldAmount.GetValue(context) > 0;
		}
		return false;
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		int num = _shieldAmount.GetValue(context);
		if (num < 0)
		{
			num = Math.Max(num, -(int)combatant.shield);
		}
		combatant.shield.value += num;
		if (num > 0)
		{
			context.gameState.SignalShieldGain(context, this, num);
		}
		else if (num < 0)
		{
			context.gameState.SignalShieldDamageDealt(context, this, -num, DamageSource.Ability);
		}
	}

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		yield return AbilityKeyword.Shields;
	}

	protected override string _ToStringUnique()
	{
		return $"+{_shieldAmount} Shield to";
	}
}
