using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Tests
{
	public class LocalisationTests
	{
		private const string TEST_DATA_PATH = "Assets/unity-localisation/Tests/Data";
		private const string TEST_TABLE_PATH1 = TEST_DATA_PATH + "/TestLocTable1";
		private const string TEST_TABLE_PATH2 = TEST_DATA_PATH + "/TestLocTable2";

		private const string RESOURCES_PATH = "Resources/";

		private bool didResourcesExist;
		private string[] copiedLocalisationTableGuids;

		private string ResourcesDataPath
		{
			get { return Application.dataPath + "/" + RESOURCES_PATH; }
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			Assert.IsTrue(AssetDatabase.IsValidFolder(TEST_DATA_PATH));

			didResourcesExist = Directory.Exists(ResourcesDataPath);
			if (!didResourcesExist)
			{
				Directory.CreateDirectory(ResourcesDataPath);
			}

			var localisationTableGuids = AssetDatabase.FindAssets("t: LocalisationTable", new []{ TEST_DATA_PATH });

			copiedLocalisationTableGuids = new string[localisationTableGuids.Length];

			for (var i = 0; i < localisationTableGuids.Length; i++)
			{
				var sourcePath = AssetDatabase.GUIDToAssetPath(localisationTableGuids[i]);
				var destinationPath = "Assets/" + RESOURCES_PATH + Path.GetFileName(AssetDatabase.GUIDToAssetPath(localisationTableGuids[i]));

				if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
				{
					throw new IOException("Could not create test asset: " + destinationPath);
				}

				copiedLocalisationTableGuids[i] = AssetDatabase.AssetPathToGUID(destinationPath);
			}

			AssetDatabase.Refresh();
		}

		[Test]
		public void ExpectLocaliserToInitialise()
		{
			Assert.DoesNotThrow(() =>
			{
				Localiser.Initialise("", "en-GB");
			});

			Assert.IsTrue(Localiser.Initialised);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			foreach (var guid in copiedLocalisationTableGuids)
			{
				AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
			}

			if (!didResourcesExist)
			{
				Directory.Delete(ResourcesDataPath, recursive: true);
				File.Delete(Application.dataPath + "Resources.meta");
			}

			AssetDatabase.Refresh();
		}
	}
}

