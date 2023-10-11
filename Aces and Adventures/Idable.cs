public interface Idable
{
	ushort id { get; set; }

	ushort tableId { get; set; }
}
public interface Idable<T> : Idable where T : class
{
}
