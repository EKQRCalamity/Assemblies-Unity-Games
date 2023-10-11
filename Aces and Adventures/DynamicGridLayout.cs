using UnityEngine;
using UnityEngine.UI;

public class DynamicGridLayout : GridLayoutGroup
{
	public enum PreferredSizeType
	{
		Min,
		Average,
		Max,
		Constant
	}

	[SerializeField]
	protected PreferredSizeType _cellSizeType = PreferredSizeType.Average;

	[SerializeField]
	protected bool _respectMinSize = true;

	public PreferredSizeType cellSizeType
	{
		get
		{
			return _cellSizeType;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _cellSizeType, value))
			{
				SetDirty();
			}
		}
	}

	public bool respectMinSize
	{
		get
		{
			return _respectMinSize;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _respectMinSize, value))
			{
				SetDirty();
			}
		}
	}

	public override void CalculateLayoutInputHorizontal()
	{
		Vector2 vector = ((cellSizeType == PreferredSizeType.Average) ? Vector2.zero : ((cellSizeType == PreferredSizeType.Max) ? new Vector2(float.MinValue, float.MinValue) : new Vector2(float.MaxValue, float.MaxValue)));
		Vector2 vector2 = Vector2.zero;
		int num = 0;
		foreach (Transform item in base.transform)
		{
			RectTransform rect = item as RectTransform;
			if (rect.LayoutIsReady())
			{
				num++;
				Vector2 vector3 = new Vector2(LayoutUtility.GetPreferredWidth(rect), LayoutUtility.GetPreferredHeight(rect));
				switch (cellSizeType)
				{
				case PreferredSizeType.Min:
					vector = Vector2.Min(vector, vector3);
					break;
				case PreferredSizeType.Average:
					vector += vector3;
					break;
				case PreferredSizeType.Max:
					vector = Vector2.Max(vector, vector3);
					break;
				}
				if (respectMinSize)
				{
					vector2 = Vector2.Max(vector2, new Vector2(LayoutUtility.GetMinWidth(rect), LayoutUtility.GetMinHeight(rect)));
				}
			}
		}
		if (cellSizeType != PreferredSizeType.Constant)
		{
			if (num > 0)
			{
				base.cellSize = ((cellSizeType == PreferredSizeType.Average) ? (vector / num) : vector);
			}
			if (respectMinSize)
			{
				base.cellSize = Vector2.Max(base.cellSize, vector2);
			}
		}
		base.CalculateLayoutInputHorizontal();
	}

	public DynamicGridLayout SetCellSize(Vector2? constantCellSize, PreferredSizeType? fallbackPreferredSizeType = null)
	{
		_cellSizeType = (constantCellSize.HasValue ? PreferredSizeType.Constant : (fallbackPreferredSizeType ?? _cellSizeType));
		if (constantCellSize.HasValue)
		{
			base.cellSize = constantCellSize.Value;
		}
		return this;
	}
}
