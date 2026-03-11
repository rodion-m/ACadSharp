using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockBasePointParameterTemplate : CadBlock1PtParameterTemplate
	{
		public new BlockBasePointParameter CadObject => (BlockBasePointParameter)base.CadObject;

		public CadBlockBasePointParameterTemplate()
			: base(new BlockBasePointParameter())
		{
		}

		public CadBlockBasePointParameterTemplate(BlockBasePointParameter cadObject)
			: base(cadObject)
		{
		}
	}
}
