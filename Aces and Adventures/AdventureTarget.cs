using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[ProtoInclude(10, typeof(StoryCard))]
[ProtoInclude(11, typeof(EncounterCard))]
[ProtoInclude(12, typeof(ProceduralCard))]
public abstract class AdventureTarget : ATarget, IAdventureCard
{
	[ProtoMember(1)]
	public AdventureCard.Common adventureCardCommon { get; set; }

	public ATarget adventureCard => this;

	public virtual AdventureCard.Pile pileToTransferToOnDraw => AdventureCard.Pile.SelectionHand;

	public virtual AdventureCard.Pile pileToTransferToOnSelect => AdventureCard.Pile.Discard;

	public virtual GameStep selectTransferStep => base.gameState.adventureDeck.TransferCardStep(this, pileToTransferToOnSelect);

	public virtual string name => adventureCardCommon.name;

	public virtual string description => adventureCardCommon.description;

	public virtual CroppedImageRef image => adventureCardCommon.image;

	public virtual IEnumerable<GameStep> selectedGameSteps
	{
		get
		{
			yield break;
		}
	}

	public virtual ResourceBlueprint<GameObject> blueprint => AdventureTargetView.Blueprint;
}
