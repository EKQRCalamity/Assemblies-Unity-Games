using UnityEngine;
using UnityEngine.UI;

public class UIHorizontalLayoutAttribute : UILayoutAttribute
{
	private bool _expandWidth = true;

	private bool _expandHeight = true;

	private float _preferredWidth = -1f;

	private float _flexibleWidth = -1f;

	private float _minWidth = -1f;

	public int left { get; set; }

	public int right { get; set; }

	public int top { get; set; }

	public int bottom { get; set; }

	public float spacing { get; set; }

	public TextAnchor alignment { get; set; }

	public bool expandWidth
	{
		get
		{
			return _expandWidth;
		}
		set
		{
			_expandWidth = value;
		}
	}

	public bool expandHeight
	{
		get
		{
			return _expandHeight;
		}
		set
		{
			_expandHeight = value;
		}
	}

	public float preferredWidth
	{
		get
		{
			return _preferredWidth;
		}
		set
		{
			_preferredWidth = value;
		}
	}

	public float flexibleWidth
	{
		get
		{
			return _flexibleWidth;
		}
		set
		{
			_flexibleWidth = value;
		}
	}

	public float minWidth
	{
		get
		{
			return _minWidth;
		}
		set
		{
			_minWidth = value;
		}
	}

	public UIHorizontalLayoutAttribute()
	{
	}

	public UIHorizontalLayoutAttribute(string name)
	{
		base.name = name;
	}

	public override GameObject CreateLayoutObject(Transform parent)
	{
		HorizontalLayoutGroup horizontalLayoutGroup = new GameObject("UIHorizontalLayout").transform.SetParentAndReturn(parent, worldPositionStays: false).gameObject.AddComponent<HorizontalLayoutGroup>();
		horizontalLayoutGroup.childAlignment = alignment;
		horizontalLayoutGroup.padding = new RectOffset(left, right, top, bottom);
		horizontalLayoutGroup.spacing = spacing;
		horizontalLayoutGroup.childForceExpandWidth = expandWidth;
		horizontalLayoutGroup.childForceExpandHeight = expandHeight;
		return horizontalLayoutGroup.gameObject;
	}

	public override void SetLayoutElementData(GameObject gameObject)
	{
		if (!(flexibleWidth < 0f) || !(preferredWidth < 0f) || !(minWidth < 0f))
		{
			LayoutElement orAddComponent = gameObject.GetOrAddComponent<LayoutElement>();
			if (flexibleWidth >= 0f)
			{
				orAddComponent.flexibleWidth = flexibleWidth;
			}
			if (preferredWidth >= 0f)
			{
				orAddComponent.preferredWidth = preferredWidth;
			}
			if (minWidth >= 0f)
			{
				orAddComponent.minWidth = minWidth;
			}
		}
	}
}
