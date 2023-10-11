using System;
using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[Flags]
public enum ContentInstallStatusFlags : byte
{
	Installed = 1,
	Update = 2,
	New = 4
}
