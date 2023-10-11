using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract]
public class AdventureCompletion
{
	[ProtoContract]
	public struct Data
	{
		[ProtoMember(1)]
		private int _time;

		[ProtoMember(2)]
		private int _strategyTime;

		public int time => _time;

		public int strategyTime => _strategyTime;

		public Data(int time, int strategyTime)
		{
			_time = time;
			_strategyTime = strategyTime;
		}

		public Data Best(Data other)
		{
			return new Data(Math.Min(time, other.time), Math.Min(strategyTime, other.strategyTime));
		}
	}

	[ProtoMember(1, OverwriteList = true)]
	private Dictionary<PlayerClass, Data> _classCompletion;

	private Dictionary<PlayerClass, Data> classCompletion => _classCompletion ?? (_classCompletion = new Dictionary<PlayerClass, Data>());

	public AdventureCompletion()
	{
	}

	public AdventureCompletion(PlayerClass playerClass, Data data)
	{
		Best(playerClass, data);
	}

	public Data? GetData(PlayerClass playerClass)
	{
		if (!classCompletion.ContainsKey(playerClass))
		{
			return null;
		}
		return classCompletion[playerClass];
	}

	public void Best(PlayerClass playerClass, Data data)
	{
		if (classCompletion.ContainsKey(playerClass))
		{
			classCompletion[playerClass] = classCompletion[playerClass].Best(data);
		}
		else
		{
			classCompletion.Add(playerClass, data);
		}
	}

	public KeyValuePair<PlayerClass, Data>? GetBestTime()
	{
		return (classCompletion.Count > 0) ? classCompletion.MinBy((KeyValuePair<PlayerClass, Data> p) => p.Value.time) : default(KeyValuePair<PlayerClass, Data>);
	}

	public KeyValuePair<PlayerClass, Data>? GetBestStrategyTime()
	{
		return (classCompletion.Count > 0) ? classCompletion.MinBy((KeyValuePair<PlayerClass, Data> p) => p.Value.strategyTime) : default(KeyValuePair<PlayerClass, Data>);
	}

	public KeyValuePair<PlayerClass, AdventureCompletionRank>? GetBestCompletionRank(AdventureData adventure, NewGameType? newGameType = null)
	{
		KeyValuePair<PlayerClass, Data>? bestStrategyTime = GetBestStrategyTime();
		KeyValuePair<PlayerClass, AdventureCompletionRank> value;
		if (bestStrategyTime.HasValue)
		{
			KeyValuePair<PlayerClass, Data> valueOrDefault = bestStrategyTime.GetValueOrDefault();
			value = new KeyValuePair<PlayerClass, AdventureCompletionRank>(valueOrDefault.Key, adventure.GetCompletionRank(valueOrDefault.Value.strategyTime, newGameType));
		}
		else
		{
			value = default(KeyValuePair<PlayerClass, AdventureCompletionRank>);
		}
		return value;
	}

	public IEnumerable<PlayerClass> GetCompletedWithClasses()
	{
		IEnumerable<PlayerClass> enumerable = _classCompletion?.Keys;
		return enumerable ?? Enumerable.Empty<PlayerClass>();
	}
}
