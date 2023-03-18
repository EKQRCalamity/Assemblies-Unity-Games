using Com.LuisPedroFonseca.ProCamera2D;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Camera;
using UnityEngine;

public class CameraBoundariesTrigger : MonoBehaviour
{
	public LayerMask triggerMask;

	public CameraNumericBoundaries cameraBoundariesOnEnter;

	private bool boundariesAlreadySet;

	private bool levelLoaded;

	private void Awake()
	{
		LevelManager.OnLevelLoaded += OnLevelLoaded;
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		levelLoaded = true;
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (!(cameraBoundariesOnEnter == null) && !boundariesAlreadySet && levelLoaded && (triggerMask.value & (1 << collision.gameObject.layer)) > 0)
		{
			SwitchCameraBoundaries(cameraBoundariesOnEnter);
			boundariesAlreadySet = true;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		boundariesAlreadySet = false;
	}

	private void SwitchCameraBoundaries(CameraNumericBoundaries camBounds)
	{
		ProCamera2DNumericBoundaries component = Camera.main.GetComponent<ProCamera2DNumericBoundaries>();
		camBounds.SetBoundaries();
		component.ResetVerticalBoundedDuration();
		component.ResetHorizontalBoundedDuration();
	}
}
