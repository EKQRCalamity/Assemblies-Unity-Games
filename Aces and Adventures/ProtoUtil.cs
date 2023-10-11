using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ProtoBuf;
using ProtoBuf.Meta;
using UnityEngine;

public static class ProtoUtil
{
	[ProtoContract]
	public class Vector2Surrogate
	{
		[ProtoMember(1)]
		public float x;

		[ProtoMember(2)]
		public float y;

		public static explicit operator Vector2(Vector2Surrogate surrogate)
		{
			return new Vector2(surrogate.x, surrogate.y);
		}

		public static explicit operator Vector2Surrogate(Vector2 vec)
		{
			return new Vector2Surrogate
			{
				x = vec.x,
				y = vec.y
			};
		}
	}

	[ProtoContract]
	public class Color32Surrogate
	{
		[ProtoMember(1, IsRequired = true)]
		public byte r;

		[ProtoMember(2, IsRequired = true)]
		public byte g;

		[ProtoMember(3, IsRequired = true)]
		public byte b;

		[ProtoMember(4, IsRequired = true)]
		public byte a;

		public static explicit operator Color32(Color32Surrogate surrogate)
		{
			return new Color32(surrogate.r, surrogate.g, surrogate.b, surrogate.a);
		}

		public static explicit operator Color32Surrogate(Color32 color)
		{
			return new Color32Surrogate
			{
				r = color.r,
				g = color.g,
				b = color.b,
				a = color.a
			};
		}
	}

	[ProtoContract]
	public class ResolutionSurrogate
	{
		[ProtoMember(1)]
		public int width;

		[ProtoMember(2)]
		public int height;

		[ProtoMember(3)]
		public int refreshRate;

		public static explicit operator Resolution(ResolutionSurrogate surrogate)
		{
			Resolution result = default(Resolution);
			result.width = surrogate.width;
			result.height = surrogate.height;
			result.refreshRate = surrogate.refreshRate;
			return result;
		}

		public static explicit operator ResolutionSurrogate(Resolution resolution)
		{
			return new ResolutionSurrogate
			{
				width = resolution.width,
				height = resolution.height,
				refreshRate = resolution.refreshRate
			};
		}
	}

	[ProtoContract]
	public class QueueSurrogate<T>
	{
		[ProtoMember(1, OverwriteList = true)]
		private List<T> _list;

		private QueueSurrogate()
		{
		}

		private QueueSurrogate(Queue<T> queue)
		{
			_list = new List<T>(queue);
		}

		public static explicit operator Queue<T>(QueueSurrogate<T> surrogate)
		{
			if (surrogate == null || surrogate._list.IsNullOrEmpty())
			{
				return null;
			}
			return new Queue<T>(surrogate._list);
		}

		public static explicit operator QueueSurrogate<T>(Queue<T> queue)
		{
			if (queue == null)
			{
				return null;
			}
			return new QueueSurrogate<T>(queue);
		}
	}

	[ProtoContract]
	public class RandomSurrogate
	{
		private static Func<System.Random, int?> _GetInext;

		private static Func<System.Random, int?> _GetInextp;

		private static Func<System.Random, int[]> _GetSeedArray;

		private static Action<System.Random, int> _SetInext;

		private static Action<System.Random, int> _SetInextp;

		private static Action<System.Random, int[]> _SetSeedArray;

		[ProtoMember(1)]
		private int inext;

		[ProtoMember(2)]
		private int inextp;

		[ProtoMember(3, OverwriteList = true, DataFormat = DataFormat.FixedSize)]
		private int[] SeedArray;

		private static FieldInfo _InextField => typeof(System.Random).GetField("_inext", BindingFlags.Instance | BindingFlags.NonPublic);

		private static FieldInfo _InextpField => typeof(System.Random).GetField("_inextp", BindingFlags.Instance | BindingFlags.NonPublic);

		private static FieldInfo _SeedArrayField => typeof(System.Random).GetField("_seedArray", BindingFlags.Instance | BindingFlags.NonPublic);

		private static Func<System.Random, int?> GetInext => _GetInext ?? (_GetInext = _InextField.GetValueGetter<System.Random, int?>());

		private static Func<System.Random, int?> GetInextp => _GetInextp ?? (_GetInextp = _InextpField.GetValueGetter<System.Random, int?>());

		private static Func<System.Random, int[]> GetSeedArray => _GetSeedArray ?? (_GetSeedArray = _SeedArrayField.GetValueGetter<System.Random, int[]>());

		private static Action<System.Random, int> SetInext => _SetInext ?? (_SetInext = _InextField.GetValueSetter<System.Random, int>());

		private static Action<System.Random, int> SetInextp => _SetInextp ?? (_SetInextp = _InextpField.GetValueSetter<System.Random, int>());

		private static Action<System.Random, int[]> SetSeedArray => _SetSeedArray ?? (_SetSeedArray = _SeedArrayField.GetValueSetter<System.Random, int[]>());

		private RandomSurrogate()
		{
		}

		private RandomSurrogate(System.Random random)
		{
			if (random != null)
			{
				inext = GetInext(random).GetValueOrDefault();
				inextp = GetInextp(random).GetValueOrDefault();
				SeedArray = GetSeedArray(random);
			}
		}

		public static explicit operator System.Random(RandomSurrogate surrogate)
		{
			System.Random random = new System.Random();
			SetInext(random, surrogate.inext);
			SetInextp(random, surrogate.inextp);
			SetSeedArray(random, surrogate.SeedArray);
			return random;
		}

		public static explicit operator RandomSurrogate(System.Random random)
		{
			return new RandomSurrogate(random);
		}

		public override string ToString()
		{
			return $"inext: {inext}, inextp: {inextp}, SeedArray: [{SeedArray.ToStringSmart()}]";
		}
	}

	public static bool InitializeSurrogateTrigger;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoadRuntimeMethod()
	{
		InitializeSurrogateTrigger = true;
	}

	private static void SetSurrogate<T, S>()
	{
		RuntimeTypeModel.Default[typeof(T)].SetSurrogate(typeof(S));
	}

	private static void SetQueueSurrogate<T>()
	{
		SetSurrogate<Queue<T>, QueueSurrogate<T>>();
	}

	static ProtoUtil()
	{
		SetSurrogate<Vector2, Vector2Surrogate>();
		SetSurrogate<Color32, Color32Surrogate>();
		SetSurrogate<Resolution, ResolutionSurrogate>();
		SetSurrogate<System.Random, RandomSurrogate>();
	}

	public static PoolHandle<MemoryStream> Serialize<T>(T data)
	{
		PoolHandle<MemoryStream> poolHandle = Pools.Use<MemoryStream>();
		Serializer.Serialize((Stream)poolHandle.value, data);
		return poolHandle;
	}

	public static T Deserialize<T>(MemoryStream stream)
	{
		lock (stream)
		{
			stream.Position = 0L;
			return Serializer.Deserialize<T>(stream);
		}
	}

	public static void DeserializeInto<T>(MemoryStream stream, T deserializeInto)
	{
		lock (stream)
		{
			stream.Position = 0L;
			Serializer.Merge((Stream)stream, deserializeInto);
		}
	}

	public static T Clone<T>(T data)
	{
		using PoolHandle<MemoryStream> poolHandle = Pools.Use<MemoryStream>();
		Serializer.Serialize((Stream)(MemoryStream)poolHandle, data);
		poolHandle.value.Position = 0L;
		return Serializer.Deserialize<T>((MemoryStream)poolHandle);
	}

	public static T CloneTarget<T>(T target) where T : ATarget
	{
		T val = Clone(target);
		val.id = 0;
		val.registerId = 0;
		return val;
	}

	public static IEnumerable<T> CloneTargets<T>(this IEnumerable<T> targets) where T : ATarget
	{
		foreach (T target in targets)
		{
			yield return CloneTarget(target);
		}
	}

	public static object CloneNonGeneric(object data)
	{
		using PoolHandle<MemoryStream> poolHandle = Pools.Use<MemoryStream>();
		Serializer.Serialize((Stream)(MemoryStream)poolHandle, data);
		poolHandle.value.Position = 0L;
		return Serializer.NonGeneric.Deserialize(data.GetType(), (MemoryStream)poolHandle);
	}

	public static PoolStructArrayHandle<byte> ToByteArrayHandle<T>(T data)
	{
		using PoolHandle<MemoryStream> poolHandle = Serialize(data);
		MemoryStream value = poolHandle.value;
		value.Position = 0L;
		int num = (int)value.Length;
		PoolStructArrayHandle<byte> poolStructArrayHandle = Pools.UseArray<byte>(num);
		value.Read(poolStructArrayHandle, 0, num);
		return poolStructArrayHandle;
	}

	public static byte[] ToByteArray<T>(T data)
	{
		using PoolHandle<MemoryStream> poolHandle = Pools.Use<MemoryStream>();
		Serializer.Serialize((Stream)(MemoryStream)poolHandle, data);
		return poolHandle.value.ToArray();
	}

	public static T FromBytes<T>(byte[] bytes)
	{
		using PoolHandle<MemoryStream> poolHandle = Pools.Use<MemoryStream>();
		MemoryStream value = poolHandle.value;
		value.Write(bytes, 0, bytes.Length);
		return Deserialize<T>(value);
	}

	public static object FromBytes(Type type, byte[] bytes)
	{
		using PoolHandle<MemoryStream> poolHandle = Pools.Use<MemoryStream>();
		poolHandle.value.Write(bytes, 0, bytes.Length);
		poolHandle.value.Position = 0L;
		return Serializer.NonGeneric.Deserialize(type, poolHandle.value);
	}

	public static long SizeInBytes<T>(T data)
	{
		if (data == null)
		{
			return 0L;
		}
		using PoolHandle<MemoryStream> poolHandle = Serialize(data);
		return poolHandle.value.Length;
	}

	public static bool HasSerializedData<T>(T data)
	{
		return SizeInBytes(data) > 0;
	}

	public static string ToString<T>(T data)
	{
		string text = "";
		int num = 0;
		foreach (TreeNode<ReflectTreeData<ProtoMemberAttribute>> item in ReflectionUtil.GetMembersWithAttribute(data, (ProtoMemberAttribute p) => (uint)p.Tag).DepthFirstEnumNodes())
		{
			if (num++ != 0)
			{
				text = text + item.value.ToString().Indent(item.DepthLevel - 1) + "\n";
			}
		}
		return text;
	}

	public static bool Equal<T>(T a, T b)
	{
		if (a == null)
		{
			return b == null;
		}
		if (b == null)
		{
			return false;
		}
		using PoolStructArrayHandle<byte> poolStructArrayHandle = ToByteArrayHandle(a);
		using PoolStructArrayHandle<byte> poolStructArrayHandle2 = ToByteArrayHandle(b);
		return poolStructArrayHandle.value.SequenceEqual(poolStructArrayHandle2.value);
	}

	public static byte[] Encrypt<T>(T data, byte[] key, byte[] iv)
	{
		return ToByteArray(data).Encrypt(key, ref iv);
	}

	public static T Decrypt<T>(byte[] data, byte[] key, byte[] iv)
	{
		return FromBytes<T>(data.Decrypt(key, iv));
	}

	[Conditional("UNITY_EDITOR")]
	public static void MemberwiseEqualCheck<T>(byte[] a, byte[] b, bool ignoreReferenceTypes = true)
	{
		T val = FromBytes<T>(a);
		T val2 = FromBytes<T>(b);
		List<ReflectTreeData<ProtoMemberAttribute>> list = ReflectionUtil.GetMembersWithAttribute(val, (ProtoMemberAttribute p) => (uint)p.Tag).DepthFirstEnum().ToList();
		List<ReflectTreeData<ProtoMemberAttribute>> list2 = ReflectionUtil.GetMembersWithAttribute(val2, (ProtoMemberAttribute p) => (uint)p.Tag).DepthFirstEnum().ToList();
		if (list.Count != list2.Count)
		{
			UnityEngine.Debug.LogError("MemberwiseEqualCheck: Compared values created different sized proto trees.");
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			ReflectTreeData<ProtoMemberAttribute> reflectTreeData = list[i];
			ReflectTreeData<ProtoMemberAttribute> reflectTreeData2 = list2[i];
			if (!(reflectTreeData.memberInfo == null) && (!ignoreReferenceTypes || !reflectTreeData.memberInfo.GetUnderlyingType().IsNonStringReferenceType()))
			{
				if (reflectTreeData.memberInfo.Name != reflectTreeData2.memberInfo.Name)
				{
					UnityEngine.Debug.LogError("MemberwiseEqualCheck: Member info mismatch [" + reflectTreeData.memberInfo.Name + "," + reflectTreeData2.memberInfo.Name + "] at index " + i);
				}
				if (!ReflectionUtil.SafeEquals(reflectTreeData.GetValue(), reflectTreeData2.GetValue()))
				{
					UnityEngine.Debug.LogWarning("MemberwiseEqualCheck: MemberInfo [" + reflectTreeData.memberInfo.Name + "] as unequal values [" + reflectTreeData.GetValue()?.ToString() + "," + reflectTreeData2.GetValue()?.ToString() + "]");
				}
			}
		}
	}
}
