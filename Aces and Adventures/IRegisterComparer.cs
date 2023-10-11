using System.Collections.Generic;

public class IRegisterComparer : IComparer<IRegister>
{
	public static readonly IRegisterComparer Default = new IRegisterComparer();

	public int Compare(IRegister x, IRegister y)
	{
		return (x?.registerId ?? (-1)) - (y?.registerId ?? (-1));
	}
}
