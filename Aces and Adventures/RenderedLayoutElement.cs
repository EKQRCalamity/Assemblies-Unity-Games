using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Graphic), typeof(LayoutElement))]
public class RenderedLayoutElement : MonoBehaviour
{
	private Graphic _graphic;

	private LayoutElement _layoutElement;

	private void Awake()
	{
		_graphic = GetComponent<Graphic>();
		_layoutElement = GetComponent<LayoutElement>();
	}

	private void Update()
	{
		_layoutElement.enabled = _graphic.enabled;
	}
}
