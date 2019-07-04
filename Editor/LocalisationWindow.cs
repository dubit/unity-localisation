﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Editor
{
	public class LocalisationWindow : EditorWindow
	{
		// Code generation parameters for the Loc bridge class
		private const string CONFIG_REPLACE_SUFFIX = ".old";
		private const string CONFIG_LOC_CLASS_NAME = "Loc";

		public static LocalisationKeySchema CurrentSchema { get; private set; }

		// Avoid the editor window losing this reference whenever Unity is refreshed
		private LocalisationKeySchema localSchema;

		private TextAsset configTemplate;
		private Dictionary<string, string> tablePaths;
		private bool tablesFoldout;
		private bool[] tableFoldouts = new bool[10];
		private LocalisationTableMetaData[] metaData = new LocalisationTableMetaData[10];

		private struct LocalisationTableMetaData
		{
			public int missingValues;
			public int keyEncodingVersion;

			public bool HasProblem { get { return missingValues > 0 || keyEncodingVersion != CrcUtils.KEY_CRC_ENCODING_VERSION; } }
		}

		[MenuItem("DUCK/Localisation")]
		public static void ShowWindow()
		{
			GetWindow(typeof(LocalisationWindow), false, "Localisation");
		}

		private void OnEnable()
		{
			if (localSchema == null)
			{
				localSchema = CurrentSchema;
			}
		}

		private void OnGUI()
		{
			GUILayout.Label("Key schema", EditorStyles.boldLabel);

			if (localSchema == null)
			{
				EditorGUILayout.HelpBox(
					"A key schema is a list of all the localisation keys in the project - generate a new one using the Assets menu, " +
					"and populate it with all the keys you need, or drag an existing asset into the field above.",
					MessageType.Info);

				var schema =
					(LocalisationKeySchema)EditorGUILayout.ObjectField("Key schema", CurrentSchema, typeof(LocalisationKeySchema),
						false);
				if (schema != null)
				{
					localSchema = schema;
				}

				if (GUILayout.Button("Create new empty key schema"))
				{
					var asset = CreateInstance<LocalisationKeySchema>();
					var path = "LocalisationKeySchema.asset";
					ProjectWindowUtil.CreateAsset(asset, path);

					Debug.Log("Created new KeySchema.asset in Resources/");
				}
			}
			else
			{
				var numberOfCategories = (localSchema.categories == null) ? 0 : localSchema.categories.Length;
				EditorGUILayout.HelpBox(
					string.Format("Key schema supplied ({0} categories defined)", numberOfCategories), MessageType.None);
				if (GUILayout.Button("Remove schema"))
				{
					CurrentSchema = null;
					localSchema = null;
				}
			}

			if (localSchema != null && CurrentSchema == null)
			{
				CurrentSchema = localSchema;
			}

			if (localSchema == null)
			{
				EditorGUILayout.HelpBox("No schema supplied. Once your schema is ready, supply it above.", MessageType.Warning);
				return;
			}

			GUILayout.Label("Localisation config / setup", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			EditorGUILayout.HelpBox(
				"Click Generate to build a class file which can be referenced application-side to get localisation keys and other data in-game.",
				MessageType.Info);
			EditorGUILayout.LabelField("Output filename:", LocalisationSettings.Current.CodeGenerationFilePath);
			EditorGUILayout.LabelField("Localisation data path:", LocalisationSettings.Current.LocalisationTableFolder + "/");

			EditorGUILayout.LabelField("Template required to generate localisation class");
			EditorGUILayout.BeginHorizontal();
			configTemplate =
				(TextAsset)EditorGUILayout.ObjectField("Config template:", configTemplate, typeof(TextAsset), false);

			EditorGUI.BeginDisabledGroup(configTemplate == null);
			if (GUILayout.Button("Generate Config Class"))
			{
				GenerateLocalisationConfig();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUI.indentLevel--;
			EditorGUILayout.EndHorizontal();

			GUILayout.Label("Localisation tables", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Find / Refresh"))
			{
				FindAllTables();
			}

			EditorGUI.BeginDisabledGroup(false);
			if (GUILayout.Button("Create new"))
			{
				GenerateLocalisationTable();
			}
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndHorizontal();

			if (tablePaths != null)
			{
				DrawLocalisationTables();
			}
			else
			{
				EditorGUILayout.HelpBox(
					string.Format(
						"Click 'find' to search for localisation table assets in {0}",
						LocalisationSettings.Current.LocalisationTableFolder),
					MessageType.Info);
			}
		}

		private void FindAllTables()
		{
			var localisationTables = AssetDatabase.FindAssets($"t:{nameof(LocalisationTable)}")
				.Select(AssetDatabase.GUIDToAssetPath)
				.Where(p => !p.Contains("Tests/Data/Test"));

			tablePaths = localisationTables.ToDictionary(Path.GetFileName, v => v);

			if (metaData.Length < tablePaths.Keys.Count)
			{
				metaData = new LocalisationTableMetaData[tablePaths.Keys.Count];
			}

			var i = 0;
			foreach (var tablePath in tablePaths)
			{
				var locTable = AssetDatabase.LoadAssetAtPath<LocalisationTable>(tablePath.Value);
				if (locTable != null)
				{
					metaData[i].missingValues = LocalisationTableEditor.FindEmptyValues(locTable, CurrentSchema, true);
					metaData[i].keyEncodingVersion = locTable.CRCEncodingVersion;

					Resources.UnloadAsset(locTable);
				}
				i++;
			}
		}

		private void DrawLocalisationTables()
		{
			var errorLabelStyle = new GUIStyle(EditorStyles.label) {normal = {textColor = Color.red}};
			var okLabelStyle = new GUIStyle(EditorStyles.label) {normal = {textColor = Color.black}};
			var fineLabelStyle = new GUIStyle(EditorStyles.label) {normal = {textColor = Color.Lerp(Color.green, Color.black, 0.5f)}};
			var errorIconStyle = new GUIStyle(errorLabelStyle) {fontStyle = FontStyle.Bold};
			var fineIconStyle = new GUIStyle(fineLabelStyle) {fontStyle = FontStyle.Bold};

			var tableCount = tablePaths.Keys.Count;
			EditorGUI.BeginDisabledGroup(tableCount == 0);
			tablesFoldout = EditorGUILayout.Foldout(tablesFoldout, string.Format("Tables found: {0}", tableCount));
			if (tablesFoldout && tableCount > 0)
			{
				if (tableFoldouts.Length < tableCount)
				{
					tableFoldouts = new bool[tableCount];
				}

				EditorGUI.indentLevel++;
				var i = 0;
				foreach (var tablePath in tablePaths)
				{
					EditorGUILayout.BeginHorizontal();
					tableFoldouts[i] = EditorGUILayout.Foldout(tableFoldouts[i],
						$"{tablePath.Key}\t{tablePath.Value}");
					if (metaData[i].HasProblem)
					{
						EditorGUILayout.LabelField("✕", errorIconStyle, GUILayout.MaxWidth(40f));
					}
					else
					{
						EditorGUILayout.LabelField("✓", fineIconStyle, GUILayout.MaxWidth(40f));
					}
					EditorGUILayout.EndHorizontal();

					if (tableFoldouts[i])
					{
						if (!metaData[i].HasProblem)
						{
							EditorGUI.indentLevel++;
							EditorGUILayout.LabelField("No errors detected.", fineLabelStyle);
							EditorGUI.indentLevel--;
							i++;
							continue;
						}

						var wrongKeyEncoding = metaData[i].keyEncodingVersion != CrcUtils.KEY_CRC_ENCODING_VERSION;
						EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
						EditorGUI.BeginDisabledGroup(!wrongKeyEncoding);
						GUILayout.Label(string.Format("Encoding version: {0}", metaData[i].keyEncodingVersion),
							wrongKeyEncoding ? errorLabelStyle : okLabelStyle, GUILayout.MaxWidth(160f));
						if (GUILayout.Button("Select (fix problem)", GUILayout.MaxWidth(160f)))
						{
							var assetPath = tablePath.Value;
							Selection.activeObject = AssetDatabase.LoadAssetAtPath<LocalisationTable>(assetPath);
						}
						EditorGUILayout.EndHorizontal();
						EditorGUI.EndDisabledGroup();

						var missingValues = metaData[i].missingValues;
						EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
						EditorGUI.BeginDisabledGroup(missingValues == 0);
						GUILayout.Label(string.Format("Empty values: {0}", missingValues),
							missingValues > 0 ? errorLabelStyle : okLabelStyle, GUILayout.MaxWidth(160f));
						if (GUILayout.Button("Export missing values", GUILayout.MaxWidth(160f)))
						{
							var locTable = AssetDatabase.LoadAssetAtPath<LocalisationTable>(tablePath.Value);
							if (locTable != null)
							{
								LocalisationTableEditor.ExportEmptyValues(locTable, CurrentSchema);
								Resources.UnloadAsset(locTable);
							}
							else
							{
								Debug.LogError(string.Format("Could not load table: Resources/{0}.asset", tablePath.Value));
							}
						}
						EditorGUILayout.EndHorizontal();
						EditorGUI.EndDisabledGroup();
					}
					i++;
				}
				EditorGUI.indentLevel--;
			}
			EditorGUI.EndDisabledGroup();
		}

		/// <summary>
		/// Generates the LocalisationConfig class from the supplied key schema - this allows application-side code to access the available keys
		/// and request localised content with them. Also exports some cross functionality such as the Localisation folder path which the application
		/// will need.
		/// </summary>
		public void GenerateLocalisationConfig()
		{
			if (configTemplate == null)
			{
				Debug.LogError("No template supplied - can't generate localisation config file");
				return;
			}

			var filePath = $"{Application.dataPath}/{LocalisationSettings.Current.CodeGenerationFilePath}";
			var directoryPath = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			if (File.Exists(filePath))
			{
				var renamedPath = filePath + CONFIG_REPLACE_SUFFIX;
				if (File.Exists(renamedPath))
				{
					File.Delete(renamedPath);
				}

				File.Delete(filePath);
			}

			Debug.Log("Writing to " + filePath);
			var totalKeys = 0;

			var stringBuilder = new StringBuilder();
			foreach (var category in CurrentSchema.categories)
			{
				stringBuilder.AppendLine(string.Format("\t\tpublic enum {0}", category.name));
				stringBuilder.AppendLine("\t\t{");
				for (var i = 0; i < category.keys.Length; i++)
				{
					var key = category.keys[i];

					stringBuilder.Append("\t\t\t" + key + " = " + CrcUtils.GetCrc(category.name, key));
					if (i < category.keys.Length - 1)
					{
						stringBuilder.AppendLine(",");
					}
					else
					{
						stringBuilder.AppendLine();
					}
				}
				stringBuilder.AppendLine("\t\t}");
				totalKeys += category.keys.Length;
			}

			var outputText = configTemplate.text;
			outputText = outputText.Replace("{CLASS}", CONFIG_LOC_CLASS_NAME);
			outputText = outputText.Replace("{KEYS}", stringBuilder.ToString());
			File.WriteAllText(filePath, outputText);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			Debug.Log(string.Format("Successfully generated {0} localisation keys.", totalKeys));
		}

		public void GenerateLocalisationTable()
		{
			var relativePath = $"{LocalisationSettings.Current.LocalisationTableFolder}";
			var directoryPath = Path.GetDirectoryName($"{Application.dataPath}/{relativePath}");
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			var path = $"Assets/{relativePath}";
			if (!AssetDatabase.IsValidFolder(path))
			{
				var parts = relativePath.Split('/');
				var constructedPath = "Assets";
				foreach (var part in parts)
				{
					var lastConstructedPath = constructedPath;
					constructedPath += "/" + part;
					if (!AssetDatabase.IsValidFolder(constructedPath))
					{
						AssetDatabase.CreateFolder(lastConstructedPath, part);
					}
				}
			}

			var asset = CreateInstance<LocalisationTable>();
			Debug.Log($"Created new localisation file at path: {path}");
			ProjectWindowUtil.CreateAsset(asset, path + "/New Localisation Table.asset");
		}
	}
}