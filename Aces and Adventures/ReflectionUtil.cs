using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public static class ReflectionUtil
{
	private static Thread _MainThread;

	private static readonly Dictionary<Type, Dictionary<Type, Func<object, object>>> CustomConversions;

	private static readonly Dictionary<Type, Type[]> _DerivedClassesCache;

	private static readonly Dictionary<Type, Type[]> _BaseClassesCache;

	private static readonly Dictionary<Type, Type> _RootTypeCache;

	private static readonly Dictionary<Type, Type[]> _AssignableClassesCache;

	private static readonly Dictionary<Type, Type[]> _ConcreteClassesInEntireInheritanceHierarchyCache;

	private static readonly HashSet<Type> _TypesThatShouldNotKeepLabelInKeyValuePair;

	private static readonly bool DebugMethodInfoSmart;

	private static Dictionary<Type, FieldInfo[]> _AllInstanceFieldInfoMap;

	private static BindingFlags _AllInstancedFieldInfoFlags;

	private static Dictionary<PropertyInfo, bool> _IsAutoPropertyCache;

	private static readonly Dictionary<Type, Dictionary<Type, Dictionary<string, Attribute>>> CachedEnumAttributes;

	public const BindingFlags BFLAGS_INSTANCE = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public const BindingFlags BFLAGS_INSTANCE_PUBLIC = BindingFlags.Instance | BindingFlags.Public;

	private const string _CollectionTypePath = "System.Collections.Generic.";

	private static readonly HashSet<string> _CollectionTypeNames;

	private static readonly BindingFlags _PreJitFlags;

	public static bool OnMainThread => Thread.CurrentThread.Equals(_MainThread);

	public static void CacheMainThread()
	{
		_MainThread = _MainThread ?? Thread.CurrentThread;
	}

	private static void AddCustomConversion(Type inputType, Type outputType, Func<object, object> conversion)
	{
		if (!CustomConversions.ContainsKey(inputType))
		{
			CustomConversions.Add(inputType, new Dictionary<Type, Func<object, object>>());
		}
		if (!CustomConversions[inputType].ContainsKey(outputType))
		{
			CustomConversions[inputType].Add(outputType, null);
		}
		CustomConversions[inputType][outputType] = conversion;
	}

	public static Func<object, object> GetCustomConverter(Type inputType, Type outputType)
	{
		if (!CustomConversions.ContainsKey(inputType))
		{
			return null;
		}
		Dictionary<Type, Func<object, object>> dictionary = CustomConversions[inputType];
		if (!dictionary.ContainsKey(outputType))
		{
			return null;
		}
		return dictionary[outputType];
	}

	static ReflectionUtil()
	{
		CustomConversions = new Dictionary<Type, Dictionary<Type, Func<object, object>>>();
		_DerivedClassesCache = new Dictionary<Type, Type[]>();
		_BaseClassesCache = new Dictionary<Type, Type[]>();
		_RootTypeCache = new Dictionary<Type, Type>();
		_AssignableClassesCache = new Dictionary<Type, Type[]>();
		_ConcreteClassesInEntireInheritanceHierarchyCache = new Dictionary<Type, Type[]>();
		_TypesThatShouldNotKeepLabelInKeyValuePair = new HashSet<Type>
		{
			typeof(Color),
			typeof(Color32),
			typeof(RangeF),
			typeof(RangeByte),
			typeof(RangeInt)
		};
		DebugMethodInfoSmart = false;
		_AllInstanceFieldInfoMap = new Dictionary<Type, FieldInfo[]>();
		_AllInstancedFieldInfoFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		_IsAutoPropertyCache = new Dictionary<PropertyInfo, bool>();
		CachedEnumAttributes = new Dictionary<Type, Dictionary<Type, Dictionary<string, Attribute>>>();
		_CollectionTypeNames = new HashSet<string> { "List", "HashSet", "Dictionary", "SortedList", "Queue", "Stack" };
		_PreJitFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
		AddCustomConversion(typeof(string), typeof(Color32), (object obj) => Colors.FromString32(obj as string));
	}

	public static object ChangeType(object value, Type conversionType)
	{
		if (value == null)
		{
			return null;
		}
		Type type = value.GetType();
		if (conversionType.IsAssignableFrom(type))
		{
			return value;
		}
		Func<object, object> customConverter = GetCustomConverter(type, conversionType);
		if (customConverter != null)
		{
			return customConverter(value);
		}
		if (value is IConvertible)
		{
			try
			{
				return Convert.ChangeType(value, conversionType);
			}
			catch (Exception)
			{
			}
		}
		TryTypeConverter(ref value, conversionType);
		return value;
	}

	public static Type[] GetDerivedClasses(this Type baseType)
	{
		if (!_DerivedClassesCache.ContainsKey(baseType))
		{
			_DerivedClassesCache.Add(baseType, (from type in Assembly.GetCallingAssembly().GetTypes()
				where type.IsSubclassOf(baseType)
				select type).ToArray());
		}
		return _DerivedClassesCache[baseType];
	}

	public static Type[] GetBaseClasses(this Type type)
	{
		if (!_BaseClassesCache.ContainsKey(type))
		{
			_BaseClassesCache.Add(type, CollectionUtil.TakeWhile(type, (Type t) => t.BaseType).ToArray());
		}
		return _BaseClassesCache[type];
	}

	public static Type RootType(this Type type)
	{
		if (!_RootTypeCache.ContainsKey(type))
		{
			_RootTypeCache.Add(type, CollectionUtil.TakeWhile(type, (Type t) => t.BaseType, includeInitialValue: true, (Type t) => t != typeof(object)).LastOrDefault() ?? type);
		}
		return _RootTypeCache[type];
	}

	public static Type[] GetAssignableClasses(this Type baseType)
	{
		if (!_AssignableClassesCache.ContainsKey(baseType))
		{
			_AssignableClassesCache.Add(baseType, Assembly.GetCallingAssembly().GetTypes().Where(baseType.IsAssignableFrom)
				.ToArray());
		}
		return _AssignableClassesCache[baseType];
	}

	public static Type[] GetConcreteClassesInEntireInheritanceHierarchy(this Type type)
	{
		type = type.RootType();
		if (!_ConcreteClassesInEntireInheritanceHierarchyCache.ContainsKey(type))
		{
			_ConcreteClassesInEntireInheritanceHierarchyCache.Add(type, type.GetInheritanceHierarchyClasses().ToArray());
		}
		return _ConcreteClassesInEntireInheritanceHierarchyCache[type];
	}

	public static IEnumerable<Type> GetTypesWithAttribute<T>() where T : Attribute
	{
		return from type in Assembly.GetCallingAssembly().GetTypes()
			where type.HasAttribute<T>()
			select type;
	}

	public static IEnumerable<Type> GetTypesWhichImplementInterface<T>()
	{
		return from type in Assembly.GetCallingAssembly().GetTypes()
			where type.ImplementsInterface(typeof(T))
			select type;
	}

	public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
	{
		if (assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types.Where((Type t) => t != null);
		}
	}

	public static bool IsOpenGeneric(this Type type)
	{
		if (type.IsGenericType)
		{
			return type.ContainsGenericParameters;
		}
		return false;
	}

	public static IEnumerable<Type> GetInheritanceHierarchyClasses(this Type baseType, bool includeBaseType = true, bool includeDerivedTypesIfBaseIsConcrete = true, bool includeAbstractTypes = false)
	{
		if (baseType.IsConcrete() && !includeDerivedTypesIfBaseIsConcrete)
		{
			yield return baseType;
			yield break;
		}
		Type[] array = (includeBaseType ? baseType.GetAssignableClasses() : baseType.GetDerivedClasses());
		Type[] array2 = array;
		foreach (Type type in array2)
		{
			if (includeAbstractTypes || type.IsConcrete())
			{
				yield return type;
			}
		}
	}

	public static bool IsUserClassOrStruct(this Type type)
	{
		if (!type.IsClass || !(type != typeof(string)))
		{
			if (type.IsValueType && !type.IsPrimitive)
			{
				return !type.IsEnum;
			}
			return false;
		}
		return true;
	}

	public static bool ShouldKeepLabelInKeyValuePair(this Type type)
	{
		if (type.IsUserClassOrStruct())
		{
			return !_TypesThatShouldNotKeepLabelInKeyValuePair.Contains(type);
		}
		return false;
	}

	public static bool IsNonPrimitiveStruct(this Type type)
	{
		if (type.IsValueType && !type.IsPrimitive)
		{
			return !type.IsEnum;
		}
		return false;
	}

	public static bool IsSimple(this Type type)
	{
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			return type.GetGenericArguments()[0].IsSimple();
		}
		if (!type.IsPrimitive && !type.IsEnum && !(type == typeof(string)))
		{
			return type == typeof(decimal);
		}
		return true;
	}

	public static bool Is<T>(this Type type)
	{
		return typeof(T).IsAssignableFrom(type);
	}

	public static bool IsSameOrSubclass(this Type baseType, Type subType)
	{
		if (!(subType == baseType))
		{
			return subType.IsSubclassOf(baseType);
		}
		return true;
	}

	public static bool IsList(this Type type)
	{
		return typeof(IList).IsAssignableFrom(type);
	}

	public static bool IsCollection(this Type type)
	{
		return typeof(ICollection).IsAssignableFrom(type);
	}

	public static Type GetICollectionElementType(this Type type)
	{
		if (!type.IsGenericType || !type.IsCollection())
		{
			return null;
		}
		return type.GetInterfaces().First((Type t) => t.GetGenericTypeDefinition() == typeof(ICollection<>)).GetGenericArguments()[0];
	}

	public static Type GetGenericTypeDefinitionSafe(this Type type)
	{
		if (!type.IsGenericType)
		{
			return null;
		}
		return type.GetGenericTypeDefinition();
	}

	public static bool IsGenericIList(this Type type)
	{
		if (type.IsList())
		{
			return type.ImplementsUnboundGenericInterface(typeof(IList<>));
		}
		return false;
	}

	public static bool IsGenericList(this Type type)
	{
		if (type.IsGenericType)
		{
			return type.GetGenericTypeDefinition() == typeof(List<>);
		}
		return false;
	}

	public static bool IsGenericICollection(this Type type)
	{
		return type.ImplementsUnboundGenericInterface(typeof(ICollection<>));
	}

	public static bool IsGenericIDictionary(this Type type)
	{
		return type.ImplementsUnboundGenericInterface(typeof(IDictionary<, >));
	}

	public static bool IsGenericDictionary(this Type type)
	{
		if (!type.IsGenericType)
		{
			return false;
		}
		return type.GetGenericTypeDefinition() == typeof(Dictionary<, >);
	}

	public static bool IsGenericHashSet(this Type type)
	{
		if (!type.IsGenericType)
		{
			return false;
		}
		return type.GetGenericTypeDefinition() == typeof(HashSet<>);
	}

	public static bool IsCollectionThatShouldShowAddData(this Type type)
	{
		if (!type.IsGenericIDictionary())
		{
			return type.IsGenericHashSet();
		}
		return true;
	}

	public static bool IsKeyValuePair(this Type type)
	{
		if (!type.IsGenericType)
		{
			return false;
		}
		Type genericTypeDefinition = type.GetGenericTypeDefinition();
		return typeof(KeyValuePair<, >) == genericTypeDefinition;
	}

	public static bool ShouldIndentUIOfChildren(this Type type)
	{
		if (!type.IsKeyValuePair())
		{
			return !type.IsCollection();
		}
		return false;
	}

	public static bool ShouldShowTypeComboBox(this Type type)
	{
		UIFieldAttribute attribute = type.GetAttribute<UIFieldAttribute>();
		if (attribute != null)
		{
			return (bool?)attribute.filter != false;
		}
		return true;
	}

	public static bool ImplementsUnboundGenericInterface(this Type type, Type unboundGenericInterfaceType)
	{
		return type.GetInterfaces().Any((Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == unboundGenericInterfaceType);
	}

	public static bool ImplementsInterface(this Type type, Type interfaceType)
	{
		return type.GetInterfaces().Any((Type t) => t == interfaceType);
	}

	public static Type BaseTypeWhichImplementsInterface(this Type type, Type interfaceType)
	{
		Type result = null;
		while (type.ImplementsInterface(interfaceType))
		{
			result = type;
			type = type.BaseType;
		}
		return result;
	}

	public static bool IsIntegral(this Type t)
	{
		if (!(t == typeof(sbyte)) && !(t == typeof(byte)) && !(t == typeof(ushort)) && !(t == typeof(short)) && !(t == typeof(uint)) && !(t == typeof(int)) && !(t == typeof(ulong)))
		{
			return t == typeof(long);
		}
		return true;
	}

	public static bool IsFloatingPoint(this Type t)
	{
		if (!(t == typeof(float)))
		{
			return t == typeof(double);
		}
		return true;
	}

	public static bool IsNumeric(this Type t)
	{
		if (!t.IsIntegral())
		{
			return t.IsFloatingPoint();
		}
		return true;
	}

	public static float GetMinValue(this Type t, float minClamp = float.MinValue)
	{
		float a = 0f;
		if (t == typeof(sbyte))
		{
			a = -128f;
		}
		else if (t == typeof(byte))
		{
			a = 0f;
		}
		else if (t == typeof(ushort))
		{
			a = 0f;
		}
		else if (t == typeof(short))
		{
			a = -32768f;
		}
		else if (t == typeof(uint))
		{
			a = 0f;
		}
		else if (t == typeof(int))
		{
			a = -2.1474836E+09f;
		}
		else if (t == typeof(ulong))
		{
			a = 0f;
		}
		else if (t == typeof(long))
		{
			a = -9.223372E+18f;
		}
		else if (t == typeof(float))
		{
			a = float.MinValue;
		}
		else if (t == typeof(double))
		{
			a = float.NegativeInfinity;
		}
		return Mathf.Max(a, minClamp);
	}

	public static float GetMaxValue(this Type t, float maxClamp = float.MaxValue)
	{
		float a = 1f;
		if (t == typeof(sbyte))
		{
			a = 127f;
		}
		else if (t == typeof(byte))
		{
			a = 255f;
		}
		else if (t == typeof(ushort))
		{
			a = 65535f;
		}
		else if (t == typeof(short))
		{
			a = 32767f;
		}
		else if (t == typeof(uint))
		{
			a = 4.2949673E+09f;
		}
		else if (t == typeof(int))
		{
			a = 2.1474836E+09f;
		}
		else if (t == typeof(ulong))
		{
			a = 1.8446744E+19f;
		}
		else if (t == typeof(long))
		{
			a = 9.223372E+18f;
		}
		else if (t == typeof(float))
		{
			a = float.MaxValue;
		}
		else if (t == typeof(double))
		{
			a = float.PositiveInfinity;
		}
		return Mathf.Min(a, maxClamp);
	}

	public static object GetDefault(this Type type)
	{
		if (!type.IsValueType)
		{
			return null;
		}
		return Activator.CreateInstance(type);
	}

	public static bool IsReferenceType(this Type type)
	{
		return !type.IsValueType;
	}

	public static bool IsNonStringReferenceType(this Type type)
	{
		if (type != typeof(string))
		{
			return type.IsReferenceType();
		}
		return false;
	}

	public static bool IsConcrete(this Type type)
	{
		return !type.IsAbstract;
	}

	public static bool IsNullableStruct(this Type type)
	{
		if (type.IsGenericType)
		{
			return type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
		return false;
	}

	public static MethodInfo GetInheritedMethod(this Type type, string methodName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type[] parameterTypes = null, Type returnType = null)
	{
		while (type != null)
		{
			MethodInfo methodInfo = ((parameterTypes == null) ? type.GetMethod(methodName, flags) : type.GetMethod(methodName, flags, null, parameterTypes, null));
			if (methodInfo != null && (returnType == null || returnType.IsAssignableFrom(methodInfo.ReturnType)))
			{
				return methodInfo;
			}
			type = type.BaseType;
		}
		return null;
	}

	public static bool InvokeMethod(this object obj, string methodName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, params object[] parameters)
	{
		MethodInfo methodInfo = obj?.GetType().GetInheritedMethod(methodName, flags);
		if (methodInfo != null && methodInfo.GetParameters().Length == parameters.Length)
		{
			methodInfo.Invoke(obj, parameters);
			return true;
		}
		return false;
	}

	public static T InvokeMethod<T>(this object obj, string methodName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, bool checkProperties = true, params object[] parameters)
	{
		Type type = obj.GetType();
		MethodInfo inheritedMethod = type.GetInheritedMethod(methodName, flags);
		if (inheritedMethod != null && inheritedMethod.GetParameters().Length == parameters.Length)
		{
			return (T)inheritedMethod.Invoke(obj, parameters);
		}
		if (checkProperties)
		{
			PropertyInfo property = type.GetProperty(methodName, flags);
			if (property != null && parameters.Length == 0)
			{
				return (T)property.AttemptGetValue(obj);
			}
		}
		return default(T);
	}

	public static void InvokeGenericMethod(this object obj, string methodName, Type genericType, params object[] parameters)
	{
		if (obj != null)
		{
			MethodInfo method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!(method == null))
			{
				method.MakeGenericMethod(genericType).Invoke(obj, parameters);
			}
		}
	}

	public static MethodInfo GetMethodInfoSmart<T>(this Type type, string methodName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, bool searchOverloads = false, Type returnType = null, params object[] parameters)
	{
		MethodInfo inheritedMethod = type.GetInheritedMethod(methodName, flags, (!searchOverloads) ? null : parameters.Select((object obj) => (!(obj is Type)) ? obj.GetType() : ((Type)obj)).ToArray(), returnType);
		if (inheritedMethod == null)
		{
			return null;
		}
		if (!typeof(T).IsAssignableFrom(inheritedMethod.ReturnType))
		{
			return null;
		}
		ParameterInfo[] parameters2 = inheritedMethod.GetParameters();
		if (parameters2.Length != parameters.Length)
		{
			return null;
		}
		if (parameters.Length != 0 && !(parameters[0] is Type))
		{
			for (int i = 0; i < parameters2.Length; i++)
			{
				if (parameters[i] != null && !parameters2[i].ParameterType.IsInstanceOfType(parameters[i]))
				{
					return null;
				}
			}
		}
		return inheritedMethod;
	}

	public static MethodInfo GetMethodInfoSmart(this Type type, string methodName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, bool searchOverloads = false, Type returnType = null, params object[] parameters)
	{
		MethodInfo inheritedMethod = type.GetInheritedMethod(methodName, flags, (!searchOverloads) ? null : parameters.Select((object obj) => (!(obj is Type)) ? obj.GetType() : ((Type)obj)).ToArray(), returnType);
		if (inheritedMethod == null)
		{
			return null;
		}
		ParameterInfo[] parameters2 = inheritedMethod.GetParameters();
		if (parameters2.Length != parameters.Length)
		{
			return null;
		}
		if (!(parameters is Type[]))
		{
			for (int i = 0; i < parameters2.Length; i++)
			{
				if (parameters[i] != null && !parameters2[i].ParameterType.IsInstanceOfType(parameters[i]))
				{
					return null;
				}
			}
		}
		return inheritedMethod;
	}

	public static Type GetInheritedInterface(this Type type, Type interfaceType)
	{
		if (interfaceType.IsGenericType && !interfaceType.IsGenericTypeDefinition)
		{
			throw new ArgumentException("GetInheritedInterface requires that if the interface type is generic, then it must be the generic type definition, not a specific generic type instantiation");
		}
		while (type != null)
		{
			Type[] interfaces = type.GetInterfaces();
			foreach (Type type2 in interfaces)
			{
				if (type2.IsGenericType)
				{
					if (interfaceType == type2.GetGenericTypeDefinition())
					{
						return type2;
					}
				}
				else if (interfaceType == type2)
				{
					return type2;
				}
			}
			type = type.BaseType;
		}
		return null;
	}

	public static FieldInfo[] GetAllInstancedFieldInfo(this Type type)
	{
		if (type.IsValueType)
		{
			return type.GetFields(_AllInstancedFieldInfoFlags);
		}
		if (!_AllInstanceFieldInfoMap.ContainsKey(type))
		{
			List<FieldInfo> list = new List<FieldInfo>();
			Type type2 = type;
			while (type2 != null)
			{
				list.AddRange(type2.GetFields(_AllInstancedFieldInfoFlags));
				type2 = type2.BaseType;
			}
			_AllInstanceFieldInfoMap[type] = list.ToArray();
		}
		return _AllInstanceFieldInfoMap[type];
	}

	public static void RunStaticConstructor<T>()
	{
		StaticConstructorCache<T>.Run();
	}

	public static string GetUILabel(this MemberInfo info)
	{
		UIFieldAttribute attribute = info.GetAttribute<UIFieldAttribute>();
		if (attribute != null && attribute.label != null)
		{
			return attribute.label;
		}
		return info.Name.FriendlyFromCamelOrPascalCase();
	}

	public static string GetUIFieldCategory(this MemberInfo info)
	{
		UIFieldAttribute attribute = info.GetAttribute<UIFieldAttribute>();
		if (attribute == null)
		{
			return "";
		}
		return attribute.category;
	}

	public static uint GetUIOrder(this MemberInfo info)
	{
		uint num = info.GetAttribute<UIFieldAttribute>()?.order ?? 0;
		if (num == 0)
		{
			return uint.MaxValue;
		}
		return num;
	}

	public static T GetAttribute<T>(this MemberInfo info, bool inherit = true, bool exactType = true) where T : Attribute
	{
		Type type = typeof(T);
		Attribute[] customAttributes = Attribute.GetCustomAttributes(info, type, inherit);
		return (exactType ? customAttributes.FirstOrDefault((Attribute a) => a.GetType() == type) : customAttributes.FirstOrDefault()) as T;
	}

	public static bool HasAttribute<T>(this MemberInfo info, bool inherit = true, bool exactType = true) where T : Attribute
	{
		if (info != null)
		{
			return info.GetAttribute<T>(inherit, exactType) != null;
		}
		return false;
	}

	public static object AttemptGetValue(this MemberInfo info, object valueContainer)
	{
		if (info is FieldInfo)
		{
			return (info as FieldInfo).GetValue(valueContainer);
		}
		if (info is PropertyInfo)
		{
			return (info as PropertyInfo).GetValue(valueContainer, null);
		}
		return null;
	}

	public static void AttemptSetValue(this MemberInfo info, object valueContainer, object value, object[] indexedValues = null)
	{
		object value2 = ChangeType(value, info.GetUnderlyingType());
		if (info is FieldInfo)
		{
			(info as FieldInfo).SetValue(valueContainer, value2);
		}
		else if (info is PropertyInfo)
		{
			(info as PropertyInfo).SetValue(valueContainer, value2, indexedValues);
		}
	}

	public static Type GetUnderlyingType(this MemberInfo member)
	{
		return member.MemberType switch
		{
			MemberTypes.Event => ((EventInfo)member).EventHandlerType, 
			MemberTypes.Field => ((FieldInfo)member).FieldType, 
			MemberTypes.Method => ((MethodInfo)member).ReturnType, 
			MemberTypes.Property => ((PropertyInfo)member).PropertyType, 
			_ => throw new ArgumentException("Input MemberInfo must be of type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"), 
		};
	}

	public static bool CanRead(this MemberInfo info)
	{
		if (info is FieldInfo)
		{
			return true;
		}
		if (info is PropertyInfo)
		{
			return (info as PropertyInfo).CanRead;
		}
		return false;
	}

	public static bool CanWrite(this MemberInfo info, bool nonPublic = false)
	{
		if (info is FieldInfo)
		{
			return true;
		}
		if (info is PropertyInfo)
		{
			return (info as PropertyInfo).GetSetMethod(nonPublic) != null;
		}
		return false;
	}

	public static bool IsAutoProperty(this PropertyInfo propertyInfo)
	{
		if (!propertyInfo.CanWrite || !propertyInfo.CanRead || propertyInfo.DeclaringType == null)
		{
			return false;
		}
		if (!_IsAutoPropertyCache.ContainsKey(propertyInfo))
		{
			_IsAutoPropertyCache.Add(propertyInfo, propertyInfo.DeclaringType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Any((FieldInfo f) => f.Name.Contains("<" + propertyInfo.Name + ">")));
		}
		return _IsAutoPropertyCache[propertyInfo];
	}

	public static T GetDistinctValue<T>(T value)
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle.IsNumeric())
		{
			return (T)Convert.ChangeType((double)Convert.ChangeType(value, typeof(double)) + 1.0, typeFromHandle);
		}
		if (typeFromHandle == typeof(string))
		{
			return (T)Convert.ChangeType((value as string == "") ? "a" : "", typeFromHandle);
		}
		if (typeFromHandle == typeof(bool))
		{
			return (T)Convert.ChangeType(!(bool)Convert.ChangeType(value, typeof(bool)), typeFromHandle);
		}
		return value;
	}

	public static MemberInfo GetMemberInfo<T>(this TreeNode<ReflectTreeData<T>> node)
	{
		if (node.value.memberInfo != null)
		{
			return node.value.memberInfo;
		}
		return CollectionUtil.TakeWhile(node, (TreeNode<ReflectTreeData<T>> n) => n.parent).SelectValid((TreeNode<ReflectTreeData<T>> n) => n.value.memberInfo).FirstOrDefault();
	}

	public static TreeNode<ReflectTreeData<T>> GetNodeWithMemberInfo<T>(this TreeNode<ReflectTreeData<T>> node)
	{
		if (node.value.memberInfo != null)
		{
			return node;
		}
		return CollectionUtil.TakeWhile(node, (TreeNode<ReflectTreeData<T>> n) => n.parent).FirstOrDefault((TreeNode<ReflectTreeData<T>> n) => n.value.memberInfo != null, node);
	}

	public static TreeNode<ReflectTreeData<T>> GetNodeWithMethod<T>(this TreeNode<ReflectTreeData<T>> node, string methodName, out MethodInfo methodInfo)
	{
		TreeNode<ReflectTreeData<T>> result = null;
		methodInfo = null;
		foreach (TreeNode<ReflectTreeData<T>> item in node.Parents(includeSelf: true))
		{
			if (item.value.self != null && (methodInfo = item.value.self.GetType().GetInheritedMethod(methodName)) != null)
			{
				return item;
			}
		}
		return result;
	}

	public static Couple<TreeNode<ReflectTreeData<T>>, MethodInfo> GetMethodInfoSmart<T, O>(this TreeNode<ReflectTreeData<T>> node, string methodName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, bool searchOverloads = false, params object[] parameters)
	{
		MethodInfo methodInfoSmart;
		do
		{
			methodInfoSmart = node.value.ownerObject.GetType().GetMethodInfoSmart<O>(methodName, flags, searchOverloads, null, parameters);
		}
		while (methodInfoSmart == null && (node = node.parent) != null);
		return new Couple<TreeNode<ReflectTreeData<T>>, MethodInfo>(node, methodInfoSmart);
	}

	public static Func<I, O> GetMethodInfoSmartFunc<T, I, O>(this TreeNode<ReflectTreeData<T>> node, string methodName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, bool searchOverloads = false, object parameter = null)
	{
		if (methodName.IsNullOrEmpty())
		{
			return null;
		}
		Couple<TreeNode<ReflectTreeData<T>>, MethodInfo> methodInfo = node.GetMethodInfoSmart<T, O>(methodName, flags, searchOverloads, new object[1] { parameter });
		if (methodInfo.b == null)
		{
			return null;
		}
		return (I input) => (O)methodInfo.b.Invoke(methodInfo.a.value.ownerObject, new object[1] { input });
	}

	public static TreeNode<ReflectTreeData<UIFieldAttribute>> Refresh(this TreeNode<ReflectTreeData<UIFieldAttribute>> node)
	{
		string label = node.value.data.label;
		TreeNode<ReflectTreeData<UIFieldAttribute>> treeNode = GetMembersWithAttribute(node.Root.value.self, (UIFieldAttribute ui) => ui.order, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, fallbackBindings: BindingFlags.Instance | BindingFlags.Public, fallbackAttributeLogic: UIFieldAttribute.CreateFromMemberInfo, initializeAttribute: UIFieldAttribute.InitializeFromMemberInfo, getCollectionItemAttribute: UIFieldAttribute.GetCollectionItemAttribute, validateMethod: "OnValidateUI").DepthFirstEnumNodes().FirstOrDefault((TreeNode<ReflectTreeData<UIFieldAttribute>> n) => SafeEquals(n.value.self, node.value.self) && SafeEquals(n.value.memberInfo, node.value.memberInfo));
		if (treeNode != null)
		{
			treeNode.value.data.label = label;
		}
		return treeNode;
	}

	public static bool ShouldHide<T>(this TreeNode<ReflectTreeData<T>> node)
	{
		if (node.value.shouldHide)
		{
			return true;
		}
		if (node.value.memberInfo != null)
		{
			UIHideIfAttribute attribute = node.value.memberInfo.GetAttribute<UIHideIfAttribute>();
			if (attribute != null && node.value.ownerObject != null)
			{
				return node.value.ownerObject.InvokeMethod<bool>(attribute.methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, checkProperties: true, Array.Empty<object>());
			}
		}
		return false;
	}

	public static bool IsInActiveCategory<T>(this TreeNode<ReflectTreeData<T>> node, string activeCategory)
	{
		if (activeCategory.IsNullOrEmpty() || node.DepthLevel != 1)
		{
			return true;
		}
		UICategoryAttribute attribute = node.value.memberInfo.GetAttribute<UICategoryAttribute>();
		if (attribute == null)
		{
			return activeCategory == "Misc";
		}
		return attribute.category == activeCategory;
	}

	public static string GetCategoryTab<T>(this TreeNode<ReflectTreeData<T>> node)
	{
		UICategoryAttribute attribute = node.value.memberInfo.GetAttribute<UICategoryAttribute>();
		if (attribute == null)
		{
			return "Misc";
		}
		return attribute.category;
	}

	public static void AddOnValueChangeListener<T>(this TreeNode<ReflectTreeData<T>> node, Action action)
	{
		foreach (ReflectTreeData<T> item in node.DepthFirstEnum())
		{
			item.OnValueChanged = (Action)Delegate.Combine(item.OnValueChanged, action);
		}
	}

	public static void CacheExcludeValuesMethod(this TreeNode<ReflectTreeData<UIFieldAttribute>> tNode, TreeNode<ReflectTreeData<UIFieldAttribute>> findMethodIn = null)
	{
		if (tNode.value.data.excludedValuesMethod.IsNullOrEmpty())
		{
			return;
		}
		MethodInfo excludeMethodInfo;
		TreeNode<ReflectTreeData<UIFieldAttribute>> excludeMethodTarget = (findMethodIn ?? tNode).GetNodeWithMethod(tNode.value.data.excludedValuesMethod, out excludeMethodInfo);
		if (excludeMethodInfo != null && excludeMethodInfo.ReturnType == typeof(bool) && excludeMethodInfo.GetParameters().Length == 1)
		{
			ReflectTreeData<UIFieldAttribute> value = tNode.value;
			value.excludedValues = (Func<object, bool>)Delegate.Combine(value.excludedValues, (Func<object, bool>)((object v) => (bool)excludeMethodInfo.Invoke(excludeMethodTarget.value.self, new object[1] { v })));
		}
	}

	public static bool TryConvertTo(this TypeConverter converter, ref object value, Type conversionType)
	{
		if (!converter.CanConvertTo(conversionType))
		{
			return false;
		}
		value = converter.ConvertTo(value, conversionType);
		return true;
	}

	public static bool TryConvertFrom(this TypeConverter converter, ref object value)
	{
		if (value == null || !converter.CanConvertFrom(value.GetType()))
		{
			return false;
		}
		value = converter.ConvertFrom(value);
		return true;
	}

	public static bool TryTypeConverter(ref object value, Type conversionType)
	{
		if (value != null)
		{
			if (!TypeDescriptor.GetConverter(value).TryConvertTo(ref value, conversionType))
			{
				return TypeDescriptor.GetConverter(conversionType).TryConvertFrom(ref value);
			}
			return true;
		}
		return false;
	}

	public static TreeNode<ReflectTreeData<T>> GetMembersWithAttribute<T>(object obj, Func<T, uint> attributeOrder = null, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Func<T, MemberInfo, uint, string, T> fallbackAttributeLogic = null, BindingFlags? fallbackBindings = null, Action<T, object, MemberInfo> initializeAttribute = null, Func<object, MemberInfo, T> getCollectionItemAttribute = null, string validateMethod = null, bool includePrivateMembersOfBaseClasses = false) where T : Attribute
	{
		TreeNode<ReflectTreeData<T>> treeNode = new TreeNode<ReflectTreeData<T>>(new ReflectTreeData<T>(0u, obj));
		_getMembersWithAttributeInner(treeNode, bindings, attributeOrder, fallbackBindings, fallbackAttributeLogic, initializeAttribute, getCollectionItemAttribute, validateMethod, includePrivateMembersOfBaseClasses);
		treeNode.Sort();
		return treeNode;
	}

	private static void _getMembersWithAttributeInner<T>(TreeNode<ReflectTreeData<T>> node, BindingFlags bindings, Func<T, uint> attributeOrder, BindingFlags? fallbackBindings, Func<T, MemberInfo, uint, string, T> fallbackAttributeLogic, Action<T, object, MemberInfo> initializeAttribute, Func<object, MemberInfo, T> getCollectionItemAttribute, string validateMethod, bool includePrivateMembersOfBaseClasses) where T : Attribute
	{
		if (!validateMethod.IsNullOrEmpty())
		{
			node.value.self.InvokeMethod(validateMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}
		List<TreeNode<ReflectTreeData<T>>> list2 = new List<TreeNode<ReflectTreeData<T>>>();
		List<TreeNode<ReflectTreeData<T>>> list3 = new List<TreeNode<ReflectTreeData<T>>>();
		object self = node.value.self;
		Type ownerObjectType = self.GetType();
		BindingFlags currentBindings = bindings;
		bool flag = false;
		if (fallbackBindings.HasValue && fallbackAttributeLogic != null && ownerObjectType.GetAttribute<T>() == null)
		{
			currentBindings = fallbackBindings.Value;
			flag = true;
		}
		bool flag2 = ownerObjectType.IsKeyValuePair();
		FieldInfo[] array;
		PropertyInfo[] array2;
		MethodInfo[] array3;
		if (!includePrivateMembersOfBaseClasses)
		{
			array = ownerObjectType.GetFields(currentBindings | (flag2 ? BindingFlags.NonPublic : BindingFlags.Default));
			array2 = ownerObjectType.GetProperties(currentBindings);
			array3 = ownerObjectType.GetMethods(currentBindings | BindingFlags.FlattenHierarchy);
		}
		else
		{
			currentBindings |= BindingFlags.DeclaredOnly | BindingFlags.NonPublic;
			List<Type> source = CollectionUtil.TakeWhile(ownerObjectType, (Type type) => type.BaseType, includeInitialValue: true).ToList();
			array = source.SelectMany((Type type) => type.GetFields(currentBindings)).ToArray();
			array2 = source.SelectMany((Type type) => type.GetProperties(currentBindings)).ToArray();
			array3 = source.SelectMany((Type type) => type.GetMethods(currentBindings | BindingFlags.FlattenHierarchy)).ToArray();
		}
		List<MemberInfo> list4 = new List<MemberInfo>(array.Length + array2.Length + array3.Length);
		for (int i = 0; i < array.Length; i++)
		{
			list4.Add(array[i]);
		}
		for (int j = 0; j < array2.Length; j++)
		{
			if (array2[j].CanRead && array2[j].CanWrite && array2[j].GetIndexParameters().Length == 0)
			{
				list4.Add(array2[j]);
			}
		}
		for (int k = 0; k < array3.Length; k++)
		{
			if ((array3[k].ReturnType == typeof(void) || array3[k].ReturnType == typeof(Task)) && array3[k].GetParameters().Length == 0)
			{
				list4.Add(array3[k]);
			}
		}
		uint num = 10000000u;
		bool flag3 = ownerObjectType.IsGenericICollection();
		Func<object, bool> func = null;
		if (!flag3 && flag2 && node.parent != null && node.parent.value.GetUnderlyingType().IsCollectionThatShouldShowAddData())
		{
			Type underlyingType = node.parent.value.GetUnderlyingType();
			bool flag4 = underlyingType.IsGenericIDictionary();
			MethodInfo containsKeyMethodInfo = (flag4 ? underlyingType.GetInheritedMethod("ContainsKey") : underlyingType.GetInheritedMethod("Contains"));
			object originalCollection2 = node.parent.value.originalCollection;
			func = ((!flag4) ? ((Func<object, bool>)((object obj) => (bool)containsKeyMethodInfo.Invoke(originalCollection2, new object[1] { CreateInstanceSmart(ownerObjectType, new object[2] { obj, null }) }))) : ((Func<object, bool>)((object obj) => (bool)containsKeyMethodInfo.Invoke(originalCollection2, new object[1] { obj }))));
		}
		for (int l = 0; l < list4.Count; l++)
		{
			T val = list4[l].GetAttribute<T>();
			if (val == null && flag)
			{
				if (list4[l] is MethodInfo || flag3 || ownerObjectType.IsSimple())
				{
					continue;
				}
				val = fallbackAttributeLogic(node.value.data, list4[l], num, null);
				num++;
			}
			if (val != null)
			{
				initializeAttribute?.Invoke(val, self, list4[l]);
				uint num2 = attributeOrder?.Invoke(val) ?? num;
				num2 = ((num2 != 0) ? num2 : (++num));
				num = Math.Max(num2, num);
				TreeNode<ReflectTreeData<T>> treeNode = node.AddChild(new ReflectTreeData<T>(num2, list4[l].AttemptGetValue(self), self, list4[l], val));
				Type underlyingType2 = list4[l].GetUnderlyingType();
				if (underlyingType2.IsGenericICollection())
				{
					list3.Add(treeNode);
				}
				else if (underlyingType2.IsUserClassOrStruct() && list4[l].AttemptGetValue(self) != null)
				{
					list2.Add(treeNode);
				}
				if (func != null && list4[l].Name == "Key")
				{
					ReflectTreeData<T> value = treeNode.value;
					value.excludedValues = (Func<object, bool>)Delegate.Combine(value.excludedValues, func);
				}
			}
		}
		if (flag3 && node.parent != null && node.parent.value.GetUnderlyingType().IsGenericICollection())
		{
			list3.Add(node);
		}
		for (int m = 0; m < list3.Count; m++)
		{
			if (list3[m].value.self == null)
			{
				list3[m].value.self = CreateInstanceSmart(list3[m].value.GetUnderlyingType());
				list3[m].value.SetValue(node, list3[m].value.self);
			}
			IList list = list3[m].value.self as IList;
			object originalCollection = list3[m].value.self;
			list3[m].value.originalCollection = originalCollection;
			MethodInfo clearMethod = null;
			MethodInfo addMethod = null;
			Func<object, bool> func2 = null;
			Action commonCollectionUpdate = null;
			if (list == null || !list.GetType().IsGenericList())
			{
				Type type2 = originalCollection.GetType();
				bool isArray = type2.IsArray;
				Type elementType = (isArray ? type2.GetElementType() : null);
				Type inheritedInterface = type2.GetInheritedInterface(typeof(ICollection<>));
				Type type3 = typeof(List<>).MakeGenericType(inheritedInterface.GetGenericArguments());
				list = Activator.CreateInstance(type3) as IList;
				foreach (object item in list3[m].value.self as IEnumerable)
				{
					list.Add(item);
				}
				clearMethod = inheritedInterface.GetInheritedMethod("Clear");
				addMethod = inheritedInterface.GetInheritedMethod("Add");
				if (!isArray && type2.IsGenericHashSet())
				{
					MethodInfo containsMethodInfo = type2.GetInheritedMethod("Contains");
					func2 = (object obj) => (bool)containsMethodInfo.Invoke(originalCollection, new object[1] { obj });
				}
				ReflectTreeData<T> collectionReflectNode = list3[m].value;
				commonCollectionUpdate = delegate
				{
					if (!isArray)
					{
						clearMethod.Invoke(originalCollection, new object[0]);
						{
							foreach (object item2 in list)
							{
								try
								{
									addMethod.Invoke(originalCollection, new object[1] { item2 });
								}
								catch (Exception ex)
								{
									Debug.LogWarning(ex.InnerException.Message);
								}
							}
							return;
						}
					}
					ArrayList arrayList = new ArrayList(list.Count);
					for (int num4 = 0; num4 < list.Count; num4++)
					{
						arrayList.Add(list[num4]);
					}
					collectionReflectNode.SetValue(node, arrayList.ToArray(elementType));
				};
				list3[m].value.self = list;
				list3[m].value.getValueLogic = () => list;
				list3[m].value.countChangedLogic = commonCollectionUpdate;
			}
			TreeNode<ReflectTreeData<T>> nodeWithMemberInfo = list3[m].GetNodeWithMemberInfo();
			MemberInfo memberInfo = nodeWithMemberInfo.value.memberInfo;
			for (int num3 = list.Count - 1; num3 >= 0; num3--)
			{
				int index = num3;
				object self2 = list[num3];
				TreeNode<ReflectTreeData<T>> treeNode2 = list3[m].AddChild(new ReflectTreeData<T>((uint)num3, self2, list3[m].value.self));
				object obj2 = list[index];
				if (obj2 == null)
				{
					list.RemoveAt(num3);
				}
				else
				{
					treeNode2.value.underlyingType = obj2.GetType();
					if (fallbackAttributeLogic != null)
					{
						string text = ((UIUtil.TypeViews.ContainsKey(treeNode2.value.underlyingType) || !treeNode2.value.underlyingType.IsUserClassOrStruct()) ? ">" : "");
						T val2 = ((getCollectionItemAttribute != null && memberInfo != null) ? getCollectionItemAttribute(nodeWithMemberInfo.value.ownerObject, memberInfo) : null);
						string text2 = ((val2 == null && treeNode2.value.underlyingType.HasAttribute<UIFieldAttribute>()) ? treeNode2.value.underlyingType.GetUILabel() : null) ?? ((list[index] != null) ? list[index].ToString() : null) ?? list3[m].value.data.ToString();
						treeNode2.value.data = fallbackAttributeLogic(val2 ?? list3[m].value.data, null, (uint)num3, text + (num3 + 1) + ": " + text2);
					}
					if (func2 != null)
					{
						ReflectTreeData<T> value2 = treeNode2.value;
						value2.excludedValues = (Func<object, bool>)Delegate.Combine(value2.excludedValues, func2);
					}
					treeNode2.value.setValueLogic = delegate(object v)
					{
						list[index] = ChangeType(v, list[index].GetType());
						if (commonCollectionUpdate != null)
						{
							commonCollectionUpdate();
						}
					};
					treeNode2.value.getValueLogic = () => list[index];
					_getMembersWithAttributeInner(treeNode2, bindings, attributeOrder, fallbackBindings, fallbackAttributeLogic, initializeAttribute, getCollectionItemAttribute, validateMethod, includePrivateMembersOfBaseClasses);
				}
			}
		}
		for (int n = 0; n < list2.Count; n++)
		{
			_getMembersWithAttributeInner(list2[n], bindings, attributeOrder, fallbackBindings, fallbackAttributeLogic, initializeAttribute, getCollectionItemAttribute, validateMethod, includePrivateMembersOfBaseClasses);
		}
	}

	public static C[] GetFieldsAndPropertiesOfType<C>(object obj)
	{
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		Type type = obj.GetType();
		FieldInfo[] fields = type.GetFields(bindingAttr);
		PropertyInfo[] properties = type.GetProperties(bindingAttr);
		List<MemberInfo> list = new List<MemberInfo>();
		for (int i = 0; i < fields.Length; i++)
		{
			if (typeof(C).IsAssignableFrom(fields[i].FieldType))
			{
				list.Add(fields[i]);
			}
		}
		for (int j = 0; j < properties.Length; j++)
		{
			if (typeof(C).IsAssignableFrom(properties[j].PropertyType))
			{
				list.Add(properties[j]);
			}
		}
		C[] array = new C[list.Count];
		for (int k = 0; k < array.Length; k++)
		{
			array[k] = (C)list[k].AttemptGetValue(obj);
		}
		return array;
	}

	public static List<T> FindAllInstances<T>(object value, bool stopAtMatch = true, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) where T : class
	{
		HashSet<object> exploredObjects = new HashSet<object>();
		List<T> list = new List<T>();
		_FindAllInstances(value, exploredObjects, list, stopAtMatch, bindings);
		return list;
	}

	private static void _FindAllInstances<T>(object value, HashSet<object> exploredObjects, List<T> found, bool stopAtMatch, BindingFlags bindings) where T : class
	{
		if (value == null)
		{
			return;
		}
		Type type = value.GetType();
		if (type.IsSimple() || !exploredObjects.Add(value))
		{
			return;
		}
		if (value is IEnumerable enumerable)
		{
			{
				foreach (object item2 in enumerable)
				{
					_FindAllInstances(item2, exploredObjects, found, stopAtMatch, bindings);
				}
				return;
			}
		}
		if (value is T item)
		{
			found.Add(item);
			if (stopAtMatch)
			{
				return;
			}
		}
		bool flag = EnumUtil.HasFlag(bindings, BindingFlags.NonPublic);
		if (!flag)
		{
			PropertyInfo[] properties = type.GetProperties(bindings);
			for (int i = 0; i < properties.Length; i++)
			{
				if (properties[i].IsAutoProperty())
				{
					_FindAllInstances(properties[i].GetValue(value, null), exploredObjects, found, stopAtMatch, bindings);
				}
			}
		}
		FieldInfo[] array = (flag ? type.GetAllInstancedFieldInfo() : type.GetFields(bindings));
		for (int j = 0; j < array.Length; j++)
		{
			_FindAllInstances(array[j].GetValue(value), exploredObjects, found, stopAtMatch, bindings);
		}
	}

	private static string _ProcessLabelForLocalizedKey(string label)
	{
		string text = label.Split(':')[0];
		if (uint.TryParse(text, out var result))
		{
			return result.ToString().PrefixToLength('0', 3);
		}
		return text;
	}

	public static string GetLocalizationKeyFromNodePath(this PoolKeepItemListHandle<TreeNode<ReflectTreeData<UIFieldAttribute>>> path)
	{
		return path.value.Skip(1).ToStringSmart((TreeNode<ReflectTreeData<UIFieldAttribute>> n) => n.value.GetValue()?.InvokeMethod<string>("_GetLocalizedKeyLabel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, checkProperties: true, Array.Empty<object>()) ?? _ProcessLabelForLocalizedKey(n.value.data.label), "/", visibleEntriesOnly: true);
	}

	public static string GetLocalizationKey(this TreeNode<ReflectTreeData<UIFieldAttribute>> node)
	{
		using PoolKeepItemListHandle<TreeNode<ReflectTreeData<UIFieldAttribute>>> path = node.GetPath();
		return path.GetLocalizationKeyFromNodePath();
	}

	public static IEnumerable<T> GetValuesFromUI<T>(object obj)
	{
		TreeNode<ReflectTreeData<UIFieldAttribute>> treeNode = GetMembersWithAttribute(obj, (UIFieldAttribute ui) => ui.order, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, fallbackBindings: BindingFlags.Instance | BindingFlags.Public, fallbackAttributeLogic: UIFieldAttribute.CreateFromMemberInfo, initializeAttribute: UIFieldAttribute.InitializeFromMemberInfo, getCollectionItemAttribute: UIFieldAttribute.GetCollectionItemAttribute);
		foreach (TreeNode<ReflectTreeData<UIFieldAttribute>> item in treeNode.DepthFirstEnumNodes())
		{
			object value = item.value.GetValue();
			if (value is T)
			{
				yield return (T)value;
			}
		}
	}

	public static IEnumerable<(TreeNode<ReflectTreeData<UIFieldAttribute>>, T)> GetNodesFromUI<T>(object obj)
	{
		TreeNode<ReflectTreeData<UIFieldAttribute>> treeNode = GetMembersWithAttribute(obj, (UIFieldAttribute ui) => ui.order, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, fallbackBindings: BindingFlags.Instance | BindingFlags.Public, fallbackAttributeLogic: UIFieldAttribute.CreateFromMemberInfo, initializeAttribute: UIFieldAttribute.InitializeFromMemberInfo, getCollectionItemAttribute: UIFieldAttribute.GetCollectionItemAttribute);
		foreach (TreeNode<ReflectTreeData<UIFieldAttribute>> item2 in treeNode.DepthFirstEnumNodes())
		{
			if (item2.value.GetValue() is T item)
			{
				yield return (item2, item);
			}
		}
	}

	public static int LocalizeDataRef(ContentRef dataRef, object data = null, string title = null)
	{
		if (!dataRef || !dataRef.isDataRef)
		{
			return 0;
		}
		if (data == null)
		{
			data = dataRef.GetContentImmediate();
		}
		if (title == null)
		{
			title = (data as IDataContent)?.GetTitle() ?? "TITLE";
		}
		string text = title + "/";
		_ = (TableReference)LocalizationSettings.StringDatabase.GetTable(dataRef.specificTypeFriendly).TableCollectionName;
		string text2 = $"{dataRef.key.fileId}/";
		TreeNode<ReflectTreeData<UIFieldAttribute>> treeNode = GetMembersWithAttribute(data, (UIFieldAttribute ui) => ui.order, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, fallbackBindings: BindingFlags.Instance | BindingFlags.Public, fallbackAttributeLogic: UIFieldAttribute.CreateFromMemberInfo, initializeAttribute: UIFieldAttribute.InitializeFromMemberInfo, getCollectionItemAttribute: UIFieldAttribute.GetCollectionItemAttribute);
		int num = 0;
		using (DataRefControl.BeginEdit(dataRef))
		{
			foreach (TreeNode<ReflectTreeData<UIFieldAttribute>> item in treeNode.DepthFirstEnumNodes())
			{
				if (item.value.GetValue() is LocalizedStringData localizedStringData && !localizedStringData.id && localizedStringData.rawText.HasVisibleCharacter())
				{
					using PoolKeepItemListHandle<TreeNode<ReflectTreeData<UIFieldAttribute>>> path = item.GetPath();
					_ = text2 + path.GetLocalizationKeyFromNodePath();
					localizedStringData.id.tableEntry.Key = text + localizedStringData.id.tableEntry.Key;
					num++;
				}
			}
		}
		if (num == 0)
		{
			return num;
		}
		Debug.Log($"Localized {num} entries for {title} {dataRef.specificTypeFriendly}\n");
		dataRef.SaveFromUIWithoutValidation(data);
		if (DataRefControl.ActiveEditKey == dataRef)
		{
			UIUtil.BeginProcessJob(DataRefControl.ActiveControl.transform, null, Department.UI).Then().DoProcess(Job.WaitForDepartmentEmpty(Department.Content))
				.Then()
				.Do(delegate
				{
					DataRefControl.ActiveControl.RevertChanges();
				})
				.Then()
				.Do(UIUtil.EndProcess);
		}
		return num;
	}

	public static async Task LocalizeAllDataRefsAsync()
	{
		Debug.Log("LOCALIZING ALL DATA REFERENCES\n");
		int count = 0;
		object inputLock = new object();
		InputManager.SetEventSystemEnabled(inputLock, enabled: false);
		foreach (Type item in from t in GetTypesWhichImplementInterface<IDataContent>()
			where t.IsConcrete() && t.GetAttribute<LocalizeAttribute>() != null
			select t)
		{
			foreach (ContentRef item2 in from c in ContentRef.SearchData(item)
				orderby c.lastUpdateTime
				select c)
			{
				ContentRef.TimeOverride = item2.lastUpdateTime;
				int num = LocalizeDataRef(item2);
				count += num;
				if (num > 0)
				{
					await Job.Process(Job.WaitForDepartmentEmpty(Department.Content));
				}
			}
		}
		ContentRef.TimeOverride = null;
		InputManager.SetEventSystemEnabled(inputLock, enabled: true);
		Debug.Log($"LOCALIZED {count} ENTRIES\n");
	}

	public static Dictionary<K, V> CreateEnumResourceMap<K, V>(string resourcePath, IEqualityComparer<K> comparer = null) where K : struct, IConvertible where V : UnityEngine.Object
	{
		Dictionary<K, V> dictionary = new Dictionary<K, V>(comparer);
		HashSet<K> hashSet = EnumUtil<K>.Values.ToHash();
		string text = resourcePath + "/";
		List<K> list = new List<K>(0);
		foreach (K item in hashSet)
		{
			K val = item;
			V val2 = Resources.Load<V>(text + val);
			if ((bool)(UnityEngine.Object)val2)
			{
				dictionary.Add(item, val2);
			}
			else
			{
				list.Add(item);
			}
		}
		if (dictionary.Count == 0)
		{
			V value = Resources.LoadAll<V>(resourcePath).FirstOrDefault();
			dictionary.Add(hashSet.First(), value);
			list.Remove(hashSet.First());
		}
		if (list.Count > 0 && dictionary.Count > 0)
		{
			V value2 = dictionary.Values.First();
			for (int i = 0; i < list.Count; i++)
			{
				dictionary.Add(list[i], value2);
			}
		}
		return dictionary;
	}

	public static Dictionary<K, Dictionary<K2, V>> CreateEnumResourceMap<K, K2, V>(string resourcePath, IEqualityComparer<K> comparer = null, IEqualityComparer<K2> comparer2 = null) where K : struct, IConvertible where K2 : struct, IConvertible where V : UnityEngine.Object
	{
		Dictionary<K, Dictionary<K2, V>> dictionary = new Dictionary<K, Dictionary<K2, V>>(comparer);
		if (!resourcePath.EndsWith("/"))
		{
			resourcePath += "/";
		}
		K[] values = EnumUtil<K>.Values;
		for (int i = 0; i < values.Length; i++)
		{
			K key = values[i];
			Dictionary<K2, V> dictionary2 = new Dictionary<K2, V>(comparer2);
			dictionary.Add(key, dictionary2);
			string text = key.ToString();
			string text2 = resourcePath + text + "/";
			K2[] values2 = EnumUtil<K2>.Values;
			foreach (K2 val in values2)
			{
				K2 val2 = val;
				dictionary2.Add(val, Resources.Load<V>(text2 + text + val2));
			}
		}
		return dictionary;
	}

	public static HashSet<string> GetExcludedEnumMembers<T>(TreeNode<ReflectTreeData<T>> owningNode, ReflectTreeData<T> rData)
	{
		return _GetEnumMembers(owningNode, rData, included: false);
	}

	public static HashSet<string> GetIncludedEnumMembers<T>(TreeNode<ReflectTreeData<T>> owningNode, ReflectTreeData<T> rData)
	{
		return _GetEnumMembers(owningNode, rData, included: true);
	}

	private static HashSet<string> _GetEnumMembers<T>(TreeNode<ReflectTreeData<T>> owningNode, ReflectTreeData<T> rData, bool included)
	{
		Type underlyingType = rData.GetUnderlyingType();
		string[] names = Enum.GetNames(underlyingType);
		HashSet<string> hashSet = new HashSet<string>();
		object value = rData.GetValue();
		for (int i = 0; i < names.Length; i++)
		{
			object obj = Enum.Parse(underlyingType, names[i]);
			rData.SetValue(owningNode, obj);
			object value2 = rData.GetValue();
			if (included == value2.Equals(obj))
			{
				hashSet.Add(names[i]);
			}
		}
		rData.SetValue(owningNode, value);
		if (included == rData.GetValue().Equals(value))
		{
			hashSet.AddUnique(value.ToString());
		}
		return hashSet;
	}

	public static HashSet<string> GetEnumMembers(Type enumType, bool included, int minValue = int.MinValue, int maxValue = int.MaxValue)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (object value in Enum.GetValues(enumType))
		{
			int num = (int)Convert.ChangeType(value, typeof(int));
			if (!(!included ^ (num < minValue || num > maxValue)))
			{
				hashSet.Add(value.ToString());
			}
		}
		return hashSet;
	}

	public static T GetAttribute<T>(this Enum value) where T : Attribute
	{
		Type type = value.GetType();
		Type typeFromHandle = typeof(T);
		string key = value.ToString();
		if (!CachedEnumAttributes.ContainsKey(type))
		{
			CachedEnumAttributes.Add(type, new Dictionary<Type, Dictionary<string, Attribute>>());
		}
		if (!CachedEnumAttributes[type].ContainsKey(typeFromHandle))
		{
			CachedEnumAttributes[type].Add(typeFromHandle, new Dictionary<string, Attribute>());
		}
		if (CachedEnumAttributes[type][typeFromHandle].ContainsKey(key))
		{
			return (T)CachedEnumAttributes[type][typeFromHandle][key];
		}
		object[] customAttributes = type.GetMember(value.ToString())[0].GetCustomAttributes(typeof(T), inherit: false);
		T val = ((customAttributes.Length != 0) ? ((T)customAttributes[0]) : null);
		CachedEnumAttributes[type][typeFromHandle].Add(key, val);
		return val;
	}

	public static bool IsNullOrDefault<T>(T argument)
	{
		if (argument == null)
		{
			return true;
		}
		if (object.Equals(argument, default(T)))
		{
			return true;
		}
		Type typeFromHandle = typeof(T);
		if (Nullable.GetUnderlyingType(typeFromHandle) != null)
		{
			return false;
		}
		Type type = argument.GetType();
		if (type.IsValueType && type != typeFromHandle)
		{
			return Activator.CreateInstance(argument.GetType()).Equals(argument);
		}
		return false;
	}

	public static bool IsProtoDefault<T>(T a)
	{
		return IOUtil.CompareProtoBytes(a, CreateInstanceSmart<T>());
	}

	public static bool IsProtoDefault(object a)
	{
		return IOUtil.CompareProtoBytes(a, CreateInstanceSmart(a.GetType()));
	}

	public static bool SafeEquals(object a, object b)
	{
		return a?.Equals(b) ?? (b == null);
	}

	public static string SafeToString(object obj)
	{
		if (obj == null)
		{
			return "Null";
		}
		return obj.ToString();
	}

	public static object CreateInstanceSmart<T>()
	{
		return CreateInstanceSmart(typeof(T));
	}

	public static object CreateInstanceSmart(Type type)
	{
		if (type == typeof(string))
		{
			return "";
		}
		if (type.IsArray)
		{
			return Array.CreateInstance(type.GetElementType(), 0);
		}
		if (!type.IsAbstract)
		{
			return Activator.CreateInstance(type, nonPublic: true);
		}
		return Activator.CreateInstance((from t in type.GetDerivedClasses()
			where t.IsConcrete()
			select t).MinBy((Type t) => t.GetUIOrder()), nonPublic: true);
	}

	public static object CreateInstanceSmart<T>(object[] constructorParameters)
	{
		return CreateInstanceSmart(typeof(T), constructorParameters);
	}

	public static object CreateInstanceSmart(Type type, object[] constructorParameters)
	{
		if (type == typeof(string))
		{
			return "";
		}
		if (type.IsArray)
		{
			return Array.CreateInstance(type.GetElementType(), 0);
		}
		if (!type.IsAbstract)
		{
			return Activator.CreateInstance(type, constructorParameters);
		}
		return Activator.CreateInstance(type.GetDerivedClasses().First((Type t) => t.IsConcrete()), constructorParameters);
	}

	public static Type GetTypeFromFriendlyName(string friendlyTypeName)
	{
		Type type = Type.GetType(friendlyTypeName);
		if (type != null)
		{
			return type;
		}
		string[] array = friendlyTypeName.Split(new char[1] { '<' }, 2);
		if (array.Length > 1)
		{
			List<string> list = (from s in array[1].Substring(0, array[1].Length - 1).Split(',')
				select s.Trim()).ToList();
			type = Type.GetType((_CollectionTypeNames.Contains(array[0]) ? "System.Collections.Generic." : "") + array[0] + "`" + list.Count);
			type = type.MakeGenericType(list.Select(GetTypeFromFriendlyName).ToArray());
		}
		return type;
	}

	public static string FriendlyName(this Type type)
	{
		string name = type.Name;
		if (!type.IsGenericType)
		{
			return name;
		}
		Type[] genericArguments = type.GetGenericArguments();
		name = name.Split('`')[0] + "<";
		name += genericArguments.ToStringSmart((Type t) => t.FriendlyName());
		return name + ">";
	}

	public static Func<I, V> GetValueGetter<I, V>(this PropertyInfo propertyInfo)
	{
		if (typeof(I) != propertyInfo.DeclaringType)
		{
			throw new ArgumentException();
		}
		ParameterExpression parameterExpression = Expression.Parameter(propertyInfo.DeclaringType, "i");
		return (Func<I, V>)Expression.Lambda(Expression.TypeAs(Expression.Property(parameterExpression, propertyInfo), typeof(V)), parameterExpression).Compile();
	}

	public static Action<I, V> GetValueSetter<I, V>(this PropertyInfo propertyInfo)
	{
		if (typeof(I) != propertyInfo.DeclaringType)
		{
			throw new ArgumentException();
		}
		ParameterExpression parameterExpression = Expression.Parameter(propertyInfo.DeclaringType, "i");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(V), "a");
		return (Action<I, V>)Expression.Lambda(Expression.Call(parameterExpression, propertyInfo.GetSetMethod(), Expression.Convert(parameterExpression2, propertyInfo.PropertyType)), parameterExpression, parameterExpression2).Compile();
	}

	public static Func<I, V> GetValueGetter<I, V>(this FieldInfo fieldInfo)
	{
		if (typeof(I) != fieldInfo.DeclaringType)
		{
			throw new ArgumentException();
		}
		ParameterExpression parameterExpression = Expression.Parameter(fieldInfo.DeclaringType, "i");
		return (Func<I, V>)Expression.Lambda(Expression.TypeAs(Expression.Field(parameterExpression, fieldInfo), typeof(V)), parameterExpression).Compile();
	}

	public static Action<T, TValue> GetValueSetter<T, TValue>(this FieldInfo field, bool skipVisibility = true)
	{
		DynamicMethod dynamicMethod = new DynamicMethod("setter", typeof(void), new Type[2]
		{
			typeof(T),
			typeof(TValue)
		}, typeof(ReflectionUtil), skipVisibility);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Stfld, field);
		iLGenerator.Emit(OpCodes.Ret);
		return (Action<T, TValue>)dynamicMethod.CreateDelegate(typeof(Action<T, TValue>));
	}

	public static T CacheMethod<T>(this Type type, string methodName, bool nonPublic = true, bool checkStaticMethods = true, bool searchOverloads = false, Type[] parameterTypes = null, Type returnType = null) where T : class
	{
		BindingFlags bindingFlags = BindingFlags.Public;
		if (nonPublic)
		{
			bindingFlags |= BindingFlags.NonPublic;
		}
		parameterTypes = parameterTypes ?? new Type[0];
		BindingFlags flags = bindingFlags | BindingFlags.Instance;
		object[] parameters = parameterTypes;
		MethodInfo methodInfoSmart = type.GetMethodInfoSmart(methodName, flags, searchOverloads, returnType, parameters);
		if (methodInfoSmart != null)
		{
			return Delegate.CreateDelegate(typeof(T), methodInfoSmart) as T;
		}
		if (checkStaticMethods)
		{
			BindingFlags flags2 = bindingFlags | BindingFlags.Static;
			parameters = parameterTypes;
			MethodInfo methodInfoSmart2 = type.GetMethodInfoSmart(methodName, flags2, searchOverloads, returnType, parameters);
			if (methodInfoSmart2 != null)
			{
				return Delegate.CreateDelegate(typeof(T), methodInfoSmart2) as T;
			}
		}
		return null;
	}

	private static void _PreJitAll(Type type)
	{
		if (type.IsGenericType)
		{
			return;
		}
		MethodInfo[] methods = type.GetMethods(_PreJitFlags);
		foreach (MethodInfo methodInfo in methods)
		{
			if (!methodInfo.IsAbstract && !methodInfo.IsGenericMethod)
			{
				methodInfo.MethodHandle.GetFunctionPointer();
			}
		}
	}

	public static void PreJitAll(Type type, bool jitSubClasses = false)
	{
		_PreJitAll(type);
		if (jitSubClasses)
		{
			Type[] derivedClasses = type.GetDerivedClasses();
			for (int i = 0; i < derivedClasses.Length; i++)
			{
				_PreJitAll(derivedClasses[i]);
			}
		}
	}

	public static void PreJitAll<T>(bool jitSubClasses = false)
	{
		PreJitAll(typeof(T), jitSubClasses);
	}
}
