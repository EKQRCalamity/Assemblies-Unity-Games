using System;
using System.Collections.Generic;
using System.Reflection;

[AttributeUsage(AttributeTargets.All)]
public class UIFieldAttribute : Attribute
{
	protected const int MAX_COUNT_DEFAULT = 5;

	protected const float FLEXIBLE_WIDTH = 1f;

	public uint order { get; set; }

	public string label { get; set; }

	public object min { get; set; }

	public object max { get; set; }

	public object stepSize { get; set; }

	public object defaultValue { get; set; }

	public string category { get; set; }

	public string view { get; set; }

	public bool validateOnChange { get; set; }

	public HashSet<string> dependentOn { get; private set; }

	public int maxCount { get; set; }

	public bool fixedSize { get; set; }

	public bool showAddData { get; set; }

	public object filter { get; set; }

	public string filterMethod { get; set; }

	public bool readOnly { get; set; }

	public string dynamicInitMethod { get; set; }

	public string onValueChangedMethod { get; set; }

	public string excludedValuesMethod { get; set; }

	public bool searchInterfaceViews { get; set; }

	public UICollapseType collapse { get; set; }

	public string tooltip { get; set; }

	public UIFieldAttribute()
	{
		maxCount = 5;
	}

	public UIFieldAttribute(string label, uint order = 0u, object min = null, object max = null, object stepSize = null, object defaultValue = null, string category = null, string view = null, bool validateOnChange = false, string[] dependentOn = null, int maxCount = 5, bool showAddData = false, object filter = null)
	{
		this.label = label;
		this.order = order;
		this.min = min;
		this.max = max;
		this.stepSize = stepSize;
		this.defaultValue = defaultValue;
		this.category = category;
		this.view = view;
		this.validateOnChange = validateOnChange;
		if (!dependentOn.IsNullOrEmpty())
		{
			this.dependentOn = dependentOn.ToHash();
		}
		this.maxCount = maxCount;
		this.showAddData = showAddData;
		this.filter = filter;
	}

	public override string ToString()
	{
		return label;
	}

	public void TransferKeyValuePairData(UIFieldAttribute otherAttribute)
	{
		otherAttribute.min = min;
		otherAttribute.max = max;
		otherAttribute.defaultValue = defaultValue;
		otherAttribute.stepSize = stepSize;
		otherAttribute.view = view;
		otherAttribute.validateOnChange = validateOnChange;
		otherAttribute.maxCount = maxCount;
		otherAttribute.showAddData = showAddData;
		otherAttribute.fixedSize = fixedSize;
		otherAttribute.readOnly = readOnly;
		otherAttribute.dynamicInitMethod = dynamicInitMethod;
		otherAttribute.onValueChangedMethod = onValueChangedMethod;
		otherAttribute.excludedValuesMethod = excludedValuesMethod;
		otherAttribute.filter = filter;
		otherAttribute.filterMethod = filterMethod;
		otherAttribute.collapse = collapse;
	}

	public static UIFieldAttribute CreateFromMemberInfo(UIFieldAttribute parentAttribute, MemberInfo info, uint order, string overrideLabel = null)
	{
		UIFieldAttribute uIFieldAttribute = new UIFieldAttribute(overrideLabel ?? ((info != null) ? info.Name.FriendlyFromCamelOrPascalCase() : "N/A"), order);
		if (parentAttribute != null)
		{
			uIFieldAttribute.min = parentAttribute.min;
			uIFieldAttribute.max = parentAttribute.max;
			uIFieldAttribute.stepSize = parentAttribute.stepSize;
			uIFieldAttribute.readOnly = parentAttribute.readOnly;
			if (parentAttribute.GetType() == typeof(UIFieldCollectionItemAttribute))
			{
				uIFieldAttribute.view = parentAttribute.view;
				uIFieldAttribute.defaultValue = parentAttribute.defaultValue;
				uIFieldAttribute.validateOnChange = parentAttribute.validateOnChange;
				uIFieldAttribute.maxCount = parentAttribute.maxCount;
				uIFieldAttribute.showAddData = parentAttribute.showAddData;
				uIFieldAttribute.fixedSize = parentAttribute.fixedSize;
				uIFieldAttribute.filter = parentAttribute.filter;
				uIFieldAttribute.filterMethod = parentAttribute.filterMethod;
				uIFieldAttribute.dynamicInitMethod = parentAttribute.dynamicInitMethod;
				uIFieldAttribute.onValueChangedMethod = parentAttribute.onValueChangedMethod;
				uIFieldAttribute.excludedValuesMethod = parentAttribute.excludedValuesMethod;
				uIFieldAttribute.searchInterfaceViews = parentAttribute.searchInterfaceViews;
			}
		}
		return uIFieldAttribute;
	}

	public static void InitializeFromMemberInfo(UIFieldAttribute attribute, object ownerObject, MemberInfo info)
	{
		attribute.label = (attribute.label.IsNullOrEmpty() ? info.Name.FriendlyFromCamelOrPascalCase() : attribute.label);
		Type underlyingType = info.GetUnderlyingType();
		if (underlyingType.IsGenericICollection() && (attribute.defaultValue != null || underlyingType.IsCollectionThatShouldShowAddData()))
		{
			attribute.showAddData = true;
		}
		if (!attribute.dynamicInitMethod.IsNullOrEmpty())
		{
			ownerObject.InvokeMethod(attribute.dynamicInitMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, attribute);
		}
	}

	public void DynamicInitialize(TreeNode<ReflectTreeData<UIFieldAttribute>> node)
	{
		if (!dynamicInitMethod.IsNullOrEmpty())
		{
			while (node != null && !node.value.ownerObject.InvokeMethod(dynamicInitMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, this))
			{
				node = node.parent;
			}
		}
	}

	public static UIFieldCollectionItemAttribute GetCollectionItemAttribute(object ownerObject, MemberInfo info)
	{
		UIFieldCollectionItemAttribute attribute = info.GetAttribute<UIFieldCollectionItemAttribute>();
		if (attribute != null)
		{
			InitializeFromMemberInfo(attribute, ownerObject, info);
		}
		return attribute;
	}
}
