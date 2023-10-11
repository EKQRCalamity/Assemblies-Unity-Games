using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "ScriptableObject/SoundPack/CardLayoutSoundPack")]
public class CardLayoutSoundPack : ScriptableObject
{
	public AudioMixerGroup mixerGroup;

	[Header("Pointer Over")]
	public PooledAudioPack onPointerEnter;

	public PooledAudioPack onPointerExit;

	[Header("Click")]
	public PooledAudioPack onPointerDown;

	public PooledAudioPack onPointerUp;

	[Header("Drag")]
	public PooledAudioPack onDragBegin;

	public PooledAudioPack onDragEnd;

	[Header("Enter Layout")]
	public bool customEnterEmptyLayout;

	[HideInInspectorIf("_hideOnEnterLayoutEmpty", false)]
	public PooledAudioPack onEnterEmptyLayout;

	public PooledAudioPack onEnterLayout;

	[Header("Exit Layout")]
	public bool customExitEmptyLayout;

	[HideInInspectorIf("_hideOnExitLayoutEmpty", false)]
	public PooledAudioPack onExitEmptyLayout;

	public PooledAudioPack onExitLayout;

	private bool _hideOnEnterLayoutEmpty => !customEnterEmptyLayout;

	private bool _hideOnExitLayoutEmpty => !customExitEmptyLayout;

	public PooledAudioPack GetEnterLayout(bool enteringEmptyLayout)
	{
		if (!(customEnterEmptyLayout && enteringEmptyLayout))
		{
			return onEnterLayout;
		}
		return onEnterEmptyLayout;
	}

	public PooledAudioPack GetExitLayout(bool exitingEmptyLayout)
	{
		if (!(customExitEmptyLayout && exitingEmptyLayout))
		{
			return onExitLayout;
		}
		return onExitEmptyLayout;
	}
}
