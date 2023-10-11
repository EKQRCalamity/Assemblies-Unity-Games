using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public struct Id<T> : IEquatable<Id<T>>, IComparable<Id<T>> where T : class, Idable
{
	public static readonly Id<T> Null = new Id<T>(0, 0);

	private static Type _TableType;

	private static List<Ids> _Tables;

	private static Ref<ushort> _RecentTableId;

	[ProtoMember(1)]
	public readonly ushort id;

	private ushort _tableId;

	private static Type TableType => _TableType ?? (_TableType = typeof(T).BaseTypeWhichImplementsInterface(typeof(Idable)));

	private static List<Ids> Tables => _Tables ?? (_Tables = Ids.Tables[TableType]);

	private static Ref<ushort> RecentTableId => _RecentTableId ?? (_RecentTableId = Ids.MostRecentTableIds[TableType]);

	public T value => _table?.idMap.GetValue(id) as T;

	public bool shouldSerialize => id != 0;

	public ushort tableId => _tableId;

	private Ids _table => Tables[tableId];

	public Id(ushort id, ushort tableId)
	{
		this.id = id;
		_tableId = tableId;
	}

	public void ReleaseId()
	{
		_table.Remove(id, _tableId);
	}

	public Id<T> SetTable(Ids table)
	{
		if (id == 0)
		{
			return Null;
		}
		return new Id<T>(id, table.tableId);
	}

	public V Get<V>() where V : class, T
	{
		return value as V;
	}

	public static implicit operator ushort(Id<T> i)
	{
		return i.id;
	}

	public static implicit operator Ushort2(Id<T> i)
	{
		return new Ushort2(i.id, i.tableId);
	}

	public static implicit operator T(Id<T> i)
	{
		return i.value;
	}

	public static implicit operator Id<T>(T value)
	{
		if (value == null)
		{
			return Null;
		}
		if (value.id == 0)
		{
			return Tables[(ushort)RecentTableId].Add(value).ToId<T>();
		}
		return new Id<T>(value.id, value.tableId);
	}

	public static implicit operator bool(Id<T> id)
	{
		Ids ids = ((id._tableId < Tables.Count) ? id._table : null);
		if (ids != null && id.id < ids.idMap.Length)
		{
			return id.value != null;
		}
		return false;
	}

	public static bool operator ==(Id<T> a, Id<T> b)
	{
		return a.id == b.id;
	}

	public static bool operator !=(Id<T> a, Id<T> b)
	{
		return a.id != b.id;
	}

	public int CompareTo(Id<T> other)
	{
		return Comparer<T>.Default.Compare(this, other);
	}

	public override bool Equals(object obj)
	{
		if (obj is Id<T> id)
		{
			return this.id == id.id;
		}
		return false;
	}

	public bool Equals(Id<T> other)
	{
		return id == other.id;
	}

	public override int GetHashCode()
	{
		return id;
	}

	public override string ToString()
	{
		if (!this)
		{
			return $"id: {id}, tableId: {tableId}";
		}
		return value.ToString();
	}

	[ProtoAfterDeserialization]
	private void ProtoAfterDeserialization()
	{
		_tableId = RecentTableId;
	}
}
