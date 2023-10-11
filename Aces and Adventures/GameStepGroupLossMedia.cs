using System.Collections.Generic;

public class GameStepGroupLossMedia : GameStepGroup
{
	protected override IEnumerable<GameStep> _GetSteps()
	{
		if ((bool)base.gameState.game.data.lossMusic)
		{
			yield return new GameStepMusic(MusicPlayType.Play, base.gameState.game.data.lossMusic, base.gameState.game.data.lossMusicVolume);
		}
		if ((bool)base.gameState.game.data.lossLighting)
		{
			yield return GameStepLighting.Create(base.gameState.game.data.lossLighting.data);
		}
		if ((bool)base.gameState.game.data.lossAmbient)
		{
			yield return new GameStepAmbient(MusicPlayType.Resume, base.gameState.game.data.lossAmbient, base.gameState.game.data.lossAmbientVolume);
		}
	}
}
