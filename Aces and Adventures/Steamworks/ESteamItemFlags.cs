using System;

namespace Steamworks;

[Flags]
public enum ESteamItemFlags
{
	k_ESteamItemNoTrade = 1,
	k_ESteamItemRemoved = 0x100,
	k_ESteamItemConsumed = 0x200
}
