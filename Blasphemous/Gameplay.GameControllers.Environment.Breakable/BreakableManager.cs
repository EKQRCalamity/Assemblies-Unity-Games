using System.Collections.Generic;

namespace Gameplay.GameControllers.Environment.Breakable;

public class BreakableManager
{
	private List<int> breakablesId = new List<int>();

	public void AddBreakable(int breakableObjectId)
	{
		if (!breakablesId.Contains(breakableObjectId))
		{
			breakablesId.Add(breakableObjectId);
		}
	}

	public void RemoveBreakable(int breakableObjectId)
	{
		if (breakablesId.Contains(breakableObjectId))
		{
			breakablesId.Remove(breakableObjectId);
		}
	}

	public bool ContainsBreakable(int breakableId)
	{
		return breakablesId.Contains(breakableId);
	}

	public void Reset()
	{
		breakablesId.Clear();
	}
}
