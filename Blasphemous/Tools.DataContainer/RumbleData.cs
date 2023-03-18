using Framework.Util;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.DataContainer;

[CreateAssetMenu(fileName = "rumble", menuName = "Blasphemous/Rumble")]
public class RumbleData : ScriptableObject
{
	public enum RumbleType
	{
		All,
		Left,
		Rigth
	}

	public float duration = 1f;

	[ShowIf("ShowLeft", true)]
	public AnimationCurve left;

	public bool loop;

	[ShowIf("loop", true)]
	[Range(0f, 99f)]
	public int loopCount;

	[ShowIf("ShowRight", true)]
	public AnimationCurve right;

	[ShowIf("ShowSameCurve", true)]
	public bool sameCurve;

	public RumbleType type;

	private bool ShowRight()
	{
		return (type == RumbleType.All && !sameCurve) || type == RumbleType.Rigth;
	}

	private bool ShowLeft()
	{
		return type == RumbleType.All || type == RumbleType.Left;
	}

	private bool ShowSameCurve()
	{
		return type == RumbleType.All;
	}

	[HideInEditorMode]
	[Button(ButtonSizes.Small)]
	private void TestRummble()
	{
		SingletonSerialized<RumbleSystem>.Instance.ApplyRumble(this);
	}

	[HideInEditorMode]
	[Button(ButtonSizes.Small)]
	private void StopRummble()
	{
		SingletonSerialized<RumbleSystem>.Instance.StopAllRumbles();
	}
}
