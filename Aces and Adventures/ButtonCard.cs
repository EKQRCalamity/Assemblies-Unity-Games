using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class ButtonCard : ATarget
{
	[ProtoContract(EnumPassthru = true)]
	public enum Pile
	{
		Inactive,
		Active
	}

	public static readonly HashSet<ButtonCardType> CancelButtons = new HashSet<ButtonCardType>
	{
		ButtonCardType.CancelAttack,
		ButtonCardType.CancelDefense,
		ButtonCardType.CancelAbility,
		ButtonCardType.ClearTargets,
		ButtonCardType.ClearActionHand,
		ButtonCardType.Cancel,
		ButtonCardType.Back,
		ButtonCardType.Skip,
		ButtonCardType.Finish
	};

	[ProtoMember(1)]
	private ButtonCardType _type;

	public ButtonCardType type => _type;

	public Pile pile => base.gameState.buttonDeck[this].GetValueOrDefault();

	public bool isActive => pile == Pile.Active;

	private ButtonCard()
	{
	}

	public ButtonCard(ButtonCardType type)
	{
		_type = type;
	}

	public static implicit operator ButtonCardType(ButtonCard card)
	{
		return card._type;
	}

	public static implicit operator ButtonCard(ButtonCardType type)
	{
		return new ButtonCard(type);
	}

	public static implicit operator bool(ButtonCard card)
	{
		return card.isActive;
	}
}
