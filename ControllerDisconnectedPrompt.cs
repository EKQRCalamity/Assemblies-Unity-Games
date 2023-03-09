using UnityEngine;
using UnityEngine.UI;

public class ControllerDisconnectedPrompt : InterruptingPrompt
{
	public static ControllerDisconnectedPrompt Instance;

	public PlayerId currentPlayer;

	public bool allowedToShow;

	[SerializeField]
	private Text playerText;

	[SerializeField]
	private LocalizationHelper localizationHelper;

	protected override void Awake()
	{
		base.Awake();
		Instance = this;
	}

	public void Show(PlayerId player)
	{
		currentPlayer = player;
		localizationHelper.currentID = Localization.Find((player != 0) ? "XboxPlayer2" : "XboxPlayer1").id;
		PlayerManager.OnDisconnectPromptDisplayed(player);
		Show();
	}

	private void Update()
	{
		if (base.Visible && !PlayerManager.IsControllerDisconnected(currentPlayer))
		{
			FrameDelayedCallback(base.Dismiss, 2);
		}
	}

	private void OnDestroy()
	{
		Instance = null;
	}
}
