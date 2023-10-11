using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIArcBar))]
public class UIArcBarSection : AUIBarSection
{
	[SerializeField]
	protected UIArcBar _referenceArcBar;

	private UIArcBar _arcBar;

	public UIArcBar referenceArcBar
	{
		get
		{
			return _referenceArcBar;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _referenceArcBar, value))
			{
				_SetDirty();
			}
		}
	}

	public UIArcBar arcBar
	{
		get
		{
			if (!_arcBar)
			{
				return _arcBar = GetComponent<UIArcBar>();
			}
			return _arcBar;
		}
	}

	public override Graphic graphic => arcBar;

	protected override void _UpdateLayoutUnique()
	{
		if ((bool)referenceArcBar)
		{
			arcBar.clockwiseFill = !referenceArcBar.clockwiseFill;
			arcBar.startDegree = Mathf.Lerp(referenceArcBar.endDegree, referenceArcBar.startDegree, base.normalizedMinMax.Max());
			arcBar.endDegree = Mathf.Lerp(referenceArcBar.endDegree, referenceArcBar.startDegree, base.normalizedMinMax.Min());
			arcBar.fillAmount = 1f;
			arcBar.SetVerticesDirty();
		}
	}
}
