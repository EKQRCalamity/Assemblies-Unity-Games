public class CupheadShopCamera : AbstractCupheadGameCamera
{
	public static CupheadShopCamera Current { get; private set; }

	public override float OrthographicSize => 360f;

	protected override void Awake()
	{
		base.Awake();
		Current = this;
	}

	private void OnDestroy()
	{
		if (Current == this)
		{
			Current = null;
		}
	}
}
