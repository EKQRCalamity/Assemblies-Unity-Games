using System;
using System.Collections.Generic;

public class AbilityDataRefNameComparer : IComparer<DataRef<AbilityData>>
{
	public static readonly AbilityDataRefNameComparer Default = new AbilityDataRefNameComparer();

	public int Compare(DataRef<AbilityData> x, DataRef<AbilityData> y)
	{
		return string.Compare(x?.friendlyName, y?.friendlyName, StringComparison.CurrentCultureIgnoreCase);
	}
}
