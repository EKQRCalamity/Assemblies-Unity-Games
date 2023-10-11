using System;

[Flags]
public enum CanActivateOn
{
	OwnerTurn = 1,
	OwnerPrepareAttack = 2,
	OwnerPrepareDefense = 4,
	OwnerReaction = 8
}
