using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/Ability/Media/ProjectileFlightAudio/", false)]
public enum ProjectileFlightAudioType : ushort
{
	TEMP
}
