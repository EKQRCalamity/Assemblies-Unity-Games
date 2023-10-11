using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ProtoBuf.Meta;
using UnityEngine;

public static class Pools
{
	public abstract class PoolBase
	{
		public abstract int Count { get; }

		public abstract int Capacity { get; }

		public abstract void RepoolObject(object obj);

		public abstract void ClearObject(object obj);

		public abstract void Clear();
	}

	private class Pool<T> : PoolBase where T : class
	{
		public static Pool<T> Instance;

		private static Action<T> ClearMethod;

		private static Func<T> ConstructorMethod;

		private static Action<T> OnUnpoolMethod;

		private static object Lock = new object();

		private static bool HasSetAsProtoFactory;

		private HashSet<T> _Pool;

		private int _capacity;

		public override int Count
		{
			get
			{
				lock (Lock)
				{
					return _Pool.Count;
				}
			}
		}

		public override int Capacity
		{
			get
			{
				lock (Lock)
				{
					return _capacity;
				}
			}
		}

		public static void Initialize(bool initializeHierarchy, bool setAsProtoFactory, Func<T> constructor, Action<T> clear, Action<T> onUnpool)
		{
			lock (Lock)
			{
				if (HasSetAsProtoFactory)
				{
					setAsProtoFactory = false;
				}
				if (Instance != null && !initializeHierarchy && !setAsProtoFactory && constructor == null && clear == null)
				{
					return;
				}
				Type typeFromHandle = typeof(T);
				if (typeFromHandle.IsArray)
				{
					typeof(ArrayPool<>).MakeGenericType(typeFromHandle.GetElementType()).GetMethodInfoSmart("Initialize", BindingFlags.Static | BindingFlags.Public, false, null).Invoke(null, new object[0]);
					return;
				}
				if (typeFromHandle == typeof(string))
				{
					StringPool.Initialize();
					return;
				}
				if (typeFromHandle.IsConcrete())
				{
					object obj = constructor ?? ConstructorMethod;
					if (obj == null)
					{
						obj = ConstructorCache<T>.Constructor;
					}
					ConstructorMethod = (Func<T>)obj;
					ClearMethod = (clear ?? ClearMethod) ?? ClearMethodCache<T>.ClearMethod;
					OnUnpoolMethod = (onUnpool ?? OnUnpoolMethod) ?? OnUnpoolMethodCache<T>.OnUnpoolMethod;
				}
				else
				{
					initializeHierarchy = true;
				}
				if (initializeHierarchy && !typeFromHandle.IsGenericType)
				{
					Type[] concreteClassesInEntireInheritanceHierarchy = typeFromHandle.GetConcreteClassesInEntireInheritanceHierarchy();
					object[] parameters = new object[5] { false, setAsProtoFactory, null, null, null };
					Type[] array = concreteClassesInEntireInheritanceHierarchy;
					foreach (Type type in array)
					{
						typeof(Pool<>).MakeGenericType(type).GetMethodInfoSmart("Initialize", BindingFlags.Static | BindingFlags.Public, searchOverloads: false, null, parameters).Invoke(null, parameters);
					}
					return;
				}
				if (setAsProtoFactory)
				{
					if (RuntimeTypeModel.Default.IsDefined(typeFromHandle))
					{
						RuntimeTypeModel.Default.Add(typeFromHandle, applyDefaultBehaviour: true).SetFactory(typeof(Pool<T>).GetMethod("_Factory", BindingFlags.Static | BindingFlags.NonPublic));
					}
					HasSetAsProtoFactory = true;
				}
				if (Instance == null)
				{
					Instance = new Pool<T>();
					PoolsByType.Add(typeFromHandle, Instance);
				}
			}
		}

		private static T _Factory()
		{
			return Instance.Unpool();
		}

		private Pool()
		{
			_Pool = new HashSet<T>(ReferenceEqualityComparer<T>.Default);
		}

		public T Unpool()
		{
			lock (Lock)
			{
				if (_Pool.Count == 0)
				{
					_Pool.Add(ConstructorMethod());
					_capacity++;
				}
				T val = null;
				using (HashSet<T>.Enumerator enumerator = _Pool.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						val = enumerator.Current;
					}
				}
				_Pool.Remove(val);
				if (OnUnpoolMethod != null)
				{
					OnUnpoolMethod(val);
				}
				return val;
			}
		}

		public void Repool(T item)
		{
			lock (Lock)
			{
				if (ClearMethod != null)
				{
					ClearMethod(item);
				}
				if (_Pool.Count < _capacity)
				{
					_Pool.Add(item);
				}
			}
		}

		public override void RepoolObject(object obj)
		{
			Repool(obj as T);
		}

		public override void ClearObject(object obj)
		{
			if (ClearMethod == null)
			{
				return;
			}
			lock (Lock)
			{
				ClearMethod(obj as T);
			}
		}

		public override void Clear()
		{
			lock (Lock)
			{
				_Pool.Clear();
				_capacity = 0;
			}
		}

		public void Prewarm(int count)
		{
			lock (Lock)
			{
				for (int i = 0; i < count; i++)
				{
					_Pool.Add(ConstructorMethod());
					_capacity++;
				}
			}
		}
	}

	public class ArrayPool<T> : PoolBase
	{
		public static ArrayPool<T> Instance;

		private static object Lock;

		private Dictionary<int, HashSet<T[]>> _ArraysByLength;

		private Dictionary<int, int> _Capicities;

		private bool _elementsByRef;

		public override int Count
		{
			get
			{
				lock (Lock)
				{
					int num = 0;
					Dictionary<int, HashSet<T[]>>.KeyCollection.Enumerator enumerator = _ArraysByLength.Keys.GetEnumerator();
					while (enumerator.MoveNext())
					{
						num += _ArraysByLength[enumerator.Current].Count;
					}
					return num;
				}
			}
		}

		public override int Capacity
		{
			get
			{
				lock (Lock)
				{
					int num = 0;
					Dictionary<int, int>.KeyCollection.Enumerator enumerator = _Capicities.Keys.GetEnumerator();
					while (enumerator.MoveNext())
					{
						num += _Capicities[enumerator.Current];
					}
					return num;
				}
			}
		}

		public static void Initialize()
		{
		}

		static ArrayPool()
		{
			Lock = new object();
			Instance = new ArrayPool<T>();
			PoolsByType.Add(typeof(T[]), Instance);
		}

		private ArrayPool()
		{
			_ArraysByLength = new Dictionary<int, HashSet<T[]>>();
			_Capicities = new Dictionary<int, int>();
			_elementsByRef = typeof(T).IsReferenceType();
		}

		public T[] Unpool(int length)
		{
			lock (Lock)
			{
				if (!_ArraysByLength.ContainsKey(length))
				{
					_ArraysByLength.Add(length, new HashSet<T[]>(ReferenceEqualityComparer<T[]>.Default));
					_Capicities.Add(length, 0);
				}
				HashSet<T[]> hashSet = _ArraysByLength[length];
				if (hashSet.Count == 0)
				{
					hashSet.Add(new T[length]);
					_Capicities[length]++;
				}
				T[] array = null;
				using (HashSet<T[]>.Enumerator enumerator = hashSet.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						array = enumerator.Current;
					}
				}
				hashSet.Remove(array);
				return array;
			}
		}

		public void Repool(T[] array)
		{
			lock (Lock)
			{
				if (!_ArraysByLength.ContainsKey(array.Length))
				{
					return;
				}
				if (_elementsByRef)
				{
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = default(T);
					}
				}
				HashSet<T[]> hashSet = _ArraysByLength[array.Length];
				if (hashSet.Count < _Capicities[array.Length])
				{
					hashSet.Add(array);
				}
			}
		}

		public override void RepoolObject(object obj)
		{
			Repool(obj as T[]);
		}

		public override void ClearObject(object obj)
		{
			if (!_elementsByRef)
			{
				return;
			}
			lock (Lock)
			{
				T[] array = obj as T[];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = default(T);
				}
			}
		}

		public override void Clear()
		{
			lock (Lock)
			{
				_ArraysByLength.Clear();
				_Capicities.Clear();
			}
		}
	}

	private class StringPool : PoolBase
	{
		public static StringPool Instance;

		public override int Count => 0;

		public override int Capacity => 0;

		public static void Initialize()
		{
		}

		static StringPool()
		{
			Instance = new StringPool();
			Instance = new StringPool();
			PoolsByType.Add(typeof(string), Instance);
		}

		public override void RepoolObject(object obj)
		{
		}

		public override void ClearObject(object obj)
		{
		}

		public override void Clear()
		{
		}
	}

	public enum PoolDebugSortType
	{
		Capacity,
		Count,
		Alphabetically
	}

	public struct PoolDebugData
	{
		public readonly Type type;

		public readonly int count;

		public readonly int capacity;

		public string name => type.FriendlyName();

		public PoolDebugData(Type type)
		{
			PoolBase poolBase = PoolsByType[type];
			this.type = type;
			count = poolBase.Count;
			capacity = poolBase.Capacity;
		}

		public PoolDebugData(PoolDebugData data, PoolDebugData compareTo)
		{
			type = data.type;
			count = data.count - compareTo.count;
			capacity = data.capacity - compareTo.capacity;
		}

		public override string ToString()
		{
			string[] obj = new string[6] { name, ": [", null, null, null, null };
			int num = count;
			obj[2] = num.ToString();
			obj[3] = "/";
			num = capacity;
			obj[4] = num.ToString();
			obj[5] = "]";
			return string.Concat(obj);
		}
	}

	private static Dictionary<Type, PoolBase> PoolsByType;

	private static List<PoolDebugData> _PreviousSnapshot;

	static Pools()
	{
		PoolsByType = new Dictionary<Type, PoolBase>();
		CreatePool<GameState>();
		CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new BezierSpline(), delegate(BezierSpline b)
		{
			b.Clear();
		}, delegate(BezierSpline b)
		{
			b.OnUnpool();
		});
		CreatePool(createHierarchy: false, setAsProtoFactory: false, null, delegate(MemoryStream m)
		{
			m.SetLength(0L);
		});
		CreatePool(createHierarchy: false, setAsProtoFactory: false, null, delegate(StringBuilder b)
		{
			b.Length = 0;
		});
	}

	[RuntimeInitializeOnLoadMethod]
	private static void _AutoInit()
	{
	}

	public static void CreatePool<T>(bool createHierarchy = false, bool setAsProtoFactory = false, Func<T> constructor = null, Action<T> clear = null, Action<T> onUnpool = null) where T : class
	{
		if (Pool<T>.Instance == null)
		{
			ReflectionUtil.RunStaticConstructor<T>();
		}
		Pool<T>.Initialize(createHierarchy, setAsProtoFactory, constructor, clear, onUnpool);
	}

	public static void CreatePoolList<T>()
	{
		CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new List<T>(), delegate(List<T> list)
		{
			list.Clear();
		}, delegate
		{
		});
	}

	public static void CreatePoolHashSet<T>()
	{
		CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new HashSet<T>(), delegate(HashSet<T> hash)
		{
			hash.Clear();
		}, delegate
		{
		});
	}

	public static void CreatePoolDictionary<K, V>()
	{
		CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new Dictionary<K, V>(), delegate(Dictionary<K, V> dictionary)
		{
			dictionary.Clear();
		}, delegate
		{
		});
	}

	public static void CreatePoolStack<T>()
	{
		CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new Stack<T>(), delegate(Stack<T> stack)
		{
			stack.Clear();
		}, delegate
		{
		});
	}

	public static void CreatePoolQueue<T>()
	{
		CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new Queue<T>(), delegate(Queue<T> queue)
		{
			queue.Clear();
		}, delegate
		{
		});
	}

	public static void CreatePoolWRandomD<T>()
	{
		CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new WRandomD<T>(), delegate(WRandomD<T> wRandom)
		{
			wRandom.Clear();
		}, delegate
		{
		});
	}

	public static int Count<T>() where T : class
	{
		return Pool<T>.Instance.Count;
	}

	private static void ClearObject(object obj)
	{
		PoolsByType[obj.GetType()].ClearObject(obj);
	}

	public static PooledValue<T> Value<T>(T value) where T : struct
	{
		return PooledValue<T>.Create(value);
	}

	public static void Prewarm<T>(int count) where T : class
	{
		Pool<T>.Instance.Prewarm(count);
	}

	public static T TryUnpool<T>() where T : class
	{
		if (Pool<T>.Instance == null)
		{
			CreatePool<T>();
		}
		return Pool<T>.Instance.Unpool();
	}

	public static List<T> TryUnpoolList<T>()
	{
		if (Pool<List<T>>.Instance == null)
		{
			CreatePoolList<T>();
		}
		return Pool<List<T>>.Instance.Unpool();
	}

	public static HashSet<T> TryUnpoolHashSet<T>()
	{
		if (Pool<HashSet<T>>.Instance == null)
		{
			CreatePoolHashSet<T>();
		}
		return Pool<HashSet<T>>.Instance.Unpool();
	}

	public static Dictionary<K, V> TryUnpoolDictionary<K, V>()
	{
		if (Pool<Dictionary<K, V>>.Instance == null)
		{
			CreatePoolDictionary<K, V>();
		}
		return Pool<Dictionary<K, V>>.Instance.Unpool();
	}

	public static Stack<T> TryUnpoolStack<T>()
	{
		if (Pool<Stack<T>>.Instance == null)
		{
			CreatePoolStack<T>();
		}
		return Pool<Stack<T>>.Instance.Unpool();
	}

	public static Queue<T> TryUnpoolQueue<T>()
	{
		if (Pool<Queue<T>>.Instance == null)
		{
			CreatePoolQueue<T>();
		}
		return Pool<Queue<T>>.Instance.Unpool();
	}

	public static Ids<T> TryUnpoolIds<T>() where T : class, Idable<T>
	{
		if (Pool<Ids<T>>.Instance == null)
		{
			Ids<T>.CreatePool();
		}
		return Pool<Ids<T>>.Instance.Unpool();
	}

	public static T Unpool<T>() where T : class
	{
		return Pool<T>.Instance.Unpool();
	}

	public static T Unpool<T>(ref T unpoolIntoIfNull) where T : class
	{
		return unpoolIntoIfNull ?? (unpoolIntoIfNull = Pool<T>.Instance.Unpool());
	}

	public static T TryUnpool<T>(ref T unpoolIntoIfNull) where T : class
	{
		return unpoolIntoIfNull ?? (unpoolIntoIfNull = TryUnpool<T>());
	}

	public static List<T> TryUnpool<T>(ref List<T> unpoolIntoIfNull)
	{
		return unpoolIntoIfNull ?? (unpoolIntoIfNull = TryUnpoolList<T>());
	}

	public static HashSet<T> TryUnpool<T>(ref HashSet<T> unpoolIntoIfNull)
	{
		return unpoolIntoIfNull ?? (unpoolIntoIfNull = TryUnpoolHashSet<T>());
	}

	public static Dictionary<K, V> TryUnpool<K, V>(ref Dictionary<K, V> unpoolIntoIfNull)
	{
		return unpoolIntoIfNull ?? (unpoolIntoIfNull = TryUnpoolDictionary<K, V>());
	}

	public static Stack<T> TryUnpool<T>(ref Stack<T> unpoolIntoIfNull)
	{
		return unpoolIntoIfNull ?? (unpoolIntoIfNull = TryUnpoolStack<T>());
	}

	public static Queue<T> TryUnpool<T>(ref Queue<T> unpoolIntoIfNull)
	{
		return unpoolIntoIfNull ?? (unpoolIntoIfNull = TryUnpoolQueue<T>());
	}

	public static Ids<T> TryUnpool<T>(ref Ids<T> unpoolIntoIfNull) where T : class, Idable<T>
	{
		return unpoolIntoIfNull ?? (unpoolIntoIfNull = TryUnpoolIds<T>());
	}

	public static T[] Unpool<T>(int length) where T : struct
	{
		return ArrayPool<T>.Instance.Unpool(length);
	}

	public static T[] Unpool<T>(int length, bool unpoolElements) where T : class
	{
		T[] array = ArrayPool<T>.Instance.Unpool(length);
		if (unpoolElements)
		{
			for (int i = 0; i < length; i++)
			{
				array[i] = Unpool<T>();
			}
		}
		return array;
	}

	public static PoolHandle<T> Use<T>() where T : class
	{
		return PoolHandle<T>.GetHandle();
	}

	public static PoolCollectionHandle<C, E> UseCollection<C, E>() where C : class, ICollection<E> where E : class
	{
		return PoolCollectionHandle<C, E>.GetHandle();
	}

	public static PoolListHandle<T> UseList<T>(int count = 0) where T : class
	{
		PoolListHandle<T> handle = PoolListHandle<T>.GetHandle();
		for (int i = 0; i < count; i++)
		{
			handle.value.Add(Unpool<T>());
		}
		return handle;
	}

	public static PoolListHandle<T> UseList<T>(IEnumerable<T> copyFrom) where T : class
	{
		return PoolListHandle<T>.GetHandle().CopyFrom(copyFrom);
	}

	public static PoolKeepItemListHandle<T> UseKeepItemList<T>()
	{
		return PoolKeepItemListHandle<T>.GetHandle();
	}

	public static PoolKeepItemListHandle<T> UseKeepItemList<T>(List<T> copyFrom)
	{
		return PoolKeepItemListHandle<T>.GetHandle().CopyFrom(copyFrom);
	}

	public static PoolKeepItemListHandle<T> UseKeepItemList<T>(HashSet<T> copyFrom)
	{
		return PoolKeepItemListHandle<T>.GetHandle().CopyFrom(copyFrom);
	}

	public static PoolKeepItemListHandle<T> UseKeepItemList<T>(IEnumerable<T> copyFrom)
	{
		return PoolKeepItemListHandle<T>.GetHandle().CopyFrom(copyFrom);
	}

	public static PoolKeepItemListHandle<T> UseKeepItemList<T>(T item)
	{
		PoolKeepItemListHandle<T> handle = PoolKeepItemListHandle<T>.GetHandle();
		handle.Add(item);
		return handle;
	}

	public static PoolKeepItemListHandle<T> UseKeepItemList<T>(ListEnumerator<T> listEnumerator)
	{
		PoolKeepItemListHandle<T> handle = PoolKeepItemListHandle<T>.GetHandle();
		ListEnumerator<T>.Enumerator enumerator = listEnumerator.GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			handle.value.Add(current);
		}
		return handle;
	}

	public static PoolStructListHandle<T> UseStructList<T>() where T : struct
	{
		return PoolStructListHandle<T>.GetHandle();
	}

	public static PoolStructListHandle<T> UseStructList<T>(int count) where T : struct
	{
		PoolStructListHandle<T> handle = PoolStructListHandle<T>.GetHandle();
		List<T> value = handle.value;
		for (int i = 0; i < count; i++)
		{
			value.Add(default(T));
		}
		return handle;
	}

	public static PoolStructListHandle<T> UseStructList<T>(List<T> copyFrom, int? count = null) where T : struct
	{
		return PoolStructListHandle<T>.GetHandle().CopyFrom(copyFrom, count);
	}

	public static PoolStructListHandle<T> UseStructList<T>(HashSet<T> copyFrom) where T : struct
	{
		return PoolStructListHandle<T>.GetHandle().CopyFrom(copyFrom);
	}

	public static PoolStructListHandle<T> UseStructList<T>(IEnumerable<T> copyFrom) where T : struct
	{
		return PoolStructListHandle<T>.GetHandle().CopyFrom(copyFrom);
	}

	public static PoolHashSetHandle<T> UseHashSet<T>() where T : class
	{
		return PoolHashSetHandle<T>.GetHandle();
	}

	public static PoolKeepItemHashSetHandle<T> UseKeepItemHashSet<T>()
	{
		return PoolKeepItemHashSetHandle<T>.GetHandle();
	}

	public static PoolKeepItemHashSetHandle<T> UseKeepItemHashSet<T>(List<T> copyFrom)
	{
		return PoolKeepItemHashSetHandle<T>.GetHandle().CopyFrom(copyFrom);
	}

	public static PoolKeepItemHashSetHandle<T> UseKeepItemHashSet<T>(HashSet<T> copyFrom)
	{
		return PoolKeepItemHashSetHandle<T>.GetHandle().CopyFrom(copyFrom);
	}

	public static PoolKeepItemHashSetHandle<T> UseKeepItemHashSet<T>(IEnumerable<T> copyFrom)
	{
		return PoolKeepItemHashSetHandle<T>.GetHandle().CopyFrom(copyFrom);
	}

	public static PoolKeepItemHashSetHandle<T> UseKeepItemHashSet<T>(T value)
	{
		return PoolKeepItemHashSetHandle<T>.GetHandle().AddReturn(value);
	}

	public static PoolStructHashSetHandle<T> UseStructHashSet<T>() where T : struct
	{
		return PoolStructHashSetHandle<T>.GetHandle();
	}

	public static PoolStructHashSetHandle<T> UseStructHashSet<T>(HashSet<T> copyFrom) where T : struct
	{
		return PoolStructHashSetHandle<T>.GetHandle().CopyFrom(copyFrom);
	}

	public static PoolStructHashSetHandle<T> UseStructHashSet<T>(List<T> copyFrom, int? count = null) where T : struct
	{
		return PoolStructHashSetHandle<T>.GetHandle().CopyFrom(copyFrom, count);
	}

	public static PoolStructHashSetHandle<T> UseStructHashSet<T>(IEnumerable<T> copyFrom) where T : struct
	{
		return PoolStructHashSetHandle<T>.GetHandle().CopyFrom(copyFrom);
	}

	public static PoolStructHashSetHandle<T> UseStructHashSet<T>(PoolStructListHandle<T> list) where T : struct
	{
		PoolStructHashSetHandle<T> handle = PoolStructHashSetHandle<T>.GetHandle();
		foreach (T item in list)
		{
			handle.Add(item);
		}
		return handle;
	}

	public static PoolStructHashSetHandle<K> UseStructHashSet<K, V>(Dictionary<K, V> dictionary) where K : struct
	{
		PoolStructHashSetHandle<K> handle = PoolStructHashSetHandle<K>.GetHandle();
		DictionaryKeyEnumerator<K, V>.Enumerator enumerator = dictionary.EnumerateKeys().GetEnumerator();
		while (enumerator.MoveNext())
		{
			K current = enumerator.Current;
			handle.Add(current);
		}
		return handle;
	}

	public static PoolStructHashSetHandle<T> UseStructHashSetValue<T>(T value) where T : struct
	{
		PoolStructHashSetHandle<T> handle = PoolStructHashSetHandle<T>.GetHandle();
		handle.Add(value);
		return handle;
	}

	public static PoolStackHandle<T> UseStack<T>() where T : class
	{
		return PoolStackHandle<T>.GetHandle();
	}

	public static PoolKeepItemStackHandle<T> UseKeepItemStack<T>()
	{
		return PoolKeepItemStackHandle<T>.GetHandle();
	}

	public static PoolKeepItemStackHandle<T> UseKeepItemStack<T>(IEnumerable<T> items)
	{
		return PoolKeepItemStackHandle<T>.GetHandle().CopyFrom(items);
	}

	public static PoolStructStackHandle<T> UseStructStack<T>() where T : struct
	{
		return PoolStructStackHandle<T>.GetHandle();
	}

	public static PoolQueueHandle<T> UseQueue<T>() where T : class
	{
		return PoolQueueHandle<T>.GetHandle();
	}

	public static PoolStructQueueHandle<T> UseStructQueue<T>() where T : struct
	{
		return PoolStructQueueHandle<T>.GetHandle();
	}

	public static PoolArrayHandle<T> UseArray<T>(int length, bool unpoolElements) where T : class
	{
		PoolArrayHandle<T> poolArrayHandle = PoolArrayHandle<T>.GetHandle()._SetValue(ArrayPool<T>.Instance.Unpool(length));
		if (unpoolElements)
		{
			for (int i = 0; i < length; i++)
			{
				poolArrayHandle.value[i] = Unpool<T>();
			}
		}
		return poolArrayHandle;
	}

	public static PoolStructArrayHandle<T> UseArray<T>(int length) where T : struct
	{
		return PoolStructArrayHandle<T>.GetHandle()._SetValue(ArrayPool<T>.Instance.Unpool(length));
	}

	public static PoolDictionaryHandle<K, V> UseDictionary<K, V>() where K : class where V : class
	{
		return PoolDictionaryHandle<K, V>.GetHandle();
	}

	public static PoolDictionaryKeyHandle<K, V> UseDictionaryKeys<K, V>() where K : class where V : struct
	{
		return PoolDictionaryKeyHandle<K, V>.GetHandle();
	}

	public static PoolDictionaryValuesHandle<K, V> UseDictionaryValues<K, V>() where V : class
	{
		return PoolDictionaryValuesHandle<K, V>.GetHandle();
	}

	public static PoolStructDictionaryHandle<K, V> UseStructDictionary<K, V>() where K : struct where V : struct
	{
		return PoolStructDictionaryHandle<K, V>.GetHandle();
	}

	public static PoolDictionaryStructCollectionHandle<K, V, E> UseDictionaryStructCollection<K, V, E>() where K : struct where V : class, ICollection<E> where E : struct
	{
		return PoolDictionaryStructCollectionHandle<K, V, E>.GetHandle();
	}

	public static PoolKeepItemDictionaryHandle<K, V> UseKeepItemDictionary<K, V>()
	{
		return PoolKeepItemDictionaryHandle<K, V>.GetHandle();
	}

	public static PoolKeepItemDictionaryHandle<K, V> UseKeepItemDictionary<K, V>(Dictionary<K, V> dictionary)
	{
		return PoolKeepItemDictionaryHandle<K, V>.GetHandle().CopyFrom(dictionary);
	}

	public static PoolKeepItemDictionaryHandle<K, V> UseKeepItemDictionary<K, V>(IEnumerable<KeyValuePair<K, V>> pairs)
	{
		return PoolKeepItemDictionaryHandle<K, V>.GetHandle().CopyFrom(pairs);
	}

	public static PoolWRandomDHandle<T> UseWRandomD<T>()
	{
		return PoolWRandomDHandle<T>.GetHandle();
	}

	private static void _Unpool(GameObject go, bool setActive, bool autoRepool)
	{
		go.SetActive(setActive);
		if (autoRepool && setActive)
		{
			UPools.Instance.MarkForAutoRepool(go);
		}
	}

	public static GameObject Unpool(GameObject blueprint, bool setActive = true, bool autoRepool = true)
	{
		GameObject gameObject = UPools.Instance.Unpool(blueprint, autoRepool);
		_Unpool(gameObject, setActive, autoRepool);
		return gameObject;
	}

	public static GameObject Unpool(GameObject blueprint, Transform parent, bool setActive = true, bool autoRepool = true, bool worldPositionStays = false)
	{
		GameObject gameObject = UPools.Instance.Unpool(blueprint, autoRepool);
		gameObject.transform.SetParent(parent, worldPositionStays);
		_Unpool(gameObject, setActive, autoRepool);
		return gameObject;
	}

	public static GameObject Unpool(GameObject blueprint, Vector3 position, Quaternion? rotation = null, Transform parent = null, bool setActive = true, bool autoRepool = true)
	{
		GameObject gameObject = UPools.Instance.Unpool(blueprint, autoRepool, position, rotation);
		gameObject.transform.SetParent(parent, worldPositionStays: true);
		_Unpool(gameObject, setActive, autoRepool);
		return gameObject;
	}

	public static GameObject TryUnpool(GameObject blueprint, Vector3 position, Quaternion? rotation = null, Transform parent = null, bool setActive = true, bool autoRepool = true)
	{
		if (!blueprint)
		{
			return null;
		}
		return Unpool(blueprint, position, rotation, parent, setActive, autoRepool);
	}

	public static void Repool<T>(T item) where T : class
	{
		Pool<T>.Instance.Repool(item);
	}

	public static void Repool<T>(ref T item) where T : class
	{
		if (item != null)
		{
			Pool<T>.Instance.Repool(item);
			item = null;
		}
	}

	public static void TryRepool<T>(T item) where T : class
	{
		if (item != null && Pool<T>.Instance != null)
		{
			Pool<T>.Instance.Repool(item);
		}
	}

	public static void TryRepool<T>(ref T item) where T : class
	{
		if (item != null)
		{
			if (Pool<T>.Instance == null)
			{
				CreatePool<T>();
			}
			Pool<T>.Instance.Repool(item);
			item = null;
		}
	}

	public static void RepoolObject(object obj)
	{
		PoolsByType[obj.GetType()].RepoolObject(obj);
	}

	public static void RepoolObject<T>(ref T obj) where T : class
	{
		if (obj != null)
		{
			RepoolObject(obj);
			obj = null;
		}
	}

	public static void TryRepoolObject(object obj)
	{
		if (obj != null)
		{
			Type type = obj.GetType();
			if (PoolsByType.ContainsKey(type))
			{
				PoolsByType[type].RepoolObject(obj);
			}
		}
	}

	public static void Repool<T>(T[] array)
	{
		ArrayPool<T>.Instance.Repool(array);
	}

	public static void Repool(string s)
	{
	}

	public static void TryRefresh<T>(ref T item) where T : class
	{
		TryRepool(ref item);
		TryUnpool(ref item);
	}

	private static void RepoolItems(ICollection iCollection, bool repoolCollection)
	{
		if (iCollection == null)
		{
			return;
		}
		if (!(iCollection is IDictionary dictionary))
		{
			IEnumerator enumerator = iCollection.GetEnumerator();
			while (enumerator.MoveNext())
			{
				object current = enumerator.Current;
				if (!(current is ICollection iCollection2))
				{
					if (current != null)
					{
						TryRepoolObject(current);
					}
				}
				else
				{
					RepoolItems(iCollection2, repoolCollection);
				}
			}
		}
		else
		{
			IDictionaryEnumerator enumerator2 = dictionary.GetEnumerator();
			bool? flag = null;
			bool? flag2 = null;
			while (enumerator2.MoveNext())
			{
				if (!flag.HasValue || flag.Value)
				{
					object key = enumerator2.Key;
					if (!(key is ICollection iCollection3))
					{
						if (key != null)
						{
							flag = flag ?? key.GetType().IsReferenceType();
							bool? flag3 = flag;
							if (flag3.Value)
							{
								TryRepoolObject(key);
							}
						}
					}
					else
					{
						RepoolItems(iCollection3, repoolCollection);
					}
				}
				if (flag2.HasValue && !flag2.Value)
				{
					continue;
				}
				object value = enumerator2.Value;
				if (!(value is ICollection iCollection4))
				{
					if (value != null)
					{
						flag2 = flag2 ?? value.GetType().IsReferenceType();
						bool? flag3 = flag2;
						if (flag3.Value)
						{
							TryRepoolObject(value);
						}
					}
				}
				else
				{
					RepoolItems(iCollection4, repoolCollection);
				}
			}
		}
		if (repoolCollection)
		{
			RepoolObject(iCollection);
		}
		else
		{
			ClearObject(iCollection);
		}
	}

	public static void RepoolItems<C, E>(C collection, bool repoolCollection = false) where C : class, ICollection<E> where E : class
	{
		if (collection != null)
		{
			if (collection is List<E> list)
			{
				RepoolItems(list, repoolCollection);
			}
			else if (collection is HashSet<E> hashSet)
			{
				RepoolItems(hashSet, repoolCollection);
			}
			else if (collection is ICollection iCollection)
			{
				RepoolItems(iCollection, repoolCollection);
			}
		}
	}

	public static void RepoolItems<T>(List<T> list, bool repoolCollection = false) where T : class
	{
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			T val = list[i];
			if (!(val is ICollection iCollection))
			{
				if (val != null)
				{
					RepoolObject(val);
				}
			}
			else
			{
				RepoolItems(iCollection, repoolCollection);
			}
		}
		if (repoolCollection)
		{
			Repool(list);
		}
		else
		{
			list.Clear();
		}
	}

	public static void RepoolItems<T>(HashSet<T> hashSet, bool repoolCollection = false) where T : class
	{
		if (hashSet == null)
		{
			return;
		}
		HashSet<T>.Enumerator enumerator = hashSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			if (!(current is ICollection iCollection))
			{
				if (current != null)
				{
					RepoolObject(current);
				}
			}
			else
			{
				RepoolItems(iCollection, repoolCollection);
			}
		}
		if (repoolCollection)
		{
			Repool(hashSet);
		}
		else
		{
			hashSet.Clear();
		}
	}

	public static void RepoolItems<T>(Stack<T> stack, bool repoolCollection = false) where T : class
	{
		if (stack == null)
		{
			return;
		}
		while (stack.Count > 0)
		{
			T val = stack.Pop();
			if (!(val is ICollection iCollection))
			{
				if (val != null)
				{
					RepoolObject(val);
				}
			}
			else
			{
				RepoolItems(iCollection, repoolCollection);
			}
		}
		if (repoolCollection)
		{
			Repool(stack);
		}
		else
		{
			stack.Clear();
		}
	}

	public static void RepoolItems<T>(Queue<T> queue, bool repoolCollection = false) where T : class
	{
		if (queue == null)
		{
			return;
		}
		while (queue.Count > 0)
		{
			T val = queue.Dequeue();
			if (!(val is ICollection iCollection))
			{
				if (val != null)
				{
					RepoolObject(val);
				}
			}
			else
			{
				RepoolItems(iCollection, repoolCollection);
			}
		}
		if (repoolCollection)
		{
			Repool(queue);
		}
		else
		{
			queue.Clear();
		}
	}

	public static void RepoolItems<T>(T[] array, bool repoolCollection = false) where T : class
	{
		if (array == null)
		{
			return;
		}
		foreach (T val in array)
		{
			if (!(val is ICollection iCollection))
			{
				if (val != null)
				{
					RepoolObject(val);
				}
			}
			else
			{
				RepoolItems(iCollection, repoolCollection);
			}
		}
		if (repoolCollection)
		{
			Repool(array);
			return;
		}
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = null;
		}
	}

	public static void RepoolDictionaryItems<K, V>(Dictionary<K, V> dictionary, bool repoolCollection = false) where K : class where V : class
	{
		if (dictionary == null)
		{
			return;
		}
		Dictionary<K, V>.Enumerator enumerator = dictionary.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<K, V> current = enumerator.Current;
			K key = current.Key;
			if (!(key is ICollection iCollection))
			{
				if (key != null)
				{
					RepoolObject(key);
				}
			}
			else
			{
				RepoolItems(iCollection, repoolCollection);
			}
			V value = current.Value;
			if (!(value is ICollection iCollection2))
			{
				if (value != null)
				{
					RepoolObject(value);
				}
			}
			else
			{
				RepoolItems(iCollection2, repoolCollection);
			}
		}
		if (repoolCollection)
		{
			Repool(dictionary);
		}
		else
		{
			dictionary.Clear();
		}
	}

	public static void RepoolDictionaryKeys<K, V>(Dictionary<K, V> dictionary, bool repoolCollection = false) where K : class where V : struct
	{
		if (dictionary == null)
		{
			return;
		}
		Dictionary<K, V>.Enumerator enumerator = dictionary.GetEnumerator();
		while (enumerator.MoveNext())
		{
			K key = enumerator.Current.Key;
			if (!(key is ICollection iCollection))
			{
				if (key != null)
				{
					RepoolObject(key);
				}
			}
			else
			{
				RepoolItems(iCollection, repoolCollection);
			}
		}
		if (repoolCollection)
		{
			Repool(dictionary);
		}
		else
		{
			dictionary.Clear();
		}
	}

	public static void RepoolDictionaryValues<K, V>(Dictionary<K, V> dictionary, bool repoolCollection = false) where V : class
	{
		if (dictionary == null)
		{
			return;
		}
		Dictionary<K, V>.Enumerator enumerator = dictionary.GetEnumerator();
		while (enumerator.MoveNext())
		{
			V value = enumerator.Current.Value;
			if (!(value is ICollection iCollection))
			{
				if (value != null)
				{
					RepoolObject(value);
				}
			}
			else
			{
				RepoolItems(iCollection, repoolCollection);
			}
		}
		if (repoolCollection)
		{
			Repool(dictionary);
		}
		else
		{
			dictionary.Clear();
		}
	}

	public static bool Repool(GameObject gameObject)
	{
		return UPools.Instance.Repool(gameObject, isAutoRepool: false);
	}

	public static bool ForceAutoRepool(GameObject gameObject)
	{
		return UPools.Instance.ForceAutoRepool(gameObject);
	}

	public static void Clear<T>(bool clearEntireInheritanceHierarchy = false) where T : class
	{
		if (!clearEntireInheritanceHierarchy)
		{
			Pool<T>.Instance.Clear();
			return;
		}
		Type[] concreteClassesInEntireInheritanceHierarchy = typeof(T).GetConcreteClassesInEntireInheritanceHierarchy();
		for (int i = 0; i < concreteClassesInEntireInheritanceHierarchy.Length; i++)
		{
			if (PoolsByType.ContainsKey(concreteClassesInEntireInheritanceHierarchy[i]))
			{
				PoolsByType[concreteClassesInEntireInheritanceHierarchy[i]].Clear();
			}
		}
	}

	public static void ClearAll(bool clearGameObjectPools = true)
	{
		foreach (PoolBase value in PoolsByType.Values)
		{
			value.Clear();
		}
		if (clearGameObjectPools)
		{
			UPools.Clear();
		}
	}

	public static void ClearAll(Func<Type, bool> clearType)
	{
		Dictionary<Type, PoolBase>.KeyCollection.Enumerator enumerator = PoolsByType.Keys.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Type current = enumerator.Current;
			if (clearType(current))
			{
				PoolsByType[current].Clear();
			}
		}
	}

	public static void ClearCollections()
	{
		Dictionary<Type, PoolBase>.KeyCollection.Enumerator enumerator = PoolsByType.Keys.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Type current = enumerator.Current;
			if (current.IsCollection())
			{
				PoolsByType[current].Clear();
			}
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void DebugCounts()
	{
	}

	[Conditional("UNITY_EDITOR")]
	public static void DebugTakeSnapShot(PoolDebugSortType sortBy = PoolDebugSortType.Capacity, bool sortAscending = false, bool logCurrentSnapshot = true, bool logComparisonToPreviousSnapshot = true)
	{
		List<PoolDebugData> list = PoolsByType.Keys.Select((Type t) => new PoolDebugData(t)).ToList();
		switch (sortBy)
		{
		case PoolDebugSortType.Capacity:
			list.Sort((PoolDebugData a, PoolDebugData b) => a.capacity - b.capacity);
			break;
		case PoolDebugSortType.Count:
			list.Sort((PoolDebugData a, PoolDebugData b) => a.count - b.count);
			break;
		case PoolDebugSortType.Alphabetically:
			list.Sort((PoolDebugData a, PoolDebugData b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
			break;
		}
		if (!sortAscending)
		{
			list.Reverse();
		}
		if (_PreviousSnapshot != null && logComparisonToPreviousSnapshot)
		{
			Dictionary<Type, PoolDebugData> currentSnapshotMap = list.ToDictionary((PoolDebugData d) => d.type, (PoolDebugData d) => d);
			Dictionary<Type, PoolDebugData> previousSnapshotMap = _PreviousSnapshot.ToDictionary((PoolDebugData d) => d.type, (PoolDebugData d) => d);
			string text = "[COMPARISON TO PREVIOUS POOL SNAPSHOT]=================\n";
			text += "[Capacity Changes]========================================\n";
			foreach (PoolDebugData item in from t in currentSnapshotMap.Keys.Where(previousSnapshotMap.ContainsKey)
				select new PoolDebugData(currentSnapshotMap[t], previousSnapshotMap[t]) into d
				where d.capacity != 0
				orderby d.capacity descending
				select d)
			{
				string[] obj = new string[7] { text, item.name, ": ", null, null, null, null };
				int capacity = item.capacity;
				obj[3] = capacity.ToString("+#;-#;0");
				obj[4] = " => ";
				obj[5] = PoolsByType[item.type].Capacity.ToString();
				obj[6] = "\n";
				text = string.Concat(obj);
			}
			text += "\n[Count Changes]==============================================\n";
			foreach (PoolDebugData item2 in from t in currentSnapshotMap.Keys.Where(previousSnapshotMap.ContainsKey)
				select new PoolDebugData(currentSnapshotMap[t], previousSnapshotMap[t]) into d
				where d.count != 0
				orderby d.count descending
				select d)
			{
				string[] obj2 = new string[7] { text, item2.name, ": ", null, null, null, null };
				int capacity = item2.count;
				obj2[3] = capacity.ToString("+#;-#;0");
				obj2[4] = " => ";
				obj2[5] = PoolsByType[item2.type].Count.ToString();
				obj2[6] = "\n";
				text = string.Concat(obj2);
			}
			text += "\n[Newly Created Pools]========================================\n";
			DictionaryKeyEnumerator<Type, PoolDebugData>.Enumerator enumerator2 = currentSnapshotMap.EnumerateKeys().GetEnumerator();
			while (enumerator2.MoveNext())
			{
				Type current3 = enumerator2.Current;
				if (!previousSnapshotMap.ContainsKey(current3))
				{
					text = text + current3.FriendlyName() + "\n";
				}
			}
		}
		_PreviousSnapshot = list;
	}
}
