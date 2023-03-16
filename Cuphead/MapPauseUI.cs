public class MapPauseUI : LevelPauseGUI
{
	protected override bool CanPause
	{
		get
		{
			if (base.state != State.Animating && MapDifficultySelectStartUI.Current.CurrentState == AbstractMapSceneStartUI.State.Inactive && MapConfirmStartUI.Current.CurrentState == AbstractMapSceneStartUI.State.Inactive && MapBasicStartUI.Current.CurrentState == AbstractMapSceneStartUI.State.Inactive && (SpeechBubble.Instance == null || (SpeechBubble.Instance != null && SpeechBubble.Instance.displayState != SpeechBubble.DisplayState.WaitForSelection)) && (!(Map.Current != null) || Map.Current.CurrentState != Map.State.Graveyard))
			{
				return true;
			}
			return false;
		}
	}
}
