using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockLinearParameterTemplate : CadBlock2PtParameterTemplate
	{
		public new BlockLinearParameter CadObject => (BlockLinearParameter)base.CadObject;

		public CadBlockLinearParameterTemplate()
			: base(new BlockLinearParameter())
		{
		}

		public CadBlockLinearParameterTemplate(BlockLinearParameter cadObject)
			: base(cadObject)
		{
		}
	}
}
