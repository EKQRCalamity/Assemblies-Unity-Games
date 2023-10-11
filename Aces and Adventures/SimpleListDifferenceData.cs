public struct SimpleListDifferenceData<T>
{
	public readonly T data;

	public readonly SimpleListDifferenceType type;

	public readonly int index;

	public SimpleListDifferenceData(T data, SimpleListDifferenceType type, int index)
	{
		this.data = data;
		this.type = type;
		this.index = index;
	}
}
