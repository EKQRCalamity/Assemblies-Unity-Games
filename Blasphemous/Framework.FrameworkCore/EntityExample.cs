using System.Collections.Generic;
using Gameplay.GameControllers.Entities;

namespace Framework.FrameworkCore;

public class EntityExample
{
	public delegate void EntityFlagEvent(string key, bool active);

	private List<string> flags;

	public EntityStats Attributes { get; set; }

	public EntityStatus Status { get; set; }

	public event EntityFlagEvent OnFlagChanged;

	public void SetFlag(string flag, bool active)
	{
		if (active && !HasFlag(flag))
		{
			flags.Add(flag);
			if (this.OnFlagChanged != null)
			{
				this.OnFlagChanged(flag, active: true);
			}
		}
		else if (!active && HasFlag(flag))
		{
			flags.Remove(flag);
			if (this.OnFlagChanged != null)
			{
				this.OnFlagChanged(flag, active: false);
			}
		}
	}

	public bool HasFlag(string key)
	{
		return flags.Contains(key);
	}
}
