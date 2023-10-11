using UnityEngine;
using UnityEngine.Video;

public class VideoClipRef : MonoBehaviour
{
	public VideoClip clip;

	public static implicit operator VideoClip(VideoClipRef r)
	{
		return r?.clip;
	}
}
