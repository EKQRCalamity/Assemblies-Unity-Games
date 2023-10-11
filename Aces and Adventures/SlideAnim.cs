using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SlideAnim : MonoBehaviour
{
	public Vector2 direction = new Vector2(1f, 0f);

	public bool applyToPivot;

	public Vector2 scaleOnOpen = new Vector2(1f, 1f);

	public AnimationCurve interpolation = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float animTime = 0.2f;

	public bool useUnscaledTime = true;

	public bool automateInteractable = true;

	public GameObject automateInteractableOf;

	public bool setDefaultRectDelayed = true;

	public bool animatePreferredSize;

	public List<SlideAnim> siblings;

	public List<SlideAnim> children;

	public LogicGateType shareStateType;

	public List<SlideAnim> shareStateWith;

	private List<SlideAnim> allSiblings;

	private SlideAnim parent;

	private bool open;

	private float elapsedTime;

	private float lastElapsedTime;

	private RectTransform rt;

	private CanvasGroup canvasGroup;

	private Vector3 defaultLocalPos;

	private Vector2 defaultPivot;

	private Vector2 defaultSize;

	private LayoutElement layoutElement;

	private Vector2 defaultPreferredSize;

	private bool _shouldUpdateSharedStates;

	public BoolEvent OnStateChange;

	public UnityEvent OnOpen;

	public UnityEvent OnClosed;

	private void Awake()
	{
		rt = base.transform as RectTransform;
		if (automateInteractable)
		{
			canvasGroup = ((automateInteractableOf == null) ? base.gameObject.GetOrAddComponent<CanvasGroup>() : automateInteractableOf.GetOrAddComponent<CanvasGroup>());
		}
		if (animatePreferredSize)
		{
			layoutElement = GetComponent<LayoutElement>();
			if (layoutElement != null)
			{
				defaultPreferredSize = new Vector2(layoutElement.preferredWidth, layoutElement.preferredHeight);
			}
		}
		children.RemoveNull();
		for (int i = 0; i < children.Count; i++)
		{
			children[i].parent = this;
		}
		if (siblings.Count > 0)
		{
			siblings.RemoveNull();
			for (int j = 0; j < siblings.Count; j++)
			{
				allSiblings = siblings[j].allSiblings ?? allSiblings ?? new List<SlideAnim>();
				siblings[j].allSiblings = allSiblings;
				allSiblings.AddUnique(siblings[j]);
			}
			allSiblings.AddUnique(this);
		}
		for (int k = 0; k < shareStateWith.Count; k++)
		{
			shareStateWith[k].OnStateChange.AddListener(delegate
			{
				_shouldUpdateSharedStates = true;
			});
		}
	}

	private void Start()
	{
		SetCurrentRectAsDefault();
		SetInteractive(interactive: false);
		if (setDefaultRectDelayed)
		{
			this.InvokeNextFrame(SetCurrentRectAsDefault);
		}
	}

	private void Update()
	{
		UpdateSharedStates();
		elapsedTime += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
		elapsedTime = Mathf.Min(elapsedTime, animTime);
		if (elapsedTime != lastElapsedTime)
		{
			float num = elapsedTime / animTime;
			SetPosition(num);
			if (num == 1f)
			{
				InvokeOpenStates();
			}
			lastElapsedTime = elapsedTime;
		}
	}

	private void SetPosition(float t)
	{
		float t2 = interpolation.Evaluate(open ? t : (1f - t));
		if (applyToPivot)
		{
			rt.pivot = Vector2.Lerp(defaultPivot, defaultPivot - direction, t2);
		}
		else
		{
			rt.localPosition = Vector3.Lerp(defaultLocalPos, new Vector3(defaultLocalPos.x + direction.x * defaultSize.x, defaultLocalPos.y + direction.y * defaultSize.y, defaultLocalPos.z), t2);
		}
		if (scaleOnOpen.x != 1f)
		{
			if (layoutElement != null && defaultPreferredSize.x > 0f)
			{
				layoutElement.preferredWidth = defaultPreferredSize.x * Mathf.Lerp(1f, scaleOnOpen.x, t2);
			}
			else
			{
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultSize.x * Mathf.Lerp(1f, scaleOnOpen.x, t2));
			}
		}
		if (scaleOnOpen.y != 1f)
		{
			if (layoutElement != null && defaultPreferredSize.y > 0f)
			{
				layoutElement.preferredHeight = defaultPreferredSize.y * Mathf.Lerp(1f, scaleOnOpen.y, t2);
			}
			else
			{
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultSize.y * Mathf.Lerp(1f, scaleOnOpen.y, t2));
			}
		}
	}

	private void InvokeOpenStates()
	{
		if (open)
		{
			if (OnOpen != null)
			{
				OnOpen.Invoke();
			}
			SetInteractive(interactive: true);
			for (int i = 0; i < children.Count; i++)
			{
				children[i].SetState(open: true);
			}
		}
		else
		{
			if (OnClosed != null)
			{
				OnClosed.Invoke();
			}
			if (parent != null)
			{
				parent.SetState(open: false);
			}
		}
	}

	private void SetInteractive(bool interactive)
	{
		if (canvasGroup != null)
		{
			canvasGroup.blocksRaycasts = interactive;
		}
	}

	private void UpdateSharedStates()
	{
		if (!_shouldUpdateSharedStates)
		{
			return;
		}
		_shouldUpdateSharedStates = false;
		if (shareStateWith.Count != 0)
		{
			bool flag = shareStateType.Process(shareStateWith[0].open);
			for (int i = 1; i < shareStateWith.Count; i++)
			{
				flag = shareStateType.Combine(flag, shareStateWith[i].open);
			}
			SetState(flag);
		}
	}

	public void SetCurrentRectAsDefault()
	{
		defaultLocalPos = rt.localPosition;
		defaultPivot = rt.pivot;
		defaultSize = rt.rect.size.Multiply(rt.localScale.Project(AxisType.Z).Abs());
	}

	public void SetState(bool open)
	{
		if (this.open == open)
		{
			return;
		}
		if (OnStateChange != null)
		{
			OnStateChange.Invoke(open);
		}
		if (open && allSiblings != null)
		{
			for (int i = 0; i < allSiblings.Count; i++)
			{
				if (allSiblings[i] != this)
				{
					allSiblings[i].SetState(open: false);
				}
			}
		}
		if (!open)
		{
			for (int j = 0; j < children.Count; j++)
			{
				children[j].SetState(open: false);
			}
			SlideAnim slideAnim = this;
			while (slideAnim.parent != null)
			{
				slideAnim = slideAnim.parent;
				slideAnim.SetInteractive(interactive: false);
			}
		}
		SetInteractive(interactive: false);
		this.open = open;
		elapsedTime = animTime - elapsedTime;
		lastElapsedTime = -1f;
	}

	public void Toggle()
	{
		SetState(!open);
	}

	public void SetBool(string name, bool open)
	{
		SetState(open);
	}

	public void ForceState(bool open)
	{
		this.open = open;
		elapsedTime = animTime;
		lastElapsedTime = elapsedTime;
		SetPosition(1f);
		InvokeOpenStates();
	}

	public void SetToInterpolatedPosition(float lerp)
	{
		SetPosition(open ? lerp : (1f - lerp));
	}
}
