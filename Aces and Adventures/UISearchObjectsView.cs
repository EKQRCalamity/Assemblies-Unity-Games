using System;
using System.Collections.Generic;
using UnityEngine;

public class UISearchObjectsView : MonoBehaviour
{
	[SerializeField]
	protected IntEvent _onCountChange;

	[SerializeField]
	protected IntEvent _onMaxIndexChange;

	[SerializeField]
	protected IntEvent _onSelectedIndexChange;

	[SerializeField]
	protected ObjectEvent _onSelectedObjectChange;

	[SerializeField]
	protected StringEvent _onNameChange;

	[SerializeField]
	protected Texture2DEvent _onImageChange;

	[SerializeField]
	protected UVCoordsEvent _onImageUVChange;

	[SerializeField]
	protected StringEvent _onDescriptionChange;

	private List<object> _objects;

	private int _selectedIndex = -1;

	private bool _isDirty;

	public IntEvent onCountChange => _onCountChange ?? (_onCountChange = new IntEvent());

	public IntEvent onMaxIndexChange => _onMaxIndexChange ?? (_onMaxIndexChange = new IntEvent());

	public IntEvent onSelectedIndexChange => _onSelectedIndexChange ?? (_onSelectedIndexChange = new IntEvent());

	public ObjectEvent onSelectedObjectChange => _onSelectedObjectChange ?? (_onSelectedObjectChange = new ObjectEvent());

	public StringEvent onNameChange => _onNameChange ?? (_onNameChange = new StringEvent());

	public Texture2DEvent onImageChange => _onImageChange ?? (_onImageChange = new Texture2DEvent());

	public UVCoordsEvent onImageUVChange => _onImageUVChange ?? (_onImageUVChange = new UVCoordsEvent());

	public StringEvent onDescriptionChange => _onDescriptionChange ?? (_onDescriptionChange = new StringEvent());

	public List<object> objects
	{
		private get
		{
			return _objects ?? (objects = new List<object>());
		}
		set
		{
			_SetObjects(value);
		}
	}

	public int selectedIndex
	{
		get
		{
			return objects.SafeIndex(_selectedIndex);
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _selectedIndex, objects.SafeIndex(value)))
			{
				_SetDirty();
			}
		}
	}

	public Func<object, string> getNameFunc { get; set; }

	public Func<object, Texture2D> getImageFunc { get; set; }

	public Func<object, UVCoords> getUVCoordsFunc { get; set; }

	public Func<object, string> getDescriptionFunc { get; set; }

	private void _SetDirty()
	{
		_isDirty = true;
	}

	private void _SetObjects(List<object> newObjects)
	{
		_objects = newObjects;
		_SetDirty();
	}

	private void _OnSelectedObjectChange()
	{
		if (objects.IsNullOrEmpty())
		{
			return;
		}
		int num = selectedIndex;
		object obj = objects[num];
		onSelectedIndexChange.Invoke(num);
		onSelectedObjectChange.Invoke(obj);
		if (obj != null)
		{
			onNameChange.Invoke((getNameFunc != null) ? getNameFunc(obj) : obj.ToString());
			if (getImageFunc != null)
			{
				onImageChange.Invoke(getImageFunc(obj));
			}
			if (getUVCoordsFunc != null)
			{
				onImageUVChange.Invoke(getUVCoordsFunc(obj));
			}
			if (getDescriptionFunc != null)
			{
				onDescriptionChange.Invoke(getDescriptionFunc(obj));
			}
		}
	}

	private void LateUpdate()
	{
		if (_isDirty)
		{
			_isDirty = false;
			onCountChange.Invoke(objects.Count);
			onMaxIndexChange.Invoke(Mathf.Max(0, objects.Count - 1));
			_OnSelectedObjectChange();
		}
	}
}
