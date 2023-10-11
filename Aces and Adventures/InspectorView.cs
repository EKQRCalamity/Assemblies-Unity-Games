using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InspectorView : MonoBehaviour
{
	[SerializeField]
	protected ObjectEvent _onInspectedItemChange;

	[SerializeField]
	protected StringEvent _onSelectedTextChange;

	[SerializeField]
	protected UICategorySetEvent[] _OnUICategorySetChange;

	[SerializeField]
	protected BoolEvent[] _OnInspectorEnableChange;

	private List<IInspectable> _inspectedItems;

	private IInspectable _inspectedItem;

	private string _inspectedItemName;

	protected List<IInspectable> inspectedItems
	{
		get
		{
			return _inspectedItems ?? (_inspectedItems = new List<IInspectable>());
		}
		set
		{
			if (!inspectedItems.SequenceEqual(value, ReferenceEqualityComparer<IInspectable>.Default))
			{
				_OnInspectedItemsChanged(value);
			}
		}
	}

	protected IInspectable inspectedItem
	{
		get
		{
			return _inspectedItem;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _inspectedItem, value))
			{
				_OnInspectedItemChange();
			}
		}
	}

	protected object inspectedValue
	{
		get
		{
			if (_inspectedItem == null)
			{
				return null;
			}
			return _inspectedItem.inspectedValue;
		}
	}

	public ObjectEvent onInspectedItemChange => _onInspectedItemChange ?? (_onInspectedItemChange = new ObjectEvent());

	public StringEvent onSelectedTextChange => _onSelectedTextChange ?? (_onSelectedTextChange = new StringEvent());

	private void _OnInspectedItemsChanged(List<IInspectable> newInspectedItems)
	{
		inspectedItems.ClearAndCopyFrom(newInspectedItems);
		inspectedItem = ((inspectedItems.Count == 1) ? inspectedItems[0] : null);
		onSelectedTextChange.Invoke(_GetInspectedItemsSelectionText(inspectedItems));
	}

	private void _OnInspectedItemChange()
	{
		SetUICategorySets((inspectedItem != null) ? inspectedItem.uiCategorySets : null);
		onInspectedItemChange.Invoke(inspectedValue);
		if (inspectedItem != null)
		{
			_inspectedItemName = null;
			OnInspectedItemValueChange();
		}
	}

	private string _GetInspectedItemsSelectionText(List<IInspectable> selection)
	{
		return (from i in selection
			select i.inspectedValue into o
			group o by o.GetType()).ToStringSmart((IGrouping<Type, object> g) => $"{g.Key.GetUILabel()} <size=75%>x{g.Count()}</size>", "\n");
	}

	public void UpdateSelection(List<Component> selection)
	{
		using PoolKeepItemListHandle<IInspectable> poolKeepItemListHandle = Pools.UseKeepItemList<IInspectable>();
		foreach (Component item in selection)
		{
			if (item is IInspectable)
			{
				poolKeepItemListHandle.Add(item as IInspectable);
			}
		}
		inspectedItems = poolKeepItemListHandle;
	}

	public void OnInspectedItemValueChange()
	{
		if (SetPropertyUtility.SetObject(ref _inspectedItemName, inspectedItem.inspectedName))
		{
			inspectedItem.inspectedName = inspectedItem.inspectedName;
		}
	}

	public void SetUICategorySets(UICategorySet[] categorySets)
	{
		if (categorySets == null)
		{
			if (_OnUICategorySetChange != null)
			{
				UICategorySetEvent[] onUICategorySetChange = _OnUICategorySetChange;
				for (int i = 0; i < onUICategorySetChange.Length; i++)
				{
					onUICategorySetChange[i].Invoke(UICategorySet.Default);
				}
			}
			if (_OnInspectorEnableChange != null)
			{
				BoolEvent[] onInspectorEnableChange = _OnInspectorEnableChange;
				for (int i = 0; i < onInspectorEnableChange.Length; i++)
				{
					onInspectorEnableChange[i].Invoke(arg0: false);
				}
			}
			return;
		}
		if (_OnUICategorySetChange != null)
		{
			for (int j = 0; j < _OnUICategorySetChange.Length; j++)
			{
				_OnUICategorySetChange[j].Invoke((j < categorySets.Length) ? categorySets[j] : UICategorySet.Default);
			}
		}
		if (_OnInspectorEnableChange != null)
		{
			for (int k = 0; k < _OnInspectorEnableChange.Length; k++)
			{
				_OnInspectorEnableChange[k].Invoke(k < categorySets.Length);
			}
		}
	}
}
