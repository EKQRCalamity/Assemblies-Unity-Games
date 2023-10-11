public class UICategorySet
{
	public static readonly UICategorySet Default = new UICategorySet(null, null);

	public static readonly UICategorySet[] Defaults = new UICategorySet[1] { Default };

	public string[] included;

	public string[] excluded;

	public UICategorySet(string[] included, string[] excluded)
	{
		this.included = included;
		this.excluded = excluded;
	}
}
