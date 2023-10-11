using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.All)]
public class YouTubeVideoAttribute : Attribute
{
	public string friendlyName;

	public string videoId;

	public int start;

	public int end;

	public YouTubeVideoAttribute(string friendlyName, string videoId, int start = -1, int end = -1)
	{
		this.friendlyName = friendlyName;
		this.videoId = videoId;
		this.start = start;
		this.end = end;
	}

	public void WatchVideo(Transform parent)
	{
		Job.Process(UIUtil.ShowWebBrowser(YouTubeUrl.ToUrl(videoId, (start > 0) ? new int?(start) : null, (end > 0) ? new int?(end) : null), parent, friendlyName, ProfileManager.options.game.ui.skipConfirmationForOpenWebPageRequests, "Play YouTube Video Request", "Would you like to play the following video?", "Play Video"), Department.UI);
	}
}
