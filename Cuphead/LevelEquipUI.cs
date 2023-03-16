public class LevelEquipUI : AbstractEquipUI
{
	private void Start()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Activate()
	{
		StartCoroutine(pause_cr());
	}

	protected override void Unpause()
	{
		LevelGameOverGUI.Current.ReactivateOnChangeEquipmentClosed();
		StartCoroutine(unpause_cr());
	}

	protected override void OnPauseSound()
	{
	}

	protected override void OnUnpauseSound()
	{
	}

	protected override void PauseGameplay()
	{
	}

	protected override void UnpauseGameplay()
	{
	}
}
