using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class ReflectTreeData<T> : IComparable<ReflectTreeData<T>>
{
	public Func<object> getValueLogic;

	public Action<object> setValueLogic;

	public Action countChangedLogic;

	public Type underlyingType;

	public bool shouldHide;

	public bool skipSetValueBubbling;

	public Func<object, bool> excludedValues;

	public HashSet<ReflectTreeData<T>> dependentOn = new HashSet<ReflectTreeData<T>>();

	public HashSet<ReflectTreeData<T>> hookedDependencies = new HashSet<ReflectTreeData<T>>();

	public Action hook;

	public Action OnValueChanged;

	public Action OnSetValue;

	public uint order { get; protected set; }

	public object self { get; set; }

	public object originalCollection { get; set; }

	public object ownerObject { get; protected set; }

	public MemberInfo memberInfo { get; protected set; }

	public bool isProperty
	{
		get
		{
			if (memberInfo != null)
			{
				return memberInfo is PropertyInfo;
			}
			return false;
		}
	}

	public bool isMethod
	{
		get
		{
			if (memberInfo != null)
			{
				return memberInfo is MethodInfo;
			}
			return false;
		}
	}

	public T data { get; set; }

	public bool CanRead
	{
		get
		{
			if (!(memberInfo != null) || ownerObject == null)
			{
				return getValueLogic != null;
			}
			return true;
		}
	}

	public bool CanWrite
	{
		get
		{
			if (!(memberInfo != null) || ownerObject == null)
			{
				return setValueLogic != null;
			}
			return true;
		}
	}

	public bool CanReadAndWrite
	{
		get
		{
			if (CanRead)
			{
				return CanWrite;
			}
			return false;
		}
	}

	public Type GetUnderlyingType()
	{
		Type type = underlyingType ?? ((memberInfo != null) ? memberInfo.GetUnderlyingType() : ((self != null) ? self.GetType() : null));
		if (!(type == typeof(Enum)))
		{
			return type;
		}
		return self.GetType();
	}

	public ReflectTreeData(uint order, object self, object ownerObject = null, MemberInfo memberInfo = null, T data = default(T))
	{
		this.order = order;
		this.self = self;
		this.ownerObject = ownerObject;
		this.memberInfo = memberInfo;
		this.data = data;
	}

	public object GetValue()
	{
		if (getValueLogic != null)
		{
			return getValueLogic();
		}
		if (memberInfo != null && ownerObject != null)
		{
			return memberInfo.AttemptGetValue(ownerObject);
		}
		return null;
	}

	public void SignalCountChanged()
	{
		if (countChangedLogic != null)
		{
			countChangedLogic();
		}
		SignalValueChanged();
	}

	public void SignalValueChanged()
	{
		if (OnValueChanged != null)
		{
			OnValueChanged();
		}
	}

	public void SetValue(TreeNode<ReflectTreeData<T>> owningNode, object value)
	{
		if (!CanWrite)
		{
			return;
		}
		if (excludedValues != null && excludedValues.GetInvocationList().Cast<Func<object, bool>>().Any((Func<object, bool> excludedValue) => excludedValue(value)))
		{
			if (OnSetValue != null)
			{
				OnSetValue();
			}
			return;
		}
		object a = ((OnValueChanged != null || OnSetValue != null) ? GetValue() : null);
		if (setValueLogic != null)
		{
			setValueLogic(value);
		}
		else
		{
			memberInfo.AttemptSetValue(ownerObject, value);
		}
		if (!skipSetValueBubbling && ownerObject != null && owningNode.parent != null && ownerObject.GetType().IsNonPrimitiveStruct())
		{
			owningNode.parent.value.SetValue(owningNode.parent, ownerObject);
		}
		if (OnValueChanged != null && !ReflectionUtil.SafeEquals(a, GetValue()))
		{
			OnValueChanged();
		}
		if (OnSetValue != null && !ReflectionUtil.SafeEquals(a, value))
		{
			OnSetValue();
		}
	}

	public void Invoke()
	{
		(memberInfo as MethodInfo).Invoke(ownerObject, null);
	}

	public void AddDependencyLink(ReflectTreeData<T> linkTo)
	{
		dependentOn.Add(linkTo);
		linkTo.dependentOn.Add(this);
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}", (memberInfo != null) ? memberInfo.Name : "", ReflectionUtil.SafeToString(GetValue()));
	}

	public int CompareTo(ReflectTreeData<T> other)
	{
		int num = (int)(order - other.order);
		if (num != 0)
		{
			return num;
		}
		if (memberInfo != null && other.memberInfo != null)
		{
			int num2 = memberInfo.Name.CompareTo(other.memberInfo.Name);
			if (num2 != 0)
			{
				return num2;
			}
		}
		return GetHashCode() - other.GetHashCode();
	}
}
