using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[ProtoInclude(26, typeof(DataRefPref<NodeGraphRef>))]
public abstract class ADataRefPref : IEquatable<ADataRefPref>
{
	public abstract ContentRef contentRef { get; set; }

	public abstract Type dataType { get; }

	public virtual IEnumerable<Type> RequestDataTypes()
	{
		yield break;
	}

	protected virtual IEnumerable<ContentRef> _GetReferences()
	{
		yield break;
	}

	protected virtual void _SetReference(ContentRef reference)
	{
	}

	public IEnumerable<ContentRef> GetReferences()
	{
		return from cRef in _GetReferences()
			where cRef
			select cRef;
	}

	public void SetReferences(List<ADataRefPref> breadCrumbs)
	{
		foreach (Type item in RequestDataTypes())
		{
			int num = breadCrumbs.Count - 1;
			while (num >= 0)
			{
				foreach (ContentRef reference in breadCrumbs[num].GetReferences())
				{
					if (reference.dataType == item)
					{
						_SetReference(reference);
						goto end_IL_006d;
					}
				}
				num--;
				continue;
				end_IL_006d:
				break;
			}
		}
	}

	public bool Equals(ADataRefPref other)
	{
		if (other != null)
		{
			return other.dataType == dataType;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ADataRefPref)
		{
			return Equals((ADataRefPref)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return dataType.GetHashCode();
	}
}
