using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AUISearchGeneric<T> : MonoBehaviour
{
	private UISearchObjectsView _searchObjectsView;

	private T _selected;

	private List<T> _list;

	private UISearchObjectsView searchObjectsView => _CacheSearchObjectsView();

	public List<T> list
	{
		get
		{
			return _list ?? (_list = new List<T>());
		}
		set
		{
			searchObjectsView.objects = (_list = value).Cast<object>().ToList();
		}
	}

	public T selected
	{
		get
		{
			return _selected;
		}
		set
		{
			searchObjectsView.selectedIndex = list.IndexOf(_selected = value);
		}
	}

	public int selectedIndex
	{
		get
		{
			return searchObjectsView.selectedIndex;
		}
		set
		{
			searchObjectsView.selectedIndex = value;
		}
	}

	private UISearchObjectsView _CacheSearchObjectsView()
	{
		if (_searchObjectsView == null)
		{
			this.CacheComponentInChildren(ref _searchObjectsView);
			_searchObjectsView.getNameFunc = (object obj) => _GetName((T)obj);
			_searchObjectsView.getImageFunc = (object obj) => _GetTexture2D((T)obj);
			_searchObjectsView.getUVCoordsFunc = (object obj) => _GetUVCoords((T)obj);
			_searchObjectsView.onSelectedObjectChange.AddListener(_OnSelectedObjectChange);
			_searchObjectsView.getDescriptionFunc = (object obj) => _GetDescription((T)obj);
		}
		return _searchObjectsView;
	}

	protected abstract void _OnSelectedObjectChange(object obj);

	protected abstract string _GetName(T value);

	protected abstract Texture2D _GetTexture2D(T value);

	protected abstract UVCoords _GetUVCoords(T value);

	protected abstract string _GetDescription(T value);
}
