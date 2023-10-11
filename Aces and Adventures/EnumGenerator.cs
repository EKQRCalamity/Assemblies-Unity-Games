using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

public static class EnumGenerator
{
	public enum Types
	{
		DisplayResolution,
		QualitySettings
	}

	private static Dictionary<string, Type> _GeneratedEnumsByName = new Dictionary<string, Type>();

	private static Dictionary<string, HashSet<string>> _Names = new Dictionary<string, HashSet<string>>();

	private static Dictionary<string, Type> _Types = new Dictionary<string, Type>();

	public static Type CreateEnumeration(string enumerationTypeName, Type valueType, IEnumerable<string> valueNames, bool flagsEnum = false)
	{
		int index = 0;
		return CreateEnumeration(enumerationTypeName, valueType, valueNames.ToDictionary((Func<string, string>)((string valueName) => valueName), (Func<string, long>)((string valueName) => flagsEnum ? (1 << index++) : index++)), flagsEnum);
	}

	public static Type CreateEnumeration(string enumerationTypeName, Type valueType, Dictionary<string, long> valueMap, bool flagsEnum = false)
	{
		if (!_GeneratedEnumsByName.ContainsKey(enumerationTypeName))
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			AssemblyName assemblyName = new AssemblyName(enumerationTypeName);
			AssemblyBuilder assemblyBuilder = currentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
			string text = assemblyName.Name + ".dll";
			EnumBuilder enumBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, text).DefineEnum(enumerationTypeName, TypeAttributes.Public, valueType);
			if (flagsEnum)
			{
				enumBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(FlagsAttribute).GetConstructor(new Type[0]), new object[0]));
			}
			foreach (KeyValuePair<string, long> item in valueMap)
			{
				enumBuilder.DefineLiteral(item.Key, item.Value);
			}
			Type value = enumBuilder.CreateType();
			assemblyBuilder.Save(text);
			_GeneratedEnumsByName.Add(enumerationTypeName, value);
		}
		return _GeneratedEnumsByName[enumerationTypeName];
	}

	public static Type GetEnumeration(string enumerationTypeName)
	{
		return _GeneratedEnumsByName[enumerationTypeName];
	}

	public static Type GetEnumeration(Types type)
	{
		return _GeneratedEnumsByName[EnumUtil.FriendlyName(type)];
	}

	public static HashSet<string> GetNames(string enumerationTypeName)
	{
		if (!_Names.ContainsKey(enumerationTypeName))
		{
			_Names.Add(enumerationTypeName, Enum.GetNames(GetEnumeration(enumerationTypeName)).ToHash());
		}
		return _Names[enumerationTypeName];
	}

	public static HashSet<string> GetNames(Types type)
	{
		return GetNames(EnumUtil.FriendlyName(type));
	}

	public static Enum TryParse(string enumerationTypeName, string valueName, string fallbackValueName = null)
	{
		HashSet<string> names = GetNames(enumerationTypeName);
		return Enum.Parse(GetEnumeration(enumerationTypeName), names.Contains(valueName) ? valueName : ((fallbackValueName != null && names.Contains(fallbackValueName)) ? fallbackValueName : names.First())) as Enum;
	}

	public static Enum TryParse(Types type, string valueName, string fallbackValueName = null)
	{
		return TryParse(EnumUtil.FriendlyName(type), valueName, fallbackValueName);
	}

	public static Type GetType(string enumName)
	{
		if (!_Types.ContainsKey(enumName))
		{
			_Types.Add(enumName, Type.GetType(enumName) ?? (_GeneratedEnumsByName.ContainsKey(enumName) ? _GeneratedEnumsByName[enumName] : AppDomain.CurrentDomain.GetAssemblies().SelectValid((Assembly assembly) => assembly.GetType(enumName)).First()));
		}
		return _Types[enumName];
	}

	public static Enum InsureValid(Types type, ref Enum enumValue, Func<string> fallbackValueName = null)
	{
		if (enumValue == null || !Enum.IsDefined(enumValue.GetType(), enumValue))
		{
			enumValue = TryParse(type, fallbackValueName?.Invoke());
		}
		return enumValue;
	}
}
