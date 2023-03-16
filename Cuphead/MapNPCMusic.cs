using UnityEngine;

public class MapNPCMusic : MonoBehaviour
{
	public enum MusicType
	{
		Regular,
		Minimalist
	}

	[SerializeField]
	private MusicType musicType;

	private void Start()
	{
		Dialoguer.events.onMessageEvent += OnDialoguerMessageEvent;
	}

	private void OnDestroy()
	{
		Dialoguer.events.onMessageEvent -= OnDialoguerMessageEvent;
	}

	private void OnDialoguerMessageEvent(string message, string metadata)
	{
		if (message == "MinimalistMusic" && musicType == MusicType.Minimalist)
		{
			PlayerData.Data.pianoAudioEnabled = true;
			PlayerData.SaveCurrentFile();
			Map.Current.OnNPCChangeMusic();
		}
		else if (message == "RegularMusic" && musicType == MusicType.Regular)
		{
			PlayerData.Data.pianoAudioEnabled = false;
			PlayerData.SaveCurrentFile();
			Map.Current.OnNPCChangeMusic();
		}
	}
}
