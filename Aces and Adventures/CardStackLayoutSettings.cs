using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Card Layout/CardStackLayoutSettings")]
public class CardStackLayoutSettings : ScriptableObject
{
	[Range(0f, 10f)]
	public float thicknessPadding = 1f;

	[Range(0f, 10f)]
	public float additionalEnterTargetThicknessPadding;

	[Range(-1f, 1f)]
	public float leftRightSkew;

	[Range(-1f, 1f)]
	public float downUpSkew;

	public bool thicknessPadBottom = true;

	public CardLayoutRandomSettings randomness;

	public float dragRotationPerSpeed = 30f;

	public bool useDynamicTransitions;

	[Header("Transition Randomness Nudging")]
	public int randomNudgeCount;

	public float randomNudgeFadePower;
}
