using System.Collections.Generic;
using DialoguerEditor;

namespace DialoguerCore;

public class ConditionalPhase : AbstractDialoguePhase
{
	public readonly VariableEditorScopes scope;

	public readonly VariableEditorTypes type;

	public readonly int variableId;

	public readonly VariableEditorGetEquation equation;

	public readonly string getValue;

	private bool _parsedBool;

	private bool _checkBool;

	private float _parsedFloat;

	private float _checkFloat;

	private string _parsedString;

	private string _checkString;

	public ConditionalPhase(VariableEditorScopes scope, VariableEditorTypes type, int variableId, VariableEditorGetEquation equation, string getValue, List<int> outs)
		: base(outs)
	{
		this.scope = scope;
		this.type = type;
		this.variableId = variableId;
		this.equation = equation;
		this.getValue = getValue;
	}

	protected override void onStart()
	{
		bool flag = true;
		switch (type)
		{
		case VariableEditorTypes.Boolean:
			if (!bool.TryParse(getValue, out _parsedBool))
			{
				Debug.LogError("[ConditionalPhase] Could Not Parse Bool: " + getValue);
			}
			if (scope == VariableEditorScopes.Local)
			{
				_checkBool = _localVariables.booleans[variableId];
			}
			else
			{
				_checkBool = Dialoguer.GetGlobalBoolean(variableId);
			}
			break;
		case VariableEditorTypes.Float:
			if (!Parser.FloatTryParse(getValue, out _parsedFloat))
			{
				Debug.LogError("[ConditionalPhase] Could Not Parse Float: " + getValue);
			}
			if (scope == VariableEditorScopes.Local)
			{
				_checkFloat = _localVariables.floats[variableId];
			}
			else
			{
				_checkFloat = Dialoguer.GetGlobalFloat(variableId);
			}
			break;
		case VariableEditorTypes.String:
			_parsedString = getValue;
			if (scope == VariableEditorScopes.Local)
			{
				_checkString = _localVariables.strings[variableId];
			}
			else
			{
				_checkString = Dialoguer.GetGlobalString(variableId);
			}
			break;
		}
		bool flag2 = false;
		switch (type)
		{
		case VariableEditorTypes.Boolean:
			switch (equation)
			{
			case VariableEditorGetEquation.Equals:
				if (_parsedBool == _checkBool)
				{
					flag2 = true;
				}
				break;
			case VariableEditorGetEquation.NotEquals:
				if (_parsedBool != _checkBool)
				{
					flag2 = true;
				}
				break;
			}
			break;
		case VariableEditorTypes.Float:
			switch (equation)
			{
			case VariableEditorGetEquation.Equals:
				if (_checkFloat == _parsedFloat)
				{
					flag2 = true;
				}
				break;
			case VariableEditorGetEquation.NotEquals:
				if (_checkFloat != _parsedFloat)
				{
					flag2 = true;
				}
				break;
			case VariableEditorGetEquation.EqualOrGreaterThan:
				if (_checkFloat >= _parsedFloat)
				{
					flag2 = true;
				}
				break;
			case VariableEditorGetEquation.EqualOrLessThan:
				if (_checkFloat <= _parsedFloat)
				{
					flag2 = true;
				}
				break;
			case VariableEditorGetEquation.GreaterThan:
				if (_checkFloat > _parsedFloat)
				{
					flag2 = true;
				}
				break;
			case VariableEditorGetEquation.LessThan:
				if (_checkFloat < _parsedFloat)
				{
					flag2 = true;
				}
				break;
			}
			break;
		case VariableEditorTypes.String:
			switch (equation)
			{
			case VariableEditorGetEquation.Equals:
				if (_parsedString == _checkString)
				{
					flag2 = true;
				}
				break;
			case VariableEditorGetEquation.NotEquals:
				if (_parsedString != _checkString)
				{
					flag2 = true;
				}
				break;
			}
			break;
		}
		if (flag2)
		{
			Continue(0);
		}
		else
		{
			Continue(1);
		}
		base.state = PhaseState.Complete;
	}

	public override string ToString()
	{
		object[] obj = new object[15]
		{
			"Set Variable Phase\nScope: ", null, null, null, null, null, null, null, null, null,
			null, null, null, null, null
		};
		VariableEditorScopes variableEditorScopes = scope;
		obj[1] = variableEditorScopes.ToString();
		obj[2] = "\nType: ";
		VariableEditorTypes variableEditorTypes = type;
		obj[3] = variableEditorTypes.ToString();
		obj[4] = "\nVariable ID: ";
		obj[5] = variableId;
		obj[6] = "\nEquation: ";
		VariableEditorGetEquation variableEditorGetEquation = equation;
		obj[7] = variableEditorGetEquation.ToString();
		obj[8] = "\nGet Value: ";
		obj[9] = getValue;
		obj[10] = "\nTrue Out: ";
		obj[11] = outs[0];
		obj[12] = "\nFalse Out: ";
		obj[13] = outs[1];
		obj[14] = "\n";
		return string.Concat(obj);
	}
}
