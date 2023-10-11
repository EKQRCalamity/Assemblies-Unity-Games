using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField]
public class SpecificType
{
	[ProtoMember(1)]
	[UIField(validateOnChange = true)]
	[UIHorizontalLayout("Specific", expandHeight = false, flexibleWidth = 0f, minWidth = 200f)]
	private bool _specificType;

	[ProtoMember(2)]
	[UIField(validateOnChange = true)]
	[UIHideIf("_hideType")]
	[UIHorizontalLayout("Specific", flexibleWidth = 999f)]
	private Type _type;

	public Type type
	{
		get
		{
			if (!_specificType)
			{
				return null;
			}
			return _type;
		}
	}

	private bool _hideType => !_specificType;

	public IEnumerable<T> Filter<T>(IEnumerable<T> items)
	{
		if (!this)
		{
			foreach (T item in items)
			{
				yield return item;
			}
			yield break;
		}
		foreach (T item2 in items)
		{
			if (item2.GetType() == type)
			{
				yield return item2;
			}
		}
	}

	public void _PrepareForSave()
	{
		if (_hideType)
		{
			_type = null;
		}
	}

	public override string ToString()
	{
		if (!(type != null))
		{
			return "";
		}
		return type.GetUILabel();
	}

	public static implicit operator Type(SpecificType specificType)
	{
		return specificType?.type;
	}

	public static implicit operator bool(SpecificType specificType)
	{
		if (specificType != null)
		{
			return specificType.type != null;
		}
		return false;
	}

	public static implicit operator string(SpecificType specificType)
	{
		if (!specificType)
		{
			return "";
		}
		return specificType.type.GetUILabel();
	}
}
