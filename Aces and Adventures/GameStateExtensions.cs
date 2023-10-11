public static class GameStateExtensions
{
	public static bool IsRegistered(this IRegister register)
	{
		if (register == null)
		{
			return false;
		}
		return register.registerId > 0;
	}

	public static bool Register(this IRegister register)
	{
		return GameState.Instance?.Register(register) ?? false;
	}

	public static bool Unregister(this IRegister register)
	{
		return GameState.Instance?.Unregister(register) ?? false;
	}
}
