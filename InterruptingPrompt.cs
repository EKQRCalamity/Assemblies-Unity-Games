public class InterruptingPrompt : AbstractMonoBehaviour
{
	private bool wasPausedBeforeInterrupt;

	public bool Visible => base.gameObject.activeSelf;

	public static void SetCanInterrupt(bool canInterrupt)
	{
		if (ControllerDisconnectedPrompt.Instance != null)
		{
			ControllerDisconnectedPrompt.Instance.allowedToShow = canInterrupt;
		}
	}

	public static bool IsInterrupting()
	{
		return ControllerDisconnectedPrompt.Instance != null && ControllerDisconnectedPrompt.Instance.Visible;
	}

	public static bool CanInterrupt()
	{
		if (ControllerDisconnectedPrompt.Instance != null)
		{
			return ControllerDisconnectedPrompt.Instance.allowedToShow;
		}
		return false;
	}

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(value: false);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		wasPausedBeforeInterrupt = PauseManager.state == PauseManager.State.Paused;
		if (!wasPausedBeforeInterrupt)
		{
			PauseManager.Pause();
		}
	}

	public void Dismiss()
	{
		base.gameObject.SetActive(value: false);
		if (!wasPausedBeforeInterrupt)
		{
			PauseManager.Unpause();
		}
	}
}
