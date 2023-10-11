using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ButtonCardType
{
	EndReaction,
	EndTurn,
	ConfirmAttack,
	CancelAttack,
	ConfirmDefense,
	CancelDefense,
	CancelAbility,
	ClearTargets,
	ClearActionHand,
	Cancel,
	Back,
	Skip,
	Finish
}
