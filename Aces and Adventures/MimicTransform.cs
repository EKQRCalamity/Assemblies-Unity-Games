using UnityEngine;

public class MimicTransform : MonoBehaviour
{
	[SerializeField]
	protected Transform _target;

	public bool mimicPositon = true;

	public bool mimicRotation;

	public bool mimicScale;

	public bool mimicLife;

	public Transform target
	{
		get
		{
			return _target;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _target, value))
			{
				LateUpdate();
			}
		}
	}

	private void LateUpdate()
	{
		if (!target)
		{
			if (mimicLife)
			{
				Object.Destroy(base.gameObject);
			}
			return;
		}
		if (mimicPositon)
		{
			base.transform.position = target.position;
		}
		if (mimicRotation)
		{
			base.transform.rotation = target.rotation;
		}
		if (mimicScale)
		{
			base.transform.SetWorldScale(target.GetWorldScale());
		}
		if (mimicLife && !target.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public MimicTransform SetData(Transform target, bool position = true, bool rotation = true, bool scale = true, bool life = true)
	{
		this.target = target;
		mimicPositon = position;
		mimicRotation = rotation;
		mimicScale = scale;
		mimicLife = life;
		return this;
	}
}
