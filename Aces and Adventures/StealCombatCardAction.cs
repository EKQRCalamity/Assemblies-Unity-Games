using ProtoBuf;

[ProtoContract]
[UIField("Steal Combat Card", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Resource Card")]
public class StealCombatCardAction : AResourceAction
{
	protected override void _Tick(ActionContext context, ResourceCard resourceCard)
	{
	}

	protected override string _ToStringUnique()
	{
		return "Steal";
	}
}
