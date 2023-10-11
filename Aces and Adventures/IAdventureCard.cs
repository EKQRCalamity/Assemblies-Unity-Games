using System.Collections.Generic;
using UnityEngine;

public interface IAdventureCard
{
	ATarget adventureCard { get; }

	AdventureCard.Pile pileToTransferToOnDraw { get; }

	AdventureCard.Pile pileToTransferToOnSelect { get; }

	GameStep selectTransferStep { get; }

	AdventureCard.Common adventureCardCommon { get; set; }

	string name { get; }

	string description { get; }

	CroppedImageRef image { get; }

	IEnumerable<GameStep> selectedGameSteps { get; }

	ResourceBlueprint<GameObject> blueprint { get; }
}
