using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasInputFocus : UIBehaviour
{
	private static readonly Dictionary<Canvas, CanvasInputFocus> _FocusPerCanvas = new Dictionary<Canvas, CanvasInputFocus>();

	private static Action OnFocusDirty;

	private bool _dirty;

	private Canvas _parentCanvas;

	private bool _hasFocus;

	public static bool HasActiveComponents => OnFocusDirty != null;

	public Canvas parentCanvas
	{
		get
		{
			return this.CacheComponentInParent(ref _parentCanvas);
		}
		private set
		{
			_parentCanvas = null;
		}
	}

	public bool hasFocus
	{
		get
		{
			if (base.isActiveAndEnabled && (_dirty ? (_hasFocus = _HasFocus()) : _hasFocus))
			{
				return !CanvasInputFocusBlocker.IsBlocking;
			}
			return false;
		}
	}

	private static void _SignalFocusDirty()
	{
		OnFocusDirty?.Invoke();
	}

	private void SetDirty()
	{
		_dirty = true;
		Canvas canvas = parentCanvas;
		if ((bool)canvas)
		{
			_FocusPerCanvas.Remove(canvas);
		}
	}

	private bool _HasFocus()
	{
		_dirty = false;
		if (!parentCanvas)
		{
			return false;
		}
		if (!_FocusPerCanvas.ContainsKey(parentCanvas))
		{
			_FocusPerCanvas.Add(parentCanvas, parentCanvas.gameObject.GetComponentsInChildrenPooled<CanvasInputFocus>().AsEnumerable().LastOrDefault());
		}
		return this == _FocusPerCanvas[parentCanvas];
	}

	protected override void OnTransformParentChanged()
	{
		parentCanvas = null;
		_SignalFocusDirty();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		OnFocusDirty = (Action)Delegate.Combine(OnFocusDirty, new Action(SetDirty));
		_SignalFocusDirty();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		OnFocusDirty = (Action)Delegate.Remove(OnFocusDirty, new Action(SetDirty));
		_SignalFocusDirty();
	}
}
