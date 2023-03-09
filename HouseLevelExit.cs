using System.Collections;
using UnityEngine;

public class HouseLevelExit : AbstractLevelInteractiveEntity
{
	private bool activated;

	[SerializeField]
	private GameObject cupheadRunning;

	[SerializeField]
	private GameObject mugmanRunning;

	[SerializeField]
	private GameObject chaliceRunning;

	[SerializeField]
	private Scenes sceneLoadOnExit;

	private LevelPlayerController nonActivating;

	protected override void Activate()
	{
		if (!activated)
		{
			base.playerActivating.transform.position += ((LevelPlayerController)base.playerActivating).motor.DistanceToGround() * Vector3.down;
			nonActivating = null;
			SwitchToRun(base.playerActivating.id);
			if (PlayerManager.Multiplayer)
			{
				nonActivating = (LevelPlayerController)PlayerManager.GetPlayer(1 - base.playerActivating.id);
			}
			StartCoroutine(go_cr());
		}
	}

	private void SwitchToRun(PlayerId id)
	{
		GameObject gameObject = ((id == PlayerId.PlayerOne) ? ((!PlayerManager.player1IsMugman) ? cupheadRunning : mugmanRunning) : ((!PlayerManager.player1IsMugman) ? mugmanRunning : cupheadRunning));
		AbstractPlayerController player = PlayerManager.GetPlayer(id);
		if (player.stats.isChalice)
		{
			gameObject = chaliceRunning;
		}
		player.gameObject.SetActive(value: false);
		gameObject.transform.position = player.transform.position;
		gameObject.gameObject.SetActive(value: true);
		((LevelPlayerController)player).DisableInput();
		((LevelPlayerController)player).PauseAll();
	}

	private IEnumerator go_cr()
	{
		activated = true;
		float timeToGround = 0f;
		if (nonActivating != null)
		{
			while (nonActivating != null && nonActivating.gameObject.activeInHierarchy && !nonActivating.motor.Grounded)
			{
				timeToGround += (float)CupheadTime.Delta;
				yield return null;
			}
			if (nonActivating.gameObject.activeInHierarchy && nonActivating != null)
			{
				SwitchToRun(nonActivating.id);
			}
		}
		if (timeToGround < 1f)
		{
			yield return CupheadTime.WaitForSeconds(this, 1f - timeToGround);
		}
		SceneLoader.LoadScene(sceneLoadOnExit, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
	}

	protected override void Show(PlayerId playerId)
	{
		base.state = State.Ready;
		dialogue = LevelUIInteractionDialogue.Create(dialogueProperties, PlayerManager.GetPlayer(playerId).input, dialogueOffset, 0f, LevelUIInteractionDialogue.TailPosition.Left, playerTarget: false);
	}
}
