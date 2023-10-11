using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum PlayerClass
{
	Warrior,
	Rogue,
	Mage,
	Enchantress,
	Hunter
}
