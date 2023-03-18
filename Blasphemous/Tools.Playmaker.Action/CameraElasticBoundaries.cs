using Com.LuisPedroFonseca.ProCamera2D;
using Framework.Managers;
using HutongGames.PlayMaker;
using Sirenix.OdinInspector;

namespace Tools.PlayMaker.Action;

public class CameraElasticBoundaries : FsmStateAction
{
	public FsmBool UseVerticalBoundary;

	[ShowIf("UseVerticalBoundary", true)]
	public FsmFloat VerticalDurationValue;

	public FsmBool UseHorizontalBoundary;

	[ShowIf("UseHorizontalBoundary", true)]
	public FsmFloat HorizontalDurationValue;

	public override void OnEnter()
	{
		base.OnEnter();
		SetCameraElasticBoundariesValues();
		Finish();
	}

	private void SetCameraElasticBoundariesValues()
	{
		ProCamera2DNumericBoundaries proCamera2DNumericBoundaries = Core.Logic.CameraManager.ProCamera2DNumericBoundaries;
		if ((bool)proCamera2DNumericBoundaries)
		{
			if (UseHorizontalBoundary.Value)
			{
				proCamera2DNumericBoundaries.HorizontalElasticityDuration = HorizontalDurationValue.Value;
			}
			if (UseVerticalBoundary.Value)
			{
				proCamera2DNumericBoundaries.VerticalElasticityDuration = VerticalDurationValue.Value;
			}
		}
	}
}
