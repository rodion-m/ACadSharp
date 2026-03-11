using ACadSharp.Classes;

namespace ACadSharp.Objects.Evaluations
{
	/// <summary>
	/// Fallback wrapper for unsupported dynamic-block evaluation objects.
	/// It preserves handle identity so evaluation graphs and action dependencies can resolve.
	/// </summary>
	public sealed class UnknownEvaluationExpression : EvaluationExpression
	{
		public DxfClass DxfClass { get; }

		public override string ObjectName => this.DxfClass?.DxfName ?? "UNKNOWN_EVAL_EXPR";

		public override string SubclassMarker => this.DxfClass?.CppClassName ?? DxfSubclassMarker.EvalGraphExpr;

		internal UnknownEvaluationExpression(DxfClass dxfClass)
		{
			this.DxfClass = dxfClass;
		}
	}
}
