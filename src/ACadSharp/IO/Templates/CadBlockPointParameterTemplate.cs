using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates;

internal class CadBlockPointParameterTemplate : CadBlock1PtParameterTemplate
{
	public CadBlockPointParameterTemplate()
		: base(new BlockPointParameter())
	{
	}

	public CadBlockPointParameterTemplate(BlockPointParameter cadObject)
		: base(cadObject)
	{
	}
}
