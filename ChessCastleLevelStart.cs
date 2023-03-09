public class ChessCastleLevelStart : AbstractLevelInteractiveEntity
{
	private bool activated;

	protected override void Activate()
	{
		if (!activated)
		{
			activated = true;
			base.Activate();
			((ChessCastleLevel)Level.Current).StartChessLevel();
		}
	}

	protected override void Show(PlayerId playerId)
	{
		base.state = State.Ready;
		dialogue = LevelUIInteractionDialogue.Create(dialogueProperties, PlayerManager.GetPlayer(playerId).input, dialogueOffset, 0f, LevelUIInteractionDialogue.TailPosition.Bottom, playerTarget: false);
	}
}
