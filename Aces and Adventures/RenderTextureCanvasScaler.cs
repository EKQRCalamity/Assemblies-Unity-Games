using UnityEngine;
using UnityEngine.UI;

public class RenderTextureCanvasScaler : CanvasScaler
{
	[SerializeField]
	protected Camera _renderCamera;

	public bool autoMatchWidthOrHeight = true;

	public bool inverseMatchWidthOrHeight = true;

	public Camera renderCamera
	{
		get
		{
			return this.CacheComponentInParent(ref _renderCamera);
		}
		set
		{
			_renderCamera = value;
		}
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		renderCamera = null;
	}

	protected override void HandleScaleWithScreenSize()
	{
		Vector2 vector = new Vector2(Screen.width, Screen.height);
		Vector2 v = (renderCamera ? ((Vector2)renderCamera.PixelDimensions()) : vector);
		Vector2 vector2 = v.Multiply(vector.InsureNonZero().Inverse());
		if (autoMatchWidthOrHeight)
		{
			base.matchWidthOrHeight = ((vector2.x < vector2.y) ? Mathf.Lerp(0f, 0.5f, (vector2.x / vector2.y.InsureNonZero()).OneMinusIf()) : ((vector2.y < vector2.x) ? Mathf.Lerp(1f, 0.5f, (vector2.y / vector2.x.InsureNonZero()).OneMinusIf()) : 0.5f)).OneMinusIf(inverseMatchWidthOrHeight);
		}
		float num = 0f;
		switch (m_ScreenMatchMode)
		{
		case ScreenMatchMode.MatchWidthOrHeight:
			num = Mathf.Pow(2f, Mathf.Lerp(Mathf.Log(v.x / m_ReferenceResolution.x, 2f), Mathf.Log(v.y / m_ReferenceResolution.y, 2f), m_MatchWidthOrHeight));
			break;
		case ScreenMatchMode.Expand:
			num = Mathf.Min(v.x / m_ReferenceResolution.x, v.y / m_ReferenceResolution.y);
			break;
		case ScreenMatchMode.Shrink:
			num = Mathf.Max(v.x / m_ReferenceResolution.x, v.y / m_ReferenceResolution.y);
			break;
		}
		SetScaleFactor(num);
		SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
	}
}
