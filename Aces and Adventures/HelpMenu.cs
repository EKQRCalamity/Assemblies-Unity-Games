using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform), typeof(CanvasInputFocus))]
public class HelpMenu : MonoBehaviour
{
	public RectTransform pageContainer;

	[Range(1f, 1000f)]
	public float springConstant = 110f;

	[Range(1f, 100f)]
	public float springDampening = 15f;

	public StringEvent onPageTextChange;

	public BoolEvent onActiveChange;

	public FloatEvent onAlphaChange;

	private List<HelpPage> _pages;

	private int _page;

	private float _pagePosition;

	private float _pagePositionVelocity;

	private float _targetPagePosition;

	private CanvasInputFocus _inputFocus;

	private bool _active;

	private float _alphaPagePosition;

	public List<HelpPage> pages => _pages ?? (_pages = new List<HelpPage>(pageContainer.GetComponentsInChildren<HelpPage>(includeInactive: true)));

	public int page
	{
		get
		{
			return _page;
		}
		set
		{
			if (_page != value)
			{
				_OnPageChange(value);
			}
		}
	}

	public CanvasInputFocus inputFocus => this.CacheComponent(ref _inputFocus);

	public bool active
	{
		get
		{
			return _active;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _active, value))
			{
				_OnActiveChange();
			}
		}
	}

	private void _OnPageChange(int newPage)
	{
		_page = newPage;
		if (active)
		{
			onPageTextChange?.Invoke($"{_page + 1} / {pages.Count}");
		}
		_targetPagePosition = (float)_page * -1.5f + 0.5f;
	}

	private void _OnActiveChange()
	{
		_alphaPagePosition = (active ? 0.5f : _pagePosition);
		onActiveChange?.Invoke(active);
		page = -1;
	}

	private void _UpdatePagePositions()
	{
		for (int i = 0; i < pages.Count; i++)
		{
			float num = (float)i * 1.5f + _pagePosition;
			HelpPage helpPage = pages[i];
			helpPage.rect.pivot = helpPage.rect.pivot.SetAxis(0, num);
			bool activeSelf = helpPage.gameObject.activeSelf;
			bool flag = i == _page;
			bool flag2 = num > -1f && num < 2f;
			helpPage.gameObject.SetActive(activeSelf ? flag2 : (flag && flag2));
		}
		onAlphaChange?.Invoke(Mathf.Clamp01(MathUtil.GetLerpAmount(_alphaPagePosition + 1.5f, _alphaPagePosition, _pagePosition)));
	}

	private void OnEnable()
	{
		active = true;
		pages.Sort();
		_pagePositionVelocity = 0f;
		_pagePosition = _targetPagePosition;
		_UpdatePagePositions();
		page = 0;
	}

	private void Update()
	{
		MathUtil.Spring(ref _pagePosition, ref _pagePositionVelocity, _targetPagePosition, springConstant, springDampening, Time.unscaledDeltaTime);
		_UpdatePagePositions();
		if (inputFocus.HasFocusPermissive() && (InputManager.I[KeyAction.Pause][KState.Clicked] || InputManager.I[KeyAction.Back][KState.Clicked]))
		{
			Close();
		}
		if (active || _page >= 0)
		{
			return;
		}
		foreach (HelpPage page in pages)
		{
			if (page.gameObject.activeSelf)
			{
				return;
			}
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		active = false;
	}

	public void NextPage()
	{
		page = MathUtil.Wrap(page, 1, 0, pages.Count);
	}

	public void PreviousPage()
	{
		page = MathUtil.Wrap(page, -1, 0, pages.Count);
	}

	public void Close()
	{
		active = false;
	}
}
