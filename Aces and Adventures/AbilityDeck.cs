using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class AbilityDeck : ADeck
{
	[ProtoMember(1)]
	private DataRef<AbilityDeckData> _deckRef;

	public DataRef<AbilityDeckData> deckRef => _deckRef;

	public PlayerClass characterClass => deckRef.data.characterClass;

	public AbilityDeckView abilityDeckView => base.view as AbilityDeckView;

	public override ContentRef contentRef => _deckRef;

	public override ResourceBlueprint<GameObject> blueprint => AbilityDeckView.Blueprint;

	public override int count => _deckRef.data.count;

	private bool _deckRefSpecified => _deckRef.ShouldSerialize();

	public AbilityDeck(DataRef<AbilityDeckData> deckRef)
	{
		_deckRef = deckRef;
	}

	public override IEnumerable<ATarget> GenerateCards()
	{
		foreach (DataRef<AbilityData> ability in _deckRef.data.abilities)
		{
			yield return new Ability(ability, base.gameState.player);
		}
	}
}
