using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class EncounterCard : AdventureTarget
{
	[ProtoMember(1, OverwriteList = true)]
	private List<AdventureCard.SelectInstruction> _onCompletedInstructions;

	public override AdventureCard.Pile pileToTransferToOnSelect => AdventureCard.Pile.ActiveHand;

	public override IEnumerable<GameStep> selectedGameSteps
	{
		get
		{
			yield return new GameStepEncounter();
		}
	}

	public List<AdventureCard.SelectInstruction> onCompletedInstructions => _onCompletedInstructions;

	public EncounterCard()
	{
	}

	public EncounterCard(AdventureCard.Encounter encounter)
	{
		_onCompletedInstructions = encounter.onCompletedInstructions;
	}
}
