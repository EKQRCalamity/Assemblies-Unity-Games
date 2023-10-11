using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class DataRefPref<C> : ADataRefPref where C : IDataContent
{
	[ProtoMember(1)]
	private DataRef<C> _dataRef;

	[ProtoMember(2)]
	private ADataRefPrefTag _tag;

	public DataRef<C> dataRef
	{
		get
		{
			if (!_dataRef)
			{
				return null;
			}
			return _dataRef;
		}
		set
		{
			_dataRef = value;
		}
	}

	public ADataRefPrefTag tag
	{
		get
		{
			return _tag;
		}
		set
		{
			_tag = value;
		}
	}

	public override ContentRef contentRef
	{
		get
		{
			return dataRef;
		}
		set
		{
			dataRef = value as DataRef<C>;
		}
	}

	public override Type dataType => typeof(C);

	private bool _dataRefSpecified => _dataRef.ShouldSerialize();

	public override IEnumerable<Type> RequestDataTypes()
	{
		if (tag == null)
		{
			tag = ADataRefPrefTag.GetTagForDataType(dataType);
		}
		if (tag == null)
		{
			yield break;
		}
		foreach (Type item in tag.RequestDataTypes())
		{
			yield return item;
		}
	}

	protected override IEnumerable<ContentRef> _GetReferences()
	{
		yield return dataRef;
		if (tag == null)
		{
			yield break;
		}
		foreach (ContentRef item in tag._GetReferences())
		{
			yield return item;
		}
	}

	protected override void _SetReference(ContentRef reference)
	{
		if (tag != null)
		{
			tag.SetReference(reference);
		}
	}

	public DataRefPref()
	{
	}

	public DataRefPref(DataRef<C> dataRef, ADataRefPrefTag tag = null)
	{
		_dataRef = dataRef;
		_tag = tag;
	}

	public T Tag<T>() where T : ADataRefPrefTag, new()
	{
		return (_tag as T) ?? ((T)(_tag = new T()));
	}

	public static implicit operator DataRef<C>(DataRefPref<C> dataRefPref)
	{
		return dataRefPref?.dataRef;
	}
}
