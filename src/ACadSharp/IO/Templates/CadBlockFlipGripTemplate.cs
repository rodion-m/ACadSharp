using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockFlipGripTemplate : CadBlockGripTemplate
	{
		public new BlockFlipGrip CadObject => (BlockFlipGrip)base.CadObject;

		public CadBlockFlipGripTemplate()
			: base(new BlockFlipGrip())
		{
		}

		public CadBlockFlipGripTemplate(BlockFlipGrip cadObject)
			: base(cadObject)
		{
		}
	}
}
