public class MapEquipUI : AbstractEquipUI
{
	protected override bool CanPause
	{
		get
		{
			if (Map.Current.CurrentState != Map.State.Ready)
			{
				return false;
			}
			if (MapDifficultySelectStartUI.Current.CurrentState != 0)
			{
				return false;
			}
			if (MapConfirmStartUI.Current.CurrentState != 0)
			{
				return false;
			}
			if (MapBasicStartUI.Current.CurrentState != 0)
			{
				return false;
			}
			if (Map.Current != null && Map.Current.CurrentState == Map.State.Graveyard)
			{
				return false;
			}
			return true;
		}
	}

	protected override void OnPause()
	{
		base.OnPause();
		CupheadMapCamera.Current.StartBlur();
	}

	protected override void OnUnpause()
	{
		base.OnUnpause();
		CupheadMapCamera.Current.EndBlur();
	}

	protected override void OnPauseAudio()
	{
		AudioManager.HandleSnapshot(AudioManager.Snapshots.EquipMenu.ToString(), 0.15f);
		AudioManager.PauseAllSFX();
	}

	protected override void OnUnpauseAudio()
	{
		AudioManager.SnapshotReset(SceneLoader.SceneName, 0.1f);
	}
}
