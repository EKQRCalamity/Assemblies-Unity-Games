using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[ProtoInclude(10, typeof(AdventureDeck))]
[ProtoInclude(11, typeof(AbilityDeck))]
public abstract class ADeck : ATarget
{
	public ADeckView deckView => base.view as ADeckView;

	public abstract ContentRef contentRef { get; }

	public abstract ResourceBlueprint<GameObject> blueprint { get; }

	public abstract int count { get; }

	public abstract IEnumerable<ATarget> GenerateCards();
}
