using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class PointerOverScalerUI : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public Padding padding = new Padding(12f, 12f, 12f, 12f);

	private GameObject _scaler;

	private GameObject scaler
	{
		get
		{
			if (!_scaler)
			{
				return _scaler = _CreateScaler();
			}
			return _scaler;
		}
	}

	private GameObject _CreateScaler()
	{
		GameObject obj = new GameObject("Scaler");
		RectTransform rect = obj.AddComponent<RectTransform>();
		obj.AddComponent<LayoutElement>().ignoreLayout = true;
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		obj.AddComponent<RayCastTarget>();
		rect.Stretch();
		rect.PadLocal(padding);
		return obj;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		scaler.SetActive(value: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		scaler.SetActive(value: false);
	}
}
