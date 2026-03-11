using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	[DxfName(DxfFileToken.ObjectDetailViewStyle)]
	[DxfSubClass(DxfSubclassMarker.DetailViewStyle)]
	public class DetailViewStyle : NonGraphicalObject
	{
		[DxfCodeValue(3, 300)]
		public string DisplayName { get; set; }

		[DxfCodeValue(70)]
		public short Flags70 { get; set; }

		[DxfCodeValue(71)]
		public short Flags71 { get; set; }

		[DxfCodeValue(90)]
		public int Value90 { get; set; }

		public override string ObjectName => DxfFileToken.ObjectDetailViewStyle;

		public override ObjectType ObjectType => ObjectType.UNLISTED;

		public override string SubclassMarker => DxfSubclassMarker.DetailViewStyle;
	}
}
