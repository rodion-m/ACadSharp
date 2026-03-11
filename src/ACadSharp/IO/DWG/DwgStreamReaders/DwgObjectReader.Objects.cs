using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using ACadSharp.Objects.Evaluations;
using System;

namespace ACadSharp.IO.DWG;

internal partial class DwgObjectReader : DwgSectionIO
{
	private void readAnnotScaleObjectContextData(CadAnnotScaleObjectContextDataTemplate template)
	{
		this.readObjectContextData(template);

		template.ScaleHandle = this.handleReference();
	}

	private CadTemplate readBlkRefObjectContextData()
	{
		BlockReferenceObjectContextData contextData = new BlockReferenceObjectContextData();
		CadAnnotScaleObjectContextDataTemplate template = new CadAnnotScaleObjectContextDataTemplate(contextData);

		this.readAnnotScaleObjectContextData(template);

		contextData.Rotation = this._mergedReaders.ReadBitDouble();
		contextData.InsertionPoint = this._mergedReaders.Read3BitDouble();
		contextData.XScale = this._mergedReaders.ReadBitDouble();
		contextData.YScale = this._mergedReaders.ReadBitDouble();
		contextData.ZScale = this._mergedReaders.ReadBitDouble();

		return template;
	}

	private void readBlock1PtParameter(CadBlock1PtParameterTemplate template)
	{
		this.readBlockParameter(template);

		// AcDbBlock1PtParameter_fields in DWG:
		// def_pt, prop1, prop2, trailing num_propinfos.
		template.Block1PtParameter.Location = this._mergedReaders.Read3BitDouble();

		int prop1Count = this._mergedReaders.ReadBitLong();
		template.Block1PtParameter.Value170 = checked((short)prop1Count);
		for (int i = 0; i < prop1Count; i++)
		{
			int code = this._mergedReaders.ReadBitLong();
			string name = this._mergedReaders.ReadVariableText();
			if (i == 0)
			{
				template.Block1PtParameter.Value91 = code;
				template.Block1PtParameter.Value301 = name;
			}
		}

		int prop2Count = this._mergedReaders.ReadBitLong();
		template.Block1PtParameter.Value171 = checked((short)prop2Count);
		for (int i = 0; i < prop2Count; i++)
		{
			int code = this._mergedReaders.ReadBitLong();
			string name = this._mergedReaders.ReadVariableText();
			if (i == 0)
			{
				template.Block1PtParameter.Value92 = code;
				template.Block1PtParameter.Value302 = name;
			}
		}

		template.Block1PtParameter.Value93 = this._mergedReaders.ReadBitLong();
	}

	private CadTemplate readBlockBasePointParameter()
	{
		BlockBasePointParameter blockBasePointParameter = new();
		CadBlockBasePointParameterTemplate template = new(blockBasePointParameter);

		this.readBlock1PtParameter(template);
		blockBasePointParameter.Point1010 = this._mergedReaders.Read3BitDouble();
		blockBasePointParameter.Point1012 = this._mergedReaders.Read3BitDouble();

		return template;
	}

	private void readBlock2PtParameter(CadBlock2PtParameterTemplate template)
	{
		this.readBlockParameter(template);

		// AcDbBlock2PtParameter_fields in DWG:
		// def_basept, def_endpt, prop1..prop4, prop_states[4], parameter_base_location.
		template.Block2PtParameter.FirstPoint = this._mergedReaders.Read3BitDouble();
		template.Block2PtParameter.SecondPoint = this._mergedReaders.Read3BitDouble();

		int[] propCounts = new int[4];
		int[] firstCodes = new int[4];
		string[] firstNames = new string[4];

		for (int i = 0; i < 4; i++)
		{
			propCounts[i] = this._mergedReaders.ReadBitLong();
			for (int j = 0; j < propCounts[i]; j++)
			{
				int code = this._mergedReaders.ReadBitLong();
				string name = this._mergedReaders.ReadVariableText();
				if (j == 0)
				{
					firstCodes[i] = code;
					firstNames[i] = name;
				}
			}
		}

		template.Block2PtParameter.Value170 = 4;
		template.Block2PtParameter.Value171 = checked((short)propCounts[0]);
		template.Block2PtParameter.Value172 = checked((short)propCounts[1]);
		template.Block2PtParameter.Value173 = checked((short)propCounts[2]);
		template.Block2PtParameter.Value174 = checked((short)propCounts[3]);
		template.Block2PtParameter.Value94 = firstCodes[2];
		template.Block2PtParameter.Value95 = firstCodes[3];
		template.Block2PtParameter.Value303 = firstNames[2];
		template.Block2PtParameter.Value304 = firstNames[3];

		for (int i = 0; i < 4; i++)
		{
			this._mergedReaders.ReadBitLong();
		}

		template.Block2PtParameter.Value177 = this._mergedReaders.ReadBitShort();
	}

	private void readBlockAction(CadBlockActionTemplate template)
	{
		this.readBlockElement(template);

		BlockAction blockAction = template.BlockAction;

		// AcDbBlockAction_fields in DWG:
		// display_location, deps, action ids.
		blockAction.ActionPoint = this._mergedReaders.Read3BitDouble();

		int entityCount = this._objectReader.ReadBitLong();
		for (int i = 0; i < entityCount; i++)
		{
			ulong entityHandle = this.handleReference();
			template.EntityHandles.Add(entityHandle);
		}

		int actionCount = this._mergedReaders.ReadBitLong();
		blockAction.Value70 = checked((short)actionCount);
		for (int i = 0; i < actionCount; i++)
		{
			this._mergedReaders.ReadBitLong();
		}
	}

	private void readBlockActionBasePt(CadBlockActionBasePtTemplate template)
	{
		this.readBlockAction(template);

		BlockActionBasePt blockActionBasePt = template.CadObject as BlockActionBasePt;

		// AcDbBlockActionWithBasePt_fields in DWG:
		// offset, two connection points, dependent, base_pt.
		blockActionBasePt.Value1011 = this._mergedReaders.Read3BitDouble();
		blockActionBasePt.Value92 = this._mergedReaders.ReadBitLong();
		blockActionBasePt.Value301 = this._mergedReaders.ReadVariableText();
		blockActionBasePt.Value93 = this._mergedReaders.ReadBitLong();
		blockActionBasePt.Value302 = this._mergedReaders.ReadVariableText();
		blockActionBasePt.Value280 = this._mergedReaders.ReadBit();
		blockActionBasePt.Value1012 = this._mergedReaders.Read3BitDouble();
	}

	private void readBlockElement(CadBlockElementTemplate template)
	{
		this.readEvaluationExpression(template);

		//300 name
		template.BlockElement.ElementName = this._mergedReaders.ReadVariableText();
		//98
		template.BlockElement.Value98 = this._mergedReaders.ReadBitLong();
		//99
		template.BlockElement.Value99 = this._mergedReaders.ReadBitLong();
		//1071
		template.BlockElement.Value1071 = this._mergedReaders.ReadBitLong();
	}

	private void readBlockGrip(CadBlockGripTemplate template)
	{
		this.readBlockElement(template);

		var blockGrip = template.CadObject as BlockGrip;

		blockGrip.Value91 = this._mergedReaders.ReadBitLong();
		blockGrip.Value92 = this._mergedReaders.ReadBitLong();
		blockGrip.Location = this._mergedReaders.Read3BitDouble();
		blockGrip.Value280 = this._mergedReaders.ReadBitAsShort();
		blockGrip.Value93 = this._mergedReaders.ReadBitLong();
	}

	private CadTemplate readBlockFlipGrip()
	{
		BlockFlipGrip blockFlipGrip = new();
		CadBlockFlipGripTemplate template = new(blockFlipGrip);

		this.readBlockGrip(template);
		blockFlipGrip.Value140 = this._mergedReaders.ReadBitDouble();
		blockFlipGrip.Value141 = this._mergedReaders.ReadBitDouble();
		blockFlipGrip.Value142 = this._mergedReaders.ReadBitDouble();
		blockFlipGrip.Value93N = this._mergedReaders.ReadBitLong();

		return template;
	}

	private CadTemplate readBlockGripLocationComponent()
	{
		BlockGripExpression gripExpression = new BlockGripExpression();
		CadBlockGripExpressionTemplate template = new CadBlockGripExpressionTemplate(gripExpression);

		this.readEvaluationExpression(template);
		gripExpression.Value91 = this._mergedReaders.ReadBitLong();
		gripExpression.Value300 = this._mergedReaders.ReadVariableText();

		return template;
	}

	private void readBlockParameter(CadBlockParameterTemplate template)
	{
		this.readBlockElement(template);

		//280
		template.BlockParameter.Value280 = this._mergedReaders.ReadBit();
		//281
		template.BlockParameter.Value281 = this._mergedReaders.ReadBit();
	}

	private CadTemplate readBlockRepresentationData()
	{
		BlockRepresentationData representation = new BlockRepresentationData();
		CadBlockRepresentationDataTemplate template = new CadBlockRepresentationDataTemplate(representation);

		this.readCommonNonEntityData(template);

		representation.Value70 = this._mergedReaders.ReadBitShort();
		template.BlockHandle = this.handleReference();

		return template;
	}

	private CadTemplate readBlockRotateAction()
	{
		BlockRotationAction rotationAction = new();
		CadBlockRotationActionTemplate template = new(rotationAction);

		this.readBlockActionBasePt(template);

		rotationAction.Value94 = this._mergedReaders.ReadBitLong();
		rotationAction.Value303 = this._mergedReaders.ReadVariableText();

		return template;
	}

	private CadTemplate readBlockRotationParameter()
	{
		BlockRotationParameter blockRotationParameter = new();
		CadBlockRotationParameterTemplate template = new CadBlockRotationParameterTemplate(blockRotationParameter);

		this.readBlock2PtParameter(template);

		//1011 1021 1031
		blockRotationParameter.Point = this._mergedReaders.Read3BitDouble();
		//305
		blockRotationParameter.Name = this._mergedReaders.ReadVariableText();
		//306
		blockRotationParameter.Description = this._mergedReaders.ReadVariableText();
		//140
		blockRotationParameter.NameOffset = this._mergedReaders.ReadBitDouble();

		blockRotationParameter.Value96 = this._mergedReaders.ReadBitLong();
		blockRotationParameter.Value141 = this._mergedReaders.ReadBitDouble();
		blockRotationParameter.Value142 = this._mergedReaders.ReadBitDouble();
		blockRotationParameter.Value143 = this._mergedReaders.ReadBitDouble();
		blockRotationParameter.Value175 = this._mergedReaders.ReadBitShort();

		return template;
	}

	private CadTemplate readBlockPointParameter()
	{
		BlockPointParameter blockPointParameter = new();
		CadBlockPointParameterTemplate template = new(blockPointParameter);

		this.readBlock1PtParameter(template);

		blockPointParameter.Name = this._mergedReaders.ReadVariableText();
		blockPointParameter.Description = this._mergedReaders.ReadVariableText();
		blockPointParameter.LabelPoint = this._mergedReaders.Read3BitDouble();

		return template;
	}

	private CadTemplate readBlockLinearParameter()
	{
		BlockLinearParameter blockLinearParameter = new();
		CadBlockLinearParameterTemplate template = new(blockLinearParameter);

		this.readBlock2PtParameter(template);
		blockLinearParameter.Label = this._mergedReaders.ReadVariableText();
		blockLinearParameter.Description = this._mergedReaders.ReadVariableText();
		blockLinearParameter.LabelOffset = this._mergedReaders.ReadBitDouble();

		return template;
	}

	private CadTemplate readBlockMoveAction()
	{
		BlockMoveAction moveAction = new();
		CadBlockMoveActionTemplate template = new(moveAction);

		this.readBlockAction(template);

		moveAction.Value92 = this._mergedReaders.ReadBitLong();
		moveAction.Value301 = this._mergedReaders.ReadVariableText();
		moveAction.Value93 = this._mergedReaders.ReadBitLong();
		moveAction.Value302 = this._mergedReaders.ReadVariableText();
		moveAction.Value140 = this._mergedReaders.ReadBitDouble();
		moveAction.Value141 = this._mergedReaders.ReadBitDouble();
		moveAction.AngleOffset = this._mergedReaders.ReadBitDouble();
		moveAction.Value280 = 0;

		return template;
	}

	private CadTemplate readBlockVisibilityParameter()
	{
		BlockVisibilityParameter blockVisibilityParameter = new BlockVisibilityParameter();
		CadBlockVisibilityParameterTemplate template = new CadBlockVisibilityParameterTemplate(blockVisibilityParameter);

		this.readBlock1PtParameter(template);

		//281
		blockVisibilityParameter.Value281 = this._mergedReaders.ReadBit();
		//301
		blockVisibilityParameter.Name = this._mergedReaders.ReadVariableText();
		//302
		blockVisibilityParameter.Description = this._mergedReaders.ReadVariableText();
		//missing bit??	91 should be an int
		blockVisibilityParameter.Value91 = this._mergedReaders.ReadBit();

		//DXF 93 Total entities count
		var totalEntitiesCount = this._objectReader.ReadBitLong();
		for (int i = 0; i < totalEntitiesCount; i++)
		{
			//331
			template.EntityHandles.Add(this.handleReference());
		}

		//DXF 92 states count
		var nstates = this._objectReader.ReadBitLong();
		for (int j = 0; j < nstates; j++)
		{
			template.StateTemplates.Add(this.readState());
		}

		return template;
	}

	private void readEvaluationExpression(CadEvaluationExpressionTemplate template)
	{
		this.readCommonNonEntityData(template);

		// AcDbEvalExpr_fields in DWG:
		// parentid, major, minor, typed value payload, nodeid.
		this._objectReader.ReadBitLong();
		template.CadObject.Value98 = this._objectReader.ReadBitLong();
		template.CadObject.Value99 = this._objectReader.ReadBitLong();

		short valueCode = this._mergedReaders.ReadBitShort();
		switch (valueCode)
		{
			case -9999:
				break;
			case 40:
				this._mergedReaders.ReadBitDouble();
				break;
			case 10:
				this._mergedReaders.Read2RawDouble();
				break;
			case 11:
				this._mergedReaders.Read3BitDouble();
				break;
			case 1:
				this._mergedReaders.ReadVariableText();
				break;
			case 70:
				this._mergedReaders.ReadBitShort();
				break;
			case 90:
				this._mergedReaders.ReadBitLong();
				break;
			case 91:
				this.handleReference();
				break;
			default:
				throw new NotSupportedException($"Unsupported AcDbEvalExpr value code '{valueCode}' in DWG stream.");
		}

		template.CadObject.Id = this._objectReader.ReadBitLong();
	}

	private CadTemplate readField()
	{
		var field = new Field();
		CadFieldTemplate template = new CadFieldTemplate(field);

		this.readCommonNonEntityData(template);

		//TV 1 Evaluator ID TV 2,3 Field code(in DXF strings longer than 255 characters
		//are written in chunks of 255 characters in one 2 group and one or
		//more 3 groups).
		field.EvaluatorId = this._mergedReaders.ReadVariableText();
		field.FieldCode = this._mergedReaders.ReadVariableText();
		//BL 90 Number of child fields
		int nchild = this._mergedReaders.ReadBitLong();
		for (int i = 0; i < nchild; i++)
		{
			//H 360 Child field handle (hard owner)
			template.ChildrenHandles.Add(this.handleReference());
		}

		//BL 97 Number of field objects
		int nfields = this._mergedReaders.ReadBitLong();
		for (int j = 0; j < nfields; j++)
		{
			//H 331 Field object handle (soft pointer)
			template.CadObjectsHandles.Add(this.handleReference());
		}

		//-R2004
		if (this._version < ACadVersion.AC1021)
		{
			//TV 4 Format string. After R2004 the format became part of the value object.
			field.FormatString = this._mergedReaders.ReadVariableText();
		}

		//Common BL 91 Evaluation option flags:
		field.EvaluationOptionFlags = (EvaluationOptionFlags)this._mergedReaders.ReadBitLong();
		//BL 92 Filing option flags:
		field.FilingOptionFlags = (FilingOptionFlags)this._mergedReaders.ReadBitLong();
		//BL 96 Evaluation error code
		field.FieldStateFlags = (FieldStateFlags)this._mergedReaders.ReadBitLong();
		//BL 94 Field state flags:
		field.EvaluationStatusFlags = (EvaluationStatusFlags)this._mergedReaders.ReadBitLong();
		//BL 96 Evaluation error code
		field.EvaluationErrorCode = this._mergedReaders.ReadBitLong();
		//TV 300 Evaluation error message
		field.EvaluationErrorMessage = this._mergedReaders.ReadVariableText();

		//... ... The field value, see paragraph 20.4.99.
		template.CadValueTemplates.Add(this.readCadValue(field.Value));

		//TV 301,9 Value string(DXF: written in 255 character chunks)
		field.FormatString = this._mergedReaders.ReadVariableText();
		this._mergedReaders.ReadBitLong();
		int num3 = this._mergedReaders.ReadBitLong();
		for (int k = 0; k < num3; k++)
		{
			//TV 6 Child field key
			string key = this._mergedReaders.ReadVariableText();
			CadValue value = new CadValue();
			template.CadValueTemplates.Add(this.readCadValue(value));
			field.Values.Add(key, value);
		}

		return template;
	}

	private CadValueTemplate readCadValue(CadValue value)
	{
		CadValueTemplate template = new CadValueTemplate(value);

		//R2007+:
		if (this.R2007Plus)
		{
			//Flags BL 93 Flags & 0x01 => type is kGeneral
			value.Flags = this._mergedReaders.ReadBitLong();
		}

		//Common:
		//Data type BL 90
		value.ValueType = (CadValueType)this._mergedReaders.ReadBitLong();
		if (!this.R2007Plus || !value.IsEmpty)
		{
			//Varies by type: Not present in case bit 1 in Flags is set
			switch (value.ValueType)
			{
				case CadValueType.Unknown:
				case CadValueType.Long:
					value.Value = this._mergedReaders.ReadBitLong();
					break;
				case CadValueType.Double:
					value.Value = this._mergedReaders.ReadBitDouble();
					break;
				case CadValueType.General:
				case CadValueType.String:
					value.Value = this.readStringCadValue();
					break;
				case CadValueType.Date:
					System.DateTime? dateTime = this.readDateCadValue();
					if (dateTime.HasValue)
					{
						value.Value = dateTime.Value;
					}
					break;
				case CadValueType.Point2D:
					value.Value = this.readCellValueXY();
					break;
				case CadValueType.Point3D:
					value.Value = this.readCellValueXYZ();
					break;
				case CadValueType.Handle:
					template.ValueHandle = this.handleReference();
					break;
				case CadValueType.Buffer:
				case CadValueType.ResultBuffer:
				default:
					throw new NotImplementedException();
			}
		}

		//R2007+:
		if (this.R2007Plus)
		{
			//Unit type BL 94 0 = no units, 1 = distance, 2 = angle, 4 = area, 8 = volume
			value.Units = (CadValueUnitType)this._mergedReaders.ReadBitLong();
			//Format String TV 300
			value.Format = this._mergedReaders.ReadVariableText();
			//Value String TV 302
			value.FormattedValue = this._mergedReaders.ReadVariableText();
		}

		return template;
	}

	private CadTemplate readFieldList()
	{
		FieldList fieldList = new FieldList();
		CadFieldListTemplate template = new CadFieldListTemplate(fieldList);

		this.readCommonNonEntityData(template);

		//BL Number of fields
		int nhandles = this._mergedReaders.ReadBitLong();
		//B Unknown
		this._mergedReaders.ReadBit();
		for (int i = 0; i < nhandles; i++)
		{
			//H 330 Field handle (soft pointer)
			template.OwnedObjectsHandlers.Add(this.handleReference());
		}

		return template;
	}

	private CadTemplate readMTextAttributeObjectContextData()
	{
		//TODO: MTextAttributeObjectContextData for dwg
		MTextAttributeObjectContextData contextData = new();
		CadAnnotScaleObjectContextDataTemplate template = new CadAnnotScaleObjectContextDataTemplate(contextData);

		//this.readAnnotScaleObjectContextData(template);

		return null;
	}

	private void readObjectContextData(CadTemplate template)
	{
		this.readCommonNonEntityData(template);

		ObjectContextData contextData = (ObjectContextData)template.CadObject;

		//BS	70	Version (default value is 3).
		contextData.Version = _objectReader.ReadBitShort();
		//B	290	Default flag (default value is false).
		contextData.Default = _objectReader.ReadBit();
	}

	private CadBlockVisibilityParameterTemplate.StateTemplate readState()
	{
		CadBlockVisibilityParameterTemplate.StateTemplate template = new CadBlockVisibilityParameterTemplate.StateTemplate();

		template.State.Name = this._textReader.ReadVariableText();

		//DXF 94 subset count 1
		int n1 = this._objectReader.ReadBitLong();
		for (int i = 0; i < n1; i++)
		{
			//332
			template.EntityHandles.Add(this.handleReference());
		}

		//DXF 95 subset count 2
		var n2 = this._objectReader.ReadBitLong();
		for (int i = 0; i < n2; i++)
		{
			//333
			template.ExpressionHandles.Add(this.handleReference());
		}

		return template;
	}
}
