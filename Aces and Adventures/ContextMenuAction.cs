using System;
using System.Collections.Generic;
using System.Linq;

public class ContextMenuAction
{
	private static readonly char[] Split = new char[2] { '/', '\\' };

	private Action<ContextMenuContext> _action;

	private string _path;

	private string _category;

	private HotKey? _hotKey;

	private Func<ContextMenuContext, bool> _inContext;

	private Func<ContextMenuContext, bool> _isActive;

	private string _icon;

	private bool _mouseMustBeInRectForHotKey;

	private string _name;

	private List<ContextMenuAction> _subActions;

	public Action<ContextMenuContext> action => delegate
	{
	};

	public string path
	{
		get
		{
			return _path ?? (_path = "");
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _path, value))
			{
				_name = null;
			}
		}
	}

	public string name => _name ?? (_name = path.Split(Split)[0]);

	public bool isFolder => !_subActions.IsNullOrEmpty();

	public List<ContextMenuAction> subActions => _subActions;

	public string category => _category ?? (_category = "");

	public HotKey? hotKey => _hotKey;

	public string icon => _icon;

	public bool mouseMustBeInRectForHotKey => _mouseMustBeInRectForHotKey;

	protected Func<ContextMenuContext, bool> inContext => (ContextMenuContext list) => true;

	protected Func<ContextMenuContext, bool> isActive => (ContextMenuContext list) => true;

	public static List<ContextMenuAction> ParseActions(IEnumerable<ContextMenuAction> contextMenuActions)
	{
		List<ContextMenuAction> list = new List<ContextMenuAction>();
		PoolDictionaryValuesHandle<string, List<ContextMenuAction>> poolDictionaryValuesHandle = Pools.UseDictionaryValues<string, List<ContextMenuAction>>();
		foreach (ContextMenuAction contextMenuAction in contextMenuActions)
		{
			poolDictionaryValuesHandle.value.GetOrAdd(contextMenuAction.name, Pools.TryUnpoolList<ContextMenuAction>).Add(contextMenuAction);
		}
		foreach (KeyValuePair<string, List<ContextMenuAction>> item in poolDictionaryValuesHandle)
		{
			list.Add((item.Value.Count == 1) ? item.Value[0] : new ContextMenuAction(item.Key, item.Value[0].category, item.Value));
		}
		return list;
	}

	private ContextMenuAction(string path, string category, IEnumerable<ContextMenuAction> subActions)
	{
		_path = path;
		_category = category;
		_subActions = ParseActions(subActions.Select((ContextMenuAction c) => c._ToSubAction()));
	}

	public ContextMenuAction(string path, string category = null, Action<ContextMenuContext> action = null, HotKey? hotKey = null, Func<ContextMenuContext, bool> inContext = null, Func<ContextMenuContext, bool> isActive = null, string icon = null, bool mouseMustBeInRectForHotKey = true)
	{
		_action = action;
		_path = path;
		_category = category;
		_hotKey = hotKey;
		_inContext = inContext;
		_isActive = isActive;
		_icon = icon;
		_mouseMustBeInRectForHotKey = mouseMustBeInRectForHotKey;
	}

	private ContextMenuAction _ToSubAction()
	{
		path = path.Split(Split, 2)[1];
		return this;
	}

	public PoolKeepItemListHandle<ContextMenuAction> GetActions()
	{
		if (!isFolder)
		{
			return Pools.UseKeepItemList(this);
		}
		return Pools.UseKeepItemList(_subActions);
	}

	public PoolKeepItemListHandle<ContextMenuAction> GetActionsRecursive(PoolKeepItemListHandle<ContextMenuAction> output = null)
	{
		output = output ?? Pools.UseKeepItemList<ContextMenuAction>();
		if (isFolder)
		{
			foreach (ContextMenuAction subAction in _subActions)
			{
				output.Add(subAction);
				subAction.GetActionsRecursive(output);
			}
			return output;
		}
		output.Add(this);
		return output;
	}

	public bool IsInContext(ContextMenuContext context)
	{
		if (!isFolder)
		{
			return inContext(context);
		}
		foreach (ContextMenuAction subAction in _subActions)
		{
			if (subAction.IsInContext(context))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsActive(ContextMenuContext context)
	{
		if (!isFolder)
		{
			return isActive(context);
		}
		return true;
	}

	public bool DoAction(ContextMenuContext context)
	{
		if (isFolder || !IsActive(context))
		{
			return false;
		}
		if (IsInContext(context))
		{
			action(context);
		}
		return true;
	}
}
