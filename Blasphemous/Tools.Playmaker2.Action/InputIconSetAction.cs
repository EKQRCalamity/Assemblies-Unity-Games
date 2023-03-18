using HutongGames.PlayMaker;
using RewiredConsts;
using Tools.UI;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Set the InputIcon action.")]
public class InputIconSetAction : FsmStateAction
{
	[ObjectType(typeof(InputIcon))]
	public FsmObject InputIconComponet;

	[ObjectType(typeof(RewiredEnum))]
	public FsmEnum enumAction;

	[ObjectType(typeof(InputIcon.AxisCheck))]
	public FsmEnum enumAxis;

	public override void OnEnter()
	{
		InputIcon inputIcon = InputIconComponet.Value as InputIcon;
		inputIcon.action = (int)enumAction.RawValue;
		inputIcon.axisCheck = (InputIcon.AxisCheck)enumAxis.RawValue;
		inputIcon.RefreshIcon();
		Finish();
	}
}
