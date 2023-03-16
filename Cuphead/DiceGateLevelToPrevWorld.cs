using System.Collections;

public class DiceGateLevelToPrevWorld : AbstractLevelInteractiveEntity
{
	private bool activated;

	protected override void Activate()
	{
		if (!activated)
		{
			base.Activate();
			StartCoroutine(go_cr());
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		CupheadTime.SetLayerSpeed(CupheadTime.Layer.Player, 1f);
	}

	private IEnumerator go_cr()
	{
		activated = true;
		CupheadTime.SetLayerSpeed(CupheadTime.Layer.Player, 0f);
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (!(allPlayer == null))
			{
				allPlayer.DisableInput();
				allPlayer.PauseAll();
			}
		}
		PlayerData.Data.CurrentMapData.hasVisitedDieHouse = true;
		yield return CupheadTime.WaitForSeconds(this, 1f);
		SceneLoader.LoadLastMap();
	}

	protected override void Show(PlayerId playerId)
	{
		base.state = State.Ready;
		dialogue = LevelUIInteractionDialogue.Create(dialogueProperties, PlayerManager.GetPlayer(playerId).input, dialogueOffset, 0f, LevelUIInteractionDialogue.TailPosition.Left, playerTarget: false);
	}
}
