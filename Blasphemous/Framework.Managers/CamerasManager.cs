using Com.LuisPedroFonseca.ProCamera2D;
using Gameplay.GameControllers.Camera;
using UnityEngine;

namespace Framework.Managers;

public class CamerasManager : GameSystem
{
	public override void OnGUI()
	{
		DebugResetLine();
		DebugDrawTextLine("CameraManager -------------------------------------");
		ProCamera2DNumericBoundaries proCamera2DNumericBoundaries = Core.Logic.CameraManager.ProCamera2DNumericBoundaries;
		DebugDrawTextLine("--Camera  SIZE:" + Core.Logic.CameraManager.ProCamera2D.GameCamera.orthographicSize);
		DebugDrawTextLine("--Boundaries use " + proCamera2DNumericBoundaries.UseNumericBoundaries);
		ShowBounday("Top", proCamera2DNumericBoundaries.UseTopBoundary, proCamera2DNumericBoundaries.TopBoundary);
		ShowBounday("Bottom", proCamera2DNumericBoundaries.UseBottomBoundary, proCamera2DNumericBoundaries.BottomBoundary);
		ShowBounday("Left", proCamera2DNumericBoundaries.UseLeftBoundary, proCamera2DNumericBoundaries.LeftBoundary);
		ShowBounday("Right", proCamera2DNumericBoundaries.UseRightBoundary, proCamera2DNumericBoundaries.RightBoundary);
		CameraPlayerOffset cameraPlayerOffset = Core.Logic.CameraManager.CameraPlayerOffset;
		string text = "NULL";
		if (cameraPlayerOffset.PlayerTarget != null)
		{
			if (cameraPlayerOffset.PlayerTarget.TargetTransform != null)
			{
				text = cameraPlayerOffset.PlayerTarget.TargetTransform.name + " ";
			}
			text = text + "Offset X:" + cameraPlayerOffset.PlayerTarget.TargetOffset.x;
			text = text + " Y:" + cameraPlayerOffset.PlayerTarget.TargetOffset.y;
		}
		DebugDrawTextLine("--Target " + text);
		text = "Default X:" + cameraPlayerOffset.DefaultTargetOffset.x;
		text = text + " Y:" + cameraPlayerOffset.DefaultTargetOffset.y;
		DebugDrawTextLine("--Offset " + text);
		Vector2 overallOffset = Core.Logic.CameraManager.ProCamera2D.OverallOffset;
		DebugDrawTextLine("--Overall Offset X:" + overallOffset.x + "  Y:" + overallOffset.y);
	}

	private void ShowBounday(string name, bool use, float value)
	{
		DebugDrawTextLine("Bound " + name + ": " + use + " (" + value + ")");
	}
}
