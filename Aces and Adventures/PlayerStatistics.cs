using System;
using ProtoBuf;

[ProtoContract]
public class PlayerStatistics
{
	[ProtoMember(1)]
	private BInt _resourceHandSize;

	[ProtoMember(2)]
	private BInt _abilityHandSize;

	[ProtoMember(3)]
	private BInt _abilityDrawCount;

	[ProtoMember(4)]
	private BInt _numberOfHeroAbilities;

	[ProtoMember(5)]
	private BInt _scryPlayingCards;

	[ProtoMember(6)]
	private BInt _scryAbilityCards;

	[ProtoMember(7)]
	private BInt _scryEnemyPlayingCards;

	public BInt resourceHandSize => _resourceHandSize ?? (_resourceHandSize = new BInt());

	public BInt abilityHandSize => _abilityHandSize ?? (_abilityHandSize = new BInt());

	public BInt abilityDrawCount => _abilityDrawCount ?? (_abilityDrawCount = new BInt());

	public BInt numberOfHeroAbilities => _numberOfHeroAbilities ?? (_numberOfHeroAbilities = new BInt());

	public BInt scryPlayingCards => _scryPlayingCards ?? (_scryPlayingCards = new BInt());

	public BInt scryAbilityCards => _scryAbilityCards ?? (_scryAbilityCards = new BInt());

	public BInt scryEnemyPlayingCards => _scryEnemyPlayingCards ?? (_scryEnemyPlayingCards = new BInt());

	public BInt this[PlayerStatType stat] => stat switch
	{
		PlayerStatType.ResourceHandSize => resourceHandSize, 
		PlayerStatType.AbilityHandSize => abilityHandSize, 
		PlayerStatType.AbilityDrawCount => abilityDrawCount, 
		PlayerStatType.NumberOfHeroAbilities => numberOfHeroAbilities, 
		PlayerStatType.ScryPlayingCards => scryPlayingCards, 
		PlayerStatType.ScryAbilityCards => scryAbilityCards, 
		PlayerStatType.ScryEnemyPlayingCards => scryEnemyPlayingCards, 
		_ => throw new ArgumentOutOfRangeException("stat", stat, null), 
	};

	public PlayerStatistics()
	{
	}

	public PlayerStatistics(int resourceHandSize, int abilityHandSize, int abilityDrawCount, int numberOfHeroAbilities)
	{
		this.resourceHandSize.value = resourceHandSize;
		this.abilityHandSize.value = abilityHandSize;
		this.abilityDrawCount.value = abilityDrawCount;
		this.numberOfHeroAbilities.value = numberOfHeroAbilities;
	}
}
