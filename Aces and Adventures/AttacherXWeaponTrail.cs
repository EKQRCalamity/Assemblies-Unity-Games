using System;
using UnityEngine;
using XftWeapon;

[RequireComponent(typeof(XWeaponTrail))]
public class AttacherXWeaponTrail : Attacher
{
	[Header("Trail=======================================================================================================")]
	public bool flipStartAndEnd;

	public AttacherGradientData color;

	private XWeaponTrail _trail;

	public XWeaponTrail trail
	{
		get
		{
			if (_trail == null)
			{
				_trail = GetComponent<XWeaponTrail>();
				_trail.UseWith2D = false;
				color.initialValue = _trail.MyColor;
			}
			return _trail;
		}
	}

	private void Awake()
	{
		trail.PointStart = base.transform;
		trail.PointEnd = base.transform;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		trail.Deactivate();
	}

	private void Update()
	{
		if ((bool)trail.PointStart && !trail.PointStart.gameObject.activeInHierarchy)
		{
			trail.PointStart = null;
		}
		if ((bool)trail.PointEnd && !trail.PointEnd.gameObject.activeInHierarchy)
		{
			trail.PointEnd = null;
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (_samplesDirty)
		{
			_OnSamplesChange();
		}
	}

	protected override void _OnSamplesChange()
	{
		if (color.enabled)
		{
			trail.MyColor = color.GetSampleValue(this);
		}
	}

	protected override void _OnBeginDetach()
	{
		trail.StopSmoothly(life.detachTime);
	}

	protected override bool _ShouldSignalDetachComplete()
	{
		if (base._ShouldSignalDetachComplete())
		{
			return trail.finished;
		}
		return false;
	}

	private void _Attach(Transform trailEnd, Transform trailStart)
	{
		attach.to = trailEnd;
		trail.PointEnd = (flipStartAndEnd ? trailStart : trailEnd);
		trail.PointStart = (flipStartAndEnd ? trailEnd : trailStart);
		trail.Activate();
	}

	public void AttachAll(Transform trailEnd, Transform trailStart, float lifetime = 0f, bool deactivateOnDetach = true, bool clearOnDisable = true, bool setChildrenAttacherLifetimes = true, bool? detachOnExpire = null)
	{
		Attach(trailEnd, lifetime, deactivateOnDetach, clearOnDisable, setChildrenAttacherLifetimes, detachOnExpire);
		Attacher[] array = base.childAttachers;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is AttacherXWeaponTrail attacherXWeaponTrail)
			{
				attacherXWeaponTrail._Attach(trailEnd, trailStart);
			}
		}
	}

	public AttacherXWeaponTrail ApplySettings(System.Random random, IAttacherXWeaponTrailSettings settings, bool applyToChildren = true)
	{
		if (applyToChildren)
		{
			foreach (AttacherXWeaponTrail item in base.gameObject.GetComponentsInChildrenPooled<AttacherXWeaponTrail>())
			{
				settings.Apply(random, item);
			}
			return this;
		}
		settings.Apply(random, this);
		return this;
	}
}
