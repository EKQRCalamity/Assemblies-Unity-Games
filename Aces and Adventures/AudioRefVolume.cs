using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
public class AudioRefVolume
{
	[ProtoMember(1)]
	[UIField]
	private AudioRef _audioRef;

	[ProtoMember(2)]
	[UIField]
	[DefaultValue(AudioVolumeType.Max)]
	private AudioVolumeType _volume = AudioVolumeType.Max;

	public AudioRef audioRef => _audioRef;

	public AudioVolumeType volume => _volume;

	private bool _audioRefSpecified => _audioRef.ShouldSerialize();

	private AudioRefVolume()
	{
	}

	public AudioRefVolume(AudioCategoryType category, AudioVolumeType volume = AudioVolumeType.Max)
	{
		_audioRef = new AudioRef(category);
		_volume = volume;
	}

	public static implicit operator AudioRef(AudioRefVolume audioRefVolume)
	{
		return audioRefVolume?._audioRef;
	}

	public static implicit operator float(AudioRefVolume audioRefVolume)
	{
		return audioRefVolume?._volume.Volume() ?? 0f;
	}

	public static implicit operator bool(AudioRefVolume audioRefVolume)
	{
		return audioRefVolume?._audioRef.IsValid() ?? false;
	}

	public override string ToString()
	{
		if (!_audioRef.IsValid())
		{
			return "[No Audio]";
		}
		return _audioRef.friendlyName + " [" + EnumUtil.FriendlyName(_volume) + "]";
	}
}
