using System;
using System.ComponentModel;
using ProtoBuf;
using TMPro;

[ProtoContract]
[UIField]
public class SaveProfile
{
	[ProtoMember(1)]
	[DefaultValue("Default")]
	private string _name = "Default";

	[ProtoMember(2)]
	[UIField("Log In Lock", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 3u, tooltip = "Set up a security question which must be answered in order to switch to this profile.")]
	[UIHideIf("_hidePassword")]
	[UIDeepValueChange]
	[UIHorizontalLayout("Lock")]
	private ParentalLock _logInPassword;

	[ProtoMember(3)]
	[UIField("Log Out Lock", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 4u, tooltip = "Set up a security question which must be answered in order to switch off this profile.\n<i>Useful for kid profiles to prevent access to all other profiles or creating new profiles.</i>")]
	[UIHideIf("_hidePassword")]
	[UIDeepValueChange]
	[UIHorizontalLayout("Lock")]
	private ParentalLock _logOutPassword;

	[ProtoMember(4)]
	private long _lastPlayed;

	public string name
	{
		get
		{
			if (!_name.HasVisibleCharacter())
			{
				return "Default";
			}
			return _name;
		}
	}

	public ParentalLock logInPassword => _logInPassword;

	public ParentalLock logOutPassword => _logOutPassword;

	public long lastPlayed
	{
		get
		{
			if (!isActiveProfile)
			{
				return _lastPlayed;
			}
			return long.MaxValue;
		}
		set
		{
			_lastPlayed = value;
		}
	}

	public bool isActiveProfile => StringComparer.OrdinalIgnoreCase.Equals(name, ProfileManager.Profile.name);

	private bool _hidePassword => !isActiveProfile;

	private bool _hideButton => isActiveProfile;

	private bool _hideDelete
	{
		get
		{
			if (!_hideButton)
			{
				return StringComparer.OrdinalIgnoreCase.Equals(name, "Default");
			}
			return true;
		}
	}

	private bool _logInPasswordSpecified => _logInPassword;

	private bool _logOutPasswordSpecified => _logOutPassword;

	public SaveProfile()
	{
	}

	public SaveProfile(string name)
	{
		_name = name;
	}

	private void _DoFullPasswordLogic(Action onContinue)
	{
		DoPasswordLogic(SaveProfileData.Data.activeProfile.logOutPassword, delegate
		{
			DoPasswordLogic(SaveProfileData.Data[name].logInPassword, onContinue);
		});
	}

	public void DoPasswordLogic(ParentalLock password, Action onContinue)
	{
		if ((bool)password)
		{
			Action<ParentalLock.QuestionAnswerPair> onFinish = null;
			onFinish = delegate(ParentalLock.QuestionAnswerPair p)
			{
				ParentalLock.QuestionAnswerPair.OnFinish -= onFinish;
				if (password.questionAnswerPair.Equals(p))
				{
					onContinue();
				}
			};
			ParentalLock.QuestionAnswerPair.OnFinish += onFinish;
			ProtoUtil.Clone(password).DisableParentalLock();
		}
		else
		{
			onContinue();
		}
	}

	public override string ToString()
	{
		return string.Format("{0}Profile: <b>{1}</b>{2}", isActiveProfile ? "<b>Active</b> " : "", name, (lastPlayed != 0L && !isActiveProfile) ? $"<line-height=.0001><align=right><size=75%>\n{DateTime.FromFileTimeUtc(lastPlayed).ToLocalTime():f}" : "");
	}

	[UIField]
	[UIHorizontalLayout("Button")]
	[UIHideIf("_hideDelete")]
	private void _DeleteProfile()
	{
		_DoFullPasswordLogic(delegate
		{
			string delete = "Delete " + name + " Profile";
			UIUtil.CreatePopup(delete, UIUtil.CreateMessageBox("Are you certain you wish to <b>" + delete + "</b>? All user generated content associated with the profile will be deleted as well. This process is not reversible.", TextAlignmentOptions.Left, 32, 600, 300, 24f), null, parent: UIGeneratorType.GetActiveUIForType<ProfileOptions>().transform, buttons: new string[2] { "Cancel", delete }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
			{
				if (!(s != delete))
				{
					SaveProfileData.DeleteProfile(name);
					UIGeneratorType.ValidateAllOfType<ProfileOptions>();
				}
			});
		});
	}

	[UIField(dynamicInitMethod = "_InitSwitchToProfile")]
	[UIHorizontalLayout("Button")]
	[UIHideIf("_hideButton")]
	private void _SwitchToProfile()
	{
		_DoFullPasswordLogic(delegate
		{
			string switchToProfile = "Switch To " + name + " Profile";
			UIUtil.CreatePopup(switchToProfile, UIUtil.CreateMessageBox("Are you certain you wish to <b>" + switchToProfile + "</b>? This will effectively Save & Exit the current profile.", TextAlignmentOptions.Left, 32, 600, 300, 24f), null, parent: UIGeneratorType.GetActiveUIForType<ProfileOptions>().transform, buttons: new string[2] { "Cancel", switchToProfile }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
			{
				if (s == switchToProfile)
				{
					SaveProfileData.SwitchToProfile(name);
				}
			});
		});
	}
}
