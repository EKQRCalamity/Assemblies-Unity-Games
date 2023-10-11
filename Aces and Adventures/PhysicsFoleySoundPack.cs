using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "ScriptableObject/PhysicsFoleySoundPack")]
public class PhysicsFoleySoundPack : ScriptableObject
{
	public AudioMixerGroup mixerGroup;

	[Header("Impact")]
	public PooledAudioPack impacts;

	public AnimationCurve impactVolumeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve impactPitchCurve = AnimationCurve.Linear(0f, 0.75f, 1f, 1.25f);

	[Header("Sliding")]
	public PooledAudioPack slideSounds;

	public AnimationCurve slideVolumeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve slidePitchCurve = AnimationCurve.Linear(0f, 0.75f, 1f, 1.25f);

	[Header("Rolling")]
	public PooledAudioPack rollSounds;

	public AnimationCurve rollVolumeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve rollPitchCurve = AnimationCurve.Linear(0f, 0.75f, 1f, 1.25f);

	[Header("Aerodynamic Drag")]
	public PooledAudioPack dragSounds;

	public AnimationCurve dragVolumeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve dragPitchCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 1.5f);
}
