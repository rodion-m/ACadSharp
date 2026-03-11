using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	[DxfName(DxfFileToken.ObjectDynamicBlockPurgePreventer)]
	[DxfSubClass(DxfSubclassMarker.DynamicBlockPurgePreventer)]
	public class DynamicBlockPurgePreventer : NonGraphicalObject
	{
		[DxfCodeValue(70)]
		public short Version { get; set; }

		public override string ObjectName => DxfFileToken.ObjectDynamicBlockPurgePreventer;

		public override ObjectType ObjectType => ObjectType.UNLISTED;

		public override string SubclassMarker => DxfSubclassMarker.DynamicBlockPurgePreventer;
	}
}
