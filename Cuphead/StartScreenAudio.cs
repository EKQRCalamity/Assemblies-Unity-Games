using Rewired;
using UnityEngine;

public class StartScreenAudio : AbstractMonoBehaviour
{
	[SerializeField]
	private AudioSource bgmAlt2;

	private static StartScreenAudio startScreenAudio;

	private CupheadButton[] code = new CupheadButton[10]
	{
		CupheadButton.MenuUp,
		CupheadButton.MenuUp,
		CupheadButton.MenuDown,
		CupheadButton.MenuDown,
		CupheadButton.MenuLeft,
		CupheadButton.MenuRight,
		CupheadButton.MenuLeft,
		CupheadButton.MenuRight,
		CupheadButton.Cancel,
		CupheadButton.Accept
	};

	private int codeIndex;

	private Player[] players;

	private bool blockInput;

	public static StartScreenAudio Instance => startScreenAudio;

	private void Start()
	{
		blockInput = CreditsScreen.goodEnding;
		players = new Player[2]
		{
			PlayerManager.GetPlayerInput(PlayerId.PlayerOne),
			PlayerManager.GetPlayerInput(PlayerId.PlayerTwo)
		};
	}

	private void Update()
	{
		if (blockInput)
		{
			return;
		}
		if (codeIndex < code.Length)
		{
			Player[] array = players;
			foreach (Player player in array)
			{
				if (player.GetAnyButtonDown())
				{
					if (player.GetButtonDown((int)code[codeIndex]))
					{
						codeIndex++;
					}
					else if (!player.GetButtonDown((int)code[codeIndex]))
					{
						codeIndex = 0;
					}
				}
			}
		}
		else
		{
			if (bgmAlt2.clip == null)
			{
				bgmAlt2.GetComponent<DeferredAudioSource>().Initialize();
			}
			AudioManager.StopBGM();
			bgmAlt2.Play();
			blockInput = true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		startScreenAudio = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
