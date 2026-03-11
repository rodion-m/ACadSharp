using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	[DxfName(DxfFileToken.ObjectCellStyleMap)]
	[DxfSubClass(DxfSubclassMarker.CellStyleMap)]
	public class CellStyleMap : NonGraphicalObject
	{
		[DxfCodeValue(90)]
		public int StyleCount { get; set; }

		[DxfCodeValue(300)]
		public string Marker { get; set; }

		public override string ObjectName => DxfFileToken.ObjectCellStyleMap;

		public override ObjectType ObjectType => ObjectType.UNLISTED;

		public override string SubclassMarker => DxfSubclassMarker.CellStyleMap;
	}
}
