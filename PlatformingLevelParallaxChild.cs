using UnityEngine;

public class PlatformingLevelParallaxChild : AbstractMonoBehaviour
{
	[SerializeField]
	private int _sortingOrderOffset;

	public int SortingOrderOffset => _sortingOrderOffset;
}
