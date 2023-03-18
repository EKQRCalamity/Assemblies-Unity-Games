using DG.Tweening;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Tools.Level.Actionables;
using UnityEngine;

public class GuillotineTrap : MonoBehaviour
{
	public float oscillationTime;

	public Ease oscillationCurve;

	public float oscillationAngle;

	[EventRef]
	public string MotionAudioFx;

	private SimpleDamageArea damageArea;

	private AreaAttackDummyEntity dummyEntity;

	private Tweener tweener;

	private void Start()
	{
		Vector3 euler = new Vector3(0f, 0f, 0f - oscillationAngle);
		Vector3 endValue = new Vector3(0f, 0f, oscillationAngle);
		damageArea = GetComponentInChildren<SimpleDamageArea>();
		dummyEntity = damageArea.GetDummyEntity();
		EntityOrientation orientation = ((!(euler.z < 0f)) ? EntityOrientation.Left : EntityOrientation.Right);
		dummyEntity.SetOrientation(orientation, allowFlipRenderer: false);
		base.transform.rotation = Quaternion.Euler(euler);
		tweener = base.transform.DORotate(endValue, oscillationTime).SetEase(oscillationCurve).SetLoops(-1, LoopType.Yoyo)
			.OnStepComplete(StepComplete);
	}

	private void Update()
	{
		if (Core.Logic.IsPaused)
		{
			tweener.Pause();
		}
		else if (!tweener.IsPlaying())
		{
			tweener.TogglePause();
		}
	}

	private void StepComplete()
	{
		PlayMotionAudio();
		EntityOrientation orientation = dummyEntity.Status.Orientation;
		dummyEntity.SetOrientation((orientation == EntityOrientation.Right) ? EntityOrientation.Left : EntityOrientation.Right, allowFlipRenderer: false);
	}

	private void PlayMotionAudio()
	{
		if (!string.IsNullOrEmpty(MotionAudioFx))
		{
			Core.Audio.PlayOneShot(MotionAudioFx, base.transform.position);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Vector2 down = Vector2.down;
		Quaternion quaternion = Quaternion.Euler(0f, 0f, oscillationAngle);
		Quaternion quaternion2 = Quaternion.Euler(0f, 0f, 0f - oscillationAngle);
		Vector3 vector = quaternion * down;
		float num = Mathf.Abs(base.transform.GetChild(0).localPosition.y);
		Gizmos.DrawLine(base.transform.position, base.transform.position + vector * num);
		vector = quaternion2 * down;
		Gizmos.DrawLine(base.transform.position, base.transform.position + vector * num);
	}
}
