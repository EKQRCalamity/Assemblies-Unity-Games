using DG.Tweening;
using UnityEngine;

public class OssuaryItem : MonoBehaviour
{
	public string itemId;

	public void ActivateItemSilently()
	{
		base.gameObject.SetActive(value: true);
	}

	public void DeactivateItemSilently()
	{
		base.gameObject.SetActive(value: false);
	}

	public void ActivateItem()
	{
		base.gameObject.SetActive(value: true);
		base.transform.DOPunchScale(Vector3.one, 0.3f, 2);
	}
}
