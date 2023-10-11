using System.Collections.Generic;

public class GameStepGroupSetupEnvironmentCosmetics : GameStepGroup
{
	public bool lighting = true;

	public bool music = true;

	public bool ambient = true;

	protected override IEnumerable<GameStep> _GetSteps()
	{
		GameData selectedGame = ProfileManager.prefs.selectedGame.data;
		if (lighting)
		{
			yield return new GameStepLighting(selectedGame.environmentLighting.data);
		}
		if (ambient)
		{
			yield return new GameStepAmbient(MusicPlayType.Resume, selectedGame.environmentAmbient, selectedGame.environmentAmbientVolume);
		}
		if (music)
		{
			yield return new GameStepMusic(MusicPlayType.Resume, selectedGame.environmentMusic, selectedGame.environmentMusicVolume);
		}
	}
}
