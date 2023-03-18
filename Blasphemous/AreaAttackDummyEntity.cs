using Gameplay.GameControllers.Entities;
using Tools.Level.Actionables;

public class AreaAttackDummyEntity : Entity
{
	private ShockwaveArea mArea;

	protected override void OnAwake()
	{
		base.OnAwake();
		mArea = GetComponent<ShockwaveArea>();
		if (mArea != null)
		{
			mArea.SetOwner(this);
		}
	}
}
