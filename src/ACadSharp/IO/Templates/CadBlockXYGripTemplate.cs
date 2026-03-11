using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates;

internal class CadBlockXYGripTemplate : CadBlockGripTemplate
{
	public CadBlockXYGripTemplate()
		: base(new BlockXYGrip())
	{
	}

	public CadBlockXYGripTemplate(BlockXYGrip cadObject)
		: base(cadObject)
	{
	}
}
