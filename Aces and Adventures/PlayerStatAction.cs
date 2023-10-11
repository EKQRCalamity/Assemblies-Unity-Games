using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField("Player Statistic", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Player")]
public class PlayerStatAction : APlayerAction
{
	[ProtoMember(1)]
	[UIField]
	private PlayerStatType _stat;

	[ProtoMember(2)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIDeepValueChange]
	private DynamicNumber _amount;

	protected override bool _canTick => false;

	protected override IEnumerable<DynamicNumber> _appliedDynamicNumbers
	{
		get
		{
			yield return _amount;
		}
	}

	protected override void _Apply(ActionContext context, Player player)
	{
		player.playerStats[_stat].value += _amount.GetValue(context);
	}

	protected override void _Unapply(ActionContext context, Player player)
	{
		player.playerStats[_stat].value -= _amount.GetValue(context, refreshValue: false);
	}

	protected override string _ToStringUnique()
	{
		return $"{_amount} {EnumUtil.FriendlyName(_stat)} for";
	}
}
