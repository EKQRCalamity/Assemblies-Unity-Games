using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf;
using Steamworks;

[ProtoContract]
public class LeaderboardProgress
{
	[ProtoContract]
	public struct Data : IComparable<Data>
	{
		[ProtoMember(1)]
		public readonly int mana;

		[ProtoMember(2)]
		public readonly int time;

		[ProtoMember(3)]
		public readonly PlayerClass playerClass;

		[ProtoMember(4)]
		public readonly int reloadCount;

		[ProtoMember(5)]
		public readonly uint modifierId;

		public bool isValid => time != 0;

		public bool completed => time > 0;

		public int score
		{
			get
			{
				if (!completed)
				{
					return Math.Max(4320000, -mana + 8640000);
				}
				return time;
			}
		}

		public int absTime => Math.Abs(time);

		public static Data Best(Data a, Data b)
		{
			if (a.CompareTo(b) > 0)
			{
				return b;
			}
			return a;
		}

		public Data(int mana, int time, bool completed, int reloadCount, PlayerClass playerClass, DataRef<ProceduralNodeData> modifier)
		{
			this.mana = mana;
			this.time = Math.Max(1, time) * completed.ToInt(1, -1);
			this.playerClass = playerClass;
			this.reloadCount = reloadCount;
			modifierId = modifier;
		}

		public Data(int[] leaderboardDetails, ref int index)
		{
			this = default(Data);
			if (index + 4 <= leaderboardDetails.Length)
			{
				mana = leaderboardDetails[index++];
				time = leaderboardDetails[index++];
				int num = leaderboardDetails[index++];
				playerClass = EnumUtil<PlayerClass>.Round(num & BitMask.First8Bits);
				reloadCount = (num >> 8) & BitMask.First24Bits;
				modifierId = (uint)leaderboardDetails[index++];
			}
		}

		public int[] GetData()
		{
			return new int[4]
			{
				mana,
				time,
				(int)playerClass | (reloadCount << 8),
				(int)modifierId
			};
		}

		public int CompareTo(Data other)
		{
			if (isValid == other.isValid)
			{
				if (completed == other.completed)
				{
					if (!completed)
					{
						if (mana == other.mana)
						{
							return other.time - time;
						}
						return other.mana - mana;
					}
					if (time == other.time)
					{
						return other.mana - mana;
					}
					return time - other.time;
				}
				return completed.ToInt(-1, 1);
			}
			return isValid.ToInt(-1, 1);
		}

		public static implicit operator bool(Data data)
		{
			return data.isValid;
		}

		public static bool operator ==(Data a, Data b)
		{
			if (a.mana == b.mana && a.time == b.time && a.playerClass == b.playerClass)
			{
				return a.reloadCount == b.reloadCount;
			}
			return false;
		}

		public static bool operator !=(Data a, Data b)
		{
			return !(a == b);
		}

		public override string ToString()
		{
			return $"Mana = {mana}, Time = {time}, Class = {playerClass}, Reload Count = {reloadCount}, Score = {score}";
		}
	}

	public struct Entry
	{
		public readonly LeaderboardEntry_t entry;

		public readonly Data data;

		public bool isPlayerEntry
		{
			get
			{
				if (entry.m_steamIDUser == Steam.SteamId)
				{
					return entry.m_steamIDUser != CSteamID.Nil;
				}
				return false;
			}
		}

		public bool isFriendEntry
		{
			get
			{
				if (!isPlayerEntry)
				{
					return Steam.Friends.IsFriend(entry.m_steamIDUser);
				}
				return true;
			}
		}

		public Entry(LeaderboardEntry_t entry, Data data)
		{
			this.entry = entry;
			this.data = data;
		}

		public async Task<string> GetPersonaNameAsync()
		{
			return await Steam.Friends.GetPersonaNameAsync(entry.m_steamIDUser);
		}

		public static implicit operator bool(Entry entry)
		{
			return entry.data;
		}

		public override string ToString()
		{
			return $"{Steam.Friends.GetPersonaNameAsync(entry.m_steamIDUser).Result}: {data}";
		}
	}

	private const int INCOMPLETE_PENALTY = 8640000;

	private const int MIN_INCOMPLETE_SCORE = 4320000;

	private static readonly DateTime START_DATE = new DateTime(2023, 5, 1, 16, 0, 0, DateTimeKind.Utc);

	public const int MIN_DAY = 31;

	private static DataRef<AdventureData>[] _DailyLeaderboardAdventurers;

	[ProtoMember(1)]
	private int _epochDay;

	[ProtoMember(2, OverwriteList = true)]
	private Dictionary<uint, Data> _data;

	[ProtoMember(3, OverwriteList = true)]
	private Dictionary<uint, List<int>> _previousEpochDays;

	[ProtoMember(4, OverwriteList = true)]
	private HashSet<uint> _completedDailies;

	private static DataRef<AdventureData>[] DailyLeaderboardAdventures => _DailyLeaderboardAdventurers ?? (_DailyLeaderboardAdventurers = DataRef<AdventureData>.All.Where((DataRef<AdventureData> d) => d.data.dailyLeaderboardEnabled).OrderBy((Func<DataRef<AdventureData>, uint>)((DataRef<AdventureData> d) => d)).ToArray());

	private Dictionary<uint, Data> data => _data ?? (_data = new Dictionary<uint, Data>());

	private Dictionary<uint, List<int>> previousEpochDays => _previousEpochDays ?? (_previousEpochDays = new Dictionary<uint, List<int>>());

	private HashSet<uint> completedDailies => _completedDailies ?? (_completedDailies = new HashSet<uint>());

	private int epochDay
	{
		get
		{
			return _epochDay;
		}
		set
		{
			if (value <= _epochDay)
			{
				return;
			}
			if (_epochDay != 0)
			{
				foreach (KeyValuePair<uint, Data> datum in data)
				{
					if ((bool)datum.Value)
					{
						(previousEpochDays.GetValueOrDefault(datum.Key) ?? (previousEpochDays[datum.Key] = new List<int>())).Add(_epochDay);
					}
				}
			}
			data.Clear();
			completedDailies.Clear();
			_epochDay = value;
		}
	}

	public static int? GetDay(uint? fallbackServerTime = null)
	{
		uint? num = Steam.Utils.GetServerRealTime() ?? fallbackServerTime;
		if (num.HasValue)
		{
			uint valueOrDefault = num.GetValueOrDefault();
			return (MathUtil.FromUnixEpoch(valueOrDefault) - START_DATE).Days;
		}
		return null;
	}

	public static TimeSpan GetTimeToReset(uint? fallbackServerTime = null)
	{
		uint? num = Steam.Utils.GetServerRealTime() ?? fallbackServerTime;
		if (num.HasValue)
		{
			uint valueOrDefault = num.GetValueOrDefault();
			int? day = GetDay(fallbackServerTime);
			if (day.HasValue)
			{
				int valueOrDefault2 = day.GetValueOrDefault();
				return GetDate(valueOrDefault2 + 1) - MathUtil.FromUnixEpoch(valueOrDefault);
			}
		}
		return default(TimeSpan);
	}

	public static DateTime GetDate(int day)
	{
		return START_DATE + TimeSpan.FromDays(day);
	}

	public static PoolKeepItemDictionaryHandle<DataRef<AdventureData>, Data> ReadDetails(int[] leaderboardDetails)
	{
		PoolKeepItemDictionaryHandle<DataRef<AdventureData>, Data> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<DataRef<AdventureData>, Data>();
		int index = 0;
		DataRef<AdventureData>[] dailyLeaderboardAdventures = DailyLeaderboardAdventures;
		foreach (DataRef<AdventureData> key in dailyLeaderboardAdventures)
		{
			poolKeepItemDictionaryHandle[key] = new Data(leaderboardDetails, ref index);
		}
		return poolKeepItemDictionaryHandle;
	}

	public static Data GetData(DataRef<AdventureData> adventure, int[] leaderboardDetails)
	{
		using PoolKeepItemDictionaryHandle<DataRef<AdventureData>, Data> poolKeepItemDictionaryHandle = ReadDetails(leaderboardDetails);
		return poolKeepItemDictionaryHandle.value.GetValueOrDefault(adventure);
	}

	public static string GetDailyName(int dailyLeaderboard)
	{
		return $"{dailyLeaderboard}";
	}

	public bool SetData(int day, DataRef<AdventureData> adventure, Data newData)
	{
		epochDay = day;
		Data data2 = (this.data[adventure] = Data.Best(newData, this.data.GetValueOrDefault(adventure)));
		return data2 == newData;
	}

	public bool SetData(int day, DataRef<AdventureData> adventure, int mana, int time, bool completed, int reloadCount, PlayerClass playerClass, DataRef<ProceduralNodeData> modifier)
	{
		return SetData(day, adventure, new Data(mana, time, completed, reloadCount, playerClass, modifier));
	}

	public void OverrideData(DataRef<AdventureData> adventure, Data? newData)
	{
		if (newData.HasValue)
		{
			data[adventure] = newData.Value;
		}
		else
		{
			data.Remove(adventure);
		}
	}

	public void AddPreviousDay(int day, DataRef<AdventureData> adventure)
	{
		if (day < epochDay)
		{
			(previousEpochDays.GetValueOrDefault(adventure) ?? (previousEpochDays[adventure] = new List<int>())).AddSortedUnique(day);
		}
	}

	public Data? GetData(DataRef<AdventureData> adventure)
	{
		if (!data.ContainsKey(adventure))
		{
			return null;
		}
		return data[adventure];
	}

	public void MarkAsComplete(DataRef<AdventureData> adventure)
	{
		completedDailies.Add(adventure);
	}

	public bool HasCompletedAdventureForDay(int day, DataRef<AdventureData> adventure)
	{
		if (!completedDailies.Contains(adventure))
		{
			List<int> valueOrDefault = previousEpochDays.GetValueOrDefault(adventure);
			if (valueOrDefault == null)
			{
				return false;
			}
			return valueOrDefault.LastValue() == day;
		}
		return true;
	}

	public int[] GetData()
	{
		return DailyLeaderboardAdventures.SelectMany((DataRef<AdventureData> d) => data.GetValueOrDefault(d).GetData()).ToArray();
	}

	public int? GetNextDayWithEntry(DataRef<AdventureData> adventure, int day)
	{
		epochDay = day;
		List<int> valueOrDefault = previousEpochDays.GetValueOrDefault(adventure);
		if (valueOrDefault != null)
		{
			int? num = valueOrDefault.BinarySearchNext(day);
			if (num.HasValue)
			{
				int valueOrDefault2 = num.GetValueOrDefault();
				return valueOrDefault[valueOrDefault2];
			}
		}
		if (day >= epochDay)
		{
			return null;
		}
		return epochDay;
	}

	public int? GetPreviousDayWithEntry(DataRef<AdventureData> adventure, int day)
	{
		epochDay = day;
		List<int> valueOrDefault = previousEpochDays.GetValueOrDefault(adventure);
		if (valueOrDefault != null)
		{
			int? num = valueOrDefault.BinarySearchPrevious(day);
			if (num.HasValue)
			{
				int valueOrDefault2 = num.GetValueOrDefault();
				int num2 = valueOrDefault[valueOrDefault2];
				if (num2 < 31)
				{
					return null;
				}
				return num2;
			}
		}
		return null;
	}
}
