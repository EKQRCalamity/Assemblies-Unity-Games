using System;

namespace Gameplay.GameControllers.Bosses.TresAngustias;

public class SingleAnguishAction
{
	public float preparationSeconds;

	public float recoverySeconds;

	public Action action;

	public SingleAnguishAction(Action a)
	{
		action = a;
	}
}
