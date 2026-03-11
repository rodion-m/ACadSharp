using ACadSharp.Classes;

namespace ACadSharp.Objects.Evaluations
{
	/// <summary>
	/// Fallback wrapper for proxy-backed dynamic-block evaluation objects.
	/// It consumes proxy metadata while preserving evaluation-expression identity.
	/// </summary>
	public sealed class UnknownProxyEvaluationExpression : EvaluationExpression, IProxy
	{
		public DxfClass DxfClass { get; set; }

		public override string ObjectName => this.DxfClass?.DxfName ?? "UNKNOWN_PROXY_EVAL_EXPR";

		public override string SubclassMarker => this.DxfClass?.CppClassName ?? DxfSubclassMarker.ProxyObject;

		public int ClassId => this.DxfClass?.ClassNumber ?? 0;

		public int ProxyClassId => 499;

		public bool OriginalDataFormatDxf { get; set; }

		public ACadVersion Version { get; set; }

		public int MaintenanceVersion { get; set; }

		internal UnknownProxyEvaluationExpression(DxfClass dxfClass)
		{
			this.DxfClass = dxfClass;
		}
	}
}
