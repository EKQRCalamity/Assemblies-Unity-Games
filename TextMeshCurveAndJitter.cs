using System.Collections;
using TMPro;
using UnityEngine;

public class TextMeshCurveAndJitter : MonoBehaviour
{
	private const float MAX_BOUNDS_TEXT_COMPONENT = 226.2879f;

	private const float MIN_BOUNDS_TEXT_COMPONENT = -229.3845f;

	[SerializeField]
	private TMP_Text m_TextComponent;

	public AnimationCurve VertexCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.25f, 2f), new Keyframe(0.5f, 0f), new Keyframe(0.75f, 2f), new Keyframe(1f, 0f));

	public AnimationCurve VertexSpacing = new AnimationCurve(new Keyframe(0f, 1.5f), new Keyframe(0.5f, 0f), new Keyframe(1f, -1.5f));

	public float AngleMultiplier = 1f;

	public float SpeedMultiplier = 1f;

	public float CurveScale = 1f;

	public float SpacingScale = 1f;

	public float jitterAmplitude = 0.1f;

	public float jitterAngleAmplitude = 0.1f;

	private float jitterDelay = 0.1f;

	private float currentJitterDelay;

	private bool applyAlpha;

	private byte alphaValue;

	public byte AlphaValue
	{
		set
		{
			applyAlpha = true;
			alphaValue = value;
		}
	}

	private void Awake()
	{
		jitterDelay = 1f / 12f;
		AlphaValue = byte.MaxValue;
		m_TextComponent = base.gameObject.GetComponent<TMP_Text>();
	}

	private void Start()
	{
		StartCoroutine(WarpText());
	}

	private AnimationCurve CopyAnimationCurve(AnimationCurve curve)
	{
		AnimationCurve animationCurve = new AnimationCurve();
		animationCurve.keys = curve.keys;
		return animationCurve;
	}

	private IEnumerator WarpText()
	{
		VertexCurve.preWrapMode = WrapMode.Once;
		VertexCurve.postWrapMode = WrapMode.Once;
		m_TextComponent.havePropertiesChanged = true;
		while (true)
		{
			currentJitterDelay -= CupheadTime.Delta;
			if (currentJitterDelay <= 0f)
			{
				currentJitterDelay = jitterDelay;
			}
			ApplyChanges(jitter: true);
			yield return null;
		}
	}

	public void ApplyChanges(bool jitter)
	{
		m_TextComponent.ForceMeshUpdate();
		TMP_TextInfo textInfo = m_TextComponent.textInfo;
		int characterCount = textInfo.characterCount;
		if (characterCount == 0 || m_TextComponent.text.Length == 0)
		{
			return;
		}
		float x = m_TextComponent.bounds.min.x;
		float x2 = m_TextComponent.bounds.max.x;
		for (int i = 0; i < characterCount; i++)
		{
			if (textInfo.characterInfo[i].isVisible)
			{
				int vertexIndex = textInfo.characterInfo[i].vertexIndex;
				int materialReferenceIndex = textInfo.characterInfo[i].materialReferenceIndex;
				Vector3[] vertices = textInfo.meshInfo[materialReferenceIndex].vertices;
				Color32[] colors = textInfo.meshInfo[materialReferenceIndex].colors32;
				if (jitter)
				{
					ApplyCurveAndJitter(jitter, vertices, vertexIndex, i, textInfo, x, x2);
				}
				if (applyAlpha)
				{
					ApplyAlpha(colors, vertexIndex);
				}
			}
		}
		jitter = false;
		m_TextComponent.UpdateVertexData();
	}

	private void ApplyCurveAndJitter(bool jitter, Vector3[] vertices, int vertexIndex, int i, TMP_TextInfo textInfo, float boundsMinX, float boundsMaxX)
	{
		Vector3 vector = new Vector2((vertices[vertexIndex].x + vertices[vertexIndex + 2].x) / 2f, textInfo.characterInfo[i].baseLine);
		float time = (vector.x - Mathf.Min(boundsMinX, -229.3845f)) / (Mathf.Max(boundsMaxX, 226.2879f) - Mathf.Min(boundsMinX, -229.3845f));
		float num = VertexSpacing.Evaluate(time) * SpacingScale;
		vertices[vertexIndex].x += num;
		vertices[vertexIndex + 1].x += num;
		vertices[vertexIndex + 2].x += num;
		vertices[vertexIndex + 3].x += num;
		vertices[vertexIndex] += -vector;
		vertices[vertexIndex + 1] += -vector;
		vertices[vertexIndex + 2] += -vector;
		vertices[vertexIndex + 3] += -vector;
		float num2 = (vector.x - boundsMinX) / (boundsMaxX - boundsMinX);
		float num3 = num2 + 0.0001f;
		float y = VertexCurve.Evaluate(num2) * CurveScale;
		float y2 = VertexCurve.Evaluate(num3) * CurveScale;
		Vector3 lhs = new Vector3(1f, 0f, 0f);
		Vector3 rhs = new Vector3(num3 * (boundsMaxX - boundsMinX) + boundsMinX, y2) - new Vector3(vector.x, y);
		float num4 = Mathf.Acos(Vector3.Dot(lhs, rhs.normalized)) * 57.29578f;
		float num5 = ((!(Vector3.Cross(lhs, rhs).z > 0f)) ? (360f - num4) : num4);
		float num6 = 0f;
		if (jitter)
		{
			num6 = Random.Range(0f - jitterAngleAmplitude, jitterAngleAmplitude);
		}
		Matrix4x4 matrix4x = Matrix4x4.TRS(new Vector3(0f, y, 0f), Quaternion.Euler(0f, 0f, num5 + num6), Vector3.one);
		ref Vector3 reference = ref vertices[vertexIndex];
		reference = matrix4x.MultiplyPoint3x4(vertices[vertexIndex]);
		ref Vector3 reference2 = ref vertices[vertexIndex + 1];
		reference2 = matrix4x.MultiplyPoint3x4(vertices[vertexIndex + 1]);
		ref Vector3 reference3 = ref vertices[vertexIndex + 2];
		reference3 = matrix4x.MultiplyPoint3x4(vertices[vertexIndex + 2]);
		ref Vector3 reference4 = ref vertices[vertexIndex + 3];
		reference4 = matrix4x.MultiplyPoint3x4(vertices[vertexIndex + 3]);
		vertices[vertexIndex] += vector;
		vertices[vertexIndex + 1] += vector;
		vertices[vertexIndex + 2] += vector;
		vertices[vertexIndex + 3] += vector;
		Vector3 vector2 = Vector3.zero;
		if (jitter)
		{
			vector2 = new Vector3(Random.Range(0f - jitterAmplitude, jitterAmplitude), Random.Range(0f - jitterAmplitude, jitterAmplitude), 0f);
		}
		vertices[vertexIndex] += vector2;
		vertices[vertexIndex + 1] += vector2;
		vertices[vertexIndex + 2] += vector2;
		vertices[vertexIndex + 3] += vector2;
	}

	private void ApplyAlpha(Color32[] vertices, int vertexIndex)
	{
		vertices[vertexIndex].a = alphaValue;
		vertices[vertexIndex + 1].a = alphaValue;
		vertices[vertexIndex + 2].a = alphaValue;
		vertices[vertexIndex + 3].a = alphaValue;
	}
}
