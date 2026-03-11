using ACadSharp.Classes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Fallback wrapper for proxy-backed non-graphical objects that are irrelevant for rendering.
	/// </summary>
	public sealed class UnknownProxyNonGraphicalObject : NonGraphicalObject, IProxy
	{
		public DxfClass DxfClass { get; set; }

		public override ObjectType ObjectType => ObjectType.UNDEFINED;

		public override string ObjectName => this.DxfClass?.DxfName ?? "UNKNOWN_PROXY_OBJECT";

		public override string SubclassMarker => this.DxfClass?.CppClassName ?? DxfSubclassMarker.ProxyObject;

		public int ClassId => this.DxfClass?.ClassNumber ?? 0;

		public int ProxyClassId => 499;

		public bool OriginalDataFormatDxf { get; set; }

		public ACadVersion Version { get; set; }

		public int MaintenanceVersion { get; set; }

		internal UnknownProxyNonGraphicalObject(DxfClass dxfClass)
		{
			this.DxfClass = dxfClass;
		}
	}
}
