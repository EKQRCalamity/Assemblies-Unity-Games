public class LevelResources : AbstractMonoBehaviour
{
	public const string EDITOR_PATH = "Assets/_CUPHEAD/Prefabs/LevelResources/Level_Resources.prefab";

	public LevelHUD levelHUD;

	public LevelGUI levelGUI;

	public LevelAudio levelAudio;

	public Effect levelBossDeathExplosion;

	public LevelPlayerController levelPlayer;

	public PlanePlayerController planePlayer;

	public PlayerJoinEffect joinEffect;

	public PlatformingLevelIntroAnimation platformingIntro;

	public PlatformingLevelWinAnimation platformingWin;

	public LevelIntroAnimation levelIntro;

	public LevelKOAnimation levelKO;

	public LevelUIInteractionDialogue levelUIInteractionDialogue;

	private void OnDestroy()
	{
		levelHUD = null;
		levelGUI = null;
		levelAudio = null;
		levelBossDeathExplosion = null;
		levelPlayer = null;
		planePlayer = null;
		joinEffect = null;
		platformingIntro = null;
		platformingWin = null;
		levelIntro = null;
		levelUIInteractionDialogue = null;
	}
}
