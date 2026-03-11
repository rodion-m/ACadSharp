using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations;

[DxfName(DxfFileToken.ObjectBlockPointParameter)]
[DxfSubClass(DxfSubclassMarker.BlockPointParameter)]
public class BlockPointParameter : Block1PtParameter
{
	[DxfCodeValue(303)]
	public string Name { get; set; }

	[DxfCodeValue(304)]
	public string Description { get; set; }

	[DxfCodeValue(1011, 1021, 1031)]
	public XYZ LabelPoint { get; set; }

	public override string ObjectName => DxfFileToken.ObjectBlockPointParameter;

	public override string SubclassMarker => DxfSubclassMarker.BlockPointParameter;
}
