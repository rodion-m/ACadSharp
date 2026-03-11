using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

[DxfName(DxfFileToken.ObjectBlockXYGrip)]
[DxfSubClass(DxfSubclassMarker.BlockXYGrip)]
public class BlockXYGrip : BlockGrip
{
	public override string ObjectName => DxfFileToken.ObjectBlockXYGrip;

	public override string SubclassMarker => DxfSubclassMarker.BlockXYGrip;
}
