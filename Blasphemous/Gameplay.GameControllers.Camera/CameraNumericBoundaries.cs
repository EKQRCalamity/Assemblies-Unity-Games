using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using Framework.Managers;
using Gameplay.UI.Widgets;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Camera;

public class CameraNumericBoundaries : MonoBehaviour
{
	public bool notSetOnLevelLoad;

	public bool UseCameraBoundaries;

	public float RayLengh = 50f;

	public Vector2 originPos;

	[BoxGroup("Boundaries Values", true, false, 0)]
	public bool UseTopBoundary;

	[ShowIf("UseTopBoundary", true)]
	[BoxGroup("Boundaries Values", true, false, 0)]
	public float TopBoundary = 10f;

	[BoxGroup("Boundaries Values", true, false, 0)]
	public bool UseBottomBoundary = true;

	[ShowIf("UseBottomBoundary", true)]
	[BoxGroup("Boundaries Values", true, false, 0)]
	public float BottomBoundary = -10f;

	[BoxGroup("Boundaries Values", true, false, 0)]
	public bool UseLeftBoundary;

	[ShowIf("UseLeftBoundary", true)]
	[BoxGroup("Boundaries Values", true, false, 0)]
	public float LeftBoundary = -10f;

	[BoxGroup("Boundaries Values", true, false, 0)]
	public bool UseRightBoundary;

	[ShowIf("UseRightBoundary", true)]
	[BoxGroup("Boundaries Values", true, false, 0)]
	public float RightBoundary = 10f;

	[BoxGroup("Debug", true, false, 0)]
	public Color gizmoColor = Color.white;

	[BoxGroup("Debug", true, false, 0)]
	public bool drawGizmosUnselected;

	private void OnDestroy()
	{
		StopCoroutine("EnableElastic");
	}

	private void SetBoundariesAndMoveToPlayer()
	{
		SetBoundaries();
		ProCamera2DNumericBoundaries component = UnityEngine.Camera.main.GetComponent<ProCamera2DNumericBoundaries>();
		component.VerticalElasticityDuration = 0.01f;
		component.HorizontalElasticityDuration = 0.01f;
		component.UseElasticBoundaries = false;
		Core.Logic.CameraManager.ProCamera2D.MoveCameraInstantlyToPosition(Core.Logic.Penitent.transform.position);
	}

	public void SetBoundariesOnLevelLoad()
	{
		SetBoundariesAndMoveToPlayer();
		StartCoroutine(EnableElastic());
	}

	public void SetBoundariesForcingPosition()
	{
		SetBoundariesAndMoveToPlayer();
		StartCoroutine(EnableElasticCancelling());
	}

	public void SetBoundaries()
	{
		ProCamera2DNumericBoundaries component = UnityEngine.Camera.main.GetComponent<ProCamera2DNumericBoundaries>();
		component.UseNumericBoundaries = UseCameraBoundaries;
		component.UseTopBoundary = UseCameraBoundaries && UseTopBoundary;
		component.TopBoundary = TopBoundary;
		component.UseBottomBoundary = UseCameraBoundaries && UseBottomBoundary;
		component.BottomBoundary = BottomBoundary;
		component.UseLeftBoundary = UseCameraBoundaries && UseLeftBoundary;
		component.LeftBoundary = LeftBoundary;
		component.UseRightBoundary = UseCameraBoundaries && UseRightBoundary;
		component.RightBoundary = RightBoundary;
	}

	[Button(ButtonSizes.Small)]
	public void SavePosition()
	{
		originPos = base.transform.position;
	}

	[Button(ButtonSizes.Small)]
	public void CenterHere()
	{
		TopBoundary = base.transform.position.y + 5.625f;
		BottomBoundary = base.transform.position.y - 5.625f;
		LeftBoundary = base.transform.position.x - 10f;
		RightBoundary = base.transform.position.x + 10f;
	}

	[Button(ButtonSizes.Small)]
	public void CenterKeepSize()
	{
		TopBoundary -= originPos.y;
		BottomBoundary -= originPos.y;
		LeftBoundary -= originPos.x;
		RightBoundary -= originPos.x;
		TopBoundary += base.transform.position.y;
		BottomBoundary += base.transform.position.y;
		LeftBoundary += base.transform.position.x;
		RightBoundary += base.transform.position.x;
		originPos = base.transform.position;
	}

	private IEnumerator EnableElastic()
	{
		yield return new WaitForSeconds(1f);
		if (FadeWidget.instance.IsActive)
		{
			yield return new WaitForSeconds(2f);
		}
		ProCamera2DNumericBoundaries boundaries = UnityEngine.Camera.main.GetComponent<ProCamera2DNumericBoundaries>();
		boundaries.UseElasticBoundaries = true;
		ResetElasticitySeconds();
	}

	private IEnumerator EnableElasticCancelling()
	{
		yield return new WaitForSeconds(1f);
		ProCamera2DNumericBoundaries boundaries = UnityEngine.Camera.main.GetComponent<ProCamera2DNumericBoundaries>();
		boundaries.UseElasticBoundaries = true;
		boundaries.VerticalElasticityDuration = 0f;
		boundaries.HorizontalElasticityDuration = 0f;
	}

	public void ResetElasticitySeconds()
	{
		ProCamera2DNumericBoundaries component = UnityEngine.Camera.main.GetComponent<ProCamera2DNumericBoundaries>();
		component.VerticalElasticityDuration = 2f;
		component.HorizontalElasticityDuration = 2f;
	}

	private void OnDrawGizmosSelected()
	{
		if (UseCameraBoundaries)
		{
			Gizmos.color = gizmoColor;
			Gizmos.DrawWireSphere(originPos, 0.25f);
			if (UseTopBoundary)
			{
				Gizmos.DrawLine(new Vector2(RayLengh, TopBoundary), new Vector2(0f - RayLengh, TopBoundary));
			}
			if (UseBottomBoundary)
			{
				Gizmos.DrawLine(new Vector2(RayLengh, BottomBoundary), new Vector2(0f - RayLengh, BottomBoundary));
			}
			if (UseLeftBoundary)
			{
				Gizmos.DrawLine(new Vector2(LeftBoundary, RayLengh), new Vector2(LeftBoundary, 0f - RayLengh));
			}
			if (UseRightBoundary)
			{
				Gizmos.DrawLine(new Vector2(RightBoundary, RayLengh), new Vector2(RightBoundary, 0f - RayLengh));
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (UseCameraBoundaries && drawGizmosUnselected)
		{
			Gizmos.color = gizmoColor;
			Gizmos.DrawWireSphere(originPos, 0.25f);
			if (UseTopBoundary)
			{
				Gizmos.DrawLine(new Vector2(RayLengh, TopBoundary), new Vector2(0f - RayLengh, TopBoundary));
			}
			if (UseBottomBoundary)
			{
				Gizmos.DrawLine(new Vector2(RayLengh, BottomBoundary), new Vector2(0f - RayLengh, BottomBoundary));
			}
			if (UseLeftBoundary)
			{
				Gizmos.DrawLine(new Vector2(LeftBoundary, RayLengh), new Vector2(LeftBoundary, 0f - RayLengh));
			}
			if (UseRightBoundary)
			{
				Gizmos.DrawLine(new Vector2(RightBoundary, RayLengh), new Vector2(RightBoundary, 0f - RayLengh));
			}
		}
	}
}
