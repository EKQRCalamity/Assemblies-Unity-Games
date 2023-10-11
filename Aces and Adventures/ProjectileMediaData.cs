using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Audio;

[ProtoContract]
[UIField]
[UIDeepValidate]
public class ProjectileMediaData : IDataContent
{
	[ProtoContract]
	[UIField]
	public class ProjectileMain
	{
		[ProtoMember(1)]
		[UIField(max = 24)]
		[DefaultValue("Unnamed Projectile Media")]
		private string _name = "Unnamed Projectile Media";

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Open)]
		private ProjectileExtremaData _preferredStartLocation = new ProjectileExtremaData(ProjectileExtremaData.Subject.Activator, CardTargets.Center);

		[ProtoMember(3)]
		[UIField(collapse = UICollapseType.Open)]
		private ProjectileExtremaData _preferredEndLocation;

		[ProtoMember(4)]
		[UIField("Finished At", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		[DefaultValue(ProjectilesFinishedAt.Impact)]
		[UIHorizontalLayout("Finish", expandHeight = false)]
		private ProjectilesFinishedAt _consideredFinishedAt = ProjectilesFinishedAt.Impact;

		[ProtoMember(5)]
		[UIField("Chained Projectile Media", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Media to play after this projectile is considered finished.")]
		[UIDeepValueChange]
		private ProjectileMediaPack _onFinishProjectileMedia;

		[ProtoMember(6)]
		[UIField("Wait For Chained Projectile Media", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "This projectile will not be considered done until chained media finishes.")]
		[UIHideIf("_hideWaitForProjectileMedia")]
		private bool _waitForProjectileMedia;

		[ProtoMember(7)]
		[UIField("Chain Delay", 0u, null, null, null, null, null, null, false, null, 5, false, null, min = 0, max = 1, tooltip = "Amount of time to wait to play next projectile media in chain.")]
		[UIHorizontalLayout("Finish")]
		[UIHideIf("_hideWaitForProjectileMedia")]
		private float _finishDelay;

		[ProtoMember(8)]
		[UIField(min = 0.01f, max = 10f, tooltip = "Use to raise or lower total emissions of effect.")]
		[DefaultValue(1)]
		[UIHorizontalLayout("Emission", flexibleWidth = 999f)]
		private float _emissionMultiplier = 1f;

		[ProtoMember(9)]
		[UIField(tooltip = "Apply emission multiplier to Chained Projectile Media as well.")]
		[UIHideIf("_hideWaitForProjectileMedia")]
		[UIHorizontalLayout("Emission")]
		private bool _chainEmissionMultiplier;

		public string name => _name;

		public ProjectileExtremaData startLocation => _preferredStartLocation ?? (_preferredStartLocation = new ProjectileExtremaData(ProjectileExtremaData.Subject.Activator, CardTargets.Center));

		public ProjectileExtremaData endLocation => _preferredEndLocation ?? (_preferredEndLocation = new ProjectileExtremaData());

		public ProjectilesFinishedAt finishedAt => _consideredFinishedAt;

		public float finishDelay => _finishDelay;

		public ProjectileMediaPack onFinishProjectileMedia => _onFinishProjectileMedia;

		public bool waitForProjectileMedia
		{
			get
			{
				if (_waitForProjectileMedia)
				{
					return onFinishProjectileMedia;
				}
				return false;
			}
		}

		public float emissionMultiplier => _emissionMultiplier;

		public bool chainEmissionMultiplier => _chainEmissionMultiplier;

		private bool _hideWaitForProjectileMedia => !_onFinishProjectileMediaSpecified;

		private bool _onFinishProjectileMediaSpecified => _onFinishProjectileMedia;
	}

	[ProtoContract]
	[UIField]
	public class ProjectileLaunch
	{
		[ProtoContract]
		[UIField]
		public class ProjectileLaunchParameters
		{
			private const byte BURST_COUNT = 1;

			private static readonly RangeByte BURST_RANGE = new RangeByte(1, 1, 1, 10, 0, 0);

			private const byte PROJECTILE_COUNT = 1;

			private static readonly RangeByte PROJECTILE_RANGE = new RangeByte(1, 1, 1, byte.MaxValue, 0, 0);

			private const float MIN_TIME_BETWEEN_BURSTS = 0.05f;

			private const float MAX_TIME_BETWEEN_BURSTS = 1f;

			private static readonly RangeF TIME_BETWEEN_BURSTS = new RangeF(0.5f, 0.5f, 0.05f);

			private static readonly RangeF TIME_SPACING = new RangeF(0.5f, 0.5f);

			[ProtoMember(1)]
			[UIField("Projectiles Per Burst (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced")]
			private RangeByte _projectilesPerBurst = PROJECTILE_RANGE;

			[ProtoMember(2)]
			[UIField("Number Of Bursts (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced")]
			private RangeByte _burstCount = BURST_RANGE;

			[ProtoMember(3)]
			[UIField("Time Between Bursts (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced")]
			private RangeF _timeBetweenBursts = TIME_BETWEEN_BURSTS;

			[ProtoMember(4)]
			[UIField]
			[UITooltip("Spaces out launching of projectiles between bursts. A value of 0 will launch projectiles all at once per burst, while a value of 1 will launch projectiles at a constant rate between bursts.")]
			private RangeF _projectileTimeSpacing = TIME_SPACING;

			[ProtoMember(5)]
			[UIField(collapse = UICollapseType.Hide)]
			private LaunchPatternShapeSettings _shape = new LaunchPatternShapeSettings();

			public RangeByte projectilesPerBurst => _projectilesPerBurst;

			public RangeByte burstCount => _burstCount;

			public LaunchPatternShapeSettings shape => _shape ?? (_shape = new LaunchPatternShapeSettings());

			public RangeF timeBetweenBursts => _timeBetweenBursts;

			public RangeF projectileTimeSpacing => _projectileTimeSpacing;

			private bool _projectilesPerBurstSpecified => _projectilesPerBurst != PROJECTILE_RANGE;

			private bool _burstsPerTargetSpecified => _burstCount != BURST_RANGE;

			private bool _timeBetweenBurstsSpecified => _timeBetweenBursts != TIME_BETWEEN_BURSTS;

			private bool _projectileTimeSpacingSpecified => _projectileTimeSpacing != TIME_SPACING;
		}

		[ProtoMember(1)]
		[UIField("Launch Parameters", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
		private ProjectileLaunchParameters _parameters;

		[ProtoMember(2)]
		[UIField("Launch Media", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
		private ProjectileBurstMedia<ProjectileLaunchSFXType> _media = new ProjectileBurstMedia<ProjectileLaunchSFXType>(AudioCategoryType.ProjectileLaunch);

		public ProjectileLaunchParameters parameters => _parameters ?? (_parameters = new ProjectileLaunchParameters());

		public ProjectileBurstMedia<ProjectileLaunchSFXType> media => _media ?? (_media = new ProjectileBurstMedia<ProjectileLaunchSFXType>(AudioCategoryType.ProjectileLaunch));
	}

	[ProtoContract]
	[UIField]
	public class ProjectileFlight
	{
		[ProtoContract]
		[UIField]
		public class ProjectileFlightParameters
		{
			[Flags]
			public enum ProjectileFlightFlags
			{
				Arc = 1,
				Wave = 2,
				Spiral = 4,
				Noise = 8,
				Roll = 0x10,
				Boomerang = 0x20
			}

			[ProtoContract]
			[UIField]
			public class ProjectileFlightModifiers
			{
				[ProtoMember(1)]
				[UIField("Enabled Modifiers", 0u, null, null, null, null, null, null, false, null, 5, false, null, validateOnChange = true)]
				private ProjectileFlightFlags _flightModifiers;

				[ProtoMember(2)]
				[UIField]
				[UIHideIf("_hideArcSettings")]
				private ArcSettings _arcSettings;

				[ProtoMember(3)]
				[UIField]
				[UIHideIf("_hideWaveSettings")]
				private WaveSettings _waveSettings;

				[ProtoMember(4)]
				[UIField]
				[UIHideIf("_hideSpiralSettings")]
				private SpiralSettings _spiralSettings;

				[ProtoMember(5)]
				[UIField]
				[UIHideIf("_hideNoiseSettings")]
				private NoiseSettings _noiseSettings;

				[ProtoMember(6)]
				[UIField]
				[UIHideIf("_hideRollSettings")]
				private RollSettings _rollSettings;

				[ProtoMember(7)]
				[UIField]
				[UIHideIf("_hideBoomerangSettings")]
				private BoomerangSettings _boomerangSettings;

				public FlightSetting this[ProjectileFlightFlags flightFlag]
				{
					get
					{
						switch (flightFlag)
						{
						case ProjectileFlightFlags.Arc:
							if (!HasFlag(ProjectileFlightFlags.Arc))
							{
								return null;
							}
							return _arcSettings ?? (_arcSettings = new ArcSettings());
						case ProjectileFlightFlags.Wave:
							if (!HasFlag(ProjectileFlightFlags.Wave))
							{
								return null;
							}
							return _waveSettings ?? (_waveSettings = new WaveSettings());
						case ProjectileFlightFlags.Spiral:
							if (!HasFlag(ProjectileFlightFlags.Spiral))
							{
								return null;
							}
							return _spiralSettings ?? (_spiralSettings = new SpiralSettings());
						case ProjectileFlightFlags.Noise:
							if (!HasFlag(ProjectileFlightFlags.Noise))
							{
								return null;
							}
							return _noiseSettings ?? (_noiseSettings = new NoiseSettings());
						case ProjectileFlightFlags.Roll:
							if (!HasFlag(ProjectileFlightFlags.Roll))
							{
								return null;
							}
							return _rollSettings ?? (_rollSettings = new RollSettings());
						case ProjectileFlightFlags.Boomerang:
							if (!HasFlag(ProjectileFlightFlags.Boomerang))
							{
								return null;
							}
							return _boomerangSettings ?? (_boomerangSettings = new BoomerangSettings());
						default:
							throw new ArgumentOutOfRangeException("flightFlag", flightFlag, null);
						}
					}
				}

				private bool _hideArcSettings => !EnumUtil.HasFlag(_flightModifiers, ProjectileFlightFlags.Arc);

				private bool _hideWaveSettings => !EnumUtil.HasFlag(_flightModifiers, ProjectileFlightFlags.Wave);

				private bool _hideSpiralSettings => !EnumUtil.HasFlag(_flightModifiers, ProjectileFlightFlags.Spiral);

				private bool _hideNoiseSettings => !EnumUtil.HasFlag(_flightModifiers, ProjectileFlightFlags.Noise);

				private bool _hideRollSettings => !EnumUtil.HasFlag(_flightModifiers, ProjectileFlightFlags.Roll);

				private bool _hideBoomerangSettings => !EnumUtil.HasFlag(_flightModifiers, ProjectileFlightFlags.Boomerang);

				public bool HasFlag(ProjectileFlightFlags flag)
				{
					return EnumUtil.HasFlag(_flightModifiers, flag);
				}

				public FlightModifier GetModifier(ProjectileFlightFlags flag, ref ProjectileFlightModiferInput modifierInput)
				{
					if (!HasFlag(flag))
					{
						return null;
					}
					return this[flag].GetModifier(ref modifierInput);
				}

				public IEnumerable<FlightModifier> GetModifiers(ProjectileFlightModiferInput modifierInput)
				{
					ProjectileFlightFlags[] values = EnumUtil<ProjectileFlightFlags>.Values;
					foreach (ProjectileFlightFlags projectileFlightFlags in values)
					{
						if (projectileFlightFlags != ProjectileFlightFlags.Boomerang && HasFlag(projectileFlightFlags))
						{
							yield return GetModifier(projectileFlightFlags, ref modifierInput);
						}
					}
				}

				public override string ToString()
				{
					return EnumUtil.FriendlyName(_flightModifiers);
				}
			}

			public interface FlightSetting
			{
				FlightModifier GetModifier(ref ProjectileFlightModiferInput input);
			}

			public abstract class FlightModifier
			{
				public abstract void Update(ref ProjectileUpdateData update);
			}

			[ProtoContract]
			[UIField]
			[ProtoInclude(10, typeof(PhysicalSettings))]
			[ProtoInclude(11, typeof(EaseSettings))]
			public abstract class BasicFlightSetting : FlightSetting
			{
				public abstract FlightModifier GetModifier(ref ProjectileFlightModiferInput input);

				public abstract float GetAverageFlightTime(float distance);
			}

			public abstract class BasicFlightModifier : FlightModifier
			{
				protected Vector3 _start;

				protected Vector3 _end;

				protected Vector3 _up;

				protected float _lifetime;

				protected void _SetData(Vector3 start, Vector3 end, Vector3 up, float lifetime)
				{
					_start = start;
					_end = end;
					_up = up;
					_lifetime = lifetime;
				}

				public override void Update(ref ProjectileUpdateData update)
				{
					update.start = update.start ?? _start;
					update.end = update.end ?? _end;
					update.forward = (update.end.Value - update.start.Value).normalized;
					update.up = update.up ?? _up;
					update.elapsedTime = Math.Min(update.elapsedTime, _lifetime);
					update.stateFlags = (ProjectileStateFlags)(((int?)update.stateFlags) ?? ((update.elapsedTime >= _lifetime) ? 3 : 0));
				}
			}

			[ProtoContract]
			[UIField("Physical", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
			private class PhysicalSettings : BasicFlightSetting
			{
				private class PhysicalModifier : BasicFlightModifier
				{
					private float _initialSpeed;

					private float _acceleration;

					private float _topSpeed;

					public PhysicalModifier SetData(Vector3 start, Vector3 end, Vector3 up, float initialSpeed, float acceleration, float topSpeed, float lifetime)
					{
						_SetData(start, end, up, lifetime);
						_initialSpeed = initialSpeed;
						_acceleration = acceleration;
						_topSpeed = topSpeed;
						return this;
					}

					public override void Update(ref ProjectileUpdateData update)
					{
						base.Update(ref update);
						update.linearDistanceTraveled = MathUtil.LinearDistanceTraveled(update.elapsedTime, _initialSpeed, _acceleration, _topSpeed);
						update.position = update.start.Value + update.forward * update.linearDistanceTraveled;
					}
				}

				private static readonly RangeF INITIAL_SPEED_RANGE = new RangeF(5f, 5f, 0f, 30f).Scale(0.1f);

				private static readonly RangeF TOP_SPEED_RANGE = new RangeF(5f, 5f, 2f, 30f).Scale(0.1f);

				private static readonly RangeF ACCELERATION_RANGE = new RangeF(0f, 0f, 0f, 30f).Scale(0.1f);

				[ProtoMember(1)]
				[UIField("Initial Speed (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced")]
				private RangeF _initialSpeedRange = INITIAL_SPEED_RANGE;

				[ProtoMember(2)]
				[UIField("Acceleration (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced")]
				private RangeF _accelerationRange = ACCELERATION_RANGE;

				[ProtoMember(3)]
				[UIField("Top Speed (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced")]
				private RangeF _topSpeedRange = TOP_SPEED_RANGE;

				private bool _initialSpeedRangeSpecified => _initialSpeedRange != INITIAL_SPEED_RANGE;

				private bool _accelerationRangeSpecified => _accelerationRange != ACCELERATION_RANGE;

				private bool _topSpeedRangeSpecified => _topSpeedRange != TOP_SPEED_RANGE;

				public override FlightModifier GetModifier(ref ProjectileFlightModiferInput input)
				{
					float num = input.random.Range(_initialSpeedRange);
					float num2 = input.random.Range(_accelerationRange);
					if (num < TOP_SPEED_RANGE.minRange)
					{
						num2 = Math.Max(TOP_SPEED_RANGE.minRange - num, num2);
					}
					float topSpeed = Math.Max(num, input.random.Range(_topSpeedRange));
					input.lifetime = Mathf.Max(MathUtil.BigEpsilon, MathUtil.TimeToLinearImpact(input.distance, num, num2, topSpeed));
					return Pools.Unpool<PhysicalModifier>().SetData(input.start, input.end, input.up, num, num2, topSpeed, input.lifetime);
				}

				public override float GetAverageFlightTime(float distance)
				{
					float num = _initialSpeedRange.Average();
					float num2 = _accelerationRange.Average();
					if (num < TOP_SPEED_RANGE.minRange)
					{
						num2 = Math.Max(TOP_SPEED_RANGE.minRange - num, num2);
					}
					return Mathf.Max(MathUtil.BigEpsilon, MathUtil.TimeToLinearImpact(distance, num, num2, _topSpeedRange.Average()));
				}
			}

			[ProtoContract]
			[UIField("Ease", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
			private class EaseSettings : BasicFlightSetting
			{
				private class EaseModifier : BasicFlightModifier
				{
					private float _easePower;

					public EaseModifier SetData(Vector3 start, Vector3 end, Vector3 up, float lifetime, float easePower)
					{
						_SetData(start, end, up, lifetime);
						_easePower = easePower;
						return this;
					}

					public override void Update(ref ProjectileUpdateData update)
					{
						base.Update(ref update);
						float num = update.elapsedTime / _lifetime;
						update.position = update.start.Value.Lerp(update.end.Value, (_easePower > 0f) ? Mathf.Pow(num, _easePower) : MathUtil.CubicSplineInterpolant(num));
						update.linearDistanceTraveled = (update.position - update.start.Value).magnitude;
					}
				}

				private static readonly RangeF EASE_TIME = new RangeF(1f, 1f, 0.1f, 3f);

				private static readonly RangeF EASE_POWER = new RangeF(2f, 2f, 1f, 3f);

				private const EaseType EASE_TYPE = EaseType.EaseIn;

				[ProtoMember(1)]
				[UIField(validateOnChange = true)]
				[DefaultValue(EaseType.EaseIn)]
				private EaseType _easeType = EaseType.EaseIn;

				[ProtoMember(2)]
				[UIField(view = "UI/Reflection/Range Slider Advanced")]
				private RangeF _easeTime = EASE_TIME;

				[ProtoMember(3)]
				[UIField(view = "UI/Reflection/Range Slider Advanced")]
				[UIHideIf("_hideEasePower")]
				private RangeF _easePower = EASE_POWER;

				private bool _hideEasePower => _easeType == EaseType.EaseInAndOut;

				private bool _easeTimeSpecified => _easeTime != EASE_TIME;

				private bool _easePowerSpecified => _easePower != EASE_POWER;

				public override FlightModifier GetModifier(ref ProjectileFlightModiferInput input)
				{
					input.lifetime = input.random.Range(_easeTime);
					float num = input.random.Range(_easePower);
					switch (_easeType)
					{
					case EaseType.EaseOut:
						num = 1f / num;
						break;
					case EaseType.EaseInAndOut:
						num = 0f;
						break;
					default:
						throw new ArgumentOutOfRangeException();
					case EaseType.Linear:
					case EaseType.EaseIn:
						break;
					}
					return Pools.Unpool<EaseModifier>().SetData(input.start, input.end, input.up, input.lifetime, num);
				}

				public override float GetAverageFlightTime(float distance)
				{
					return _easeTime.Average();
				}
			}

			[ProtoContract]
			[UIField]
			[ProtoInclude(10, typeof(ArcSettings))]
			[ProtoInclude(11, typeof(WaveSettings))]
			[ProtoInclude(12, typeof(SpiralSettings))]
			public abstract class SpecialFlightSetting : FlightSetting
			{
				[ProtoContract(EnumPassthru = true)]
				protected enum SimulationSpace : byte
				{
					LocalUp,
					WorldUp
				}

				private const bool UPDATE_ORIENT = true;

				[ProtoMember(1)]
				[UIField(order = 1u)]
				protected SimulationSpace _upDirectionToUse;

				[ProtoMember(2)]
				[UIField(order = 2u)]
				[DefaultValue(true)]
				protected bool _updateUpDirection = true;

				public bool worldSpace => _upDirectionToUse == SimulationSpace.WorldUp;

				public bool updateUp => _updateUpDirection;

				public abstract FlightModifier GetModifier(ref ProjectileFlightModiferInput input);
			}

			public abstract class SpecialFlightModifier : FlightModifier
			{
				private bool _worldSpace;

				private Vector3 _previousPosition;

				private bool _updateLocalOrientation;

				protected abstract void _Update(ref ProjectileUpdateData update);

				protected void _SetData(SpecialFlightSetting settings, Vector3 start)
				{
					_previousPosition = start;
					_worldSpace = settings.worldSpace;
					_updateLocalOrientation = settings.updateUp;
				}

				protected Vector3 Up(ref ProjectileUpdateData update)
				{
					if (!_worldSpace)
					{
						return update.up.Value;
					}
					return Vector3.up;
				}

				public sealed override void Update(ref ProjectileUpdateData update)
				{
					_Update(ref update);
					if (_updateLocalOrientation && _previousPosition != update.position)
					{
						Vector3 normalized = Vector3.Cross(Up(ref update), update.forward).normalized;
						update.forward = (update.position - _previousPosition).normalized;
						update.up = Vector3.Cross(update.forward, normalized).normalized;
					}
					_previousPosition = update.position;
				}
			}

			[ProtoContract]
			[UIField]
			private class ArcSettings : SpecialFlightSetting
			{
				[ProtoContract(EnumPassthru = true)]
				public enum ArcType : byte
				{
					Gravity,
					SetHeight
				}

				private class ArcSetHeightModifier : SpecialFlightModifier
				{
					private float _height;

					private float _peakHeightDistance;

					private float _lifetime;

					public ArcSetHeightModifier SetData(SpecialFlightSetting settings, Vector3 start, float height, float peakHeightDistance, float lifetime)
					{
						_SetData(settings, start);
						_height = height;
						_peakHeightDistance = peakHeightDistance;
						_lifetime = lifetime;
						return this;
					}

					protected override void _Update(ref ProjectileUpdateData update)
					{
						float num = update.elapsedTime / _lifetime;
						float num2 = 1f - Mathf.Pow((num <= _peakHeightDistance) ? ((_peakHeightDistance - num) / _peakHeightDistance) : ((num - _peakHeightDistance) / (1f - _peakHeightDistance)), 2f);
						update.position += Up(ref update) * (num2 * _height);
					}
				}

				private class ArcGravityModifier : SpecialFlightModifier
				{
					private float _halfGravity;

					private float _initialSpeed;

					public ArcGravityModifier SetData(SpecialFlightSetting settings, Vector3 start, float gravity, float lifetime)
					{
						_SetData(settings, start);
						_halfGravity = (0f - gravity) * 0.5f;
						_initialSpeed = (0f - _halfGravity) * lifetime;
						return this;
					}

					protected override void _Update(ref ProjectileUpdateData update)
					{
						float num = (_initialSpeed + _halfGravity * update.elapsedTime) * update.elapsedTime;
						update.position += Up(ref update) * num;
					}
				}

				private static readonly RangeF HEIGHT_RANGE = new RangeF(3f, 3f, -5f, 5f).Scale(0.1f);

				private static readonly RangeF PEAK_HEIGHT_DISTANCE = new RangeF(0.5f, 0.5f, 0.1f, 0.9f);

				private static readonly RangeF GRAVITY = new RangeF(9.81f, 9.81f, -20f, 20f);

				[ProtoMember(1)]
				[UIField(validateOnChange = true)]
				private ArcType _type;

				[ProtoMember(2)]
				[UIField("Arc Height (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
				[UIHideIf("_hideHeightRange")]
				private RangeF _heightRange = HEIGHT_RANGE;

				[ProtoMember(3)]
				[UIField("Peak Height Distance (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
				[UIHideIf("_hideHeightRange")]
				private RangeF _peakHeightDistanceRange = PEAK_HEIGHT_DISTANCE;

				[ProtoMember(4)]
				[UIField(stepSize = 0.01f)]
				[UIHideIf("_hideGravity")]
				private RangeF _gravity = GRAVITY;

				private bool _hideHeightRange => _type != ArcType.SetHeight;

				private bool _hideGravity => _type != ArcType.Gravity;

				private bool _heightRangeSpecified => _heightRange != HEIGHT_RANGE;

				private bool _peakHeightDistanceRangeSpecified => _peakHeightDistanceRange != PEAK_HEIGHT_DISTANCE;

				private bool _gravitySpecified => _gravity != GRAVITY;

				public override FlightModifier GetModifier(ref ProjectileFlightModiferInput input)
				{
					return _type switch
					{
						ArcType.SetHeight => Pools.Unpool<ArcSetHeightModifier>().SetData(this, input.start, input.random.Range(_heightRange), input.random.Range(_peakHeightDistanceRange), input.lifetime), 
						ArcType.Gravity => Pools.Unpool<ArcGravityModifier>().SetData(this, input.start, input.random.Range(_gravity), input.lifetime), 
						_ => throw new ArgumentOutOfRangeException(), 
					};
				}
			}

			[ProtoContract]
			[UIField]
			private class WaveSettings : SpecialFlightSetting
			{
				[ProtoContract(EnumPassthru = true)]
				private enum WaveDirection : byte
				{
					Up,
					Down,
					Both
				}

				private class WaveModifier : SpecialFlightModifier
				{
					private float _startFrequency;

					private float _endFrequency;

					private float _startAmplitude;

					private float _endAmplitude;

					private float _lifetime;

					public WaveModifier SetData(SpecialFlightSetting settings, Vector3 start, float startWavelength, float endWaveLength, float startAmplitude, float endAmplitude, float distance, float lifetime, float direction)
					{
						_SetData(settings, start);
						_startFrequency = MathUtil.FrequencyFromWavelength(MathUtil.RoundToNearestFactorOf(startWavelength, distance), distance);
						_endFrequency = MathUtil.FrequencyFromWavelength(MathUtil.RoundToNearestFactorOf(endWaveLength, distance), distance);
						float num = (_startFrequency + _endFrequency) * 0.5f;
						float num2 = MathUtil.RoundToNearestMultipleOf(num, MathF.PI) / num / lifetime * direction;
						_startFrequency *= num2;
						_endFrequency *= num2;
						_startAmplitude = startAmplitude;
						_endAmplitude = endAmplitude;
						_lifetime = lifetime;
						return this;
					}

					protected override void _Update(ref ProjectileUpdateData update)
					{
						float t = update.elapsedTime / _lifetime;
						float num = Mathf.Sin((_startFrequency + Mathf.Lerp(_startFrequency, _endFrequency, t)) * 0.5f * update.elapsedTime) * Mathf.Lerp(_startAmplitude, _endAmplitude, t);
						update.position += Up(ref update) * num;
					}
				}

				private static readonly RangeF WAVE_LENGTH = new RangeF(3f, 3f, 1f, 5f).Scale(0.1f);

				private static readonly RangeF AMPLITUDE = new RangeF(0.5f, 0.5f, -2f, 2f).Scale(0.1f);

				[ProtoMember(1)]
				[UIField("Start Wave Length (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001)]
				private RangeF _startWaveLengthRange = WAVE_LENGTH;

				[ProtoMember(2)]
				[UIField("End Wave Length (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001)]
				private RangeF _endWaveLengthRange = WAVE_LENGTH;

				[ProtoMember(3)]
				[UIField("Start Amplitude (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001)]
				private RangeF _startAmplitudeRange = AMPLITUDE;

				[ProtoMember(4)]
				[UIField("End Amplitude (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001)]
				private RangeF _endAmplitudeRange = AMPLITUDE;

				[ProtoMember(5)]
				[UIField]
				private WaveDirection _initialDirection;

				private bool _startWaveLengthRangeSpecified => _startWaveLengthRange != WAVE_LENGTH;

				private bool _endWaveLengthRangeSpecified => _endWaveLengthRange != WAVE_LENGTH;

				private bool _startAmplitudeRangeSpecified => _startAmplitudeRange != AMPLITUDE;

				private bool _endAmplitudeRangeSpecified => _endAmplitudeRange != AMPLITUDE;

				public override FlightModifier GetModifier(ref ProjectileFlightModiferInput input)
				{
					float direction = 1f;
					switch (_initialDirection)
					{
					case WaveDirection.Down:
						direction = -1f;
						break;
					case WaveDirection.Both:
						direction = input.random.Sign();
						break;
					default:
						throw new ArgumentOutOfRangeException();
					case WaveDirection.Up:
						break;
					}
					return Pools.Unpool<WaveModifier>().SetData(this, input.start, input.random.Range(_startWaveLengthRange), input.random.Range(_endWaveLengthRange), input.random.Range(_startAmplitudeRange), input.random.Range(_endAmplitudeRange), input.distance, input.lifetime, direction);
				}
			}

			[ProtoContract]
			[UIField]
			private class SpiralSettings : SpecialFlightSetting
			{
				[ProtoContract(EnumPassthru = true)]
				public enum SpiralDirection
				{
					Clockwise,
					CounterClockwise,
					Both
				}

				private class SpiralModifier : SpecialFlightModifier
				{
					private float _startSpiralFrequency;

					private float _endSpiralFrequency;

					private float _startRadius;

					private float _endRadius;

					private float _radiusFrequency;

					private bool _counterClockwise;

					private float _lifetime;

					private float _endAngle;

					public SpiralModifier SetData(SpecialFlightSetting settings, Vector3 start, float startSpiralLength, float endSpiralLength, float startRadius, float endRadius, float? radiusWaveLength, bool counterClockwise, float distance, float lifetime)
					{
						_SetData(settings, start);
						float velocity = distance / lifetime;
						_startSpiralFrequency = MathUtil.FrequencyFromWavelength(startSpiralLength, velocity);
						_endSpiralFrequency = MathUtil.FrequencyFromWavelength(endSpiralLength, velocity);
						_startRadius = startRadius;
						_endRadius = endRadius;
						_radiusFrequency = (radiusWaveLength.HasValue ? MathUtil.FrequencyFromWavelength(radiusWaveLength.Value, velocity) : 0f);
						_counterClockwise = counterClockwise;
						_lifetime = lifetime;
						_endAngle = (_startSpiralFrequency + _endSpiralFrequency) * 0.5f * _lifetime * (float)_counterClockwise.ToInt(1, -1);
						return this;
					}

					protected override void _Update(ref ProjectileUpdateData update)
					{
						float t = update.elapsedTime / _lifetime;
						float num = (_startSpiralFrequency + Mathf.Lerp(_startSpiralFrequency, _endSpiralFrequency, t)) * 0.5f * update.elapsedTime * (float)_counterClockwise.ToInt(1, -1);
						float num2 = Mathf.Lerp(_startRadius, _endRadius, t);
						float num3 = Mathf.Min(Mathf.Abs(num), Mathf.Abs(_endAngle - num));
						if (num3 < MathF.PI)
						{
							num2 *= num3 / MathF.PI;
						}
						if (_radiusFrequency > 0f)
						{
							num2 = MathUtil.Remap(Mathf.Sin(update.elapsedTime * _radiusFrequency - MathF.PI / 2f), new Vector2(-1f, 1f), new Vector2(0f, num2));
						}
						update.position += update.right * Mathf.Cos(num) * num2;
						update.position += Up(ref update) * Mathf.Sin(num) * num2;
					}
				}

				private static readonly RangeF SPIRAL_LENGTH = new RangeF(3f, 3f, 1f, 10f).Scale(0.1f);

				private static readonly RangeF RADIUS = new RangeF(0.5f, 0.5f, -2f, 2f).Scale(0.1f);

				private static readonly RangeF RADIUS_WAVELENGTH = new RangeF(3f, 3f, 1f, 10f).Scale(0.1f);

				private const bool WAVE_RADIUS = false;

				[ProtoMember(1)]
				[UIField]
				private SpiralDirection _spiralDirection;

				[ProtoMember(2)]
				[UIField("Start Spiral Length (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001)]
				private RangeF _startSpiralLengthRange = SPIRAL_LENGTH;

				[ProtoMember(3)]
				[UIField("End Spiral Length (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001)]
				private RangeF _endSpiralLengthRange = SPIRAL_LENGTH;

				[ProtoMember(4)]
				[UIField("Start Radius (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001)]
				private RangeF _startRadiusRange = RADIUS;

				[ProtoMember(5)]
				[UIField("End Radius (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001)]
				private RangeF _endRadiusRange = RADIUS;

				[ProtoMember(6)]
				[UIField(validateOnChange = true)]
				[DefaultValue(false)]
				private bool _waveRadius;

				[ProtoMember(7)]
				[UIField("Radius Wave Length (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001)]
				[UIHideIf("_hideRadiusWaveLengthRange")]
				private RangeF _radiusWaveLengthRange = RADIUS_WAVELENGTH;

				private bool _startSpiralLengthRangeSpecified => _startSpiralLengthRange != SPIRAL_LENGTH;

				private bool _endSpiralLengthRangeSpecified => _endSpiralLengthRange != SPIRAL_LENGTH;

				private bool _startRadiusRangeSpecified => _startRadiusRange != RADIUS;

				private bool _endRadiusRangeSpecified => _endRadiusRange != RADIUS;

				private bool _radiusWaveLengthRangeSpecified => _radiusWaveLengthRange != RADIUS_WAVELENGTH;

				private bool _hideRadiusWaveLengthRange => !_waveRadius;

				public override FlightModifier GetModifier(ref ProjectileFlightModiferInput input)
				{
					bool counterClockwise = false;
					switch (_spiralDirection)
					{
					case SpiralDirection.CounterClockwise:
						counterClockwise = true;
						break;
					case SpiralDirection.Both:
						counterClockwise = input.random.Boolean();
						break;
					default:
						throw new ArgumentOutOfRangeException();
					case SpiralDirection.Clockwise:
						break;
					}
					return Pools.Unpool<SpiralModifier>().SetData(this, input.start, input.random.Range(_startSpiralLengthRange), input.random.Range(_endSpiralLengthRange), input.random.Range(_startRadiusRange), input.random.Range(_endRadiusRange), _waveRadius ? new float?(input.random.Range(_radiusWaveLengthRange)) : null, counterClockwise, input.distance, input.lifetime);
				}
			}

			[ProtoContract]
			[UIField]
			private class NoiseSettings : FlightSetting
			{
				private class NoiseModifier : FlightModifier
				{
					private const float NOISE_OFFSET = 2048f;

					private float _startFrequency;

					private float _endFrequency;

					private float _startAmplitude;

					private float _endAmplitude;

					private DirectionTypeFlags _axes;

					private Vector3 _start;

					private Vector3 _end;

					private float _lifetime;

					private Vector2 _noiseOffset;

					public NoiseModifier SetData(float startFrequency, float endFrequency, float startAmplitude, float endAmplitude, DirectionTypeFlags axes, Vector3 start, Vector3 end, float lifetime, System.Random random)
					{
						_startFrequency = startFrequency;
						_endFrequency = endFrequency;
						_startAmplitude = startAmplitude;
						_endAmplitude = endAmplitude;
						_axes = axes;
						_start = start;
						_end = end;
						_lifetime = lifetime;
						_noiseOffset = new Vector2(random.Range(-2048f, 2048f), random.Range(-2048f, 2048f));
						return this;
					}

					public override void Update(ref ProjectileUpdateData update)
					{
						float t = update.elapsedTime / _lifetime;
						float num = (_startFrequency + Mathf.Lerp(_startFrequency, _endFrequency, t)) * 0.5f * update.elapsedTime;
						float num2 = Mathf.Lerp(_startAmplitude, _endAmplitude, t);
						float num3 = Mathf.Abs(num2);
						if (num3 < MathUtil.BigEpsilon)
						{
							return;
						}
						float num4 = Mathf.Min(update.linearDistanceTraveled, update.distance - update.linearDistanceTraveled);
						float num5 = num3 + num3;
						if (num5 > num4)
						{
							num2 *= num4 / num5;
						}
						EnumerateFlags<DirectionTypeFlags>.Enumerator enumerator = EnumUtil.Flags(_axes).GetEnumerator();
						while (enumerator.MoveNext())
						{
							switch (enumerator.Current)
							{
							case DirectionTypeFlags.Horizontal:
								update.position += update.right * MathUtil.PerlinNoise(num + _noiseOffset.x, _noiseOffset.y) * num2;
								break;
							case DirectionTypeFlags.Vertical:
								update.position += update.up.Value * MathUtil.PerlinNoise(_noiseOffset.y, num + _noiseOffset.x) * num2;
								break;
							case DirectionTypeFlags.Longitudinal:
								update.position += update.forward * MathUtil.PerlinNoise(num * 0.5f - _noiseOffset.x, num * 0.5f - _noiseOffset.y) * num2;
								break;
							default:
								throw new ArgumentOutOfRangeException();
							}
						}
					}
				}

				private static readonly RangeF FREQUENCY = new RangeF(2.5f, 2.5f, 1f, 10f);

				private static readonly RangeF AMPLITUDE = new RangeF(0.25f, 0.25f, -1f).Scale(0.1f);

				private static readonly DirectionTypeFlags DIRECTIONS = EnumUtil<DirectionTypeFlags>.AllFlags;

				[ProtoMember(1)]
				[UIField("Start Frequency (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.01)]
				private RangeF _startFrequencyRange = FREQUENCY;

				[ProtoMember(2)]
				[UIField("End Frequency (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.01)]
				private RangeF _endFrequencyRange = FREQUENCY;

				[ProtoMember(3)]
				[UIField("Start Amplitude (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001)]
				private RangeF _startAmplitudeRange = AMPLITUDE;

				[ProtoMember(4)]
				[UIField("End Amplitude (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001)]
				private RangeF _endAmplitudeRange = AMPLITUDE;

				[ProtoMember(5)]
				[UIField(min = 1)]
				private DirectionTypeFlags _applyToAxes = DIRECTIONS;

				private bool _startFrequencyRangeSpecified => _startFrequencyRange != FREQUENCY;

				private bool _endFrequencyRangeSpecified => _endFrequencyRange != FREQUENCY;

				private bool _startAmplitudeRangeSpecified => _startAmplitudeRange != AMPLITUDE;

				private bool _endAmplitudeRangeSpecified => _endAmplitudeRange != AMPLITUDE;

				private bool _applyToAxesSpecified => _applyToAxes != DIRECTIONS;

				public FlightModifier GetModifier(ref ProjectileFlightModiferInput input)
				{
					return Pools.Unpool<NoiseModifier>().SetData(input.random.Range(_startFrequencyRange), input.random.Range(_endFrequencyRange), input.random.Range(_startAmplitudeRange), input.random.Range(_endAmplitudeRange), _applyToAxes, input.start, input.end, input.lifetime, input.random);
				}
			}

			[ProtoContract]
			[UIField]
			private class RollSettings : FlightSetting
			{
				private class RollModifier : FlightModifier
				{
					private float _initialHeight;

					private float _initialVelocity;

					private float _radius;

					private float _gravity;

					private float _bounciness;

					public RollModifier SetData(float initialHeight, float firstBounceAt, float radius, float gravity, float bounciness, float lifetime)
					{
						_initialHeight = Mathf.Max(radius, initialHeight);
						_gravity = 0f - gravity;
						_bounciness = bounciness;
						_radius = radius;
						_initialVelocity = MathUtil.InitialVelocityToImpactAtTime(firstBounceAt * lifetime, _initialHeight - _radius, _gravity);
						return this;
					}

					public override void Update(ref ProjectileUpdateData update)
					{
						update.position.y = MathUtil.BounceHeight(update.elapsedTime, _initialHeight, _initialVelocity, _gravity, _radius, _bounciness).height;
					}
				}

				private static readonly RangeF FIRST_BOUNCE = new RangeF(0.5f, 0.5f, 0.1f);

				private static readonly RangeF BOUNCINESS = new RangeF(0.71f, 0.71f);

				[ProtoMember(1)]
				[UIField(min = 1, max = 20f, stepSize = 0.01f)]
				[DefaultValue(9.81f)]
				private float _gravity = 9.81f;

				[ProtoMember(2)]
				[UIField("First Bounce At (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.01)]
				private RangeF _firstBounceAt = FIRST_BOUNCE;

				[ProtoMember(3)]
				[UIField("Bounciness (Min / Max)", 0u, null, null, null, null, null, null, false, null, 5, false, null, view = "UI/Reflection/Range Slider Advanced", stepSize = 0.01)]
				private RangeF _bouncinessRange = BOUNCINESS;

				private bool _firstBounceAtSpecified => _firstBounceAt != FIRST_BOUNCE;

				private bool _bouncinessRangeSpecified => _bouncinessRange != BOUNCINESS;

				public FlightModifier GetModifier(ref ProjectileFlightModiferInput input)
				{
					return Pools.Unpool<RollModifier>().SetData(input.start.y, input.random.Range(_firstBounceAt), input.radius, _gravity, input.random.Range(_bouncinessRange), input.lifetime);
				}
			}

			[ProtoContract]
			[UIField]
			private class BoomerangSettings : FlightSetting
			{
				private class BoomerangModifier : FlightModifier
				{
					private Vector3 _up;

					private float _lifetime;

					private bool _flipUp;

					public BoomerangModifier SetData(Vector3 up, float lifetime, bool flipUp)
					{
						_up = -up;
						_lifetime = lifetime;
						_flipUp = flipUp;
						return this;
					}

					public override void Update(ref ProjectileUpdateData update)
					{
						float num = update.elapsedTime - _lifetime;
						if (!(num < 0f))
						{
							update.stateFlags = ProjectileStateFlags.Impacted;
							if (_flipUp)
							{
								update.up = _up;
							}
							update.elapsedTime = Math.Max(0f, _lifetime - num);
							update.stateFlags |= (ProjectileStateFlags)((update.elapsedTime <= 0f) ? 2 : 0);
						}
					}
				}

				private const bool FLIP_UP = true;

				private const bool REVERSE_ROTATION_SPEED = false;

				[ProtoMember(1)]
				[UIField]
				[DefaultValue(true)]
				private bool _flipUpDirection = true;

				[ProtoMember(2)]
				[UIField]
				[DefaultValue(false)]
				private bool _reverseRotationSpeed;

				public ProjectileBoomerangOptions options
				{
					get
					{
						if (!_reverseRotationSpeed)
						{
							return (ProjectileBoomerangOptions)0;
						}
						return ProjectileBoomerangOptions.ReverseRotationSpeed;
					}
				}

				public FlightModifier GetModifier(ref ProjectileFlightModiferInput input)
				{
					return Pools.Unpool<BoomerangModifier>().SetData(input.up, input.lifetime, _flipUpDirection);
				}
			}

			[ProtoMember(1)]
			[UIField("Basic Flight Settings", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Hide)]
			private BasicFlightSetting _basicFlight;

			[ProtoMember(2)]
			[UIField("Flight Modifiers", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Hide)]
			private ProjectileFlightModifiers _specialFlightModifiers;

			private BasicFlightSetting basicFlight => _basicFlight ?? (_basicFlight = new PhysicalSettings());

			private ProjectileFlightModifiers specialFlightModifiers => _specialFlightModifiers ?? (_specialFlightModifiers = new ProjectileFlightModifiers());

			public string name { get; set; }

			public PoolListHandle<FlightModifier> GetModifiers(System.Random random, Vector3 start, Vector3 end, Vector3 up, float radius, out float lifetime, out ProjectileBoomerangOptions? boomerangOptions)
			{
				ProjectileFlightModiferInput input = new ProjectileFlightModiferInput(random, start, end, up, radius, 0f);
				FlightModifier modifier = basicFlight.GetModifier(ref input);
				lifetime = input.lifetime;
				boomerangOptions = null;
				PoolListHandle<FlightModifier> poolListHandle = Pools.UseList<FlightModifier>();
				if (specialFlightModifiers.HasFlag(ProjectileFlightFlags.Boomerang))
				{
					boomerangOptions = (specialFlightModifiers[ProjectileFlightFlags.Boomerang] as BoomerangSettings).options;
					poolListHandle.Add(specialFlightModifiers.GetModifier(ProjectileFlightFlags.Boomerang, ref input));
				}
				poolListHandle.Add(modifier);
				foreach (FlightModifier modifier2 in specialFlightModifiers.GetModifiers(input))
				{
					poolListHandle.Add(modifier2);
				}
				return poolListHandle;
			}

			public float GetAverageFlightTime(float distance)
			{
				return basicFlight.GetAverageFlightTime(distance);
			}

			public override string ToString()
			{
				if (!name.IsNullOrEmpty())
				{
					return $"Flight Parameters for {name}";
				}
				return "Flight Parameters";
			}
		}

		[ProtoContract]
		[UIField]
		public class ProjectileFlightMedia
		{
			[ProtoContract]
			[UIField]
			public class ProjectileVisual
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide)]
				private CustomizedEnumContent<ProjectileFlightSFXType> _projectilePrefab;

				[ProtoMember(2)]
				[UIField(collapse = UICollapseType.Hide)]
				private ProjectileFlightMediaTransforms _transforms;

				protected CustomizedEnumContent<ProjectileFlightSFXType> projectile => _projectilePrefab ?? (_projectilePrefab = new CustomizedEnumContent<ProjectileFlightSFXType>());

				public ProjectileFlightMediaTransforms transforms => _transforms ?? (_transforms = new ProjectileFlightMediaTransforms());

				public string name => projectile.ToString();

				public bool hasPrimaryColor => projectile.hasPrimaryColor;

				public GameObject GetBlueprint()
				{
					return EnumUtil<ProjectileFlightSFXType>.GetResourceBlueprint(projectile);
				}

				public Color? ApplySettings(GameObject gameObject, System.Random random)
				{
					return projectile.ApplyTo(gameObject, random);
				}

				public override string ToString()
				{
					return name;
				}
			}

			[ProtoContract]
			[UIField]
			public class ProjectileFlightMediaTransforms
			{
				[ProtoContract]
				[UIField]
				private class ScaleTransforms
				{
					private static readonly RangeF SCALE = new RangeF(1f, 1f, 0.1f, 5f);

					private const bool SYNC_SCALE = true;

					[ProtoMember(1)]
					[UIField(stepSize = 0.01f, view = "UI/Reflection/Range Slider Advanced")]
					public RangeF _startScale = SCALE;

					[ProtoMember(2)]
					[UIField(stepSize = 0.01f, view = "UI/Reflection/Range Slider Advanced")]
					public RangeF _endScale = SCALE;

					[ProtoMember(3)]
					[UIField]
					[DefaultValue(true)]
					public bool _syncStartAndEndScale = true;

					[ProtoMember(4)]
					[UIField]
					public EaseType _scaleTransition;

					[ProtoMember(5)]
					[UIField(validateOnChange = true)]
					public bool _animateScale;

					[ProtoMember(6)]
					[UIField]
					[UIHideIf("_hideAnimation")]
					private Vector3Animator _animation;

					public Vector3Animator animation => _animation ?? (_animation = new UniformAnimator());

					private bool _startScaleSpecified => _startScale != SCALE;

					private bool _endScaleSpecified => _endScale != SCALE;

					private bool _hideAnimation => !_animateScale;
				}

				[ProtoContract]
				[UIField]
				private class RotationTransforms
				{
					[ProtoMember(1)]
					[UIField]
					[UIHeader("Orient To")]
					public ProjectileFlightOrientType _orientTo;

					[ProtoMember(2)]
					[UIField(min = 2, max = 20)]
					public float? _easeOrientTo;

					[ProtoMember(3)]
					[UIField]
					public bool _flipOrientTo;

					[ProtoMember(4)]
					[UIField(collapse = UICollapseType.Hide)]
					[UIHeader("Initial Rotation Offset")]
					private RotationRanges _initialRotation;

					[ProtoMember(5)]
					[UIField(collapse = UICollapseType.Hide)]
					[UIHeader("Rotation Speed")]
					private RotationSpeedRanges _rotationSpeed;

					public RotationRanges initialRotation => _initialRotation ?? (_initialRotation = new RotationRanges());

					public RotationSpeedRanges rotationSpeed => _rotationSpeed ?? (_rotationSpeed = new RotationSpeedRanges());

					private bool _initialRotationSpecified => _initialRotation;

					private bool _rotationSpeedSpecified => _rotationSpeed;
				}

				[ProtoMember(1)]
				[UIField]
				private RotationTransforms _rotationSettings;

				[ProtoMember(2)]
				[UIField]
				private ScaleTransforms _scaleSettings;

				private ScaleTransforms scaleSettings => _scaleSettings ?? (_scaleSettings = new ScaleTransforms());

				private RotationTransforms rotationSettings => _rotationSettings ?? (_rotationSettings = new RotationTransforms());

				public RangeF startScale => scaleSettings._startScale;

				public RangeF endScale => scaleSettings._endScale;

				public bool syncScale
				{
					get
					{
						if (scaleSettings._syncStartAndEndScale)
						{
							return startScale.range > 0f;
						}
						return false;
					}
				}

				public EaseType scaleTransition => scaleSettings._scaleTransition;

				public bool animateScale => scaleSettings._animateScale;

				public ProjectileFlightOrientType orientTo => rotationSettings._orientTo;

				public float? easeOrientTo => rotationSettings._easeOrientTo;

				public bool flipOrientTo => rotationSettings._flipOrientTo;

				public RangeInt xRotation => rotationSettings.initialRotation.pitch;

				public RangeInt yRotation => rotationSettings.initialRotation.yaw;

				public RangeInt zRotation => rotationSettings.initialRotation.roll;

				public RangeF xRotationSpeed => rotationSettings.rotationSpeed.pitchSpeed;

				public RangeF yRotationSpeed => rotationSettings.rotationSpeed.yawSpeed;

				public RangeF zRotationSpeed => rotationSettings.rotationSpeed.rollSpeed;

				public bool roundRotationSpeed => rotationSettings.rotationSpeed.roundRotationSpeed;

				public float minScale => Math.Min(scaleSettings._startScale.min, scaleSettings._endScale.min);

				public float maxScale => Math.Max(scaleSettings._startScale.max, scaleSettings._endScale.max);

				public float GetRotationSpeedMultiplier(float lifetime)
				{
					return rotationSettings.rotationSpeed.GetRotationSpeedMultiplier(lifetime);
				}

				public Vector3 GetScaleAnimation(float elapsedTime)
				{
					return scaleSettings.animation.GetValue(elapsedTime);
				}
			}

			[ProtoContract]
			[UIField]
			public class ProjectileAudio
			{
				[ProtoMember(1)]
				[UIField(validateOnChange = true)]
				private ProjectileFlightAudioType? _soundLoop;

				[ProtoMember(5)]
				[UIField(min = 0, max = 1)]
				[DefaultValue(1)]
				[UIHideIf("_hideCommon")]
				private float _playAmount = 1f;

				[ProtoMember(2)]
				[UIField]
				[DefaultValue(ProjectileLifetimeType.BeginFadingOut)]
				[UIHideIf("_hideCommon")]
				private ProjectileLifetimeType _activeUntil = ProjectileLifetimeType.BeginFadingOut;

				[ProtoMember(3)]
				[UIField]
				[UIHideIf("_hideCommon")]
				[DefaultValue(ProjectileTransformType.PointOfImpact)]
				private ProjectileTransformType _emitFrom = ProjectileTransformType.PointOfImpact;

				[ProtoMember(4)]
				[UIField(collapse = UICollapseType.Hide)]
				[UIHideIf("_hideCommon")]
				private AttacherAudioProjectileSettings _settings;

				public ProjectileFlightAudioType? soundLoop => _soundLoop;

				public float playAmount => _playAmount;

				public ProjectileLifetimeType activeUntil => _activeUntil;

				public ProjectileTransformType emitFrom => _emitFrom;

				public AttacherAudioProjectileSettings settings => _settings ?? (_settings = new AttacherAudioProjectileSettings());

				private bool _hideCommon => !_soundLoop.HasValue;

				public override string ToString()
				{
					return EnumUtil.FriendlyName(_soundLoop);
				}
			}

			[ProtoContract]
			[UIField]
			public class ProjectileParticles
			{
				[ProtoMember(1, IsRequired = true)]
				[UIField(validateOnChange = true)]
				private ProjectileFlightEmitterType? _emitter = EnumUtil<ProjectileFlightEmitterType>.GetDefaultValue();

				[ProtoMember(2)]
				[UIField]
				[DefaultValue(ProjectileLifetimeType.BeginFadingOut)]
				[UIHideIf("_hideCommon")]
				private ProjectileLifetimeType _activeUntil = ProjectileLifetimeType.BeginFadingOut;

				[ProtoMember(3)]
				[UIField]
				[UIHideIf("_hideCommon")]
				[DefaultValue(ProjectileTransformType.Emitter)]
				private ProjectileTransformType _emitFrom = ProjectileTransformType.Emitter;

				[ProtoMember(4)]
				[UIField(collapse = UICollapseType.Hide)]
				[UIHideIf("_hideCommon")]
				private AttacherParticleProjectileSettings _settings;

				public ProjectileFlightEmitterType? emitter => _emitter;

				public ProjectileLifetimeType activeUntil => _activeUntil;

				public ProjectileTransformType emitFrom => _emitFrom;

				public AttacherParticleProjectileSettings settings => _settings ?? (_settings = new AttacherParticleProjectileSettings());

				private bool _hideCommon => !_emitter.HasValue;

				public override string ToString()
				{
					if (!_emitter.HasValue)
					{
						return "Disabled";
					}
					return EnumUtil.FriendlyName(_emitter.Value);
				}
			}

			[ProtoContract]
			[UIField]
			public class ProjectileVFX
			{
				[ProtoMember(1, IsRequired = true)]
				[UIField(validateOnChange = true)]
				private ProjectileFlightVFXType? _vfx = EnumUtil<ProjectileFlightVFXType>.GetDefaultValue();

				[ProtoMember(2)]
				[UIField]
				[DefaultValue(ProjectileLifetimeType.BeginFadingOut)]
				[UIHideIf("_hideCommon")]
				private ProjectileLifetimeType _activeUntil = ProjectileLifetimeType.BeginFadingOut;

				[ProtoMember(3)]
				[UIField]
				[UIHideIf("_hideCommon")]
				[DefaultValue(ProjectileTransformType.Emitter)]
				private ProjectileTransformType _emitFrom = ProjectileTransformType.Emitter;

				[ProtoMember(4)]
				[UIField(collapse = UICollapseType.Hide)]
				[UIHideIf("_hideCommon")]
				private AttacherVFXProjectileSettings _settings;

				public ProjectileFlightVFXType? vfx => _vfx;

				public ProjectileLifetimeType activeUntil => _activeUntil;

				public ProjectileTransformType emitFrom => _emitFrom;

				public AttacherVFXProjectileSettings settings => _settings ?? (_settings = new AttacherVFXProjectileSettings());

				private bool _hideCommon => !_vfx.HasValue;

				public override string ToString()
				{
					if (!_vfx.HasValue)
					{
						return "Disabled";
					}
					return EnumUtil.FriendlyName(_vfx.Value);
				}
			}

			[ProtoContract]
			[UIField]
			public class ProjectileTrail
			{
				[ProtoMember(1)]
				[UIField(validateOnChange = true)]
				private ProjectileFlightTrailType? _trail;

				[ProtoMember(2)]
				[UIField]
				[DefaultValue(ProjectileLifetimeType.BeginFadingOut)]
				[UIHideIf("_hideCommon")]
				private ProjectileLifetimeType _activeUntil = ProjectileLifetimeType.BeginFadingOut;

				[ProtoMember(3)]
				[UIField]
				[UIHideIf("_hideCommon")]
				[DefaultValue(ProjectileTransformType.Center)]
				private ProjectileTransformType _emitFrom = ProjectileTransformType.Center;

				[ProtoMember(4)]
				[UIField(collapse = UICollapseType.Hide)]
				[UIHideIf("_hideCommon")]
				private AttacherTrailProjectileSettings _settings;

				public ProjectileFlightTrailType? trail => _trail;

				public ProjectileLifetimeType activeUntil => _activeUntil;

				public ProjectileTransformType emitFrom => _emitFrom;

				public AttacherTrailProjectileSettings settings => _settings ?? (_settings = new AttacherTrailProjectileSettings());

				private bool _hideCommon => !_trail.HasValue;

				public override string ToString()
				{
					return EnumUtil.FriendlyName(_trail);
				}
			}

			[ProtoContract]
			[UIField]
			public class ProjectileRotationTrail
			{
				[ProtoMember(1)]
				[UIField(validateOnChange = true)]
				private ProjectileRotationTrailType? _trail;

				[ProtoMember(2)]
				[UIField]
				[DefaultValue(ProjectileLifetimeType.Impact)]
				[UIHideIf("_hideCommon")]
				private ProjectileLifetimeType _activeUntil;

				[ProtoMember(3)]
				[UIField(collapse = UICollapseType.Hide)]
				[UIHideIf("_hideCommon")]
				private AttacherXWeaponTrailProjectileSettings _settings;

				public ProjectileRotationTrailType? trail => _trail;

				public ProjectileLifetimeType activeUntil => _activeUntil;

				public AttacherXWeaponTrailProjectileSettings settings => _settings ?? (_settings = new AttacherXWeaponTrailProjectileSettings());

				private bool _hideCommon => !_trail.HasValue;

				public override string ToString()
				{
					return EnumUtil.FriendlyName(_trail);
				}
			}

			[ProtoContract]
			[UIField]
			public class ProjectileLighting
			{
				[ProtoMember(1)]
				[UIField(validateOnChange = true)]
				private ProjectileFlightLightType? _light;

				[ProtoMember(2)]
				[UIField]
				[DefaultValue(ProjectileLifetimeType.BeginFadingOut)]
				[UIHideIf("_hideCommon")]
				private ProjectileLifetimeType _activeUntil = ProjectileLifetimeType.BeginFadingOut;

				[ProtoMember(3)]
				[UIField]
				[UIHideIf("_hideCommon")]
				[DefaultValue(ProjectileTransformType.Center)]
				private ProjectileTransformType _emitFrom = ProjectileTransformType.Center;

				[ProtoMember(4)]
				[UIField(collapse = UICollapseType.Hide)]
				[UIHideIf("_hideCommon")]
				private AttacherLightProjectileSettings _settings;

				public ProjectileFlightLightType? light => _light;

				public ProjectileLifetimeType activeUntil => _activeUntil;

				public ProjectileTransformType emitFrom => _emitFrom;

				public AttacherLightProjectileSettings settings => _settings ?? (_settings = new AttacherLightProjectileSettings());

				private bool _hideCommon => !_light.HasValue;

				public override string ToString()
				{
					return EnumUtil.FriendlyName(_light);
				}
			}

			[ProtoContract]
			[UIField]
			public class ProjectileAfterImage
			{
				[ProtoMember(1)]
				[UIField(validateOnChange = true)]
				private bool _enabled;

				[ProtoMember(2)]
				[UIField]
				[DefaultValue(ProjectileLifetimeType.Impact)]
				[UIHideIf("_hideCommon")]
				private ProjectileLifetimeType _activeUntil;

				[ProtoMember(3)]
				[UIField]
				[UIHideIf("_hideCommon")]
				private AfterImageGeneratorData _settings;

				public ProjectileLifetimeType activeUntil => _activeUntil;

				public AfterImageGeneratorData settings => _settings ?? (_settings = new AfterImageGeneratorData());

				private bool _hideCommon => !_enabled;

				public static implicit operator bool(ProjectileAfterImage afterImage)
				{
					return afterImage?._enabled ?? false;
				}
			}

			private const ProjectileLifetimeType LIFETIME = ProjectileLifetimeType.BeginFadingOut;

			[ProtoMember(1, OverwriteList = true)]
			[UIField]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<ProjectileVisual> _projectileVisuals = new List<ProjectileVisual>
			{
				new ProjectileVisual()
			};

			[ProtoMember(2)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileAudio _projectileAudio;

			[ProtoMember(3, OverwriteList = true)]
			[UIField]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<ProjectileParticles> _projectileParticles;

			[ProtoMember(4, OverwriteList = true)]
			[UIField]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<ProjectileVFX> _projectileVFX;

			[ProtoMember(5)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileTrail _projectileTrail;

			[ProtoMember(6)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileRotationTrail _projectileRotationTrail;

			[ProtoMember(7)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileLighting _projectileLighting;

			public List<ProjectileVisual> visuals
			{
				get
				{
					if (!_projectileVisuals.IsNullOrEmpty())
					{
						return _projectileVisuals;
					}
					return _projectileVisuals = new List<ProjectileVisual>
					{
						new ProjectileVisual()
					};
				}
			}

			public ProjectileAudio audio => _projectileAudio ?? (_projectileAudio = new ProjectileAudio());

			public List<ProjectileParticles> particles => _projectileParticles ?? (_projectileParticles = new List<ProjectileParticles>());

			public List<ProjectileVFX> vfx => _projectileVFX ?? (_projectileVFX = new List<ProjectileVFX>());

			public ProjectileTrail trail => _projectileTrail ?? (_projectileTrail = new ProjectileTrail());

			public ProjectileRotationTrail rotationTrail => _projectileRotationTrail ?? (_projectileRotationTrail = new ProjectileRotationTrail());

			public ProjectileLighting lighting => _projectileLighting ?? (_projectileLighting = new ProjectileLighting());

			public override string ToString()
			{
				if (visuals.Count <= 0)
				{
					return "<i>Null</i>";
				}
				return visuals[0].name;
			}

			private void OnValidateUI()
			{
				lighting.settings.canInheritColor = visuals.Any((ProjectileVisual visual) => visual.hasPrimaryColor);
			}
		}

		[ProtoMember(1, OverwriteList = true)]
		[UIField("Flight Parameters", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
		private List<ProjectileFlightParameters> _parameters = new List<ProjectileFlightParameters>
		{
			new ProjectileFlightParameters()
		};

		[ProtoMember(2, OverwriteList = true)]
		[UIField("Flight Media", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
		[UIFieldCollectionItem]
		private List<ProjectileFlightMedia> _media = new List<ProjectileFlightMedia>
		{
			new ProjectileFlightMedia()
		};

		[ProtoMember(3)]
		[UIField(validateOnChange = true)]
		[DefaultValue(true)]
		[UIHideIf("_hideSyncParameterAndMediaIndices")]
		private bool _syncParameterAndMediaIndices = true;

		private bool? _usesLighting;

		public List<ProjectileFlightParameters> parameters
		{
			get
			{
				if (_parameters.IsNullOrEmpty())
				{
					List<ProjectileFlightParameters> obj = new List<ProjectileFlightParameters>
					{
						new ProjectileFlightParameters()
					};
					List<ProjectileFlightParameters> result = obj;
					_parameters = obj;
					return result;
				}
				return _parameters;
			}
		}

		public List<ProjectileFlightMedia> media
		{
			get
			{
				if (_media.IsNullOrEmpty())
				{
					List<ProjectileFlightMedia> obj = new List<ProjectileFlightMedia>
					{
						new ProjectileFlightMedia()
					};
					List<ProjectileFlightMedia> result = obj;
					_media = obj;
					return result;
				}
				return _media;
			}
		}

		public bool syncParameterAndMediaIndex
		{
			get
			{
				if (_syncParameterAndMediaIndices)
				{
					return !_hideSyncParameterAndMediaIndices;
				}
				return false;
			}
		}

		public bool usesLighting
		{
			get
			{
				bool? flag = _usesLighting;
				if (!flag.HasValue)
				{
					bool? flag2 = (_usesLighting = media.Any((ProjectileFlightMedia m) => m.lighting.light.HasValue));
					return flag2.Value;
				}
				return flag.GetValueOrDefault();
			}
		}

		private bool _hideSyncParameterAndMediaIndices
		{
			get
			{
				if (parameters.Count != 1)
				{
					return parameters.Count != media.Count;
				}
				return true;
			}
		}

		private void OnValidateUI()
		{
			_usesLighting = null;
			for (int i = 0; i < parameters.Count; i++)
			{
				parameters[i].name = (syncParameterAndMediaIndex ? media[i].ToString() : null);
			}
		}
	}

	[ProtoContract]
	[UIField]
	public class ProjectileImpact
	{
		[ProtoContract]
		[UIField]
		public class ProjectileImpactParameters
		{
			[ProtoMember(1)]
			[UIField]
			[DefaultValue(true)]
			private bool _trackTarget = true;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Hide)]
			private LaunchPatternShapeSettings _shape = new LaunchPatternShapeSettings();

			public LaunchPatternShapeSettings shape => _shape ?? (_shape = new LaunchPatternShapeSettings());

			public bool trackTarget => _trackTarget;
		}

		[ProtoMember(1)]
		[UIField("Impact Parameters", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
		private ProjectileImpactParameters _parameters;

		[ProtoMember(2)]
		[UIField("Impact Media", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
		private ProjectileBurstMedia<ProjectileImpactSFXType> _media = new ProjectileBurstMedia<ProjectileImpactSFXType>(AudioCategoryType.ProjectileImpact);

		public ProjectileImpactParameters parameters => _parameters ?? (_parameters = new ProjectileImpactParameters());

		public ProjectileBurstMedia<ProjectileImpactSFXType> media => _media ?? (_media = new ProjectileBurstMedia<ProjectileImpactSFXType>(AudioCategoryType.ProjectileImpact));
	}

	[ProtoContract]
	[UIField]
	public class ProjectileAfterImpact
	{
		[ProtoMember(1, OverwriteList = true)]
		[UIField("After Impact Parameters", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
		private List<ProjectileAfterImpactParameters> _parameters = new List<ProjectileAfterImpactParameters>
		{
			new ProjectileAfterImpactParameters()
		};

		[ProtoMember(2)]
		[UIField]
		[DefaultValue(true)]
		[UIHideIf("_hideSyncWithFlightMedia")]
		private bool _syncWithFlightMedia = true;

		[ProtoMember(3)]
		[UIField("After Impact Media", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
		private ProjectileBurstMedia<ProjectileImpactSFXType> _media = new ProjectileBurstMedia<ProjectileImpactSFXType>(AudioCategoryType.ProjectileImpact);

		public List<ProjectileAfterImpactParameters> parameters
		{
			get
			{
				if (_parameters.IsNullOrEmpty())
				{
					List<ProjectileAfterImpactParameters> obj = new List<ProjectileAfterImpactParameters>
					{
						new ProjectileAfterImpactParameters()
					};
					List<ProjectileAfterImpactParameters> result = obj;
					_parameters = obj;
					return result;
				}
				return _parameters;
			}
		}

		public bool syncWithFlightMedia => _syncWithFlightMedia;

		public ProjectileBurstMedia<ProjectileImpactSFXType> media => _media ?? (_media = new ProjectileBurstMedia<ProjectileImpactSFXType>(AudioCategoryType.ProjectileImpact));

		private bool _hideSyncWithFlightMedia => parameters.Count < 2;

		private void OnValidateUI()
		{
			_ = parameters;
		}
	}

	[ProtoContract]
	[UIField]
	public class ProjectileAfterImpactParameters
	{
		[ProtoContract]
		[UIField("Do Not Attach", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		[ProtoInclude(10, typeof(LockOnAttachment))]
		[ProtoInclude(11, typeof(StickyAttachment))]
		[ProtoInclude(12, typeof(ElasticAttachment))]
		[ProtoInclude(13, typeof(TrajectoryAttachment))]
		public class Attachment
		{
			public virtual bool attachToTarget => false;

			public virtual void Update(ProjectileFlightSFX projectile, Transform attached, Transform target, ref Vector3 velocity, ref Vector4 angularVelocity)
			{
			}
		}

		[ProtoContract]
		[UIField("Lock On", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1u)]
		public class LockOnAttachment : Attachment
		{
			[ProtoMember(1)]
			[UIField]
			[DefaultValue(true)]
			private bool _matchRotation = true;

			public bool matchRotation => _matchRotation;

			public override bool attachToTarget => true;

			public override void Update(ProjectileFlightSFX projectile, Transform attached, Transform target, ref Vector3 velocity, ref Vector4 angularVelocity)
			{
				attached.position = target.position;
				if (_matchRotation)
				{
					attached.rotation = target.rotation;
				}
			}
		}

		[ProtoContract]
		[UIField("Sticky", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		public class StickyAttachment : Attachment
		{
			[ProtoMember(1)]
			[UIField(min = 0, max = 1)]
			[DefaultValue(1)]
			private float _stickyness = 1f;

			[ProtoMember(2)]
			[UIField(min = 0, max = 20f, stepSize = 0.01f)]
			[DefaultValue(9.81f)]
			private float _gravity = 9.81f;

			public override bool attachToTarget => true;

			public override void Update(ProjectileFlightSFX projectile, Transform attached, Transform target, ref Vector3 velocity, ref Vector4 angularVelocity)
			{
				attached.rotation = target.rotation;
				velocity = Vector3.Project(velocity, target.up);
				velocity *= MathUtil.FrictionSubjectToTimeSmooth(Mathf.Pow(1f - _stickyness, 10f), Time.deltaTime);
				velocity -= target.up * _gravity * Time.deltaTime;
				attached.position += velocity * Time.deltaTime;
				attached.position = Vector3.ProjectOnPlane(attached.position - target.position, target.forward) + target.position;
			}
		}

		[ProtoContract]
		[UIField("Elastic", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		public class ElasticAttachment : Attachment
		{
			[ProtoMember(1)]
			[UIField(min = 0, max = 500)]
			[DefaultValue(100)]
			private int _springConstant = 100;

			[ProtoMember(2)]
			[UIField(min = 0, max = 100)]
			[DefaultValue(10)]
			private int _dampening = 10;

			[ProtoMember(3)]
			[UIField(validateOnChange = true)]
			private bool _matchRotation;

			[ProtoMember(4)]
			[UIField(min = 0, max = 500)]
			[DefaultValue(100)]
			[UIHideIf("_hideRotation")]
			private int _rotationSpringConstant = 100;

			[ProtoMember(5)]
			[UIField(min = 0, max = 100)]
			[DefaultValue(10)]
			[UIHideIf("_hideRotation")]
			private int _rotationDampening = 10;

			public override bool attachToTarget => true;

			private bool _hideRotation => !_matchRotation;

			public override void Update(ProjectileFlightSFX projectile, Transform attached, Transform target, ref Vector3 velocity, ref Vector4 angularVelocity)
			{
				Vector3 position = attached.position;
				MathUtil.Spring(ref position, ref velocity, target.position, _springConstant, _dampening, Time.deltaTime);
				attached.position = position;
				if (_matchRotation)
				{
					Quaternion current = attached.rotation;
					MathUtil.Spring(ref current, ref angularVelocity, target.rotation, _rotationSpringConstant, _rotationDampening, Time.deltaTime);
					attached.rotation = current;
				}
			}
		}

		[ProtoContract]
		[UIField("Trajectory", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		public class TrajectoryAttachment : Attachment
		{
			[ProtoMember(1)]
			[UIField(min = 0, max = 1)]
			private float _friction;

			[ProtoMember(2)]
			[UIField(min = -20f, max = 20f, stepSize = 0.01f)]
			[DefaultValue(9.81f)]
			private float _gravity = 9.81f;

			[ProtoMember(3)]
			[UIField]
			private bool _updateRotation;

			[ProtoMember(4)]
			[UIField(min = 0, max = 1)]
			[UIHorizontalLayout("Bounce")]
			private float? _bounciness;

			[ProtoMember(5)]
			[UIField(min = 0, max = 1)]
			[UIHorizontalLayout("Bounce")]
			private float _bounceFriction;

			public override bool attachToTarget => true;

			public override void Update(ProjectileFlightSFX projectile, Transform attached, Transform target, ref Vector3 velocity, ref Vector4 angularVelocity)
			{
				velocity.y -= _gravity * Time.deltaTime;
				velocity *= MathUtil.FrictionSubjectToTimeSmooth(Mathf.Pow(1f - _friction, 10f), Time.deltaTime);
				projectile.transform.position += velocity * Time.deltaTime;
				if (_bounciness.HasValue && projectile.transform.position.y - projectile.radius < 0f)
				{
					projectile.transform.position = projectile.transform.position.SetAxis(AxisType.Y, projectile.radius);
					float y = (0f - velocity.y) * _bounciness.Value;
					velocity *= 1f - _bounceFriction;
					velocity.y = y;
				}
				if (_updateRotation && velocity.sqrMagnitude > 0.001f)
				{
					projectile.transform.rotation = Quaternion.LookRotation(velocity, projectile.transform.up);
				}
			}
		}

		[ProtoContract]
		[UIField]
		public class Animation
		{
			[ProtoContract]
			[UIField]
			public class Translations
			{
				[ProtoContract]
				[UIField]
				public class Translation
				{
					private static readonly RangeF RANGE = new RangeF(0f, 0f, -10f, 10f).Scale(0.1f);

					[ProtoMember(1)]
					[UIField(validateOnChange = true)]
					private bool _enabled;

					[ProtoMember(2)]
					[UIField("X Translation", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
					[UIHideIf("_hideCommon")]
					private RangeF _xTranslation = RANGE;

					[ProtoMember(3)]
					[UIField("Y Translation", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
					[UIHideIf("_hideCommon")]
					private RangeF _yTranslation = RANGE;

					[ProtoMember(4)]
					[UIField("Z Translation", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
					[UIHideIf("_hideCommon")]
					private RangeF _zTranslation = RANGE;

					[ProtoMember(5)]
					[UIField]
					[UIHideIf("_hideCommon")]
					private InterpolatorSettings _interpolationSettings;

					protected InterpolatorSettings interpolatorSettings => _interpolationSettings ?? (_interpolationSettings = new InterpolatorSettings());

					public bool enabled => _enabled;

					private bool _hideCommon => !_enabled;

					public Vector3 GetTranslation(float normalizedTime, Vector4 rangeSamples)
					{
						return new Vector3(_xTranslation.Lerp(rangeSamples.x), _yTranslation.Lerp(rangeSamples.y), _zTranslation.Lerp(rangeSamples.z)) * interpolatorSettings.Interpolate(normalizedTime, rangeSamples.w);
					}
				}

				[ProtoMember(1)]
				[UIField]
				private Translation _localTranslation;

				[ProtoMember(2)]
				[UIField]
				private Translation _translationAroundTarget;

				[ProtoMember(3)]
				[UIField]
				private Translation _worldTranslation;

				public Translation localTranslation => _localTranslation ?? (_localTranslation = new Translation());

				public Translation translationAroundTarget => _translationAroundTarget ?? (_translationAroundTarget = new Translation());

				public Translation worldTranslation => _worldTranslation ?? (_worldTranslation = new Translation());
			}

			[ProtoContract]
			[UIField]
			public class Rotations
			{
				[ProtoContract]
				[UIField]
				public class Rotation
				{
					[ProtoMember(1)]
					[UIField(validateOnChange = true)]
					private bool _enabled;

					[ProtoMember(2)]
					[UIField]
					[UIHideIf("_hideCommon")]
					private RotationRanges _rotation;

					[ProtoMember(3)]
					[UIField]
					[UIHideIf("_hideCommon")]
					private InterpolatorSettings _interpolationSettings;

					protected InterpolatorSettings interpolatorSettings => _interpolationSettings ?? (_interpolationSettings = new InterpolatorSettings());

					public bool enabled => _enabled;

					protected RotationRanges rotation => _rotation ?? (_rotation = new RotationRanges());

					private bool _hideCommon => !_enabled;

					private bool _rotationSpecified => _rotation;

					public Vector3 GetEulerAngles(float normalizedTime, Vector4 rangeSamples)
					{
						return new Vector3(rotation.pitch.Lerp(rangeSamples.x), rotation.yaw.Lerp(rangeSamples.y), rotation.roll.Lerp(rangeSamples.z)) * interpolatorSettings.Interpolate(normalizedTime, rangeSamples.w);
					}
				}

				[ProtoMember(1)]
				[UIField]
				private Rotation _localRotation;

				[ProtoMember(2)]
				[UIField]
				private Rotation _rotationAroundTarget;

				public Rotation localRotation => _localRotation ?? (_localRotation = new Rotation());

				public Rotation rotationAroundTarget => _rotationAroundTarget ?? (_rotationAroundTarget = new Rotation());
			}

			[ProtoContract]
			[UIField]
			public class Scales
			{
				[ProtoContract]
				[UIField]
				public class Scale
				{
					private static readonly RangeF RANGE = new RangeF(1f, 1f, 0.01f, 3f);

					private static readonly Vector3 MIN_SCALE = new Vector3(0.01f, 0.01f, 0.01f);

					[ProtoMember(1)]
					[UIField(validateOnChange = true)]
					private bool _enabled;

					[ProtoMember(2)]
					[UIField(validateOnChange = true)]
					[DefaultValue(true)]
					[UIHideIf("_hideCommon")]
					private bool _uniformScale = true;

					[ProtoMember(3)]
					[UIField("X Scale", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
					[UIHideIf("_hideCommon")]
					private RangeF _xScale = RANGE;

					[ProtoMember(4)]
					[UIField("Y Scale", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
					[UIHideIf("_hideUniform")]
					private RangeF _yScale = RANGE;

					[ProtoMember(5)]
					[UIField("Z Scale", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
					[UIHideIf("_hideUniform")]
					private RangeF _zScale = RANGE;

					[ProtoMember(6)]
					[UIField]
					[UIHideIf("_hideCommon")]
					private InterpolatorSettings _interpolationSettings;

					protected InterpolatorSettings interpolatorSettings => _interpolationSettings ?? (_interpolationSettings = new InterpolatorSettings());

					public bool enabled => _enabled;

					private bool _xScaleSpecified => _xScale != RANGE;

					private bool _yScaleSpecified => _yScale != RANGE;

					private bool _zScaleSpecified => _zScale != RANGE;

					private bool _hideCommon => !_enabled;

					private bool _hideUniform
					{
						get
						{
							if (!_hideCommon)
							{
								return _uniformScale;
							}
							return true;
						}
					}

					public Vector3 GetScale(float normalizedTime, Vector4 rangeSamples)
					{
						return Vector3.one.Lerp(_uniformScale ? _xScale.Lerp(rangeSamples.x).ToVector3() : new Vector3(_xScale.Lerp(rangeSamples.x), _yScale.Lerp(rangeSamples.y), _zScale.Lerp(rangeSamples.z)), interpolatorSettings.Interpolate(normalizedTime, rangeSamples.w)).Max(MIN_SCALE);
					}
				}

				[ProtoMember(1)]
				[UIField]
				private Scale _localScale;

				[ProtoMember(2)]
				[UIField]
				private Scale _scaleAroundTarget;

				public Scale localScale => _localScale ?? (_localScale = new Scale());

				public Scale scaleAroundTarget => _scaleAroundTarget ?? (_scaleAroundTarget = new Scale());
			}

			private static readonly RangeF DELAY = new RangeF(0f, 0f, 0f, 2f);

			[ProtoMember(1)]
			[UIField(validateOnChange = true)]
			private bool _enabled;

			[ProtoMember(2)]
			[UIField]
			[UIHideIf("_hideCommon")]
			private RangeF _delay = DELAY;

			[ProtoMember(3)]
			[UIField]
			[UIHideIf("_hideCommon")]
			private Translations _translation;

			[ProtoMember(4)]
			[UIField]
			[UIHideIf("_hideCommon")]
			private Rotations _rotation;

			[ProtoMember(5)]
			[UIField]
			[UIHideIf("_hideCommon")]
			private Scales _scale;

			public bool enabled => _enabled;

			public RangeF delay => _delay;

			public Translations translation => _translation ?? (_translation = new Translations());

			public Rotations rotation => _rotation ?? (_rotation = new Rotations());

			public Scales scale => _scale ?? (_scale = new Scales());

			private bool _hideCommon => !_enabled;
		}

		private static readonly RangeF LIFETIME = new RangeF(0f, 0f, 0f, 2f);

		[ProtoMember(1)]
		[UIField]
		private RangeF _lifetimeAfterImpact = LIFETIME;

		[ProtoMember(2)]
		[UIField(min = 0, max = 0.5f, stepSize = 0.01f)]
		private float _fadeTime;

		[ProtoMember(3)]
		[UIField]
		private Attachment _attachment;

		[ProtoMember(4)]
		[UIField]
		private Animation _animation;

		public RangeF lifetime => _lifetimeAfterImpact;

		public float fadeTime => _fadeTime;

		public bool animated
		{
			get
			{
				if (_lifetimeAfterImpact.max > 0f)
				{
					if (_animation == null || !_animation.enabled)
					{
						return attachment.attachToTarget;
					}
					return true;
				}
				return false;
			}
		}

		public Animation animation => _animation ?? (_animation = new Animation());

		public Attachment attachment => _attachment ?? (_attachment = new LockOnAttachment());

		private bool _attachmentSpecified => !(_attachment is LockOnAttachment lockOnAttachment) || !lockOnAttachment.matchRotation;

		public override string ToString()
		{
			return "After Impact Parameters";
		}
	}

	[ProtoContract(EnumPassthru = true)]
	public enum ShapeDistributionType : byte
	{
		Random,
		Uniform
	}

	[ProtoContract(EnumPassthru = true)]
	public enum ShapeUpDirectionType : byte
	{
		Upward,
		Outward,
		Inward
	}

	[ProtoContract]
	[UIField]
	public class LaunchPatternShapeSettings
	{
		private const string CAT_UP = "Up Direction";

		[ProtoMember(1)]
		[UIField]
		[UIDeepValueChange]
		private LaunchPatternShape _shapeType;

		[ProtoMember(2)]
		[UIField]
		[UIDeepValueChange]
		private OffsetRanges _shapeOffsets;

		[ProtoMember(3)]
		[UIField]
		[UIHideIf("_hideShapeRotation")]
		[UIDeepValueChange]
		private RotationRanges _shapeRotation;

		[ProtoMember(4)]
		[UIField(category = "Up Direction", excludedValuesMethod = "_ExcludedUpDirection")]
		private ShapeUpDirectionType _upDirection;

		[ProtoMember(5)]
		[UIField(min = 0, max = 360, category = "Up Direction")]
		private int _coneAngle;

		[ProtoMember(6)]
		[UIField(category = "Up Direction")]
		[UIDeepValueChange]
		private RotationRanges _upDirectionRotation;

		protected LaunchPatternShape shape => _shapeType ?? (_shapeType = new LaunchPatternShape());

		protected RotationRanges shapeRotation => _shapeRotation ?? (_shapeRotation = new RotationRanges());

		protected RotationRanges upRotation => _upDirectionRotation ?? (_upDirectionRotation = new RotationRanges());

		public OffsetRanges shapeOffsets => _shapeOffsets ?? (_shapeOffsets = new OffsetRanges());

		private bool _hideShapeRotation => shape.GetType() == typeof(LaunchPatternShape);

		private bool _shapeOffsetsSpecified => _shapeOffsets;

		private bool _shapeRotationSpecified => _shapeRotation;

		private bool _upDirectionRotationSpecified => _upDirectionRotation;

		public ProjectileLaunchPatternNodes GetNodes(System.Random random, int numberOfProjectiles)
		{
			ProjectileLaunchPatternNodes nodes = shape.GetNodes(random, numberOfProjectiles);
			nodes.RotateNodePositions(random.RangeInt(shapeRotation.pitch), random.RangeInt(shapeRotation.yaw), random.RangeInt(shapeRotation.roll));
			nodes.SetNodeRotations(_upDirection);
			nodes.RotateNodeDirections(random, upRotation.pitch, upRotation.yaw, upRotation.roll);
			nodes.RandomizeNodeDirections(random, _coneAngle);
			nodes.OffsetNodes(random.Range(shapeOffsets.horizontal), random.Range(shapeOffsets.vertical), random.Range(shapeOffsets.forward));
			nodes.nodes.Reverse();
			return nodes;
		}

		private bool _ExcludedUpDirection(ShapeUpDirectionType upDirection)
		{
			return shape.ExcludedUpDirection(upDirection);
		}
	}

	[ProtoContract]
	[UIField("Point", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	[ProtoInclude(10, typeof(LaunchPatternShape2D))]
	[ProtoInclude(11, typeof(LaunchPatternShape3D))]
	[ProtoInclude(12, typeof(LaunchPatternShapeCylinder))]
	public class LaunchPatternShape
	{
		protected virtual void _PopulateNodePositions(System.Random random, List<PositionRotation> nodes, int numberOfNodes)
		{
			for (int i = 0; i < numberOfNodes; i++)
			{
				nodes.Add(new PositionRotation(Vector3.zero));
			}
		}

		public virtual bool ExcludedUpDirection(ShapeUpDirectionType upDirection)
		{
			return upDirection != ShapeUpDirectionType.Upward;
		}

		public ProjectileLaunchPatternNodes GetNodes(System.Random random, int numberOfNodes)
		{
			ProjectileLaunchPatternNodes projectileLaunchPatternNodes = Pools.Unpool<ProjectileLaunchPatternNodes>();
			_PopulateNodePositions(random, projectileLaunchPatternNodes.nodes, numberOfNodes);
			return projectileLaunchPatternNodes;
		}

		public override string ToString()
		{
			return GetType().GetUILabel();
		}
	}

	[ProtoContract]
	[UIField]
	[ProtoInclude(10, typeof(LaunchPatternShapeCircle))]
	[ProtoInclude(11, typeof(LaunchPatternShapeSpiral))]
	[ProtoInclude(12, typeof(LaunchPatternShapeRoundedRect))]
	public abstract class LaunchPatternShape2D : LaunchPatternShape
	{
		private const ShapeDistributionType DIST_TYPE = ShapeDistributionType.Random;

		[ProtoMember(1)]
		[UIField(order = 1u, validateOnChange = true)]
		[DefaultValue(ShapeDistributionType.Random)]
		protected ShapeDistributionType _distribution;

		public override bool ExcludedUpDirection(ShapeUpDirectionType upDirection)
		{
			return false;
		}
	}

	[ProtoContract]
	[UIField("Circle", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	public class LaunchPatternShapeCircle : LaunchPatternShape2D
	{
		private static readonly RangeF RADIUS = new RangeF(0.2f, 0.2f, 0.01f, 5f).Scale(0.1f);

		private const float DISTRIBUTION_POWER = 1f;

		[ProtoMember(1)]
		[UIField(view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001f)]
		private RangeF _radius = RADIUS;

		[ProtoMember(2)]
		[UIField(min = 0.1f, max = 10, stepSize = 0.01)]
		[DefaultValue(1f)]
		[UIHideIf("_hideDistributionPower")]
		private float _distributionPower = 1f;

		private bool _radiusSpecified => _radius != RADIUS;

		private bool _hideDistributionPower => _distribution != ShapeDistributionType.Random;

		protected override void _PopulateNodePositions(System.Random random, List<PositionRotation> nodes, int numberOfNodes)
		{
			float num = random.Range(_radius);
			float p = ((_distribution == ShapeDistributionType.Random) ? _distributionPower : 0.5f);
			for (int i = 0; i < numberOfNodes; i++)
			{
				float num2 = num * Mathf.Pow(random.Value(), p);
				float f = random.Range(0f, MathF.PI * 2f);
				nodes.Add(new PositionRotation(new Vector3(Mathf.Cos(f) * num2, 0f, Mathf.Sin(f) * num2)));
			}
		}
	}

	[ProtoContract]
	[UIField("Ring", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	public class LaunchPatternShapeSpiral : LaunchPatternShape2D
	{
		private static readonly RangeF RADIUS = new RangeF(0.2f, 0.2f, 0.01f, 5f).Scale(0.1f);

		private static readonly RangeF REVOLUTIONS = new RangeF(1f, 1f, 0.25f, 10f);

		[ProtoMember(1)]
		[UIField(view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001f)]
		private RangeF _startRadius = RADIUS;

		[ProtoMember(2)]
		[UIField(view = "UI/Reflection/Range Slider Advanced", stepSize = 0.001f)]
		private RangeF _endRadius = RADIUS;

		[ProtoMember(3)]
		[UIField(view = "UI/Reflection/Range Slider Advanced")]
		private RangeF _revolutions = REVOLUTIONS;

		private bool _startRadiusSpecified => _startRadius != RADIUS;

		private bool _endRadiusSpecified => _endRadius != RADIUS;

		private bool _revolutionsSpecified => _revolutions != REVOLUTIONS;

		protected override void _PopulateNodePositions(System.Random random, List<PositionRotation> nodes, int numberOfNodes)
		{
			float num = random.Range(_revolutions) * (MathF.PI * 2f);
			float num2 = random.Range(_startRadius);
			float num3 = random.Range(_endRadius);
			switch (_distribution)
			{
			case ShapeDistributionType.Random:
			{
				for (int j = 0; j < numberOfNodes; j++)
				{
					float num8 = random.Value();
					Vector2 vector = new Vector2(MathUtil.Lerp(num2, num3, num8), num * num8);
					nodes.Add(new PositionRotation(new Vector3(Mathf.Cos(vector.y) * vector.x, 0f, Mathf.Sin(vector.y) * vector.x)));
				}
				break;
			}
			case ShapeDistributionType.Uniform:
			{
				float num4 = 0f;
				float num5 = num2;
				float num6 = num / (float)numberOfNodes;
				float num7 = (num3 - num2) / (float)Math.Max(1, numberOfNodes - 1);
				for (int i = 0; i < numberOfNodes; i++)
				{
					nodes.Add(new PositionRotation(new Vector3(Mathf.Cos(num4) * num5, 0f, Mathf.Sin(num4) * num5)));
					num4 += num6;
					num5 += num7;
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	[ProtoContract]
	[UIField("Rounded Rectangle", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	public class LaunchPatternShapeRoundedRect : LaunchPatternShape2D
	{
		private static readonly RangeF WIDTH = new RangeF(0.635f, 0.635f, 0.01f, 5f).Scale(0.1f);

		private static readonly RangeF HEIGHT = new RangeF(0.889f, 0.889f, 0.01f, 5f).Scale(0.1f);

		private static readonly RangeF ROUND = new RangeF(0.1f, 0.1f);

		private static readonly RangeF EMPTY = new RangeF(0f, 0f);

		private static readonly RangeF SCALE_RANGE = new RangeF(0.9f, 0.9f, 0.1f, 3f);

		[ProtoMember(1)]
		[UIField(stepSize = 0.0001f)]
		private RangeF _width = WIDTH;

		[ProtoMember(2)]
		[UIField(stepSize = 0.0001f)]
		private RangeF _height = HEIGHT;

		[ProtoMember(3)]
		[UIField]
		private RangeF _roundedRatio = ROUND;

		[ProtoMember(4)]
		[UIField]
		private RangeF _emptyRatio = EMPTY;

		[ProtoMember(5)]
		[UIField]
		private RangeF _scale = SCALE_RANGE;

		private bool _widthSpecified => _width != WIDTH;

		private bool _heightSpecified => _height != HEIGHT;

		private bool _roundedRatioSpecified => _roundedRatio != ROUND;

		private bool _emptyRatioSpecified => _emptyRatio != EMPTY;

		private bool _scaleSpecified => _scale != SCALE_RANGE;

		protected override void _PopulateNodePositions(System.Random random, List<PositionRotation> nodes, int numberOfNodes)
		{
			float num = random.Range(_scale);
			float num2 = random.Range(_width);
			float num3 = random.Range(_height);
			float num4 = (num - 1f) * Math.Min(num2, num3);
			num2 += num4;
			num3 += num4;
			float num5 = num2 * 0.5f;
			float num6 = 0f - num5;
			float num7 = num3 * 0.5f;
			float num8 = 0f - num7;
			float num9 = random.Range(_roundedRatio);
			float num10 = random.Range(_emptyRatio);
			using PoolKeepItemListHandle<Vector2> poolKeepItemListHandle = Pools.UseKeepItemList<Vector2>();
			List<Vector2> value = poolKeepItemListHandle.value;
			switch (_distribution)
			{
			case ShapeDistributionType.Random:
			{
				for (int k = 0; k < numberOfNodes; k++)
				{
					value.Add(new Vector2(random.Range(num6, num5), random.Range(num8, num7)));
				}
				break;
			}
			case ShapeDistributionType.Uniform:
			{
				float num11 = num2 * num3;
				float num12 = num3 / num11;
				float num13 = num2 / num11;
				float num14 = num12 * num13;
				float num15 = Mathf.Sqrt((float)numberOfNodes / num14);
				int num16;
				int num17;
				if (num12 < num13)
				{
					num16 = Math.Max(1, Mathf.RoundToInt(MathUtil.RoundToNearestFactorOf(Mathf.RoundToInt(num12 * num15), numberOfNodes)));
					num17 = Math.Max(1, Mathf.CeilToInt((float)numberOfNodes / (float)num16));
				}
				else
				{
					num17 = Math.Max(1, Mathf.RoundToInt(MathUtil.RoundToNearestFactorOf(Mathf.RoundToInt(num13 * num15), numberOfNodes)));
					num16 = Math.Max(1, Mathf.CeilToInt((float)numberOfNodes / (float)num17));
				}
				float num18 = num2 / (float)num17;
				float num19 = num6 + num18 * 0.5f;
				float num20 = num3 / (float)num16;
				float num21 = num8 + num20 * 0.5f;
				for (int i = 0; i < num17; i++)
				{
					float x = num19 + (float)i * num18;
					for (int j = 0; j < num16; j++)
					{
						if (value.Count >= numberOfNodes)
						{
							break;
						}
						value.Add(new Vector2(x, num21 + (float)j * num20));
					}
				}
				break;
			}
			}
			if (num10 > 0f)
			{
				float num22 = 1f / num10;
				float num23 = num5 * num10;
				float num24 = 1f / num23;
				float num25 = num7 * num10;
				float num26 = 1f / num25;
				for (int l = 0; l < value.Count; l++)
				{
					Vector2 vector = value[l];
					if (vector == Vector2.zero)
					{
						vector = new Vector2(random.Range(0.001f, num5), random.Range(0.001f, num7));
					}
					float num27 = new Vector2(vector.x * num24, vector.y * num26).AbsMax();
					if (!(num27 >= 1f))
					{
						float num28 = 1f / num27;
						Vector2 vector2 = vector * num28;
						Vector2 b = vector2 * num22;
						value[l] = Vector2.Lerp(vector2, b, num27);
					}
				}
			}
			if (num9 > 0f)
			{
				float num29 = Math.Min(num5, num7) * num9;
				float num30 = num5 - num29;
				float num31 = 1f / num30;
				float num32 = num7 - num29;
				float num33 = 1f / num32;
				Vector2 min = new Vector2(0f - num30, 0f - num32);
				Vector2 max = new Vector2(num30, num32);
				for (int m = 0; m < value.Count; m++)
				{
					Vector2 vector3 = value[m];
					if (!(new Vector2(vector3.x * num31, vector3.y * num33).AbsMin() <= 1f))
					{
						Vector2 vector4 = vector3.Clamp(min, max);
						Vector2 v = vector3 - vector4;
						value[m] = vector4 + v.normalized * num29 * v.AbsMax() / num29;
					}
				}
			}
			foreach (Vector2 item in value)
			{
				nodes.Add(new PositionRotation(item.Unproject(AxisType.Y)));
			}
		}
	}

	[ProtoContract]
	[UIField]
	[ProtoInclude(10, typeof(LaunchPatternShapeEllipsoid))]
	[ProtoInclude(11, typeof(LaunchPatternShapeRectPrism))]
	public abstract class LaunchPatternShape3D : LaunchPatternShape
	{
		private static readonly RangeF SIZE_RANGE = new RangeF(0.2f, 0.2f, 0.01f, 5f).Scale(0.1f);

		[ProtoMember(1)]
		[UIField]
		protected ShapeDistributionType _distribution;

		[ProtoMember(2)]
		[UIField(stepSize = 0.001f, view = "UI/Reflection/Range Slider Advanced")]
		protected RangeF _horizontalSize = SIZE_RANGE;

		[ProtoMember(3)]
		[UIField(stepSize = 0.001f, view = "UI/Reflection/Range Slider Advanced")]
		protected RangeF _verticalSize = SIZE_RANGE;

		[ProtoMember(4)]
		[UIField(stepSize = 0.001f, view = "UI/Reflection/Range Slider Advanced")]
		protected RangeF _forwardSize = SIZE_RANGE;

		protected Vector3 _minSize => new Vector3(_horizontalSize.min, _verticalSize.min, _forwardSize.min);

		protected Vector3 _maxSize => new Vector3(_horizontalSize.max, _verticalSize.max, _forwardSize.max);

		private bool _horizontalSizeSpecified => _horizontalSize != SIZE_RANGE;

		private bool _verticalSizeSpecified => _verticalSize != SIZE_RANGE;

		private bool _forwardSizeSpecified => _forwardSize != SIZE_RANGE;

		public override bool ExcludedUpDirection(ShapeUpDirectionType upDirection)
		{
			return false;
		}
	}

	[ProtoContract]
	[UIField("Ellipsoid", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	public class LaunchPatternShapeEllipsoid : LaunchPatternShape3D
	{
		private static readonly RangeF SWEEP = new RangeF(0f);

		[ProtoMember(1)]
		[UIField]
		private RangeF _sweepAngleRange = SWEEP;

		private bool _sweepAngleRangeSpecified => _sweepAngleRange != SWEEP;

		protected override void _PopulateNodePositions(System.Random random, List<PositionRotation> nodes, int numberOfNodes)
		{
			Vector3 minSize = base._minSize;
			Vector3 maxSize = base._maxSize;
			switch (_distribution)
			{
			case ShapeDistributionType.Uniform:
			{
				foreach (Vector3 item in MathUtil.UniformPointsOnUnitSphere(numberOfNodes, _sweepAngleRange.min, _sweepAngleRange.max))
				{
					nodes.Add(new PositionRotation(item.Multiply(minSize.Lerp(maxSize, random.Value()))));
				}
				break;
			}
			case ShapeDistributionType.Random:
			{
				foreach (Vector3 item2 in random.RandomOnUnitSphere(numberOfNodes, _sweepAngleRange.min, _sweepAngleRange.max))
				{
					nodes.Add(new PositionRotation(item2.Multiply(minSize.Lerp(maxSize, random.Value()))));
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	[ProtoContract]
	[UIField("Rectangular Prism", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	public class LaunchPatternShapeRectPrism : LaunchPatternShape3D
	{
		protected override void _PopulateNodePositions(System.Random random, List<PositionRotation> nodes, int numberOfNodes)
		{
			foreach (Vector3 item in random.RandomInRectPrismShell(base._minSize * 0.5f, base._maxSize * 0.5f, numberOfNodes))
			{
				nodes.Add(new PositionRotation(item));
			}
		}
	}

	[ProtoContract]
	[UIField("Cylinder", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	public class LaunchPatternShapeCylinder : LaunchPatternShape
	{
		private static readonly RangeF BASE_RADIUS = new RangeF(0.2f, 0.2f, 0.01f, 5f).Scale(0.1f);

		private static readonly RangeF HEIGHT = new RangeF(0.2f, 0.2f, 0.01f, 5f).Scale(0.1f);

		private static readonly RangeF TOP_RADIUS = new RangeF(0.2f, 0.2f, 0.01f, 5f).Scale(0.1f);

		[ProtoMember(1)]
		[UIField(stepSize = 0.001f, view = "UI/Reflection/Range Slider Advanced")]
		private RangeF _baseRadius = BASE_RADIUS;

		[ProtoMember(2)]
		[UIField(stepSize = 0.001f, view = "UI/Reflection/Range Slider Advanced")]
		private RangeF _height = HEIGHT;

		[ProtoMember(3)]
		[UIField(stepSize = 0.001f, view = "UI/Reflection/Range Slider Advanced")]
		private RangeF _topRadius = TOP_RADIUS;

		private bool _baseRadiusSpecified => _baseRadius != BASE_RADIUS;

		private bool _heightSpecified => _height != HEIGHT;

		private bool _topRadiusSpecified => _topRadius != TOP_RADIUS;

		protected override void _PopulateNodePositions(System.Random random, List<PositionRotation> nodes, int numberOfNodes)
		{
			float num = random.Range(_height);
			float num2 = num * 0.5f;
			float p = Mathf.Pow(Math.Max(MathUtil.RingArea(_baseRadius.min, _baseRadius.max), MathUtil.Circumference(_baseRadius.max)) / Math.Max(MathUtil.RingArea(_topRadius.min, _topRadius.max), MathUtil.Circumference(_topRadius.max)), 0.25f);
			for (int i = 0; i < numberOfNodes; i++)
			{
				float num3 = Mathf.Pow(random.Value(), p);
				float f = random.Range(0f, MathF.PI * 2f);
				Vector2 vector = new Vector2(Mathf.Lerp(_baseRadius.min, _topRadius.min, num3), Mathf.Lerp(_baseRadius.max, _topRadius.max, num3));
				float num4 = Mathf.Lerp(vector.x, vector.y, Mathf.Sqrt(random.Value()));
				nodes.Add(new PositionRotation(new Vector3(Mathf.Cos(f) * num4, num3 * num - num2, Mathf.Sin(f) * num4)));
			}
		}

		public override bool ExcludedUpDirection(ShapeUpDirectionType upDirection)
		{
			return false;
		}
	}

	[ProtoContract]
	[UIField]
	public class ProjectileBurstMedia<T> where T : struct, IConvertible
	{
		[ProtoContract]
		[UIField]
		public class BurstMedia
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Hide)]
			private ProjectileBurstVisual<T> _visual;

			[ProtoMember(2)]
			[UIField(validateOnChange = true)]
			private SimpleSoundPack _sound;

			[ProtoMember(3)]
			[UIField]
			[UIHideIf("_hideSoundOptions")]
			private ProjectileBurstSoundOptions _soundOptions;

			public ProjectileBurstVisual<T> visual => _visual ?? (_visual = new ProjectileBurstVisual<T>());

			public bool shouldPlaySound
			{
				get
				{
					if (_sound != null)
					{
						return _sound;
					}
					return false;
				}
			}

			protected SimpleSoundPack sound => _sound ?? (_sound = new SimpleSoundPack());

			protected ProjectileBurstSoundOptions soundOptions => _soundOptions ?? (_soundOptions = new ProjectileBurstSoundOptions());

			private bool _hideSoundOptions
			{
				get
				{
					if (_sound != null)
					{
						return !_sound;
					}
					return true;
				}
			}

			public void SetAudioCategory(AudioCategoryType audioCategory, AudioCategoryTypeFlags? additionalCategories = null)
			{
				sound.category = audioCategory;
				sound.additionalCategories = additionalCategories;
			}

			public bool PlaySound(System.Random random, Vector3 position, AudioMixerGroup group = null, PooledAudioCategory? category = null, float scaleLerp = 0.5f)
			{
				if (_sound == null)
				{
					return false;
				}
				return _sound.PlaySound(random, position, group, category, soundOptions.GetVolumeMultiplier(scaleLerp), soundOptions.GetPitchMultiplier(1f - scaleLerp));
			}

			public override string ToString()
			{
				return string.Format("Visual: {0}, Sound: {1}", visual, shouldPlaySound ? _sound.ToString() : "None");
			}

			private void OnValidateUI()
			{
				visual.lighting._hasSoundToSyncTo = sound;
			}
		}

		[ProtoMember(1, OverwriteList = true)]
		[UIField(collapse = UICollapseType.Open)]
		[UIFieldCollectionItem]
		private List<BurstMedia> _bursts;

		[ProtoMember(2)]
		[UIField(min = 0, max = 1)]
		[DefaultValue(1)]
		[UIHideIf("_hideCommon")]
		private float _playAmountRatio = 1f;

		[ProtoMember(3)]
		[UIField]
		[DefaultValue(true)]
		[UIHideIf("_hideSyncWithFlightMedia")]
		private bool _syncWithFlightMedia = true;

		[ProtoMember(15)]
		[DefaultValue(AudioCategoryType.ProjectileImpact)]
		private AudioCategoryType _audioCategory = AudioCategoryType.ProjectileImpact;

		public float playAmountRatio => _playAmountRatio;

		private bool _hideCommon => _bursts.IsNullOrEmpty();

		private bool _hideSyncWithFlightMedia
		{
			get
			{
				if (!_hideCommon)
				{
					return _bursts.Count == 1;
				}
				return true;
			}
		}

		public ProjectileBurstMedia(AudioCategoryType audioCategory)
		{
			_audioCategory = audioCategory;
		}

		public BurstMedia GetBurstMedia(System.Random random, int flightIndex, int flightMediaCount)
		{
			if (_bursts.IsNullOrEmpty())
			{
				return null;
			}
			if (!_syncWithFlightMedia || _bursts.Count != flightMediaCount)
			{
				return random.Item(_bursts);
			}
			return _bursts[flightIndex];
		}

		private void OnValidateUI()
		{
			if (_bursts == null)
			{
				return;
			}
			foreach (BurstMedia burst in _bursts)
			{
				burst.SetAudioCategory(_audioCategory, AudioCategoryTypeFlags.ProjectileLaunch | AudioCategoryTypeFlags.ProjectileImpact);
			}
		}
	}

	[ProtoContract]
	[UIField]
	public class ProjectileBurstVisual<T> where T : struct, IConvertible
	{
		[ProtoMember(1)]
		[UIField("Visual Prefab", 0u, null, null, null, null, null, null, false, null, 5, false, null, validateOnChange = true, collapse = UICollapseType.Hide)]
		private OptionalContent<CustomizedEnumContent<T>> _visual;

		[ProtoMember(6)]
		[UIField]
		[UIHideIf("_hideCommon")]
		private bool _inheritColor;

		[ProtoMember(5)]
		[UIField]
		[UIHideIf("_hideCommon")]
		[UIDeepValueChange]
		private BurstVisualTranslation _translation;

		[ProtoMember(2)]
		[UIField]
		[UIHideIf("_hideCommon")]
		[UIDeepValueChange]
		private BurstVisualRotation _rotation;

		[ProtoMember(3)]
		[UIField]
		[UIHideIf("_hideCommon")]
		[UIDeepValueChange]
		private BurstVisualScale _scale;

		[ProtoMember(4)]
		[UIField]
		[UIHideIf("_hideLighting")]
		private BurstVisualLighting _lighting;

		public T? sfx
		{
			get
			{
				if (!_visual.enabled)
				{
					return null;
				}
				return _visual.value;
			}
		}

		public BurstVisualTranslation translation => _translation ?? (_translation = new BurstVisualTranslation());

		public BurstVisualRotation rotation => _rotation ?? (_rotation = new BurstVisualRotation());

		public BurstVisualScale scale => _scale ?? (_scale = new BurstVisualScale());

		public BurstVisualLighting lighting => _lighting ?? (_lighting = new BurstVisualLighting());

		private bool _hideCommon => !_visual.enabled;

		private bool _hideLighting
		{
			get
			{
				if (!_hideCommon)
				{
					return !EnumUtil.GetResourceBlueprint((T)_visual.value).GetComponent<ProjectileBurstSFX>().mainLight;
				}
				return true;
			}
		}

		public void ApplyCustomization(GameObject gameObject, bool playedSound, bool playAmountShouldPlay, Transform attachTo, System.Random random, Color primaryColor, float emissionMultiplier)
		{
			_visual.value.ApplyTo(gameObject, random, _inheritColor ? new Color?(primaryColor) : null);
			ProjectileBurstSFX component = gameObject.GetComponent<ProjectileBurstSFX>();
			if (component.attachToTransform)
			{
				component.attachTo = attachTo;
			}
			component.SetEmissionRates(emissionMultiplier);
			Light mainLight = component.mainLight;
			if (!mainLight)
			{
				return;
			}
			if ((lighting.syncWithSound && !playedSound) || (!lighting.syncWithSound && !playAmountShouldPlay))
			{
				mainLight.enabled = false;
				return;
			}
			mainLight.enabled &= lighting.enabled;
			mainLight.color = lighting.colorOverride ?? ((Color32)primaryColor);
			if (lighting.inheritScale)
			{
				mainLight.range *= Mathf.Sqrt(gameObject.transform.localScale.Abs().Average());
			}
		}

		public override string ToString()
		{
			if (!sfx.HasValue)
			{
				return "None";
			}
			return EnumUtil.FriendlyName(sfx.Value);
		}
	}

	[ProtoContract]
	[UIField]
	public class BurstVisualTranslation
	{
		[ProtoContract(EnumPassthru = true)]
		public enum Origin
		{
			Projectile,
			CardTarget
		}

		[ProtoContract(EnumPassthru = true)]
		public enum Space
		{
			Card,
			World,
			Projectile
		}

		[ProtoMember(1)]
		[UIField]
		[UIHorizontalLayout("T")]
		private Origin _origin;

		[ProtoMember(2)]
		[UIField("Offset Space", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		[UIHorizontalLayout("T")]
		private Space _space;

		[ProtoMember(3)]
		[UIField]
		[UIDeepValueChange]
		private OffsetRanges _offsets;

		public Origin origin => _origin;

		public Space space => _space;

		public OffsetRanges offsets => _offsets ?? (_offsets = new OffsetRanges());

		public Vector3 GetPosition(System.Random random, Transform projectile, Transform target)
		{
			Vector3 vector = origin switch
			{
				Origin.Projectile => projectile.position, 
				Origin.CardTarget => target.position, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			Vector3 vector2 = offsets.GetOffset(random);
			switch (space)
			{
			case Space.Card:
				vector2 = target.rotation * vector2;
				break;
			case Space.Projectile:
				vector2 = projectile.rotation * vector2;
				break;
			}
			return vector + vector2;
		}

		public override string ToString()
		{
			return "<size=66%>Origin:</size> " + EnumUtil.FriendlyName(origin) + StringUtil.ToText(_offsets, $", <size=66%>Offset</size> {EnumUtil.FriendlyName(space)}:{_offsets}");
		}
	}

	[ProtoContract]
	[UIField]
	public class BurstVisualRotation
	{
		private static readonly Quaternion Y_FORWARD = Quaternion.FromToRotation(Vector3.up, Vector3.forward);

		private static readonly Quaternion X_FORWARD = Quaternion.FromToRotation(Vector3.right, Vector3.forward);

		[ProtoMember(1)]
		[UIField(tooltip = "Determines the <u>Forward</u> direction of the burst visual's transform.")]
		[DefaultValue(ProjectileBurstVisualOrientType.Shape)]
		[UIHorizontalLayout("A", flexibleWidth = 999f, preferredWidth = 1f)]
		private ProjectileBurstVisualOrientType _orientTo = ProjectileBurstVisualOrientType.Shape;

		[ProtoMember(2)]
		[UIField]
		[UIHorizontalLayout("A", flexibleWidth = 0f, preferredWidth = 1f, minWidth = 224f)]
		private bool _flipOrientTo;

		[ProtoMember(3)]
		[UIField]
		[UIDeepValueChange]
		private RotationRanges _orientationOffset;

		[ProtoMember(4)]
		[UIField("Project Orientation", 0u, null, null, null, null, null, null, false, null, 5, false, null, validateOnChange = true)]
		[UIHorizontalLayout("Project", preferredWidth = 1f, flexibleWidth = 3.1f)]
		private ProjectileBurstVisualOrientType? _projectOrientationOnto;

		[ProtoMember(5)]
		[UIField("Plane", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		[UIHideIf("_hideOntoAxes")]
		[UIHorizontalLayout("Project", preferredWidth = 1f, flexibleWidth = 1f)]
		[DefaultValue(PlaneAxes.XZ)]
		private PlaneAxes _ontoAxes = PlaneAxes.XZ;

		[ProtoMember(6)]
		[UIField("Forward Axis", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Axis of the Burst Visual that will be aligned with the forward vector of \"Orient To\"")]
		[UIHideIf("_hideOntoAxes")]
		[DefaultValue(AxisType.Y)]
		private AxisType _forward = AxisType.Y;

		public ProjectileBurstVisualOrientType orientTo => _orientTo;

		public RotationRanges orientationOffset => _orientationOffset ?? (_orientationOffset = new RotationRanges());

		private bool _hideOntoAxes => !_projectOrientationOnto.HasValue;

		private bool _orientationOffsetSpecified => _orientationOffset;

		private Quaternion _GetRotation(ProjectileBurstVisualOrientType orient, Transform projectile, PositionRotation shape, PositionRotation target, Vector3 velocity)
		{
			switch (orient)
			{
			case ProjectileBurstVisualOrientType.Projectile:
				return projectile.rotation;
			case ProjectileBurstVisualOrientType.Velocity:
				if (!(velocity != Vector3.zero))
				{
					return projectile.rotation;
				}
				return Quaternion.LookRotation(velocity, projectile.up);
			case ProjectileBurstVisualOrientType.Shape:
				return shape.rotation;
			case ProjectileBurstVisualOrientType.ToTarget:
			{
				Vector3 vector2 = target.position - projectile.position;
				if (!(vector2 != Vector3.zero))
				{
					return projectile.rotation;
				}
				return Quaternion.LookRotation(vector2, projectile.up);
			}
			case ProjectileBurstVisualOrientType.Camera:
			{
				Vector3 vector = CameraManager.Instance.mainCamera.transform.position - projectile.position;
				if (!(vector != Vector3.zero))
				{
					return projectile.rotation;
				}
				return Quaternion.LookRotation(vector, CameraManager.Instance.mainCamera.transform.up);
			}
			case ProjectileBurstVisualOrientType.Card:
				return target.rotation;
			default:
				return Quaternion.identity;
			}
		}

		public Quaternion GetRotation(System.Random random, Transform projectile, PositionRotation shape, PositionRotation target, Vector3 velocity)
		{
			Quaternion quaternion = _GetRotation(_orientTo, projectile, shape, target, velocity);
			if (_projectOrientationOnto.HasValue)
			{
				Vector3 axis = _GetRotation(_projectOrientationOnto.Value, projectile, shape, target, velocity).GetAxis(_ontoAxes.NormalAxis());
				Plane plane = new Plane(axis, Vector3.zero);
				Vector3 vector = quaternion.Forward();
				float num = Vector3.Angle(vector, plane.ClosestPointOnPlane(vector).NormalizeSafe(vector));
				quaternion *= Quaternion.AngleAxis(0f - num, Vector3.right);
				Vector3 vector2 = quaternion.Right();
				num = Vector3.Angle(vector2, plane.ClosestPointOnPlane(vector2).NormalizeSafe(vector2));
				quaternion *= Quaternion.AngleAxis(num, Vector3.forward);
				switch (_forward)
				{
				case AxisType.Y:
					quaternion *= Y_FORWARD;
					break;
				case AxisType.X:
					quaternion *= X_FORWARD;
					break;
				}
			}
			return (quaternion * orientationOffset.GetRotation(random)).Opposite(_flipOrientTo);
		}

		public override string ToString()
		{
			return "<size=66%>Orient To:</size> " + EnumUtil.FriendlyName(orientTo) + _flipOrientTo.ToText(" (Flipped)") + StringUtil.ToText(_orientationOffset, $", <size=66%>Rotation:</size> {_orientationOffset}") + _projectOrientationOnto.HasValue.ToText(", <size=66%>Project On:</size> " + EnumUtil.FriendlyName(_projectOrientationOnto) + " " + EnumUtil.FriendlyName(_ontoAxes) + "<size=66%> Plane</size>, <size=66%>Forward Axis:</size> " + EnumUtil.FriendlyName(_forward));
		}
	}

	[ProtoContract]
	[UIField]
	public class BurstVisualScale
	{
		[ProtoMember(1)]
		[UIField]
		[DefaultValue(true)]
		private bool _inheritScale = true;

		[ProtoMember(2)]
		[UIField]
		private RangeF _scale = MULTIPLIER;

		private bool _scaleSpecified => _scale != MULTIPLIER;

		public Vector3 GetScale(System.Random random, Transform projectile)
		{
			return (random.Range(_scale) * (_inheritScale ? projectile.localScale.Average() : 1f)).ToVector3();
		}

		public override string ToString()
		{
			return _inheritScale.ToText("<size=66%>(Inherit)</size> ") + _scale.ToPercentStringShort().Trim();
		}
	}

	[ProtoContract]
	[UIField]
	public class BurstVisualLighting
	{
		[ProtoMember(1)]
		[UIField(validateOnChange = true)]
		[DefaultValue(true)]
		private bool _enabled = true;

		[ProtoMember(2)]
		[UIField(validateOnChange = true)]
		[DefaultValue(true)]
		[UIHideIf("_hideSyncWithSound")]
		private bool _syncWithSound = true;

		[ProtoMember(3)]
		[UIField(min = 0, max = 1)]
		[DefaultValue(1)]
		[UIHideIf("_hidePlayAmountRatio")]
		private float _playAmountRatio = 1f;

		[ProtoMember(4)]
		[UIField]
		[DefaultValue(true)]
		[UIHideIf("_hideCommon")]
		private bool _inheritScale = true;

		[ProtoMember(5)]
		[UIField]
		[UIHideIf("_hideCommon")]
		private Color32? _colorOverride;

		[ProtoMember(6)]
		public bool _hasSoundToSyncTo;

		public bool enabled => _enabled;

		public Color32? colorOverride => _colorOverride;

		public float playAmountRatio => _playAmountRatio;

		public bool inheritScale => _inheritScale;

		public bool syncWithSound
		{
			get
			{
				if (_syncWithSound)
				{
					return _hasSoundToSyncTo;
				}
				return false;
			}
		}

		private bool _hideCommon => !_enabled;

		private bool _hideSyncWithSound
		{
			get
			{
				if (!_hideCommon)
				{
					return !_hasSoundToSyncTo;
				}
				return true;
			}
		}

		private bool _hidePlayAmountRatio
		{
			get
			{
				if (!_hideCommon)
				{
					return syncWithSound;
				}
				return true;
			}
		}
	}

	[ProtoContract]
	[UIField]
	public class ProjectileBurstSoundOptions
	{
		private static readonly RangeF VOLUME = new RangeF(0.5f);

		private static readonly RangeF PITCH = new RangeF(0.5f, 2f, 0.25f, 4f);

		[ProtoMember(1)]
		[UIField(validateOnChange = true)]
		private bool _projectileScaleEffectsVolume;

		[ProtoMember(2)]
		[UIField]
		[UIHideIf("_hideVolume")]
		private RangeF _volumeMultiplier = VOLUME;

		[ProtoMember(3)]
		[UIField(validateOnChange = true)]
		private bool _projectileScaleEffectsPitch;

		[ProtoMember(4)]
		[UIField]
		[UIHideIf("_hidePitch")]
		private RangeF _pitchMultiplier = PITCH;

		private bool _volumeMultiplierSpecified => _volumeMultiplier != VOLUME;

		private bool _pitchMultiplierSpecified => _pitchMultiplier != PITCH;

		private bool _hideVolume => !_projectileScaleEffectsVolume;

		private bool _hidePitch => !_projectileScaleEffectsPitch;

		public float GetVolumeMultiplier(float scaleLerp)
		{
			if (!_projectileScaleEffectsVolume)
			{
				return 1f;
			}
			return _volumeMultiplier.Lerp(scaleLerp);
		}

		public float GetPitchMultiplier(float scaleLerp)
		{
			if (!_projectileScaleEffectsPitch)
			{
				return 1f;
			}
			return _pitchMultiplier.Lerp(scaleLerp);
		}
	}

	public const float SCALE = 0.1f;

	public const float REFERENCE_SPEED = 1f;

	public const float REFERENCE_SPEED_MAX = 3f;

	public const float GRAVITY = 9.81f;

	public const float GRAVITY_MAX = 20f;

	private const float SHAPE_SIZE = 0.2f;

	private const float SHAPE_SIZE_MAX = 5f;

	private const string DEFAULT_NAME = "Unnamed Projectile Media";

	private const string RANGE_TEXT = "(Min / Max)";

	public static readonly RangeF MULTIPLIER;

	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Main")]
	private ProjectileMain _main;

	[ProtoMember(2)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Launch")]
	private ProjectileLaunch _launch;

	[ProtoMember(3)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Flight")]
	private ProjectileFlight _flight;

	[ProtoMember(4)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Impact")]
	private ProjectileImpact _impact;

	[ProtoMember(5)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("After Impact")]
	private ProjectileAfterImpact _afterImpact;

	[ProtoMember(15)]
	private string _tags;

	public string name => main.name;

	public ProjectileMain main => _main ?? (_main = new ProjectileMain());

	public ProjectileLaunch launch => _launch ?? (_launch = new ProjectileLaunch());

	public ProjectileFlight flight => _flight ?? (_flight = new ProjectileFlight());

	public ProjectileImpact impact => _impact ?? (_impact = new ProjectileImpact());

	public ProjectileAfterImpact afterImpact => _afterImpact ?? (_afterImpact = new ProjectileAfterImpact());

	public string tags
	{
		get
		{
			return _tags;
		}
		set
		{
			_tags = value;
		}
	}

	static ProjectileMediaData()
	{
		MULTIPLIER = new RangeF(1f, 1f, 0.01f, 10f);
		Pools.CreatePool<ProjectileFlight.ProjectileFlightParameters.FlightModifier>(createHierarchy: true);
		Pools.CreatePool<ProjectileLaunchPatternNodes>();
	}

	private float _GetAverageProjectileLifetime(float distance)
	{
		float num = 0f;
		foreach (ProjectileFlight.ProjectileFlightParameters parameter in flight.parameters)
		{
			num += parameter.GetAverageFlightTime(distance);
		}
		num /= (float)flight.parameters.Count;
		float num2 = 0f;
		foreach (ProjectileAfterImpactParameters parameter2 in afterImpact.parameters)
		{
			num2 += parameter2.lifetime.Average() + parameter2.fadeTime;
		}
		num2 /= (float)afterImpact.parameters.Count;
		return num + num2;
	}

	private float _GetAverageProjectilesLaunchedPerSecond()
	{
		float num = launch.parameters.projectilesPerBurst.Average();
		return Mathf.Min(num * launch.parameters.burstCount.Average(), num / launch.parameters.timeBetweenBursts.Average());
	}

	public float GetAverageNumberOfLivingProjectiles(float distance)
	{
		return Mathf.Min(launch.parameters.projectilesPerBurst.Average() * launch.parameters.burstCount.Average(), _GetAverageProjectilesLaunchedPerSecond() * Math.Max(0.1f, _GetAverageProjectileLifetime(distance)));
	}

	public string GetTitle()
	{
		return name;
	}

	public string GetAutomatedDescription()
	{
		return "";
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public Texture2D GetPreviewImage()
	{
		return null;
	}

	public void PrepareDataForSave()
	{
	}

	public string GetSaveErrorMessage()
	{
		if (name.IsNullOrEmpty() || name == "Unnamed Projectile Media")
		{
			return "Projectile Media has not been given a name, please enter a name before saving.";
		}
		return null;
	}

	public void OnLoadValidation()
	{
	}
}
