using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class HighWillsFacesMovementManager : MonoBehaviour
{
	[Serializable]
	public struct FaceMovement
	{
		public Ease Ease;

		public float Length;

		public float Time;
	}

	public GameObject LeftHW;

	public GameObject MiddleHW;

	public GameObject RightHW;

	public float LeftHWMaxHorSeparation;

	public float RightHWMaxHorSeparation;

	public FaceMovement LeftHWUpwardsMov;

	public FaceMovement MiddleHWUpwardsMov;

	public FaceMovement RightHWUpwardsMov;

	public FaceMovement LeftHWDownwardsMov;

	public FaceMovement MiddleHWDownwardsMov;

	public FaceMovement RightHWDownwardsMov;

	private float leftHWMinHorSeparation;

	private float rightHWMinHorSeparation;

	private Vector3 leftHWStartPos;

	private Vector3 middleHWStartPos;

	private Vector3 rightHWStartPos;

	private float leftHWTargetHorSeparation;

	private float rightHWTargetHorSeparation;

	private bool leftHWHorMoving;

	private bool rightHWHorMoving;

	private void Start()
	{
		leftHWStartPos = LeftHW.transform.position;
		middleHWStartPos = MiddleHW.transform.position;
		rightHWStartPos = RightHW.transform.position;
		leftHWMinHorSeparation = LeftHW.transform.position.x - MiddleHW.transform.position.x;
		rightHWMinHorSeparation = RightHW.transform.position.x - MiddleHW.transform.position.x;
		leftHWTargetHorSeparation = leftHWMinHorSeparation;
		rightHWTargetHorSeparation = rightHWMinHorSeparation;
		StartLeftHWUpwardsMovement();
		StartMiddleHWDownwardsMovement();
		StartRightHWUpwardsMovement();
	}

	private void Update()
	{
		float num = LeftHW.transform.position.x - MiddleHW.transform.position.x;
		if (!Mathf.Approximately(num, leftHWTargetHorSeparation) && !leftHWHorMoving)
		{
			float endValue = LeftHW.transform.position.x + (leftHWTargetHorSeparation - num);
			leftHWHorMoving = true;
			LeftHW.transform.DOMoveX(endValue, 0.2f).SetEase(Ease.InOutQuad).OnComplete(delegate
			{
				leftHWHorMoving = false;
			});
		}
		float num2 = RightHW.transform.position.x - MiddleHW.transform.position.x;
		if (!Mathf.Approximately(num2, rightHWTargetHorSeparation) && !rightHWHorMoving)
		{
			float endValue2 = RightHW.transform.position.x + (rightHWTargetHorSeparation - num2);
			rightHWHorMoving = true;
			RightHW.transform.DOMoveX(endValue2, 0.2f).SetEase(Ease.InOutQuad).OnComplete(delegate
			{
				rightHWHorMoving = false;
			});
		}
	}

	[Button(ButtonSizes.Small)]
	public void ResetFaces()
	{
		leftHWTargetHorSeparation = leftHWMinHorSeparation;
		rightHWTargetHorSeparation = rightHWMinHorSeparation;
		LeftHW.transform.DOKill();
		MiddleHW.transform.DOKill();
		RightHW.transform.DOKill();
		LeftHW.transform.position = leftHWStartPos;
		MiddleHW.transform.position = middleHWStartPos;
		RightHW.transform.position = rightHWStartPos;
		StartLeftHWUpwardsMovement();
		StartMiddleHWDownwardsMovement();
		StartRightHWUpwardsMovement();
	}

	public void SeparateLeftFace(float portion)
	{
		leftHWTargetHorSeparation = (LeftHWMaxHorSeparation - leftHWMinHorSeparation) * portion + leftHWMinHorSeparation;
	}

	public void SeparateRightFace(float portion)
	{
		rightHWTargetHorSeparation = (RightHWMaxHorSeparation - rightHWMinHorSeparation) * portion + rightHWMinHorSeparation;
	}

	[Button(ButtonSizes.Small)]
	public void SeparateLeftFaceToMax()
	{
		SeparateLeftFace(1f);
	}

	[Button(ButtonSizes.Small)]
	public void SeparateRightFaceToMax()
	{
		SeparateRightFace(1f);
	}

	[Button(ButtonSizes.Small)]
	public void SeparateLeftFaceToMin()
	{
		SeparateLeftFace(0f);
	}

	[Button(ButtonSizes.Small)]
	public void SeparateRightFaceToMin()
	{
		SeparateRightFace(0f);
	}

	[Button(ButtonSizes.Small)]
	public void SeparateFacesToMax()
	{
		SeparateLeftFaceToMax();
		SeparateRightFaceToMax();
	}

	[Button(ButtonSizes.Small)]
	public void SeparateFacesToMin()
	{
		SeparateLeftFaceToMin();
		SeparateRightFaceToMin();
	}

	public void StartLeftHWUpwardsMovement()
	{
		StartHWVerMovement(LeftHW, LeftHWUpwardsMov, StartLeftHWDownwardsMovement);
	}

	public void StartLeftHWDownwardsMovement()
	{
		StartHWVerMovement(LeftHW, LeftHWDownwardsMov, StartLeftHWUpwardsMovement);
	}

	public void StartMiddleHWUpwardsMovement()
	{
		StartHWVerMovement(MiddleHW, MiddleHWUpwardsMov, StartMiddleHWDownwardsMovement);
	}

	public void StartMiddleHWDownwardsMovement()
	{
		StartHWVerMovement(MiddleHW, MiddleHWDownwardsMov, StartMiddleHWUpwardsMovement);
	}

	public void StartRightHWUpwardsMovement()
	{
		StartHWVerMovement(RightHW, RightHWUpwardsMov, StartRightHWDownwardsMovement);
	}

	public void StartRightHWDownwardsMovement()
	{
		StartHWVerMovement(RightHW, RightHWDownwardsMov, StartRightHWUpwardsMovement);
	}

	private void StartHWVerMovement(GameObject hw, FaceMovement fm, TweenCallback onComplete)
	{
		float endValue = hw.transform.position.y + fm.Length;
		hw.transform.DOMoveY(endValue, fm.Time).SetEase(fm.Ease).OnComplete(onComplete);
	}
}
