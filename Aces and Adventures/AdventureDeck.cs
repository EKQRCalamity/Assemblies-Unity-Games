using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class AdventureDeck : ADeck
{
	[ProtoMember(1)]
	private DataRef<AdventureData> _adventureDataRef;

	public DataRef<AdventureData> adventureDataRef => _adventureDataRef;

	public AdventureDeckView adventureDeckView => base.view as AdventureDeckView;

	public override ContentRef contentRef => _adventureDataRef;

	public override ResourceBlueprint<GameObject> blueprint => AdventureDeckView.Blueprint;

	public override int count => _adventureDataRef.data.count;

	private bool _adventureDataRefSpecified => _adventureDataRef.ShouldSerialize();

	public AdventureDeck(DataRef<AdventureData> adventureDataRef)
	{
		_adventureDataRef = adventureDataRef;
	}

	public override IEnumerable<ATarget> GenerateCards()
	{
		return _adventureDataRef.data.GenerateCards(base.gameState);
	}
}
