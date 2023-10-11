using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UITabs : LayoutGroup
{
	[Serializable]
	public class UITabData
	{
		public string tabName;

		[HideInInspector]
		[SerializeField]
		public RectTransform tab;

		public GameObject tabContent;

		[NonSerialized]
		[HideInInspector]
		public bool enabled = true;

		public UITabData(string tabName, RectTransform tab, GameObject tabContent, bool enabled = true)
		{
			this.tabName = tabName;
			this.tab = tab;
			this.tabContent = tabContent;
			this.enabled = enabled;
		}
	}

	[Serializable]
	public struct UITabEvent
	{
		public string tabName;

		public UnityEvent onEvent;

		public UITabEvent(string tabName, UnityEvent onEvent)
		{
			this.tabName = tabName;
			this.onEvent = onEvent;
		}
	}

	[SerializeField]
	protected float m_Spacing = -16f;

	[SerializeField]
	protected bool m_ChildForceExpandWidth;

	[SerializeField]
	protected bool m_ChildForceExpandHeight;

	[Header("Tabs")]
	public SpriteToggleSwapper tabBlueprint;

	public float disabledTabYOffset = -24f;

	[SerializeField]
	protected List<UITabData> _tabs;

	private string _selectedTab = "";

	[SerializeField]
	protected List<UITabEvent> _onTabSelected;

	public float spacing
	{
		get
		{
			return m_Spacing;
		}
		set
		{
			SetProperty(ref m_Spacing, value);
		}
	}

	public bool childForceExpandWidth
	{
		get
		{
			return m_ChildForceExpandWidth;
		}
		set
		{
			SetProperty(ref m_ChildForceExpandWidth, value);
		}
	}

	public bool childForceExpandHeight
	{
		get
		{
			return m_ChildForceExpandHeight;
		}
		set
		{
			SetProperty(ref m_ChildForceExpandHeight, value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (Application.isPlaying)
		{
			for (int i = 0; i < _tabs.Count; i++)
			{
				_CreateTab(_tabs[i]);
			}
			if (_tabs.Count > 0)
			{
				SelectTab(_tabs[0].tabName);
			}
		}
	}

	protected UITabData _GetTab(string tabName)
	{
		for (int i = 0; i < _tabs.Count; i++)
		{
			if (_tabs[i].tabName.Equals(tabName, StringComparison.OrdinalIgnoreCase))
			{
				return _tabs[i];
			}
		}
		return null;
	}

	protected void _CreateTab(UITabData data)
	{
		SpriteToggleSwapper spriteToggleSwapper = UnityEngine.Object.Instantiate(tabBlueprint);
		data.enabled = true;
		spriteToggleSwapper.transform.SetParent(base.transform, worldPositionStays: false);
		spriteToggleSwapper.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true).text = data.tabName;
		spriteToggleSwapper.GetComponent<Button>().onClick.AddListener(delegate
		{
			SelectTab(data.tabName);
		});
		data.tab = spriteToggleSwapper.transform as RectTransform;
		_UnselectTab(data.tabName);
	}

	protected void _UnselectTab(string tabName)
	{
		UITabData uITabData = _GetTab(tabName);
		SpriteToggleSwapper component = uITabData.tab.GetComponent<SpriteToggleSwapper>();
		component.isOn = false;
		component.GetComponent<SlideAnim>().SetState(open: false);
		component.GetComponentInChildren<TextMeshHook>(includeInactive: true).SetUnderline(isUnderlined: false);
		if (uITabData.tabContent != null)
		{
			uITabData.tabContent.SetActive(value: false);
		}
	}

	protected void _RaiseEvent(List<UITabEvent> eventList, string tabName)
	{
		for (int i = 0; i < eventList.Count; i++)
		{
			if (eventList[i].tabName.Equals(tabName, StringComparison.OrdinalIgnoreCase))
			{
				eventList[i].onEvent.Invoke();
			}
		}
	}

	protected void _CalcAlongAxis(int axis, bool isVertical)
	{
		float num = ((axis == 0) ? base.padding.horizontal : base.padding.vertical);
		float num2 = num;
		float num3 = num;
		float num4 = 0f;
		bool flag = isVertical ^ (axis == 1);
		for (int i = 0; i < _tabs.Count; i++)
		{
			RectTransform tab = _tabs[i].tab;
			float minSize = LayoutUtility.GetMinSize(tab, axis);
			float preferredSize = LayoutUtility.GetPreferredSize(tab, axis);
			float num5 = LayoutUtility.GetFlexibleSize(tab, axis);
			if ((axis == 0) ? childForceExpandWidth : childForceExpandHeight)
			{
				num5 = Mathf.Max(num5, 1f);
			}
			if (flag)
			{
				num2 = Mathf.Max(minSize + num, num2);
				num3 = Mathf.Max(preferredSize + num, num3);
				num4 = Mathf.Max(num5, num4);
			}
			else
			{
				num2 += minSize + spacing;
				num3 += preferredSize + spacing;
				num4 += num5;
			}
		}
		if (!flag && _tabs.Count > 0)
		{
			num2 -= spacing;
			num3 -= spacing;
		}
		num3 = Mathf.Max(num2, num3);
		SetLayoutInputForAxis(num2, num3, num4, axis);
	}

	protected void _SetChildrenAlongAxis(int axis, bool isVertical)
	{
		float num = base.rectTransform.rect.size[axis];
		if (isVertical ^ (axis == 1))
		{
			float value = num - (float)((axis == 0) ? base.padding.horizontal : base.padding.vertical);
			for (int i = 0; i < _tabs.Count; i++)
			{
				RectTransform tab = _tabs[i].tab;
				float minSize = LayoutUtility.GetMinSize(tab, axis);
				float preferredSize = LayoutUtility.GetPreferredSize(tab, axis);
				float num2 = LayoutUtility.GetFlexibleSize(tab, axis);
				if ((axis == 0) ? childForceExpandWidth : childForceExpandHeight)
				{
					num2 = Mathf.Max(num2, 1f);
				}
				float num3 = Mathf.Clamp(value, minSize, (num2 > 0f) ? num : preferredSize);
				float startOffset = GetStartOffset(axis, num3);
				float num4 = (_tabs[i].enabled ? 0f : disabledTabYOffset);
				SetChildAlongAxis(tab, axis, startOffset + num4, num3);
			}
			return;
		}
		float num5 = ((axis == 0) ? base.padding.left : base.padding.top);
		if (GetTotalFlexibleSize(axis) == 0f && GetTotalPreferredSize(axis) < num)
		{
			num5 = GetStartOffset(axis, GetTotalPreferredSize(axis) - (float)((axis == 0) ? base.padding.horizontal : base.padding.vertical));
		}
		float t = 0f;
		if (GetTotalMinSize(axis) != GetTotalPreferredSize(axis))
		{
			t = Mathf.Clamp01((num - GetTotalMinSize(axis)) / (GetTotalPreferredSize(axis) - GetTotalMinSize(axis)));
		}
		float num6 = 0f;
		if (num > GetTotalPreferredSize(axis) && GetTotalFlexibleSize(axis) > 0f)
		{
			num6 = (num - GetTotalPreferredSize(axis)) / GetTotalFlexibleSize(axis);
		}
		for (int j = 0; j < _tabs.Count; j++)
		{
			RectTransform tab2 = _tabs[j].tab;
			float minSize2 = LayoutUtility.GetMinSize(tab2, axis);
			float preferredSize2 = LayoutUtility.GetPreferredSize(tab2, axis);
			float num7 = LayoutUtility.GetFlexibleSize(tab2, axis);
			if ((axis == 0) ? childForceExpandWidth : childForceExpandHeight)
			{
				num7 = Mathf.Max(num7, 1f);
			}
			float num8 = Mathf.Lerp(minSize2, preferredSize2, t);
			num8 += num7 * num6;
			SetChildAlongAxis(tab2, axis, num5, num8);
			num5 += num8 + spacing;
		}
	}

	protected void _UpdateSelectedTabDueToEnabledState()
	{
		if (!ContainsTab(_selectedTab) || _GetTab(_selectedTab).enabled)
		{
			return;
		}
		for (int i = 0; i < _tabs.Count; i++)
		{
			if (_tabs[i].enabled)
			{
				SelectTab(_tabs[i].tabName);
				break;
			}
		}
	}

	public bool ContainsTab(string tabName)
	{
		for (int i = 0; i < _tabs.Count; i++)
		{
			if (_tabs[i].tabName.Equals(tabName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public void SelectTab(string tabName)
	{
		if (!_selectedTab.Equals(tabName, StringComparison.OrdinalIgnoreCase) && ContainsTab(tabName))
		{
			if (ContainsTab(_selectedTab))
			{
				_UnselectTab(_selectedTab);
			}
			_selectedTab = tabName;
			UITabData uITabData = _GetTab(tabName);
			SpriteToggleSwapper component = uITabData.tab.GetComponent<SpriteToggleSwapper>();
			component.isOn = true;
			component.GetComponent<SlideAnim>().SetState(open: true);
			component.GetComponentInChildren<TextMeshHook>(includeInactive: true).SetUnderline(isUnderlined: true);
			for (int num = _tabs.Count - 1; num >= 0; num--)
			{
				_tabs[num].tab.SetAsLastSibling();
			}
			component.transform.SetAsLastSibling();
			_RaiseEvent(_onTabSelected, tabName);
			if (uITabData.tabContent != null)
			{
				uITabData.tabContent.SetActive(value: true);
			}
		}
	}

	public void SetTabEnabledState(string tabName, bool enabled)
	{
		UITabData uITabData = _GetTab(tabName);
		if (uITabData != null && uITabData.enabled != enabled)
		{
			uITabData.enabled = enabled;
			uITabData.tab.GetComponentInChildren<Button>().interactable = enabled;
			_UpdateSelectedTabDueToEnabledState();
			SetDirty();
		}
	}

	public void RemoveTab(string tabName)
	{
	}

	public void AddTab(string tabName, GameObject tabContent)
	{
	}

	public void SetTabs(Dictionary<string, GameObject> tabData)
	{
	}

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
		_CalcAlongAxis(0, isVertical: false);
	}

	public override void CalculateLayoutInputVertical()
	{
		_CalcAlongAxis(1, isVertical: false);
	}

	public override void SetLayoutHorizontal()
	{
		_SetChildrenAlongAxis(0, isVertical: false);
	}

	public override void SetLayoutVertical()
	{
		_SetChildrenAlongAxis(1, isVertical: false);
	}
}
