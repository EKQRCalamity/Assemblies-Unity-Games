public class AbstractLevelHUDComponent : AbstractMonoBehaviour
{
	protected bool _parentToHudCanvas;

	protected LevelHUDPlayer _hud { get; private set; }

	protected AbstractPlayerController _player => _hud.player;

	protected override void Awake()
	{
		base.Awake();
		ignoreGlobalTime = true;
		timeLayer = CupheadTime.Layer.UI;
	}

	private void Start()
	{
		if (_parentToHudCanvas)
		{
			base.transform.SetParent(LevelHUD.Current.Canvas.transform, worldPositionStays: false);
		}
	}

	public virtual void Init(LevelHUDPlayer hud)
	{
		_hud = hud;
	}
}
