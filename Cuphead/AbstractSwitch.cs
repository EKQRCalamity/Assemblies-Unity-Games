using System;

public abstract class AbstractSwitch : AbstractCollidableObject
{
	public event Action OnActivate;

	protected void DispatchEvent()
	{
		if (this.OnActivate != null)
		{
			this.OnActivate();
		}
	}
}
