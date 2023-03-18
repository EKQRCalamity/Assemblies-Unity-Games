namespace Gameplay.GameControllers.Entities.StateMachine;

public class State
{
	public StateMachine Machine;

	public static implicit operator bool(State state)
	{
		return state != null;
	}

	public virtual void OnStateInitialize(StateMachine machine)
	{
		Machine = machine;
	}

	public virtual void OnStateEnter()
	{
	}

	public virtual void OnStateExit()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void LateUpdate()
	{
	}

	public virtual void Destroy()
	{
	}
}
