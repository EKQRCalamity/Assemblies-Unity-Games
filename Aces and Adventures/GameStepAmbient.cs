public class GameStepAmbient : GameStep
{
	public MusicPlayType command;

	public AudioRef ambient;

	public float volume;

	public GameStepAmbient(MusicPlayType command, AudioRef ambient, float volume = 1f)
	{
		this.command = command;
		this.ambient = ambient;
		this.volume = volume;
	}

	public override void Start()
	{
		if (command == MusicPlayType.Stop)
		{
			AmbientManager.Instance.Stop();
		}
		else if ((bool)ambient)
		{
			AmbientManager.Instance.Play(ambient.audioClip, command, volume);
		}
	}
}
