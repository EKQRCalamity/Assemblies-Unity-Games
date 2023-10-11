using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhysicsChainGenerator : MonoBehaviour
{
	public enum AddLinksWhen
	{
		OnAwake,
		OnStart,
		OnEnable,
		HandledByCode
	}

	[Header("Main Chain")]
	public List<PhysicsChainLink> chainLinkBlueprintPattern;

	[Range(1f, 100f)]
	public int repeatPatternCount = 10;

	[Header("Sub Chains")]
	public List<PhysicsChainLink> subChainLinkBlueprintPattern;

	[Range(0f, 100f)]
	public int subChainRepeatPatternCount;

	[Header("Add Link Settings")]
	public AddLinksWhen addLinks = AddLinksWhen.HandledByCode;

	public bool clearLinksOnAdd;

	[Header("Physics Solver")]
	[Range(0f, 200f)]
	public int solverIterations = 30;

	[Range(0f, 30f)]
	public int solverVelocityIterations = 1;

	private float? _chainLength;

	private PhysicsChainLink _firstLink;

	private PhysicsChainLink _lastLink;

	private Vector3? _firstLinkDefaultPosition;

	private Vector3? _lastLinkDefaultPosition;

	private float? _collapse;

	public float chainLength
	{
		get
		{
			float? num = _chainLength;
			if (!num.HasValue)
			{
				float? num2 = (_chainLength = _GetChainLength());
				return num2.Value;
			}
			return num.GetValueOrDefault();
		}
	}

	public Vector3 centerSimple => (_firstLink.transform.position + _lastLink.transform.position) * 0.5f;

	public float? collapse
	{
		get
		{
			return _collapse;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _collapse, value))
			{
				_SetCollapse(value);
			}
		}
	}

	public PhysicsChainLink firstLink => _firstLink;

	public PhysicsChainLink lastLink => _lastLink;

	public static void GeneratePhysicsChain(Transform parent, IEnumerable<PhysicsChainLink> linkBlueprints, ref PhysicsChainLink firstLinkInChain, ref PhysicsChainLink lastLinkInChain, int solverIterations = 30, int solverVelocityIterations = 1)
	{
		foreach (PhysicsChainLink linkBlueprint in linkBlueprints)
		{
			PhysicsChainLink component = Object.Instantiate(linkBlueprint.gameObject, parent).GetComponent<PhysicsChainLink>();
			component.body.solverIterations = ((solverIterations > 0) ? solverIterations : component.body.solverIterations);
			component.body.solverVelocityIterations = ((solverVelocityIterations > 0) ? solverVelocityIterations : component.body.solverVelocityIterations);
			firstLinkInChain = (firstLinkInChain ? firstLinkInChain : component);
			if ((bool)lastLinkInChain)
			{
				lastLinkInChain.AttachLink(component);
			}
			lastLinkInChain = component;
		}
	}

	private void _SetDirty()
	{
		_chainLength = null;
		_collapse = null;
		_firstLinkDefaultPosition = null;
		_lastLinkDefaultPosition = null;
	}

	private IEnumerable<PhysicsChainLink> _GetLinkBlueprints()
	{
		if (chainLinkBlueprintPattern.IsNullOrEmpty())
		{
			yield break;
		}
		for (int x = 0; x < repeatPatternCount; x++)
		{
			foreach (PhysicsChainLink item in chainLinkBlueprintPattern)
			{
				yield return item;
			}
		}
	}

	private void _AddLinkPattern()
	{
		AddLinks(_GetLinkBlueprints(), clearLinksOnAdd);
	}

	private float _GetChainLength()
	{
		return (from link in GetLinksInChain()
			where link.outputTransform
			select link).Sum((PhysicsChainLink link) => (link.outputTransform.position - link.inputTransform.position).magnitude);
	}

	private void _SetCollapse(float? collapseAmount)
	{
		_collapse = collapseAmount;
		if (_collapse.HasValue)
		{
			if (!_firstLinkDefaultPosition.HasValue)
			{
				SetCurrentExtremaPositionsAsDefault();
			}
			Vector3 value = _firstLinkDefaultPosition.Value;
			Vector3 value2 = _lastLinkDefaultPosition.Value;
			Vector3 b = (value + value2) * 0.5f;
			_firstLink.transform.position = _firstLink.transform.position.Lerp(b, _collapse.Value);
			_lastLink.transform.position = _lastLink.transform.position.Lerp(b, _collapse.Value);
		}
	}

	private void Awake()
	{
		if (addLinks == AddLinksWhen.OnAwake)
		{
			_AddLinkPattern();
		}
	}

	private void OnEnable()
	{
		if (addLinks == AddLinksWhen.OnEnable)
		{
			_AddLinkPattern();
		}
	}

	private void Start()
	{
		if (addLinks == AddLinksWhen.OnStart)
		{
			_AddLinkPattern();
		}
	}

	public void AddLinks(IEnumerable<PhysicsChainLink> linkBlueprints, bool clearExistingLinks)
	{
		_SetDirty();
		if (clearExistingLinks)
		{
			base.gameObject.DestroyChildren();
		}
		GeneratePhysicsChain(base.transform, linkBlueprints, ref _firstLink, ref _lastLink, solverIterations, solverVelocityIterations);
		if (!subChainLinkBlueprintPattern.IsNullOrEmpty() && subChainRepeatPatternCount > 0)
		{
			PhysicsChainLink[] componentsInChildren = base.transform.GetComponentsInChildren<PhysicsChainLink>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].CreateSubChain(this);
			}
		}
	}

	private IEnumerable<PhysicsChainLink> _GetLinksInChainBackwards()
	{
		PhysicsChainLink link = _lastLink;
		while ((bool)link)
		{
			yield return link;
			link = (link.joint.connectedBody ? link.joint.connectedBody.GetComponent<PhysicsChainLink>() : null);
		}
	}

	public IEnumerable<PhysicsChainLink> GetLinksInChain()
	{
		return _GetLinksInChainBackwards().Reverse();
	}

	public PhysicsChainGenerator CreateFromSubChainData(PhysicsChainGenerator parentGenerator, PhysicsChainLink parentLink)
	{
		chainLinkBlueprintPattern = parentGenerator.subChainLinkBlueprintPattern;
		repeatPatternCount = parentGenerator.subChainRepeatPatternCount;
		solverIterations = parentGenerator.solverIterations;
		solverVelocityIterations = parentGenerator.solverVelocityIterations;
		SetRootLink(parentLink);
		_AddLinkPattern();
		return this;
	}

	public void SetRootLink(PhysicsChainLink rootLink)
	{
		_firstLink = rootLink;
		_lastLink = rootLink;
	}

	public void SetExtremaLinkIsKinematic(bool isKinematic)
	{
		_firstLink.body.isKinematic = isKinematic;
		_lastLink.body.isKinematic = isKinematic;
	}

	public void SetEveryXLinkAsCollidable(PhysicMaterial physicMaterial, CollisionDetectionMode collisionDetectionMode, int startIndex = 1, int every = 2, bool addTriggerCollidersToNonCollidable = false)
	{
		int num = 0;
		foreach (PhysicsChainLink item in GetLinksInChain())
		{
			int num2 = num++ - startIndex;
			if (num2 >= 0 && num2 % every == 0)
			{
				if (!item.GetComponent<Collider>())
				{
					item.gameObject.AddComponent<MeshCollider>().convex = true;
				}
				item.gameObject.GetComponent<Collider>().sharedMaterial = physicMaterial;
				item.body.collisionDetectionMode = collisionDetectionMode;
				item.joint.enableCollision = true;
				continue;
			}
			if (addTriggerCollidersToNonCollidable)
			{
				if (!item.GetComponent<Collider>())
				{
					item.gameObject.AddComponent<MeshCollider>().convex = true;
				}
				item.gameObject.GetComponent<Collider>().isTrigger = true;
			}
			item.joint.enableCollision = false;
		}
	}

	public void SetCurrentExtremaPositionsAsDefault()
	{
		_firstLinkDefaultPosition = _firstLink.transform.position;
		_lastLinkDefaultPosition = _lastLink.transform.position;
	}
}
