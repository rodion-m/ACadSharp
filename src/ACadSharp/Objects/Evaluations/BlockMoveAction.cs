using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations;

[DxfName(DxfFileToken.ObjectBlockMoveAction)]
[DxfSubClass(DxfSubclassMarker.BlockMoveAction)]
public class BlockMoveAction : BlockAction
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockMoveAction;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockMoveAction;

	[DxfCodeValue(301)]
	public string Value301 { get; set; }

	[DxfCodeValue(302)]
	public string Value302 { get; set; }

	[DxfCodeValue(92)]
	public int Value92 { get; set; }

	[DxfCodeValue(93)]
	public int Value93 { get; set; }

	[DxfCodeValue(140)]
	public double Value140 { get; set; }

	[DxfCodeValue(141)]
	public double Value141 { get; set; }

	[DxfCodeValue(280)]
	public short Value280 { get; set; }

	/// <summary>
	/// DWG-only trailing offset read after <see cref="Value141"/>.
	/// DXF stores a synthetic 280 flag instead of this raw value.
	/// </summary>
	public double AngleOffset { get; set; }
}
