using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	[DxfName(DxfFileToken.ObjectSectionViewStyle)]
	[DxfSubClass(DxfSubclassMarker.SectionViewStyle)]
	public class SectionViewStyle : NonGraphicalObject
	{
		[DxfCodeValue(3, 300)]
		public string DisplayName { get; set; }

		[DxfCodeValue(70)]
		public short Flags70 { get; set; }

		[DxfCodeValue(71)]
		public short Flags71 { get; set; }

		[DxfCodeValue(90)]
		public int Value90 { get; set; }

		public override string ObjectName => DxfFileToken.ObjectSectionViewStyle;

		public override ObjectType ObjectType => ObjectType.UNLISTED;

		public override string SubclassMarker => DxfSubclassMarker.SectionViewStyle;
	}
}
