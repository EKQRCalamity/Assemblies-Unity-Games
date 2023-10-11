using UnityEngine;

public class UILayout3DSlotChainLinker : MonoBehaviour
{
	[SerializeField]
	private UILayout3DSlot _uiSlot;

	[SerializeField]
	private PhysicsChainLink _chainLink;

	private GameObject _attachedGameObject;

	private UILayout3DSlot uiSlot => this.CacheComponentInChildren(ref _uiSlot);

	private PhysicsChainLink chainLink => this.CacheComponentInChildren(ref _chainLink);

	private void _RegisterEvents()
	{
		uiSlot.OnSlottedObjectChanged.AddListener(_OnSlottedObjectChanged);
		uiSlot.OnObjectBeginAnimateOut.AddListener(_OnObjectBeginAnimateOut);
		uiSlot.OnObjectBeginDrag.AddListener(_OnObjectBeginDrag);
		uiSlot.OnObjectEndDrag.AddListener(_OnObjectEndDrag);
	}

	private void _OnSlottedObjectChanged(GameObject go)
	{
		_AttachToChain(go);
	}

	private void _OnObjectBeginAnimateOut(GameObject go)
	{
		_DetachFromChain(go);
	}

	private void _OnObjectBeginDrag(GameObject go)
	{
		_DetachFromChain(go);
	}

	private void _OnObjectEndDrag(GameObject go)
	{
		_AttachToChain(go);
	}

	private void _AttachToChain(GameObject go)
	{
		GameObject attachedGameObject = _attachedGameObject;
		if (SetPropertyUtility.SetObject(ref _attachedGameObject, go))
		{
			_DetachFromChain(attachedGameObject);
		}
		if ((bool)go)
		{
			PhysicsChainLink component = go.GetComponent<PhysicsChainLink>();
			if (!(component.joint.connectedBody == chainLink.body))
			{
				uiSlot.slot.CopyFrom(chainLink.outputTransform);
				chainLink.AttachLink(component);
				component.gameObject.SetActive(value: false);
				component.gameObject.SetActive(value: true);
				component.SetPhysicsEnabled(enable: true);
			}
		}
	}

	private void _DetachFromChain(GameObject go)
	{
		if (!go)
		{
			return;
		}
		PhysicsChainLink component = go.GetComponent<PhysicsChainLink>();
		if (!(component.joint.connectedBody != chainLink.body))
		{
			component.SetPhysicsEnabled(enable: false);
			component.joint.connectedBody = null;
			if (_attachedGameObject == go)
			{
				_attachedGameObject = null;
			}
		}
	}

	private void Awake()
	{
		_RegisterEvents();
		uiSlot.animateOnlyWhileDragging = true;
	}
}
