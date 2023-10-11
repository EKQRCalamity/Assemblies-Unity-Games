using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Graphic))]
public class GraphicHook : MonoBehaviour
{
	private Graphic _graphic;

	private Graphic graphic => _graphic ?? (_graphic = GetComponent<Graphic>());

	public float r
	{
		get
		{
			return graphic.color.r;
		}
		set
		{
			graphic.color = new Color(value, graphic.color.g, graphic.color.b, graphic.color.a);
		}
	}

	public float g
	{
		get
		{
			return graphic.color.g;
		}
		set
		{
			graphic.color = new Color(graphic.color.r, value, graphic.color.b, graphic.color.a);
		}
	}

	public float b
	{
		get
		{
			return graphic.color.b;
		}
		set
		{
			graphic.color = new Color(graphic.color.r, graphic.color.g, value, graphic.color.a);
		}
	}

	public float a
	{
		get
		{
			return graphic.color.a;
		}
		set
		{
			graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, value);
		}
	}

	public float a2
	{
		set
		{
			a = value * value;
		}
	}

	public float h
	{
		get
		{
			return graphic.color.ToHSV().r;
		}
		set
		{
			graphic.color = graphic.color.ToHSV(value, null, null).ToRGB();
		}
	}

	public float s
	{
		get
		{
			return graphic.color.ToHSV().g;
		}
		set
		{
			graphic.color = graphic.color.ToHSV(null, value, null).ToRGB();
		}
	}

	public float v
	{
		get
		{
			return graphic.color.ToHSV().b;
		}
		set
		{
			graphic.color = graphic.color.ToHSV(null, null, value).ToRGB();
		}
	}

	public Color32 color32
	{
		get
		{
			return graphic.color;
		}
		set
		{
			graphic.color = value;
		}
	}

	public void SetAlpha(float alpha)
	{
		graphic.color = graphic.color.SetAlpha(alpha);
		graphic.enabled = alpha > 0f;
	}

	public void SetRendererAlpha(float alpha)
	{
		graphic.canvasRenderer.SetAlpha(alpha);
		graphic.enabled = alpha > 0f;
	}

	public void SetRGB(Color color)
	{
		color.a = graphic.color.a;
		graphic.color = color;
	}
}
