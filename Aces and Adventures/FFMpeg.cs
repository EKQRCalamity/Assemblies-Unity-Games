using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public static class FFMpeg
{
	public struct VolumeData
	{
		public float integrated;

		public float peak;

		public float loudnessRange;

		public float threshold;

		public float offset;

		public override string ToString()
		{
			return $"Integrated: {integrated}, Peak: {peak}, LoudnessRange: {loudnessRange}, Threshold: {threshold}, Offset: {offset}";
		}
	}

	private const string FFMPEG = "ffmpeg";

	private const string FFPROBE = "ffprobe";

	public const int OGG_QUALITY = 10;

	public const int OGG_QUALITY_USER = 0;

	public const int MIN_OGG_QUALITY = 0;

	public const int MAX_OGG_QUALITY = 10;

	public const float LOUDNESS_TARGET = -16f;

	public const float MUSIC_LOUDNESS_TARGET = -10f;

	public const float AMBIENT_LOUDNESS_TARGET = -24f;

	public const float MAX_TRUE_PEAK_DB = -1f;

	public const float MAX_LOUDNESS_RANGE = 11f;

	private const float MIN_DURATION_FOR_LOUDNESS_NORMALIZATION = 6f;

	public static readonly string[] IMPORT_AUDIO_FORMATS;

	public static readonly string IMPORT_AUDIO_FILTER;

	private const string INTEGRATED_LOUDNESS = "Input Integrated";

	private const string TRUE_PEAK = "Input True Peak";

	private const string LOUDNESS_RANGE = "Input LRA";

	private const string THRESHOLD = "Input Threshold";

	private const string OFFSET = "Target Offset";

	private static readonly string[] LOUD_SPLIT;

	private static readonly string[] LOUD_SPLIT_SECOND;

	private static readonly HashSet<string> LOUDNESS_PARAMS;

	static FFMpeg()
	{
		IMPORT_AUDIO_FORMATS = new string[48]
		{
			".flac", ".wav", ".wave", ".pac", ".ofr", ".shn", ".wv", ".ape", ".aiff", ".au",
			".alac", ".aal", ".mlp", ".tta", ".opus", ".aac", ".atrac", ".ac3", ".mp2", ".mpc",
			".wma", ".mp3", ".ogg", ".m4a", ".avi", ".mp4", ".m4p", ".m4v", ".mov", ".qt",
			".wmv", ".flv", ".f4v", ".f4p", ".f4a", ".f4b", ".vob", ".ogv", ".rm", ".rmvb",
			".mpg", ".mp2", ".mpeg", ".mpe", ".mpv", ".m2v", ".3gp", ".3g2"
		};
		LOUD_SPLIT = new string[1] { ":" };
		LOUD_SPLIT_SECOND = new string[1] { " " };
		LOUDNESS_PARAMS = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Input Integrated", "Input True Peak", "Input LRA", "Input Threshold", "Target Offset" };
		IMPORT_AUDIO_FILTER = StringUtil.CreateFileFilter(IMPORT_AUDIO_FORMATS);
	}

	public static string Transcode(string inputPath, string outputPath = null, int? quality = 10, bool forceMono = false, float? normalizeLUFS = null, float? trimStart = null, float? trimEnd = null, float? fadeInEnd = null, float? fadeOutStart = null, float? maxPeak = null, bool dualMono = false)
	{
		outputPath = Path.ChangeExtension(outputPath ?? inputPath, quality.HasValue ? ".ogg" : ".wav");
		maxPeak = maxPeak ?? (-1f);
		VolumeData? volumeData = (normalizeLUFS.HasValue ? new VolumeData?(GetLoudness(inputPath, normalizeLUFS.Value, maxPeak.Value, 11f, dualMono)) : null);
		float valueOrDefault = trimStart.GetValueOrDefault();
		float length = GetLength(inputPath);
		trimEnd = trimEnd ?? Mathf.Min(600f, length);
		float value = trimEnd.Value;
		string text = StringUtil.Build(",", volumeData.HasValue ? $"loudnorm=I={normalizeLUFS.Value.ToStringNoScientificNotation()}:TP={maxPeak.Value.ToStringNoScientificNotation()}:LRA={11f.ToStringNoScientificNotation()}:measured_I={volumeData.Value.integrated.ToStringNoScientificNotation()}:measured_TP={volumeData.Value.peak.ToStringNoScientificNotation()}:measured_LRA={volumeData.Value.loudnessRange.ToStringNoScientificNotation()}:measured_thresh={volumeData.Value.threshold.ToStringNoScientificNotation()}:offset={volumeData.Value.offset.ToStringNoScientificNotation()}:linear=true:print_format=summary" : "", (trimStart.HasValue || trimEnd.HasValue) ? ("atrim=" + (trimStart.HasValue ? ("start=" + trimStart.Value.ToStringNoScientificNotation() + (trimEnd.HasValue ? ":" : "")) : "") + (trimEnd.HasValue ? ("end=" + trimEnd.Value.ToStringNoScientificNotation()) : "")) : "", (fadeInEnd.HasValue && fadeInEnd.Value - valueOrDefault > MathUtil.BigEpsilon) ? ("afade=t=in:st=" + valueOrDefault.ToStringNoScientificNotation() + ":d=" + (fadeInEnd.Value - valueOrDefault).ToStringNoScientificNotation()) : "", (fadeOutStart.HasValue && value - fadeOutStart.Value > MathUtil.BigEpsilon) ? ("afade=t=out:st=" + fadeOutStart.Value.ToStringNoScientificNotation() + ":d=" + (value - fadeOutStart.Value).ToStringNoScientificNotation()) : "");
		if (!text.IsNullOrEmpty())
		{
			text = "-af " + text + " ";
		}
		string text2 = (quality.HasValue ? ("-codec:a libvorbis -qscale:a " + quality.Value + " ") : "");
		string args = "-y " + volumeData.HasValue.ToText(_GetLoopsNeededForDuration(length)) + "-i \"" + inputPath + "\" " + (forceMono ? "-ac 1 " : "") + "-vn " + text2 + text + "\"" + outputPath + "\"";
		ProcessUtil.Run("ffmpeg", args);
		Debug.Log("Transcode: File Created = " + File.Exists(outputPath));
		return outputPath;
	}

	public static void Convert(string inputPath, string outputPath)
	{
		new FileInfo(outputPath).Directory.Create();
		ProcessUtil.Run("ffmpeg", $"-y -i \"{inputPath}\" -vn \"{outputPath}\"");
	}

	public static VolumeData GetLoudness(string inputPath, float targetLoudness, float maxPeak, float loudnessRange, bool dualMono)
	{
		Dictionary<string, float> dictionary = (from s in ProcessUtil.Run("ffmpeg", string.Format(_GetLoopsNeededForDuration(GetLength(inputPath)) + "-i \"{0}\" -af loudnorm=I={1}:dual_mono={2}:TP={3}:LRA={4}:print_format=summary -f null -", inputPath, targetLoudness.ToStringNoScientificNotation(), dualMono.ToString().ToLower(), maxPeak.ToStringNoScientificNotation(), loudnessRange.ToStringNoScientificNotation()), redirectStandardInput: false, redirectStandardOutput: false, redirectStandardError: true).standardError
			where !s.IsNullOrEmpty()
			select s.Split(LOUD_SPLIT, 2, StringSplitOptions.None) into s
			where s.Length == 2 && LOUDNESS_PARAMS.Contains(s[0])
			select s).ToDictionary((string[] s) => s[0], (string[] s) => float.Parse(s[1].Split(LOUD_SPLIT_SECOND, StringSplitOptions.RemoveEmptyEntries)[0]), StringComparer.OrdinalIgnoreCase);
		VolumeData volumeData = default(VolumeData);
		volumeData.integrated = dictionary["Input Integrated"];
		volumeData.peak = dictionary["Input True Peak"];
		volumeData.loudnessRange = dictionary["Input LRA"];
		volumeData.threshold = dictionary["Input Threshold"];
		volumeData.offset = dictionary["Target Offset"];
		VolumeData volumeData2 = volumeData;
		Debug.Log("GetLoudness(" + inputPath + "): " + volumeData2.ToString());
		return volumeData;
	}

	public static float GetLength(string inputPath)
	{
		return float.Parse(ProcessUtil.Run("ffprobe", $"-i \"{inputPath}\" -show_entries format=duration -v quiet -of csv=\"p=0\"", redirectStandardInput: false, redirectStandardOutput: true).standardOutput.First());
	}

	public static IEnumerator GetWaveFormTextureProcess(string input, int width = 512, int height = 256, bool nonReadable = true)
	{
		string output = IOUtil.GetUniqueTempFilepath("Process", ".png");
		yield return ToBackgroundThread.Create();
		ProcessUtil.Run("ffmpeg", string.Format("-i \"{0}\" -filter_complex \"showwavespic=s={2}x{3}:split_channels=1\" -frames:v 1 \"{1}\"", input, output, width, height));
		yield return ToMainThread.Create();
		using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(IOUtil.ToURI(output), nonReadable))
		{
			request.SendWebRequest();
			while (!request.downloadHandler.isDone)
			{
				yield return null;
			}
			yield return DownloadHandlerTexture.GetContent(request);
		}
		if (File.Exists(output))
		{
			File.Delete(output);
		}
	}

	public static Job GetWaveFormTexture(string input, int width = 512, int height = 256, bool nonReadable = true)
	{
		return Job.Process(GetWaveFormTextureProcess(input, width, height, nonReadable));
	}

	public static List<float> GetSamples(string input, ushort? sampleRate = null)
	{
		List<float> samples = new List<float>();
		ProcessUtil.Run("ffmpeg", string.Format("-i \"{0}\" -vn{1}-f f32le -", input, sampleRate.HasValue ? (" -ar " + sampleRate.Value + " ") : " "), redirectStandardInput: false, redirectStandardOutput: false, redirectStandardError: false, createNoWindow: true, useShellExecute: false, delegate(StreamReader streamReader)
		{
			byte[] array = new byte[1024];
			int num;
			while ((num = streamReader.BaseStream.Read(array, 0, array.Length)) > 0)
			{
				for (int i = 0; i < num; i += 4)
				{
					samples.Add(BitConverter.ToSingle(array, i));
				}
			}
		});
		return samples;
	}

	private static string _GetLoopsNeededForDuration(float clipLength, float targetDuration = 6f)
	{
		if (!(clipLength >= targetDuration))
		{
			return "-stream_loop " + Math.Min((long)(targetDuration / Math.Max(clipLength, 0.001f)), 32767L) + " ";
		}
		return "";
	}
}
