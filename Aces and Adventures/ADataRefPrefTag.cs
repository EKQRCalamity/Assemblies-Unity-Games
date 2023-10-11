using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract]
public abstract class ADataRefPrefTag
{
	private static Dictionary<Type, Type> _DataTypeToTagType;

	private static Dictionary<Type, Type> DataTypeToTagType => _DataTypeToTagType ?? (_DataTypeToTagType = typeof(ADataRefPrefTag).GetDerivedClasses().ToDictionary((Type t) => (ReflectionUtil.CreateInstanceSmart(t) as ADataRefPrefTag).dataType, (Type t) => t));

	public abstract Type dataType { get; }

	public static ADataRefPrefTag GetTagForDataType(Type dataType)
	{
		if (!DataTypeToTagType.ContainsKey(dataType))
		{
			return null;
		}
		return ReflectionUtil.CreateInstanceSmart(DataTypeToTagType[dataType]) as ADataRefPrefTag;
	}

	public virtual IEnumerable<ContentRef> _GetReferences()
	{
		yield break;
	}

	public virtual IEnumerable<Type> RequestDataTypes()
	{
		yield break;
	}

	public virtual void SetReference(ContentRef reference)
	{
	}

	public IEnumerable<ContentRef> GetReferences()
	{
		return from cRef in _GetReferences()
			where cRef
			select cRef;
	}
}
