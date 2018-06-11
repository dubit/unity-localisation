using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Tests
{
	public class LocalisationTests
	{
		private const string TEST_DATA_PATH = "Assets/unity-localisation/Tests/Data";
		private const string TEST_TABLE_PATH1 = TEST_DATA_PATH + "/TestLocTable1";
		private const string TEST_TABLE_PATH2 = TEST_DATA_PATH + "/TestLocTable2";

		private const string RESOURCES_TEST_FOLDER = "DUCK.Localisation-TestData";
		private const string RESOURCES_TEST_PATH = "Resources/" + RESOURCES_TEST_FOLDER;

		private string TestDataPath
		{
			get { return Application.dataPath + "/" + RESOURCES_TEST_PATH; }
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			Assert.IsTrue(AssetDatabase.IsValidFolder(TEST_DATA_PATH));

			Directory.CreateDirectory(TestDataPath);
			AssetDatabase.Refresh();

			var locTableGuids = AssetDatabase.FindAssets("t: LocalisationTable", new []{ TEST_DATA_PATH });

			foreach (var guid in locTableGuids)
			{
				var sourcePath = AssetDatabase.GUIDToAssetPath(guid);
				var destinationPath = "Assets/" + RESOURCES_TEST_PATH + "/" + Path.GetFileName(AssetDatabase.GUIDToAssetPath(guid));
				AssetDatabase.CopyAsset(sourcePath, destinationPath);
			}

			AssetDatabase.Refresh();
		}

		[Test]
		public void ExpectLocaliserToInitialise()
		{
			Assert.DoesNotThrow(() =>
			{
				Localiser.Initialise(RESOURCES_TEST_FOLDER, "en-GB");
			});

			Assert.IsTrue(Localiser.Initialised);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			Directory.Delete(TestDataPath, true);
			AssetDatabase.Refresh();
		}
	}
}

