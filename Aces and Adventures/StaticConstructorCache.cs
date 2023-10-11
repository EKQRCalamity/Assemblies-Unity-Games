using System.Runtime.CompilerServices;

public static class StaticConstructorCache<T>
{
	public static void Run()
	{
	}

	static StaticConstructorCache()
	{
		RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
	}
}
