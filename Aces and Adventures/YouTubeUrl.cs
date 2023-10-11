using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class YouTubeUrl
{
	[ProtoContract]
	[UIField]
	public struct TimeStamp : IEquatable<TimeStamp>, IComparer<TimeStamp>
	{
		public static readonly TimeStamp Default;

		[ProtoMember(1)]
		[UIField(min = 0, max = 11)]
		[UIHorizontalLayout("T")]
		private byte _hours;

		[ProtoMember(2)]
		[UIField(min = 0, max = 59)]
		[UIHorizontalLayout("T")]
		private byte _minutes;

		[ProtoMember(3)]
		[UIField(min = 0, max = 59)]
		[UIHorizontalLayout("T")]
		private byte _seconds;

		public int totalSeconds => (int)new TimeSpan(_hours, _minutes, _seconds).TotalSeconds;

		public static implicit operator int(TimeStamp timeStamp)
		{
			return timeStamp.totalSeconds;
		}

		public static implicit operator string(TimeStamp timeStamp)
		{
			return StringUtil.ToTimeFromSeconds((int)timeStamp, "d", "h", "m", "s", " ", forceShowSeconds: false).Trim();
		}

		public static implicit operator bool(TimeStamp timeStamp)
		{
			return timeStamp != Default;
		}

		public int Compare(TimeStamp x, TimeStamp y)
		{
			return x.totalSeconds.CompareTo(y.totalSeconds);
		}

		public override string ToString()
		{
			return this;
		}

		public bool Equals(TimeStamp other)
		{
			if (_hours == other._hours && _minutes == other._minutes)
			{
				return _seconds == other._seconds;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is TimeStamp)
			{
				return Equals((TimeStamp)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _seconds ^ (_minutes << 8) ^ (_hours << 16);
		}

		public static bool operator ==(TimeStamp a, TimeStamp b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(TimeStamp a, TimeStamp b)
		{
			return !a.Equals(b);
		}
	}

	private static Regex _VideoIdRegex;

	[ProtoMember(1)]
	[UIField("Video Id", 0u, null, null, null, null, null, null, false, null, 5, false, null, readOnly = true)]
	[UIHorizontalLayout("Id")]
	private string _id;

	[ProtoMember(2)]
	[UIField(order = 2u, max = 64)]
	[UIHorizontalLayout("Id")]
	private string _friendlyName;

	[ProtoMember(3)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIHeader("Start Time")]
	private TimeStamp _start;

	[ProtoMember(4)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIHeader("End Time")]
	private TimeStamp _end;

	private string _inputUrl;

	private static Regex VideoIdRegex => _VideoIdRegex ?? (_VideoIdRegex = new Regex("youtu(?:\\.be|be\\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)", RegexOptions.Compiled));

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
				_id = GetVideoId(value);
			}
		}
	}

	public string videoId => _id;

	public int? start
	{
		get
		{
			if (!_start)
			{
				return null;
			}
			return _start;
		}
	}

	public int? end
	{
		get
		{
			if (!_end)
			{
				return null;
			}
			return _end;
		}
	}

	public string url => ToUrl(videoId, start, end);

	public string friendlyName
	{
		get
		{
			if (!_friendlyName.HasVisibleCharacter())
			{
				return "<i>YouTube Video</i>";
			}
			return _friendlyName;
		}
	}

	private bool _hideTestYouTubeLink
	{
		get
		{
			if ((bool)this)
			{
				return !NodeGraphView.GetActiveView();
			}
			return true;
		}
	}

	private bool _startSpecified => _start;

	private bool _endSpecified => _end;

	public static string GetVideoId(string url)
	{
		if (!url.HasVisibleCharacter())
		{
			return null;
		}
		Match match = VideoIdRegex.Match(url);
		if (!match.Success)
		{
			return null;
		}
		return match.Groups[1].Value;
	}

	public static string ToUrl(string videoId, int? start = null, int? end = null)
	{
		string[] obj = new string[6]
		{
			"https://www.youtube.com/embed/",
			videoId,
			"?start=",
			start.GetValueOrDefault().ToString(),
			null,
			null
		};
		object obj2;
		if (!end.HasValue)
		{
			obj2 = "";
		}
		else
		{
			int? num = end;
			obj2 = "&end=" + num;
		}
		obj[4] = (string)obj2;
		obj[5] = "&rel=0";
		return string.Concat(obj);
	}

	public void PrepareForSave()
	{
		_friendlyName = _friendlyName.SetRedundantStringNull();
		if ((int)_end < (int)_start)
		{
			_end = TimeStamp.Default;
		}
	}

	public IEnumerator ShowVideo(Transform parent)
	{
		if ((bool)this)
		{
			IEnumerator showProcess = UIUtil.ShowWebBrowser(url, parent, _friendlyName, ProfileManager.options.game.ui.skipConfirmationForOpenWebPageRequests, "Play YouTube Video Request", "Would you like to play the following video?", "Play Video");
			while (showProcess.MoveNext())
			{
				yield return showProcess;
			}
		}
	}

	public static implicit operator bool(YouTubeUrl youTubeUrl)
	{
		return youTubeUrl?._id.HasVisibleCharacter() ?? false;
	}

	public static implicit operator string(YouTubeUrl url)
	{
		if (!url)
		{
			return "<i>Invalid YouTube URL</i>";
		}
		return string.Format("Id: <b>{0}</b>{1}{2}", url.videoId, url._start ? $"; Start: {url._start}" : "", url._end ? $"; End: {url._end}" : "");
	}

	public override string ToString()
	{
		return this;
	}

	[UIField("Test YouTube Link", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	[UIHideIf("_hideTestYouTubeLink")]
	[UIHorizontalLayout("Id")]
	private void _TestYouTubeLink()
	{
		if ((bool)this)
		{
			Job.Process(ShowVideo(NodeGraphView.GetActiveView().transform), Department.UI);
			return;
		}
		GameObject mainContent = UIUtil.CreateMessageBox("Could not find video id from provided URL.");
		Transform transform = NodeGraphView.GetActiveView().transform;
		UIUtil.CreatePopup("Invalid URL", mainContent, null, null, null, null, null, null, true, true, null, null, null, transform, null, null);
	}
}
