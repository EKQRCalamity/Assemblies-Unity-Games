using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UI/Animated TextMeshPro - Text (UI)", 12)]
public class AnimatedTextMeshProUGUI : CustomTextMeshProUGUI, IAnimatedUI
{
	[SerializeField]
	protected AnimationCurve _animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	protected AnimationCurve _alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	private List<UIAnimation> _animations;

	private bool _animating;

	public AnimationCurve animationCurve => _animationCurve;

	public AnimationCurve alphaCurve => _alphaCurve;

	public List<UIAnimation> animations => _animations ?? (_animations = new List<UIAnimation>());

	public bool animating
	{
		get
		{
			return _animating;
		}
		set
		{
			_animating = value;
		}
	}

	private void Update()
	{
		if (_animating)
		{
			base.havePropertiesChanged = true;
		}
	}

	protected override void GenerateTextMesh()
	{
		if (animating)
		{
			for (int i = 0; i < m_textInfo.meshInfo.Length; i++)
			{
				mesh.triangles = m_textInfo.meshInfo[i].triangles;
			}
		}
		base.GenerateTextMesh();
		if (animating)
		{
			Vector3 center = mesh.bounds.center;
			for (int j = 0; j < m_textInfo.meshInfo.Length; j++)
			{
				this.PopulateMesh(mesh, m_textInfo.meshInfo[j].vertices, m_textInfo.meshInfo[j].colors32, m_textInfo.meshInfo[j].uvs0, m_textInfo.meshInfo[j].uvs2, m_textInfo.meshInfo[j].triangles, m_textInfo.meshInfo[j].normals, m_textInfo.meshInfo[j].tangents, center);
			}
			base.canvasRenderer.SetMesh(mesh);
		}
	}
}
