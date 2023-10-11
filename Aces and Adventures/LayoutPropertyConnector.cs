using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LayoutPropertyConnector : UIBehaviour, ILayoutElement
{
	[SerializeField]
	private RectTransform _layoutReference;

	[SerializeField]
	private bool _minWidth;

	[SerializeField]
	private bool _minHeight;

	[SerializeField]
	private bool _preferredWidth;

	[SerializeField]
	private bool _preferredHeight;

	[SerializeField]
	private bool _flexibleWidth;

	[SerializeField]
	private bool _flexibleHeight;

	[SerializeField]
	private PaddingSimple _preferredPadding;

	private ILayoutElement _layout;

	private float _previousPreferredWidth;

	private float _previousPreferredHeight;

	public RectTransform layoutReference
	{
		get
		{
			return _layoutReference;
		}
		set
		{
			_layoutReference = value;
			SetDirty();
		}
	}

	public bool connectMinWidth
	{
		get
		{
			if (_minWidth)
			{
				return layout != null;
			}
			return false;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _minWidth, value))
			{
				SetDirty();
			}
		}
	}

	public bool connectMinHeight
	{
		get
		{
			if (_minHeight)
			{
				return layout != null;
			}
			return false;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _minHeight, value))
			{
				SetDirty();
			}
		}
	}

	public bool connectPreferredWidth
	{
		get
		{
			if (_preferredWidth)
			{
				return layout != null;
			}
			return false;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _preferredWidth, value))
			{
				SetDirty();
			}
		}
	}

	public bool connectPreferredHeight
	{
		get
		{
			if (_preferredHeight)
			{
				return layout != null;
			}
			return false;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _preferredHeight, value))
			{
				SetDirty();
			}
		}
	}

	public bool connectFlexibleWidth
	{
		get
		{
			if (_flexibleWidth)
			{
				return layout != null;
			}
			return false;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _flexibleWidth, value))
			{
				SetDirty();
			}
		}
	}

	public bool connectFlexibleHeight
	{
		get
		{
			if (_flexibleHeight)
			{
				return layout != null;
			}
			return false;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _flexibleHeight, value))
			{
				SetDirty();
			}
		}
	}

	public ILayoutElement layout => _layout ?? (_layout = ((_layoutReference != null) ? _layoutReference.GetComponents<Component>().SelectValid((Component c) => c as ILayoutElement).FirstOrDefault() : null));

	public virtual float minWidth
	{
		get
		{
			if (connectMinWidth)
			{
				return layout.minWidth;
			}
			return -1f;
		}
	}

	public virtual float minHeight
	{
		get
		{
			if (connectMinHeight)
			{
				return layout.minHeight;
			}
			return -1f;
		}
	}

	public virtual float preferredWidth
	{
		get
		{
			if (connectPreferredWidth)
			{
				return layout.preferredWidth + _preferredPadding.horizontal;
			}
			return -1f;
		}
	}

	public virtual float preferredHeight
	{
		get
		{
			if (connectPreferredHeight)
			{
				return layout.preferredHeight + _preferredPadding.vertical;
			}
			return -1f;
		}
	}

	public virtual float flexibleWidth
	{
		get
		{
			if (connectFlexibleWidth)
			{
				return layout.flexibleWidth;
			}
			return -1f;
		}
	}

	public virtual float flexibleHeight
	{
		get
		{
			if (connectFlexibleHeight)
			{
				return layout.flexibleHeight;
			}
			return -1f;
		}
	}

	public virtual int layoutPriority => 1;

	protected void SetDirty()
	{
		if (base.isActiveAndEnabled)
		{
			LayoutRebuilder.MarkLayoutForRebuild(base.transform as RectTransform);
		}
	}

	private void _CheckDirty()
	{
		if ((connectPreferredWidth && SetPropertyUtility.SetStruct(ref _previousPreferredWidth, layout.preferredWidth)) | (connectPreferredHeight && SetPropertyUtility.SetStruct(ref _previousPreferredHeight, layout.preferredHeight)))
		{
			SetDirty();
		}
	}

	public virtual void CalculateLayoutInputHorizontal()
	{
	}

	public virtual void CalculateLayoutInputVertical()
	{
	}

	private void Update()
	{
		_CheckDirty();
	}

	private void LateUpdate()
	{
		_CheckDirty();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SetDirty();
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		SetDirty();
	}

	protected override void OnDisable()
	{
		SetDirty();
		base.OnDisable();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		SetDirty();
	}

	protected override void OnBeforeTransformParentChanged()
	{
		SetDirty();
	}
}
