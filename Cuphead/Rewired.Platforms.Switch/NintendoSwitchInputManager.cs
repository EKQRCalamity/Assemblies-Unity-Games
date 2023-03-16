using System;
using System.Collections.Generic;
using Rewired.Data;
using Rewired.Utils.Interfaces;
using UnityEngine;

namespace Rewired.Platforms.Switch;

[AddComponentMenu("Rewired/Nintendo Switch Input Manager")]
[RequireComponent(typeof(InputManager))]
public sealed class NintendoSwitchInputManager : MonoBehaviour, IExternalInputManager
{
	[Serializable]
	private class UserData : IKeyedData<int>
	{
		[SerializeField]
		private int _allowedNpadStyles = -1;

		[SerializeField]
		private int _joyConGripStyle = 1;

		[SerializeField]
		private bool _adjustIMUsForGripStyle = true;

		[SerializeField]
		private int _handheldActivationMode;

		[SerializeField]
		private bool _assignJoysticksByNpadId = true;

		[SerializeField]
		private NpadSettings_Internal _npadNo1 = new NpadSettings_Internal(0);

		[SerializeField]
		private NpadSettings_Internal _npadNo2 = new NpadSettings_Internal(1);

		[SerializeField]
		private NpadSettings_Internal _npadNo3 = new NpadSettings_Internal(2);

		[SerializeField]
		private NpadSettings_Internal _npadNo4 = new NpadSettings_Internal(3);

		[SerializeField]
		private NpadSettings_Internal _npadNo5 = new NpadSettings_Internal(4);

		[SerializeField]
		private NpadSettings_Internal _npadNo6 = new NpadSettings_Internal(5);

		[SerializeField]
		private NpadSettings_Internal _npadNo7 = new NpadSettings_Internal(6);

		[SerializeField]
		private NpadSettings_Internal _npadNo8 = new NpadSettings_Internal(7);

		[SerializeField]
		private NpadSettings_Internal _npadHandheld = new NpadSettings_Internal(0);

		[SerializeField]
		private DebugPadSettings_Internal _debugPad = new DebugPadSettings_Internal(0);

		private Dictionary<int, object[]> __delegates;

		public int allowedNpadStyles
		{
			get
			{
				return _allowedNpadStyles;
			}
			set
			{
				_allowedNpadStyles = value;
			}
		}

		public int joyConGripStyle
		{
			get
			{
				return _joyConGripStyle;
			}
			set
			{
				_joyConGripStyle = value;
			}
		}

		public bool adjustIMUsForGripStyle
		{
			get
			{
				return _adjustIMUsForGripStyle;
			}
			set
			{
				_adjustIMUsForGripStyle = value;
			}
		}

		public int handheldActivationMode
		{
			get
			{
				return _handheldActivationMode;
			}
			set
			{
				_handheldActivationMode = value;
			}
		}

		public bool assignJoysticksByNpadId
		{
			get
			{
				return _assignJoysticksByNpadId;
			}
			set
			{
				_assignJoysticksByNpadId = value;
			}
		}

		private NpadSettings_Internal npadNo1 => _npadNo1;

		private NpadSettings_Internal npadNo2 => _npadNo2;

		private NpadSettings_Internal npadNo3 => _npadNo3;

		private NpadSettings_Internal npadNo4 => _npadNo4;

		private NpadSettings_Internal npadNo5 => _npadNo5;

		private NpadSettings_Internal npadNo6 => _npadNo6;

		private NpadSettings_Internal npadNo7 => _npadNo7;

		private NpadSettings_Internal npadNo8 => _npadNo8;

		private NpadSettings_Internal npadHandheld => _npadHandheld;

		public DebugPadSettings_Internal debugPad => _debugPad;

		private unsafe Dictionary<int, object[]> delegates
		{
			get
			{
				if (__delegates != null)
				{
					return __delegates;
				}
				return __delegates = new Dictionary<int, object[]>
				{
					{
						0,
						new object[2]
						{
							new Func<int>(this, (nint)__ldftn(UserData.get_allowedNpadStyles)),
							(Action<int>)delegate(int x)
							{
								allowedNpadStyles = x;
							}
						}
					},
					{
						1,
						new object[2]
						{
							new Func<int>(this, (nint)__ldftn(UserData.get_joyConGripStyle)),
							(Action<int>)delegate(int x)
							{
								joyConGripStyle = x;
							}
						}
					},
					{
						2,
						new object[2]
						{
							new Func<bool>(this, (nint)__ldftn(UserData.get_adjustIMUsForGripStyle)),
							(Action<bool>)delegate(bool x)
							{
								adjustIMUsForGripStyle = x;
							}
						}
					},
					{
						3,
						new object[2]
						{
							new Func<int>(this, (nint)__ldftn(UserData.get_handheldActivationMode)),
							(Action<int>)delegate(int x)
							{
								handheldActivationMode = x;
							}
						}
					},
					{
						4,
						new object[2]
						{
							new Func<bool>(this, (nint)__ldftn(UserData.get_assignJoysticksByNpadId)),
							(Action<bool>)delegate(bool x)
							{
								assignJoysticksByNpadId = x;
							}
						}
					},
					{
						5,
						new object[2]
						{
							new Func<object>(this, (nint)__ldftn(UserData.get_npadNo1)),
							null
						}
					},
					{
						6,
						new object[2]
						{
							new Func<object>(this, (nint)__ldftn(UserData.get_npadNo2)),
							null
						}
					},
					{
						7,
						new object[2]
						{
							new Func<object>(this, (nint)__ldftn(UserData.get_npadNo3)),
							null
						}
					},
					{
						8,
						new object[2]
						{
							new Func<object>(this, (nint)__ldftn(UserData.get_npadNo4)),
							null
						}
					},
					{
						9,
						new object[2]
						{
							new Func<object>(this, (nint)__ldftn(UserData.get_npadNo5)),
							null
						}
					},
					{
						10,
						new object[2]
						{
							new Func<object>(this, (nint)__ldftn(UserData.get_npadNo6)),
							null
						}
					},
					{
						11,
						new object[2]
						{
							new Func<object>(this, (nint)__ldftn(UserData.get_npadNo7)),
							null
						}
					},
					{
						12,
						new object[2]
						{
							new Func<object>(this, (nint)__ldftn(UserData.get_npadNo8)),
							null
						}
					},
					{
						13,
						new object[2]
						{
							new Func<object>(this, (nint)__ldftn(UserData.get_npadHandheld)),
							null
						}
					},
					{
						14,
						new object[2]
						{
							new Func<object>(this, (nint)__ldftn(UserData.get_debugPad)),
							null
						}
					}
				};
			}
		}

		bool IKeyedData<int>.TryGetValue<T>(int key, out T value)
		{
			if (!delegates.TryGetValue(key, out var value2))
			{
				value = default(T);
				return false;
			}
			if (!(value2[0] is Func<T> func))
			{
				value = default(T);
				return false;
			}
			value = func();
			return true;
		}

		bool IKeyedData<int>.TrySetValue<T>(int key, T value)
		{
			if (!delegates.TryGetValue(key, out var value2))
			{
				return false;
			}
			if (!(value2[1] is Action<T> action))
			{
				return false;
			}
			action(value);
			return true;
		}
	}

	[Serializable]
	private sealed class NpadSettings_Internal : IKeyedData<int>
	{
		[Tooltip("Determines whether this Npad id is allowed to be used by the system.")]
		[SerializeField]
		private bool _isAllowed = true;

		[Tooltip("The Rewired Player Id assigned to this Npad id.")]
		[SerializeField]
		private int _rewiredPlayerId;

		[Tooltip("Determines how Joy-Cons should be handled.\n\nUnmodified: Joy-Con assignment mode will be left at the system default.\nDual: Joy-Cons pairs are handled as a single controller.\nSingle: Joy-Cons are handled as individual controllers.")]
		[SerializeField]
		private int _joyConAssignmentMode = -1;

		private Dictionary<int, object[]> __delegates;

		private bool isAllowed
		{
			get
			{
				return _isAllowed;
			}
			set
			{
				_isAllowed = value;
			}
		}

		private int rewiredPlayerId
		{
			get
			{
				return _rewiredPlayerId;
			}
			set
			{
				_rewiredPlayerId = value;
			}
		}

		private int joyConAssignmentMode
		{
			get
			{
				return _joyConAssignmentMode;
			}
			set
			{
				_joyConAssignmentMode = value;
			}
		}

		private unsafe Dictionary<int, object[]> delegates
		{
			get
			{
				if (__delegates != null)
				{
					return __delegates;
				}
				return __delegates = new Dictionary<int, object[]>
				{
					{
						0,
						new object[2]
						{
							new Func<bool>(this, (nint)__ldftn(NpadSettings_Internal.get_isAllowed)),
							(Action<bool>)delegate(bool x)
							{
								isAllowed = x;
							}
						}
					},
					{
						1,
						new object[2]
						{
							new Func<int>(this, (nint)__ldftn(NpadSettings_Internal.get_rewiredPlayerId)),
							(Action<int>)delegate(int x)
							{
								rewiredPlayerId = x;
							}
						}
					},
					{
						2,
						new object[2]
						{
							new Func<int>(this, (nint)__ldftn(NpadSettings_Internal.get_joyConAssignmentMode)),
							(Action<int>)delegate(int x)
							{
								joyConAssignmentMode = x;
							}
						}
					}
				};
			}
		}

		internal NpadSettings_Internal(int playerId)
		{
			_rewiredPlayerId = playerId;
		}

		bool IKeyedData<int>.TryGetValue<T>(int key, out T value)
		{
			if (!delegates.TryGetValue(key, out var value2))
			{
				value = default(T);
				return false;
			}
			if (!(value2[0] is Func<T> func))
			{
				value = default(T);
				return false;
			}
			value = func();
			return true;
		}

		bool IKeyedData<int>.TrySetValue<T>(int key, T value)
		{
			if (!delegates.TryGetValue(key, out var value2))
			{
				return false;
			}
			if (!(value2[1] is Action<T> action))
			{
				return false;
			}
			action(value);
			return true;
		}
	}

	[Serializable]
	private sealed class DebugPadSettings_Internal : IKeyedData<int>
	{
		[Tooltip("Determines whether the Debug Pad will be enabled.")]
		[SerializeField]
		private bool _enabled;

		[Tooltip("The Rewired Player Id to which the Debug Pad will be assigned.")]
		[SerializeField]
		private int _rewiredPlayerId;

		private Dictionary<int, object[]> __delegates;

		private int rewiredPlayerId
		{
			get
			{
				return _rewiredPlayerId;
			}
			set
			{
				_rewiredPlayerId = value;
			}
		}

		private bool enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		private unsafe Dictionary<int, object[]> delegates
		{
			get
			{
				if (__delegates != null)
				{
					return __delegates;
				}
				return __delegates = new Dictionary<int, object[]>
				{
					{
						0,
						new object[2]
						{
							new Func<bool>(this, (nint)__ldftn(DebugPadSettings_Internal.get_enabled)),
							(Action<bool>)delegate(bool x)
							{
								enabled = x;
							}
						}
					},
					{
						1,
						new object[2]
						{
							new Func<int>(this, (nint)__ldftn(DebugPadSettings_Internal.get_rewiredPlayerId)),
							(Action<int>)delegate(int x)
							{
								rewiredPlayerId = x;
							}
						}
					}
				};
			}
		}

		internal DebugPadSettings_Internal(int playerId)
		{
			_rewiredPlayerId = playerId;
		}

		bool IKeyedData<int>.TryGetValue<T>(int key, out T value)
		{
			if (!delegates.TryGetValue(key, out var value2))
			{
				value = default(T);
				return false;
			}
			if (!(value2[0] is Func<T> func))
			{
				value = default(T);
				return false;
			}
			value = func();
			return true;
		}

		bool IKeyedData<int>.TrySetValue<T>(int key, T value)
		{
			if (!delegates.TryGetValue(key, out var value2))
			{
				return false;
			}
			if (!(value2[1] is Action<T> action))
			{
				return false;
			}
			action(value);
			return true;
		}
	}

	[SerializeField]
	private UserData _userData = new UserData();

	object IExternalInputManager.Initialize(Platform platform, ConfigVars configVars)
	{
		return null;
	}

	void IExternalInputManager.Deinitialize()
	{
	}
}
