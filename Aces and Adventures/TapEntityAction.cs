using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField("Tap Entity", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class TapEntityAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField]
	[DefaultValue(true)]
	private bool _tap = true;

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		yield return AbilityKeyword.Tap;
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		if (_tap)
		{
			combatant.Tap(context.actor);
		}
		else
		{
			combatant.Untap();
		}
	}

	protected override string _ToStringUnique()
	{
		if (!_tap)
		{
			return "Untap";
		}
		return "Tap";
	}
}
