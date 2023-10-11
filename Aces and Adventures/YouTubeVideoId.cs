using System.Collections;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class YouTubeVideoId
{
	[ProtoMember(1)]
	[UIField("Video Id", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 2u, readOnly = true)]
	[UIHorizontalLayout("Video Id")]
	private string _id;

	private string _inputUrl;

	[UIField("Paste YouTube Video URL Here", 0u, null, null, null, null, null, null, false, null, 5, false, null, validateOnChange = true, order = 1u, max = 256, view = "UI/Input Field Multiline", collapse = UICollapseType.Open)]
	private string inputUrl
	{
		get
		{
			return _inputUrl;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _inputUrl, value))
			{
				_id = YouTubeUrl.GetVideoId(value);
			}
		}
	}

	private bool _hideTestYouTubeLink => !this;

	public static IEnumerator ShowVideo(string videoId, string friendlyName, Transform parent)
	{
		IEnumerator showProcess = UIUtil.ShowWebBrowser(YouTubeUrl.ToUrl(videoId, 0), parent, friendlyName, ProfileManager.options.game.ui.skipConfirmationForOpenWebPageRequests, "Play YouTube Video Request", "Would you like to play the following video?", "Play Video");
		while (showProcess.MoveNext())
		{
			yield return showProcess;
		}
	}

	public static implicit operator bool(YouTubeVideoId videoId)
	{
		return videoId?._id.HasVisibleCharacter() ?? false;
	}

	public static implicit operator string(YouTubeVideoId videoId)
	{
		if (!videoId)
		{
			return "";
		}
		return videoId._id;
	}

	public override string ToString()
	{
		return this;
	}

	[UIField("Test YouTube Link", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	[UIHideIf("_hideTestYouTubeLink")]
	[UIHorizontalLayout("Video Id")]
	private void _TestYouTubeLink()
	{
		Job.Process(ShowVideo(this, "Test YouTube Link", DataRefControl.GetMainControlForDataType<ContentRef>().transform), Department.UI);
	}
}
