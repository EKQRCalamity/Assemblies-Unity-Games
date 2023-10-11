using System.Collections.Generic;

public class DepartmentEqualityComparer : IEqualityComparer<Department>, IComparer<Department>
{
	public static DepartmentEqualityComparer Default = new DepartmentEqualityComparer();

	public bool Equals(Department x, Department y)
	{
		return x == y;
	}

	public int GetHashCode(Department obj)
	{
		return (int)obj;
	}

	public int Compare(Department x, Department y)
	{
		return x - y;
	}
}
