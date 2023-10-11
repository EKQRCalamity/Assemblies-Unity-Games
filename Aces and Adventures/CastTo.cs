public static class CastTo<T>
{
	public static T From<S>(S s)
	{
		return CachedCastToExpression<S, T>.cast(s);
	}
}
