public class GameStepMusic : GameStep
{
	public MusicPlayType command;

	public DataRef<MusicData> music;

	public float volume;

	public GameStepMusic(MusicPlayType command, DataRef<MusicData> music, float volume = 1f)
	{
		this.command = command;
		this.music = music;
		this.volume = volume;
	}

	public override void Start()
	{
		if (command == MusicPlayType.Stop)
		{
			MusicManager.Instance.Stop();
		}
		else if ((bool)music)
		{
			MusicManager.Instance.Play(music.data, command, volume);
		}
	}
}
