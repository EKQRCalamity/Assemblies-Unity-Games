using System.Collections;
using Framework.Managers;
using Gameplay.GameControllers.Camera;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Pietat.PietatAnimations;

public class PietatAnimations : MonoBehaviour
{
	private CameraPlayerOffset _cameraPlayerOffset;

	private Pietat _pietat;

	public float cameraXOffset = 5f;

	private void Awake()
	{
		_pietat = GetComponent<Pietat>();
	}

	private void Start()
	{
		_cameraPlayerOffset = Core.Logic.CameraManager.CameraPlayerOffset;
	}

	public void PietatIsAttacking()
	{
		if (!(_pietat == null) && !_pietat.IsAttacking)
		{
			_pietat.IsAttacking = true;
		}
	}

	public void CenterCamera()
	{
	}

	private IEnumerator CenterCameraCoroutine()
	{
		float currentCameraXOffset = _cameraPlayerOffset.XOffset;
		while (currentCameraXOffset <= cameraXOffset)
		{
			currentCameraXOffset += 0.05f;
			_cameraPlayerOffset.XOffset = currentCameraXOffset;
			yield return new WaitForSeconds(0.02f);
		}
	}
}
