using System.Collections.Generic;
using UnityEngine;

namespace XftWeapon;

[ScriptOrder(100)]
public class XWeaponTrail : MonoBehaviour
{
	public class Element
	{
		public Vector3 PointStart;

		public Vector3 PointEnd;

		public Vector3 Pos => (PointStart + PointEnd) / 2f;

		public Element(Vector3 start, Vector3 end)
		{
			PointStart = start;
			PointEnd = end;
		}

		public Element()
		{
		}

		public void SetPoints(Vector3 start, Vector3 end)
		{
			PointStart = start;
			PointEnd = end;
		}
	}

	public class ElementPool
	{
		private readonly Stack<Element> _stack = new Stack<Element>();

		public int CountAll { get; private set; }

		public int CountActive => CountAll - CountInactive;

		public int CountInactive => _stack.Count;

		public ElementPool(int preCount)
		{
			for (int i = 0; i < preCount; i++)
			{
				Element item = new Element();
				_stack.Push(item);
				CountAll++;
			}
		}

		public Element Get()
		{
			Element result;
			if (_stack.Count == 0)
			{
				result = new Element();
				CountAll++;
			}
			else
			{
				result = _stack.Pop();
			}
			return result;
		}

		public void Release(Element element)
		{
			if (_stack.Count > 0 && _stack.Peek() == element)
			{
				Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
			}
			_stack.Push(element);
		}
	}

	private const float UPDATE_INTERVAL = 1f / 120f;

	public static string Version = "1.2.0";

	public bool UseWith2D;

	public string SortingLayerName;

	public int SortingOrder;

	public Transform PointStart;

	public Transform PointEnd;

	public Vector2 Range = new Vector2(0f, 1f);

	public int MaxFrame = 14;

	public int Granularity = 60;

	public Color MyColor = Color.white;

	public Material MyMaterial;

	protected Element mHeadElem = new Element();

	protected List<Element> mSnapshotList = new List<Element>();

	protected ElementPool mElemPool;

	protected Spline mSpline = new Spline();

	protected float mFadeT = 1f;

	protected bool mIsFading;

	protected float mFadeTime = 1f;

	protected float mElapsedTime;

	protected float mFadeElapsedime;

	protected GameObject mMeshObj;

	protected VertexPool mVertexPool;

	protected VertexPool.VertexSegment mVertexSegment;

	protected bool mInited;

	private Vector3 _startPosition
	{
		get
		{
			if (!PointStart)
			{
				return mSnapshotList[0].PointStart;
			}
			if (Range.x != 0f)
			{
				return Vector3.LerpUnclamped(PointStart.position, PointEnd.position, Range.x);
			}
			return PointStart.position;
		}
	}

	private Vector3 _endPosition
	{
		get
		{
			if (!PointEnd)
			{
				return mSnapshotList[0].PointEnd;
			}
			if (Range.y != 1f)
			{
				return Vector3.LerpUnclamped(PointStart.position, PointEnd.position, Range.y);
			}
			return PointEnd.position;
		}
	}

	public Vector3 CurHeadPos => (_startPosition + _endPosition) / 2f;

	public float TrailWidth => (_startPosition - _endPosition).magnitude;

	public bool finished => !base.isActiveAndEnabled;

	public void Init()
	{
		if (!mInited)
		{
			mElemPool = new ElementPool(MaxFrame);
			InitMeshObj();
			InitOriginalElements();
			InitSpline();
			mInited = true;
		}
	}

	public void Activate()
	{
		Init();
		base.gameObject.SetActive(value: true);
		mVertexPool.SetMeshObjectActive(flag: true);
		mFadeT = 1f;
		mIsFading = false;
		mFadeTime = 1f;
		mFadeElapsedime = 0f;
		mElapsedTime = 1f / 120f;
		ResetElements();
		ResetSpline();
		RefreshSpline();
		UpdateVertex();
	}

	public void Deactivate()
	{
		base.gameObject.SetActive(value: false);
		mVertexPool.SetMeshObjectActive(flag: false);
	}

	public void StopSmoothly(float fadeTime)
	{
		mIsFading = true;
		mFadeTime = fadeTime;
	}

	public void SetMaxFrame(int maxFrame)
	{
		if (MaxFrame != maxFrame)
		{
			MaxFrame = maxFrame;
			InitSpline();
		}
	}

	public void SetGranularity(int granularity)
	{
		if (Granularity != granularity)
		{
			Granularity = granularity;
			InitMeshObj();
			InitSpline();
		}
	}

	private void LateUpdate()
	{
		if (mInited)
		{
			UpdateHeadElem();
			mElapsedTime += Time.deltaTime;
			do
			{
				mElapsedTime -= 1f / 120f;
				RecordCurElem();
			}
			while (mElapsedTime >= 1f / 120f);
			RefreshSpline();
			UpdateFade();
			UpdateVertex();
			mVertexPool.LateUpdate();
		}
	}

	private void OnDestroy()
	{
		if (mInited && mVertexPool != null)
		{
			mVertexPool.Destroy();
		}
	}

	private void Start()
	{
		mInited = false;
		Init();
	}

	private void OnDrawGizmos()
	{
		if (!(PointEnd == null) && !(PointStart == null))
		{
			float magnitude = (_startPosition - _endPosition).magnitude;
			if (!(magnitude < Mathf.Epsilon))
			{
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(_startPosition, magnitude * 0.04f);
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(_endPosition, magnitude * 0.04f);
			}
		}
	}

	private void InitSpline()
	{
		mSpline.Granularity = Granularity;
		mSpline.Clear();
		for (int i = 0; i < MaxFrame; i++)
		{
			mSpline.AddControlPoint(CurHeadPos, _startPosition - _endPosition);
		}
	}

	private void ResetSpline()
	{
		Vector3 curHeadPos = CurHeadPos;
		Vector3 up = _startPosition - _endPosition;
		for (int i = 0; i < mSpline.ControlPoints.Count; i++)
		{
			mSpline.SetControlPointAtIndex(i, curHeadPos, up);
		}
	}

	private void RefreshSpline()
	{
		for (int i = 0; i < mSnapshotList.Count; i++)
		{
			mSpline.ControlPoints[i].Position = mSnapshotList[i].Pos;
			mSpline.ControlPoints[i].Normal = mSnapshotList[i].PointEnd - mSnapshotList[i].PointStart;
		}
		mSpline.RefreshSpline();
	}

	private void UpdateVertex()
	{
		VertexPool pool = mVertexSegment.Pool;
		float trailWidth = TrailWidth;
		for (int i = 0; i < Granularity; i++)
		{
			int num = mVertexSegment.VertStart + i * 3;
			float num2 = (float)i / (float)Granularity;
			float tl = num2 * mFadeT;
			Vector2 zero = Vector2.zero;
			Vector3 vector = mSpline.InterpolateByLen(tl);
			Vector3 vector2 = mSpline.InterpolateNormalByLen(tl);
			Vector3 vector3 = vector + vector2.normalized * trailWidth * 0.5f;
			Vector3 vector4 = vector - vector2.normalized * trailWidth * 0.5f;
			pool.Vertices[num] = vector3;
			pool.Colors[num] = MyColor;
			zero.x = 0f;
			zero.y = num2;
			pool.UVs[num] = zero;
			pool.Vertices[num + 1] = vector;
			pool.Colors[num + 1] = MyColor;
			zero.x = 0.5f;
			zero.y = num2;
			pool.UVs[num + 1] = zero;
			pool.Vertices[num + 2] = vector4;
			pool.Colors[num + 2] = MyColor;
			zero.x = 1f;
			zero.y = num2;
			pool.UVs[num + 2] = zero;
		}
		mVertexSegment.Pool.UVChanged = true;
		mVertexSegment.Pool.VertChanged = true;
		mVertexSegment.Pool.ColorChanged = true;
	}

	private void UpdateIndices()
	{
		VertexPool pool = mVertexSegment.Pool;
		for (int i = 0; i < Granularity - 1; i++)
		{
			int num = mVertexSegment.VertStart + i * 3;
			int num2 = mVertexSegment.VertStart + (i + 1) * 3;
			int num3 = mVertexSegment.IndexStart + i * 12;
			pool.Indices[num3] = num2;
			pool.Indices[num3 + 1] = num2 + 1;
			pool.Indices[num3 + 2] = num;
			pool.Indices[num3 + 3] = num2 + 1;
			pool.Indices[num3 + 4] = num + 1;
			pool.Indices[num3 + 5] = num;
			pool.Indices[num3 + 6] = num2 + 1;
			pool.Indices[num3 + 7] = num2 + 2;
			pool.Indices[num3 + 8] = num + 1;
			pool.Indices[num3 + 9] = num2 + 2;
			pool.Indices[num3 + 10] = num + 2;
			pool.Indices[num3 + 11] = num + 1;
		}
		pool.IndiceChanged = true;
	}

	private void UpdateHeadElem()
	{
		mSnapshotList[0].PointStart = _startPosition;
		mSnapshotList[0].PointEnd = _endPosition;
	}

	private void UpdateFade()
	{
		if (mIsFading)
		{
			mFadeElapsedime += Time.deltaTime;
			float num = mFadeElapsedime / mFadeTime;
			mFadeT = 1f - num;
			if (mFadeT < 0f)
			{
				Deactivate();
			}
		}
	}

	private void RecordCurElem()
	{
		Element element = mElemPool.Get();
		element.PointStart = _startPosition;
		element.PointEnd = _endPosition;
		if (mSnapshotList.Count < MaxFrame)
		{
			mSnapshotList.Insert(1, element);
			return;
		}
		mElemPool.Release(mSnapshotList[mSnapshotList.Count - 1]);
		mSnapshotList.RemoveAt(mSnapshotList.Count - 1);
		mSnapshotList.Insert(1, element);
	}

	private void InitOriginalElements()
	{
		mSnapshotList.Clear();
		mSnapshotList.Add(new Element(_startPosition, _endPosition));
		mSnapshotList.Add(new Element(_startPosition, _endPosition));
	}

	private void ResetElements()
	{
		while (mSnapshotList.Count < 2)
		{
			mSnapshotList.Add(mElemPool.Get());
		}
		for (int num = mSnapshotList.Count - 1; num >= 2; num--)
		{
			mElemPool.Release(mSnapshotList[num]);
			mSnapshotList.RemoveAt(num);
		}
		for (int i = 0; i < mSnapshotList.Count; i++)
		{
			mSnapshotList[i].SetPoints(_startPosition, _endPosition);
		}
	}

	private void InitMeshObj()
	{
		mVertexPool = new VertexPool(MyMaterial, this);
		mVertexSegment = mVertexPool.GetVertices(Granularity * 3, (Granularity - 1) * 12);
		UpdateIndices();
	}
}
