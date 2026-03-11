using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Objects.Evaluations;
using ACadSharp.Tables;
using ACadSharp.Tests.TestModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class DynamicBlockTests : IOTestsBase
	{
		public static TheoryData<FileModel> GenericDynamicBlocksPaths { get; } = new();

		public static TheoryData<FileModel> IsolatedDynamicBlocksPaths { get; } = new();

		static DynamicBlockTests()
		{
			loadSamples("./", "dxf", GenericDynamicBlocksPaths);
			loadSamples("./", "dwg", GenericDynamicBlocksPaths);

			loadSamples("./dynamic-blocks", "*dwg", IsolatedDynamicBlocksPaths);
			loadSamples("./dynamic-blocks", "*dxf", IsolatedDynamicBlocksPaths);
		}

		public DynamicBlockTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(GenericDynamicBlocksPaths))]
		public void DynamicBlocksTest(FileModel test)
		{
			CadDocument doc;

			if (test.IsDxf)
			{
				DxfReaderConfiguration configuration = new();
				configuration.KeepUnknownEntities = true;
				configuration.KeepUnknownNonGraphicalObjects = true;

				doc = DxfReader.Read(test.Path, configuration, this.onNotification);

				if (doc.Header.Version <= ACadVersion.AC1021)
				{
					return;
				}
			}
			else
			{
				DwgReaderConfiguration configuration = new DwgReaderConfiguration();
				configuration.KeepUnknownEntities = true;
				configuration.KeepUnknownNonGraphicalObjects = true;

				doc = DwgReader.Read(test.Path, configuration, this.onNotification);
			}

			string dynamicName = "my-dynamic-block";

			BlockRecord blk = doc.BlockRecords[dynamicName];

			Assert.True(blk.IsDynamic);

			//Dictionary entry
			EvaluationGraph eval = blk.XDictionary.GetEntry<EvaluationGraph>("ACAD_ENHANCEDBLOCK");

			//Extended data related to the dynamic block
			var a = blk.ExtendedData.Get(doc.AppIds["AcDbBlockRepETag"]);
			var b = blk.ExtendedData.Get(doc.AppIds["AcDbDynamicBlockTrueName"]);
			var c = blk.ExtendedData.Get(doc.AppIds["AcDbDynamicBlockGUID"]);

			Insert basic = doc.GetCadObject<Insert>(0xABA);
			Insert modified = doc.GetCadObject<Insert>(0xAC5);

			Assert.NotNull(modified.Block.Source);
			Assert.Equal(dynamicName, modified.Block.Source.Name);
		}

		[Theory]
		[MemberData(nameof(IsolatedDynamicBlocksPaths))]
		public void IsolatedTest(FileModel test)
		{
			var config = getConfiguration(test);
			var doc = this.readDocument(test, config);

			switch (test.NoExtensionName)
			{
				case DxfFileToken.ObjectBlockVisibilityParameter:
					this.assertVisibilityParameter(doc);
					break;
				case DxfFileToken.ObjectBlockRotationParameter:
					this.assertRotationParameter(doc);
					break;
				case DxfFileToken.ObjectBlockPointParameter:
					this.assertPointParameter(doc);
					break;
				default:
					throw new System.NotImplementedException();
			}
		}

		[Fact]
		public void BlockRotationParameterDwg_DoesNotEmitKnownDynamicBlockDiagnostics()
		{
			string path = Path.Combine(TestVariables.SamplesFolder, "dynamic-blocks", "BLOCKROTATIONPARAMETER.dwg");
			Assert.True(File.Exists(path), $"Sample file not found: {path}");

			DwgReaderConfiguration configuration = new DwgReaderConfiguration
			{
				KeepUnknownEntities = true,
				KeepUnknownNonGraphicalObjects = true,
			};

			List<NotificationEventArgs> notifications = new();
			CadDocument doc = DwgReader.Read(path, configuration, (sender, e) => notifications.Add(e));

			Assert.NotNull(doc);
			Assert.DoesNotContain(notifications, e => contains(e.Message, "Could not read BLOCKROTATEACTION"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "Evaluation graph couldn't find the EvaluationExpression"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "EvaluationExpression with handle"));

			var rotationActions = doc.BlockRecords["dynamic_block"]
				.EvaluationGraph
				.Nodes
				.Select(n => n.Expression)
				.OfType<BlockRotationAction>()
				.ToList();
			Assert.NotEmpty(rotationActions);
			Assert.All(rotationActions, action => Assert.False(string.IsNullOrWhiteSpace(action.Value303)));
		}

		private void assertPointParameter(CadDocument doc)
		{
			var original = doc.BlockRecords
				.Where(b => b.IsDynamic && b.EvaluationGraph != null)
				.Single();

			foreach (BlockRecord record in doc.BlockRecords.Where(b => b.IsAnonymous))
			{
				Assert.Equal(original, record.Source);
			}

			var expressions = original.EvaluationGraph.Nodes.Select(n => n.Expression).ToList();
			Assert.NotEmpty(expressions.OfType<BlockPointParameter>());
			Assert.NotEmpty(expressions.OfType<BlockMoveAction>());
			Assert.NotEmpty(expressions.OfType<BlockXYGrip>());

			var point = expressions.OfType<BlockPointParameter>().Single();
			Assert.False(string.IsNullOrWhiteSpace(point.Name));
			Assert.Equal("DisplacementX", point.Value301);
			Assert.Equal("DisplacementY", point.Value302);

			var move = expressions.OfType<BlockMoveAction>().Single();
			Assert.Equal("XDelta", move.Value301);
			Assert.Equal("YDelta", move.Value302);
			Assert.Equal(1.0, move.Value140, 6);
			Assert.Equal(0.0, move.Value141, 6);
		}

		[Fact]
		public void BlockPointParameterDxf_DoesNotEmitKnownAuxiliaryDiagnostics()
		{
			string path = Path.Combine(TestVariables.SamplesFolder, "dynamic-blocks", "BLOCKPOINTPARAMETER.dxf");
			Assert.True(File.Exists(path), $"Sample file not found: {path}");

			DxfReaderConfiguration configuration = new()
			{
				KeepUnknownEntities = true,
				KeepUnknownNonGraphicalObjects = true,
			};

			List<NotificationEventArgs> notifications = new();
			CadDocument doc = DxfReader.Read(path, configuration, (sender, e) => notifications.Add(e));

			Assert.NotNull(doc);
			Assert.DoesNotContain(notifications, e => contains(e.Message, "Section not implemented ACDSDATA"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "NonGraphicalObject not supported read as an UnknownNonGraphicalObject: ACDB_DYNAMICBLOCKPURGEPREVENTER_VERSION"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "NonGraphicalObject not supported read as an UnknownNonGraphicalObject: ACDBDETAILVIEWSTYLE"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "NonGraphicalObject not supported read as an UnknownNonGraphicalObject: ACDBSECTIONVIEWSTYLE"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "NonGraphicalObject not supported read as an UnknownNonGraphicalObject: CELLSTYLEMAP"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "Entry not found Imperial24"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "Entry not found ACAD_ROUNDTRIP_2008_TABLESTYLE_CELLSTYLEMAP"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "Entry not found 2dWireframe"));
		}

		[Fact]
		public void SampleAc1024Dxf_DoesNotEmitBenignPassThroughDiagnostics()
		{
			string path = Path.Combine(TestVariables.SamplesFolder, "sample_AC1024_ascii.dxf");
			Assert.True(File.Exists(path), $"Sample file not found: {path}");

			DxfReaderConfiguration configuration = new()
			{
				KeepUnknownEntities = true,
				KeepUnknownNonGraphicalObjects = true,
			};

			List<NotificationEventArgs> notifications = new();
			CadDocument doc = DxfReader.Read(path, configuration, (sender, e) => notifications.Add(e));

			Assert.NotNull(doc);
			Assert.DoesNotContain(notifications, e => contains(e.Message, "BLOCKLINEARGRIP"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "BLOCKSCALEACTION"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "ACDBASSOCPERSSUBENTMANAGER"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "WIPEOUTVARIABLES"));
		}

		[Fact]
		public void SampleAc1024Dwg_DoesNotEmitBenignPassThroughDiagnostics()
		{
			string path = Path.Combine(TestVariables.SamplesFolder, "sample_AC1024.dwg");
			Assert.True(File.Exists(path), $"Sample file not found: {path}");

			DwgReaderConfiguration configuration = new()
			{
				KeepUnknownEntities = true,
				KeepUnknownNonGraphicalObjects = true,
			};

			List<NotificationEventArgs> notifications = new();
			CadDocument doc = DwgReader.Read(path, configuration, (sender, e) => notifications.Add(e));

			Assert.NotNull(doc);
			Assert.DoesNotContain(notifications, e => contains(e.Message, "BLOCKLINEARGRIP"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "BLOCKSCALEACTION"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "ACDBASSOCPERSSUBENTMANAGER"));
			Assert.DoesNotContain(notifications, e => contains(e.Message, "WIPEOUTVARIABLES"));
		}

		private void assertRotationParameter(CadDocument doc)
		{
			var original = doc.BlockRecords["dynamic_block"];
			foreach (BlockRecord record in doc.BlockRecords.Where(b => b.IsAnonymous))
			{
				Assert.Equal(original, record.Source);
			}

			foreach (Insert insert in doc.Entities.OfType<Insert>())
			{
				if (insert.XDictionary == null)
				{
					continue;
				}

				var dict = insert.XDictionary.GetEntry<CadDictionary>("AcDbBlockRepresentation");
				var representation = dict.GetEntry<BlockRepresentationData>("AcDbRepData");

				Assert.NotEmpty(insert.Block.Source.EvaluationGraph.Nodes.Select(n => n.Expression).OfType<BlockRotationParameter>());

				Assert.NotNull(representation);
				Assert.Equal(original, representation.Block);

				XRecord record = insert.XDictionary
					.GetEntry<CadDictionary>("AcDbBlockRepresentation")
					.GetEntry<CadDictionary>("AppDataCache")
					.GetEntry<CadDictionary>("ACAD_ENHANCEDBLOCKDATA")
					.OfType<XRecord>().First();
			}
		}

		private void assertVisibilityParameter(CadDocument doc)
		{
			var original = doc.BlockRecords["block_visibility_parameter"];
			foreach (BlockRecord record in doc.BlockRecords.Where(b => b.IsAnonymous))
			{
				Assert.Equal(original, record.Source);
			}

			foreach (Insert insert in doc.Entities.OfType<Insert>())
			{
				var dict = insert.XDictionary.GetEntry<CadDictionary>("AcDbBlockRepresentation");
				var representation = dict.GetEntry<BlockRepresentationData>("AcDbRepData");

				Assert.NotEmpty(insert.Block.Source.EvaluationGraph.Nodes.Select(n => n.Expression).OfType<BlockVisibilityParameter>());

				Assert.NotNull(representation);
				Assert.Equal(original, representation.Block);

				XRecord record = insert.XDictionary
					.GetEntry<CadDictionary>("AcDbBlockRepresentation")
					.GetEntry<CadDictionary>("AppDataCache")
					.GetEntry<CadDictionary>("ACAD_ENHANCEDBLOCKDATA")
					.OfType<XRecord>().First();

				var name = record.Entries.FirstOrDefault(e => e.Code == 1).Value as string;
				Assert.False(string.IsNullOrEmpty(name));
			}
		}

		private static bool contains(string message, string fragment)
		{
			return !string.IsNullOrWhiteSpace(message)
				&& message.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}
}
