using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ProtoBuf;
using ProtoBuf.Meta;

[ProtoContract(IgnoreListHandling = true)]
public abstract class Ids
{
	public const ushort NULL_ID = 0;

	public static readonly Dictionary<Type, List<Ids>> Tables = new Dictionary<Type, List<Ids>>();

	public static readonly Dictionary<Type, Ref<ushort>> MostRecentTableIds = new Dictionary<Type, Ref<ushort>>();

	public abstract ushort tableId { get; }

	public abstract Array idMap { get; }

	protected abstract List<Ids> tableMap { get; }

	protected abstract Type idType { get; }

	protected static void RegisterTable(Ids table)
	{
		Type key = table.idType;
		if (!Tables.ContainsKey(key))
		{
			Tables.Add(key, table.tableMap);
			MostRecentTableIds.Add(key, new Ref<ushort>(table.tableId));
		}
		else
		{
			MostRecentTableIds[key].value = table.tableId;
		}
	}

	public static void ReleaseAllTables()
	{
		using PoolKeepItemListHandle<Ids> poolKeepItemListHandle = Pools.UseKeepItemList(Tables.Values.SelectMany((List<Ids> idsList) => idsList));
		foreach (Ids item in poolKeepItemListHandle.value)
		{
			item.SignalBeforeRelease();
		}
		foreach (Ids item2 in poolKeepItemListHandle.value)
		{
			item2.Release();
		}
	}

	public abstract void SignalBeforeRelease();

	public abstract void Release();

	public abstract Idable Add(Idable idable);

	public abstract void Remove(ushort id, ushort tableId);
}
[ProtoContract(IgnoreListHandling = true)]
public class Ids<T> : Ids where T : class, Idable<T>
{
	private static readonly int DEFAULT_CAPACITY;

	private static ushort _ActiveTableCount;

	private static List<Ids> _TableMap;

	private static int _MostRecentTableId;

	public static bool TriggerStaticConstructor;

	[ProtoMember(1, OverwriteList = true)]
	public T[] _IDMap;

	[ProtoMember(2)]
	private ushort _activeCount;

	[ProtoMember(3)]
	private int _lastId;

	private ushort _tableId;

	public Action<Id<T>> OnReleaseId;

	public Action<Ids<T>> OnBeforeReleaseTable;

	public Action<Ids<T>> OnReleaseTable;

	public int count => _activeCount;

	public override ushort tableId => _tableId;

	public override Array idMap => _IDMap;

	protected override List<Ids> tableMap => _TableMap;

	protected override Type idType => typeof(T);

	public T this[Id<T> id] => _IDMap[id.id];

	public Id<T> this[ushort id] => _IDMap[id];

	public Id<T> this[T idable]
	{
		get
		{
			ushort id = idable.id;
			if (id != 0)
			{
				return new Id<T>(id, _tableId);
			}
			while (_activeCount + 1 >= _IDMap.Length)
			{
				Array.Resize(ref _IDMap, _IDMap.Length + _IDMap.Length);
			}
			ushort num;
			do
			{
				Interlocked.Increment(ref _lastId);
				num = (ushort)(_lastId % _IDMap.Length);
			}
			while (_IDMap[num] != null || num == 0);
			idable.id = num;
			idable.tableId = _tableId;
			_IDMap[num] = idable;
			_activeCount++;
			return new Id<T>(num, _tableId);
		}
	}

	private static ushort _GetNextAvailableTableId()
	{
		lock (_TableMap)
		{
			while (_ActiveTableCount + 1 >= _TableMap.Count)
			{
				_TableMap.Add(null);
			}
			while (_TableMap[_MostRecentTableId] != null || _MostRecentTableId == 0)
			{
				_MostRecentTableId++;
				_MostRecentTableId %= _TableMap.Count;
			}
		}
		return (ushort)_MostRecentTableId;
	}

	protected static void ReleaseId(ushort id, ushort tableId, bool callClearMethodOnIdableObject = true)
	{
		Ids<T> ids = _TableMap[tableId] as Ids<T>;
		ids._IDMap[id].ClearId(tableId);
		if (callClearMethodOnIdableObject)
		{
			ClearMethodCache<T>.Clear(ids._IDMap[id]);
		}
		ids._IDMap[id] = null;
		ids._activeCount--;
		ids.OnReleaseId?.Invoke(new Id<T>(id, tableId));
	}

	static Ids()
	{
		DEFAULT_CAPACITY = 2;
		_TableMap = new List<Ids>();
		_MostRecentTableId = 1;
		RuntimeTypeModel.Default.Add(typeof(Ids<T>), applyDefaultBehaviour: true)[1].SupportNull = true;
		CreatePool();
	}

	public static void CreatePool()
	{
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new Ids<T>(), delegate(Ids<T> ids)
		{
			ids._Clear();
		}, delegate(Ids<T> ids)
		{
			ids._OnUnpool();
		});
	}

	public Ids()
	{
		_RegisterTable();
		_IDMap = new T[DEFAULT_CAPACITY];
	}

	public Id<T> Add(T itemToAdd)
	{
		return this[itemToAdd];
	}

	public T Replace(T item, Id<T> idToReplace)
	{
		if (!idToReplace && item.id == 0)
		{
			_activeCount++;
		}
		item.id = idToReplace;
		item.tableId = _tableId;
		return _IDMap[idToReplace.id] = item;
	}

	public Id<T> Transfer(T itemToTransfer)
	{
		if (itemToTransfer == null || itemToTransfer.tableId == tableId)
		{
			return itemToTransfer;
		}
		int lastId = _lastId;
		_lastId = itemToTransfer.id - 1;
		while (_IDMap.Length <= itemToTransfer.id)
		{
			Array.Resize(ref _IDMap, _IDMap.Length + _IDMap.Length);
		}
		ReleaseId(itemToTransfer.id, itemToTransfer.tableId, callClearMethodOnIdableObject: false);
		Add(itemToTransfer);
		_lastId = lastId;
		return itemToTransfer;
	}

	public S Create<S>() where S : class, Idable<T>, T
	{
		S val = OnUnpoolMethodCache<S>.OnUnpool(ConstructorCache<S>.Constructor());
		Add((T)val);
		return val;
	}

	public S CreateReplace<S>(Id<T> idToReplace) where S : class, Idable<T>, T
	{
		if (idToReplace.id != 0)
		{
			return Replace((T)OnUnpoolMethodCache<S>.OnUnpool(ConstructorCache<S>.Constructor()), idToReplace) as S;
		}
		return Create<S>();
	}

	public void Remove(Id<T> id)
	{
		if ((bool)id)
		{
			ReleaseId(id.id, id.tableId);
		}
	}

	public override void SignalBeforeRelease()
	{
		OnBeforeReleaseTable?.Invoke(this);
		OnBeforeReleaseTable = null;
	}

	public override void Release()
	{
		_ReleaseTable();
	}

	public override Idable Add(Idable idable)
	{
		Add((T)idable);
		return idable;
	}

	public override void Remove(ushort id, ushort tableIndex)
	{
		Remove(new Id<T>(id, tableIndex));
	}

	public PoolKeepItemListHandle<T> Values()
	{
		PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList<T>();
		T[] iDMap = _IDMap;
		foreach (T val in iDMap)
		{
			if (val != null)
			{
				poolKeepItemListHandle.Add(val);
			}
		}
		return poolKeepItemListHandle;
	}

	public PoolKeepItemListHandle<S> Values<S>() where S : T
	{
		PoolKeepItemListHandle<S> poolKeepItemListHandle = Pools.UseKeepItemList<S>();
		T[] iDMap = _IDMap;
		foreach (T val in iDMap)
		{
			if (val is S)
			{
				S value = (S)val;
				poolKeepItemListHandle.Add(value);
			}
		}
		return poolKeepItemListHandle;
	}

	public PoolKeepItemListHandle<Id<T>> ActiveIds()
	{
		PoolKeepItemListHandle<Id<T>> poolKeepItemListHandle = Pools.UseKeepItemList<Id<T>>();
		T[] iDMap = _IDMap;
		foreach (T val in iDMap)
		{
			if (val != null)
			{
				poolKeepItemListHandle.Add(val);
			}
		}
		return poolKeepItemListHandle;
	}

	public bool Contains(ushort id)
	{
		if (id < _IDMap.Length)
		{
			return _IDMap[id] != null;
		}
		return false;
	}

	private void _RegisterTable()
	{
		if (_tableId == 0)
		{
			_tableId = _GetNextAvailableTableId();
			_TableMap[_tableId] = this;
			_ActiveTableCount++;
			Ids.RegisterTable(this);
		}
	}

	private void _ReleaseTable()
	{
		if (_tableId == 0)
		{
			return;
		}
		SignalBeforeRelease();
		foreach (Id<T> item in ActiveIds())
		{
			Remove(item);
		}
		_TableMap[_tableId] = null;
		_ActiveTableCount--;
		OnReleaseTable?.Invoke(this);
		_tableId = 0;
		_lastId = 0;
		OnReleaseId = null;
		OnReleaseTable = null;
	}

	private void _OnUnpool()
	{
		_RegisterTable();
	}

	private void _Clear()
	{
		Release();
	}

	public override string ToString()
	{
		return $"{typeof(T).FriendlyName()} Table {_tableId}: Count = {count}";
	}

	[ProtoAfterDeserialization]
	private void ProtoAfterDeserialization()
	{
		_RegisterTable();
		for (int i = 0; i < _IDMap.Length; i++)
		{
			if (_IDMap[i] != null)
			{
				_IDMap[i].tableId = _tableId;
			}
		}
	}
}
