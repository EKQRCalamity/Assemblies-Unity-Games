using System;
using System.Linq;

public static class DelegateExtensions
{
	public static void AddSingleFire(Ptr<Action> actionPtr, Action singleFireAction)
	{
		Action wrapperAction = null;
		wrapperAction = delegate
		{
			singleFireAction();
			Ptr<Action> ptr2 = actionPtr;
			ptr2.value = (Action)Delegate.Remove(ptr2.value, wrapperAction);
		};
		Ptr<Action> ptr = actionPtr;
		ptr.value = (Action)Delegate.Combine(ptr.value, wrapperAction);
	}

	public static bool All<T>(this Func<T, bool> func, T value)
	{
		return func.GetInvocationList().Cast<Func<T, bool>>().All((Func<T, bool> subFunc) => subFunc(value));
	}

	public static bool Any<T>(this Func<T, bool> func, T value)
	{
		return func.GetInvocationList().Cast<Func<T, bool>>().Any((Func<T, bool> subFunc) => subFunc(value));
	}
}
