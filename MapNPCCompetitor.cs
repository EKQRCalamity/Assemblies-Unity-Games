using UnityEngine;

public class MapNPCCompetitor : AbstractMonoBehaviour
{
	[SerializeField]
	private MapDialogueInteraction interaction;

	[SerializeField]
	private MinMax blinkRange = new MinMax(2.5f, 4.5f);

	private float blinkTimer;

	private void Update()
	{
		base.animator.SetBool("PlayerClose", interaction.PlayerWithinDistance(0) || (PlayerManager.Multiplayer && interaction.PlayerWithinDistance(1)) || interaction.currentlySpeaking);
		blinkTimer -= CupheadTime.Delta;
		if (blinkTimer < 0f)
		{
			blinkTimer = blinkRange.RandomFloat();
			base.animator.SetTrigger("Blink");
		}
	}
}
