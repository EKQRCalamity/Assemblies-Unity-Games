using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class CustomizedEnumContent<T> where T : struct, IConvertible
{
	private static readonly RangeF CUSTOM = new RangeF(1f, 1f, -180f, 180f);

	[ProtoMember(1, IsRequired = true)]
	[UIField("Content To Use", 0u, null, null, null, null, null, null, false, null, 5, false, null, validateOnChange = true)]
	private T _content = EnumUtil<T>.GetDefaultValue();

	[ProtoMember(2)]
	[UIField]
	[UIHideIf("_hidePrimaryColor")]
	private OptionalTints _primaryColor;

	[ProtoMember(3)]
	[UIField]
	[UIHideIf("_hideSecondaryColor")]
	private OptionalTints _secondaryColor;

	[ProtoMember(4)]
	[UIField(maxCount = 0, dynamicInitMethod = "_InitCustomFloats")]
	[UIFieldKey(flexibleWidth = 1f, readOnly = true, max = 32)]
	[UIFieldValue(dynamicInitMethod = "_InitCustomFloatRange", hideInCollectionAdd = true, flexibleWidth = 2f, stepSize = 0.01f)]
	[UIHideIf("_hideCustomFloats")]
	private Dictionary<string, RangeF> _customFloats;

	[ProtoMember(5)]
	[UIField(maxCount = 0, dynamicInitMethod = "_InitCustomColors")]
	[UIFieldKey(flexibleWidth = 1f, readOnly = true, max = 32)]
	[UIFieldValue(dynamicInitMethod = "_InitCustomColorValue", hideInCollectionAdd = true, flexibleWidth = 4f)]
	[UIHideIf("_hideCustomColors")]
	private Dictionary<string, OptionalTints> _customColors;

	public bool hasPrimaryColor => EnumUtil.GetResourceBlueprint(_content).GetComponent<CustomizableEnumContent>().hasPrimaryColor;

	private bool _hidePrimaryColor => !EnumUtil.GetResourceBlueprint(_content).GetComponent<CustomizableEnumContent>().hasPrimaryColor;

	private bool _hideSecondaryColor => !EnumUtil.GetResourceBlueprint(_content).GetComponent<CustomizableEnumContent>().hasSecondaryColor;

	private bool _hideCustomFloats => !EnumUtil.GetResourceBlueprint(_content).GetComponent<CustomizableEnumContent>().hasCustomFloats;

	private bool _hideCustomColors => !EnumUtil.GetResourceBlueprint(_content).GetComponent<CustomizableEnumContent>().hasCustomColors;

	public Color? ApplyTo(GameObject gameObject, System.Random random, Color? inheritedColor = null)
	{
		Color? result = null;
		CustomizableEnumContent component = gameObject.GetComponent<CustomizableEnumContent>();
		if ((bool)_primaryColor || inheritedColor.HasValue)
		{
			Color? color = (result = inheritedColor.Multiply(_primaryColor.GetTint(random)));
			component.SetPrimaryColor(color.Value);
		}
		if ((bool)_secondaryColor)
		{
			component.SetSecondaryColor(_secondaryColor.GetTint(random).Value);
		}
		component.SetCustomFloatValues(_customFloats, random);
		component.SetCustomColorValues(_customColors, random);
		return result;
	}

	public static implicit operator T(CustomizedEnumContent<T> c)
	{
		return c?._content ?? default(T);
	}

	public override string ToString()
	{
		return EnumUtil.FriendlyName(_content);
	}

	private void _InitCustomFloatRange(UIFieldAttribute uiField)
	{
		uiField.defaultValue = CUSTOM;
	}

	private void _InitCustomFloats(UIFieldAttribute uiField)
	{
		uiField.tooltip = "Defined Custom Floats: " + EnumUtil.GetResourceBlueprint(_content).GetComponent<CustomizableEnumContent>().customFloatEvents.ToStringSmart((NamedFloatEvent namedEvent) => namedEvent.name);
	}

	private void _InitCustomColorValue(UIFieldAttribute uiField)
	{
		uiField.defaultValue = new OptionalTints
		{
			tint = Color32HDR.White
		};
	}

	private void _InitCustomColors(UIFieldAttribute uiField)
	{
		uiField.tooltip = "Defined Custom Colors: " + EnumUtil.GetResourceBlueprint(_content).GetComponent<CustomizableEnumContent>().customColorEvents.ToStringSmart((NamedColorEvent namedEvent) => namedEvent.name);
	}
}
