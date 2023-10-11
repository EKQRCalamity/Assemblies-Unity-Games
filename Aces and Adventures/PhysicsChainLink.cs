using UnityEngine;

[RequireComponent(typeof(CharacterJoint))]
[DisallowMultipleComponent]
public class PhysicsChainLink : MonoBehaviour
{
	public Transform inputTransform;

	public Transform outputTransform;

	[SerializeField]
	protected Transform[] _subConnectTransforms;

	private Rigidbody _body;

	private CharacterJoint _joint;

	public Rigidbody body
	{
		get
		{
			if (!_body)
			{
				return _body = GetComponent<Rigidbody>();
			}
			return _body;
		}
	}

	public CharacterJoint joint
	{
		get
		{
			if (!_joint)
			{
				return _joint = GetComponent<CharacterJoint>();
			}
			return _joint;
		}
	}

	public bool hasOpenSubChainConnector
	{
		get
		{
			if (!_subConnectTransforms.IsNullOrEmpty())
			{
				return !GetComponent<PhysicsChainGenerator>();
			}
			return false;
		}
	}

	private Transform _GetSubConnectTransformNearestToGravityDirection()
	{
		if (_subConnectTransforms.IsNullOrEmpty())
		{
			return null;
		}
		return _subConnectTransforms.MaxBy((Transform t) => Vector3.Dot(t.forward, Physics.gravity));
	}

	private PhysicsChainLink _AttachLink(PhysicsChainLink nextLinkInChain, Transform attachToTransform)
	{
		nextLinkInChain.transform.rotation = Quaternion.RotateTowards(nextLinkInChain.inputTransform.rotation, attachToTransform.rotation, 360f) * nextLinkInChain.inputTransform.localRotation.Inverse();
		nextLinkInChain.transform.position += attachToTransform.position - nextLinkInChain.inputTransform.position;
		nextLinkInChain.joint.connectedBody = body;
		nextLinkInChain.joint.autoConfigureConnectedAnchor = false;
		nextLinkInChain.joint.connectedAnchor = attachToTransform.localPosition;
		return nextLinkInChain;
	}

	protected virtual void Awake()
	{
		joint.anchor = inputTransform.localPosition;
		joint.axis = inputTransform.forward;
		joint.swingAxis = inputTransform.up;
	}

	public void AttachLink(PhysicsChainLink nextLinkInChain)
	{
		_AttachLink(nextLinkInChain, GetComponent<PhysicsChainGenerator>() ? _GetSubConnectTransformNearestToGravityDirection() : outputTransform);
	}

	public void CreateSubChain(PhysicsChainGenerator parentChainGenerator)
	{
		if (hasOpenSubChainConnector)
		{
			base.gameObject.AddComponent<PhysicsChainGenerator>().CreateFromSubChainData(parentChainGenerator, this);
		}
	}

	public virtual PhysicsChainLink SetPhysicsEnabled(bool enable)
	{
		body.isKinematic = !enable;
		return this;
	}
}
