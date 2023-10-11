using System;
using ProtoBuf;
using TMPro;
using UnityEngine;

[ProtoContract]
[UIField]
public class ParentalLock
{
	[ProtoContract]
	[UIField]
	public class QuestionAnswerPair : IEquatable<QuestionAnswerPair>
	{
		public static bool IsDisable;

		[ProtoMember(1)]
		[UIField(order = 1u, max = 256, view = "UI/Input Field Multiline", tooltip = "Question to ask when attempting to disable parental lock.", collapse = UICollapseType.Open, dynamicInitMethod = "_InitQuestion")]
		private string _question;

		[ProtoMember(2)]
		[UIField(order = 2u, max = 64, tooltip = "Answer to question above required to disable parental lock.")]
		private string _answer;

		public string question
		{
			get
			{
				return _question;
			}
			set
			{
				_question = value;
			}
		}

		public string answer
		{
			get
			{
				return _answer;
			}
			set
			{
				_answer = value;
			}
		}

		public static event Action<QuestionAnswerPair> OnFinish;

		public QuestionAnswerPair ClearAnswer()
		{
			answer = "";
			return this;
		}

		public static implicit operator bool(QuestionAnswerPair qaPair)
		{
			if (qaPair != null && qaPair.question.HasVisibleCharacter())
			{
				return qaPair.answer.HasVisibleCharacter();
			}
			return false;
		}

		public bool Equals(QuestionAnswerPair other)
		{
			if (other != null && StringComparer.OrdinalIgnoreCase.Equals(question, other.question))
			{
				return StringComparer.OrdinalIgnoreCase.Equals(answer, other.answer);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is QuestionAnswerPair)
			{
				return Equals((QuestionAnswerPair)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return StringComparer.OrdinalIgnoreCase.GetHashCode(question) ^ (StringComparer.OrdinalIgnoreCase.GetHashCode(answer).GetHashCode() << 16);
		}

		private void _InitQuestion(UIFieldAttribute uiField)
		{
			uiField.readOnly = IsDisable;
		}

		[UIField(order = 4u)]
		[UIHorizontalLayout("Buttons")]
		private void _Confirm()
		{
			if (!this)
			{
				string title = (IsDisable ? "Invalid Answer" : "Invalid Question & Answer");
				GameObject mainContent = UIUtil.CreateMessageBox(IsDisable ? "Please make sure to fill out answer before confirming." : "Please make sure to fill out question and answer fields before confirming.", TextAlignmentOptions.Left, 32, 600, 300, 24f);
				Transform transform = UIGeneratorType.GetActiveUIForType<ProfileOptions>().transform;
				UIUtil.CreatePopup(title, mainContent, null, null, null, null, null, null, true, true, null, null, null, transform, null, null);
			}
			else
			{
				QuestionAnswerPair.OnFinish?.Invoke(this);
			}
		}

		[UIField(order = 3u)]
		[UIHorizontalLayout("Buttons")]
		private void _Cancel()
		{
			QuestionAnswerPair.OnFinish?.Invoke(null);
		}
	}

	[ProtoMember(1)]
	private QuestionAnswerPair _questionAnswerPair;

	public QuestionAnswerPair questionAnswerPair
	{
		get
		{
			return _questionAnswerPair ?? (_questionAnswerPair = new QuestionAnswerPair());
		}
		set
		{
			_questionAnswerPair = value;
		}
	}

	private bool _hideSetParentalLock => this;

	private bool _hideDisableParentalLock => !this;

	public static implicit operator bool(ParentalLock parentalLock)
	{
		if (parentalLock != null)
		{
			return parentalLock._questionAnswerPair;
		}
		return false;
	}

	public override string ToString()
	{
		if (!this)
		{
			return "Disabled";
		}
		return "Enabled";
	}

	[UIField]
	[UIHideIf("_hideSetParentalLock")]
	private void _SetParentalLock()
	{
		QuestionAnswerPair.IsDisable = false;
		Action<QuestionAnswerPair> onFinish = null;
		GameObject popup = UIUtil.CreatePopup("Set Parental Lock", UIUtil.CreateReflectedObject(ProtoUtil.Clone(questionAnswerPair), 1280f, 230f), null, parent: UIGeneratorType.GetActiveUIForType<ProfileOptions>().transform, size: null, centerReferece: null, center: null, pivot: null, onClose: delegate
		{
			QuestionAnswerPair.OnFinish -= onFinish;
		}, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: null, rayCastBlockerColor: null, delayClose: null, referenceResolution: null, buttons: Array.Empty<string>());
		onFinish = delegate(QuestionAnswerPair qaPair)
		{
			if (qaPair != null)
			{
				UIUtil.CreatePopup("Confirm Set Parental Lock", UIUtil.CreateMessageBox("Set <b>Parental Lock</b> with the following settings:\n<b>Question:</b> <i>" + qaPair.question + "</i>\n<b>Answer:</b> <i>" + qaPair.answer + "</i>", TextAlignmentOptions.Left, 32, 600, 300, 24f), null, parent: popup.transform, buttons: new string[2] { "Set Parental Lock", "Cancel" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
				{
					if (!(s != "Set Parental Lock"))
					{
						questionAnswerPair = qaPair;
						UIGeneratorType.ValidateAllOfType<ProfileOptions>();
					}
				});
			}
			popup.GetComponentInChildren<UIPopupControl>().Close();
		};
		QuestionAnswerPair.OnFinish += onFinish;
	}

	[UIField]
	[UIHideIf("_hideDisableParentalLock")]
	public void DisableParentalLock()
	{
		QuestionAnswerPair.IsDisable = true;
		Action<QuestionAnswerPair> onFinish = null;
		GameObject popup = UIUtil.CreatePopup("Disable Parental Lock", UIUtil.CreateReflectedObject(ProtoUtil.Clone(questionAnswerPair).ClearAnswer(), 1280f, 230f), null, parent: UIGeneratorType.GetActiveUIForType<ProfileOptions>().transform, size: null, centerReferece: null, center: null, pivot: null, onClose: delegate
		{
			QuestionAnswerPair.OnFinish -= onFinish;
		}, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: null, rayCastBlockerColor: null, delayClose: null, referenceResolution: null, buttons: Array.Empty<string>());
		onFinish = delegate(QuestionAnswerPair qaPair)
		{
			if (qaPair != null)
			{
				if (StringComparer.OrdinalIgnoreCase.Equals(questionAnswerPair.answer, qaPair.answer))
				{
					questionAnswerPair = questionAnswerPair.ClearAnswer();
					UIGeneratorType.ValidateAllOfType<ProfileOptions>();
				}
				else
				{
					GameObject mainContent2 = UIUtil.CreateMessageBox("The answer given did not match with answer set by parental lock. Answers are not case sensitive.", TextAlignmentOptions.Left, 32, 600, 300, 24f);
					Transform transform2 = UIGeneratorType.GetActiveUIForType<ProfileOptions>().transform;
					UIUtil.CreatePopup("Invalid Answer", mainContent2, null, null, null, null, null, null, true, true, null, null, null, transform2, null, null);
				}
			}
			popup.GetComponentInChildren<UIPopupControl>().Close();
		};
		QuestionAnswerPair.OnFinish += onFinish;
	}
}
