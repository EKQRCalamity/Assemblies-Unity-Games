using ProtoBuf;

[ProtoContract]
public class CardPack : ATarget
{
	[ProtoMember(1)]
	private IdDeck<AppliedPile, Ability> _abilities;

	public IdDeck<AppliedPile, Ability> abilities => _abilities ?? (_abilities = new IdDeck<AppliedPile, Ability>());

	public CardPackView packView => base.view as CardPackView;
}
