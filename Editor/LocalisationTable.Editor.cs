using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Editor
{
	[CustomEditor(typeof(LocalisationTable))]
	public class LocalisationTableEditor : UnityEditor.Editor
	{
		private SerializedProperty locales;
		private SerializedProperty localisationKeys;
		private SerializedProperty localisationValues;
		private SerializedProperty crcEncodingVersion;

		private LocalisationTable targetTable;
		private bool initialised;
		private bool showCRC;
		private bool emptyValuesOnly;
		private bool[] contentToggles = new bool[10];
		private readonly Dictionary<int, int> keyMap = new Dictionary<int, int>();

		private void OnEnable()
		{
			locales = serializedObject.FindProperty("locales");
			localisationKeys = serializedObject.FindProperty("keys");
			localisationValues = serializedObject.FindProperty("values");
			crcEncodingVersion = serializedObject.FindProperty("crcEncodingVersion");
			targetTable = (LocalisationTable)target;

			initialised = false;
		}

		public static void ExportEmptyValues(LocalisationTable locTable, LocalisationKeySchema currentSchema)
		{
			if (locTable == null || currentSchema == null) return;

			SaveToFile(locTable, currentSchema, true);
		}

		public static int FindEmptyValues(LocalisationTable locTable, LocalisationKeySchema currentSchema,
			bool fixMissing = false)
		{
			var emptyValues = 0;

			foreach (var category in currentSchema.categories)
			{
				if (category == null || category.keys == null)
				{
					continue;
				}

				foreach (var key in category.keys)
				{
					var locKeyCRC = CrcUtils.GetCrc(category.name, key);
					var locValue = string.Empty;

					try
					{
						locValue = locTable.GetString(locKeyCRC);
					}
					catch
					{
						if (fixMissing)
						{
							locTable.SetData(locKeyCRC, string.Empty);
						}
					}

					if (string.IsNullOrEmpty(locValue))
					{
						emptyValues++;
					}
				}
			}

			return emptyValues;
		}

		public override void OnInspectorGUI()
		{
			GUILayout.Label("Locales (languages) supported", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(locales, true);
			EditorGUI.indentLevel--;

			GUILayout.Label("Save / load", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			var currentSchema = LocalisationSettings.Current.Schema;
			if (currentSchema == null)
			{
				EditorGUILayout.HelpBox("Please populate Localisation Key Schema (or create a new one). Menu: Duck/Localisation",
					MessageType.Warning);
			}
			else
			{
				if (!initialised)
				{
					FindEmptyValues(target as LocalisationTable, currentSchema, true);
					serializedObject.Update();
					initialised = true;
				}

				if (GUILayout.Button("Load from CSV"))
				{
					LoadFromFile(target as LocalisationTable, currentSchema, emptyValuesOnly);
					serializedObject.Update();
					return;
				}

				if (GUILayout.Button("Save to CSV"))
				{
					SaveToFile(target as LocalisationTable, currentSchema, emptyValuesOnly);
					return;
				}

				if (emptyValuesOnly)
				{
					EditorGUILayout.HelpBox(
						"When loading, only keys which are empty in the localisation table will be overwritten. When Saving, only empty keys will be output.",
						MessageType.Warning);
				}
				emptyValuesOnly = EditorGUILayout.Toggle("Empty values only", emptyValuesOnly);

				DrawLocalisationContents(currentSchema);
			}
			EditorGUI.indentLevel--;

			serializedObject.ApplyModifiedProperties();
			if (GUI.changed)
			{
				AssetDatabase.SaveAssets();
			}
		}

		private void DrawLocalisationContents(LocalisationKeySchema currentSchema)
		{
			GUILayout.Label("Localisation texts", EditorStyles.boldLabel);

			showCRC = EditorGUILayout.Toggle("Show CRC values", showCRC);
			EditorGUILayout.LabelField("CRC version: ", crcEncodingVersion.intValue.ToString(), EditorStyles.helpBox);

			if (currentSchema.categories.Length > contentToggles.Length)
			{
				contentToggles = new bool[currentSchema.categories.Length];
			}

			keyMap.Clear();
			for (var i = 0; i < localisationKeys.arraySize; i++)
			{
				// mapping of locKey to its index in the keysArray (i.e. Array.IndexOf lookup-table)
				keyMap.Add(localisationKeys.GetArrayElementAtIndex(i).intValue, i);
			}

			var updateKeyEncoding = (crcEncodingVersion.intValue >= 0 &&
			                         crcEncodingVersion.intValue != CrcUtils.KEY_CRC_ENCODING_VERSION);

			for (var i = 0; i < currentSchema.categories.Length; i++)
			{
				var category = currentSchema.categories[i];

				contentToggles[i] = EditorGUILayout.Foldout(contentToggles[i], category.name);
				if (contentToggles[i] || updateKeyEncoding)
				{
					EditorGUI.indentLevel++;
					for (var j = 0; j < category.keys.Length; j++)
					{
						var locKeyCRC =
							CrcUtils.GetCrcWithEncodingVersion(category.name, category.keys[j], crcEncodingVersion.intValue);

						if (contentToggles[i])
						{
							if (!showCRC)
							{
								EditorGUILayout.BeginHorizontal();
							}
							EditorGUILayout.LabelField(
								string.Format("{0} {1}", category.keys[j], showCRC ? "(" + locKeyCRC + ")" : string.Empty),
								EditorStyles.boldLabel, GUILayout.MaxWidth(showCRC ? 200f : 140f));
						}

						var currentValue = string.Empty;
						SerializedProperty currentValueProperty = null;

						if (keyMap.ContainsKey(locKeyCRC))
						{
							var currentArrayIndex = keyMap[locKeyCRC];

							if (updateKeyEncoding)
							{
								keyMap.Remove(locKeyCRC);
								locKeyCRC = CrcUtils.GetCrc(category.name, category.keys[j]);
								localisationKeys.GetArrayElementAtIndex(currentArrayIndex).intValue = locKeyCRC;
								keyMap.Add(locKeyCRC, currentArrayIndex);
							}

							currentValueProperty = localisationValues.GetArrayElementAtIndex(currentArrayIndex);
							currentValue = currentValueProperty.stringValue;
						}

						if (contentToggles[i])
						{
							var newValue = EditorGUILayout.DelayedTextField(currentValue);
							if (!showCRC)
							{
								EditorGUILayout.EndHorizontal();
							}

							if (newValue != currentValue)
							{
								if (currentValueProperty != null)
								{
									currentValueProperty.stringValue = newValue;
								}
								else
								{
									localisationKeys.arraySize++;
									localisationValues.arraySize++;
									localisationKeys.GetArrayElementAtIndex(localisationKeys.arraySize - 1).intValue = locKeyCRC;
									localisationValues.GetArrayElementAtIndex(localisationValues.arraySize - 1).stringValue = newValue;
								}
							}
						}
					}

					EditorGUI.indentLevel--;
				}
			}

			if (updateKeyEncoding)
			{
				crcEncodingVersion.intValue = CrcUtils.KEY_CRC_ENCODING_VERSION;
				Debug.Log(string.Format("Localisation table '{0}' updated key CRC encoding to latest version: {1}",
					targetTable.name, crcEncodingVersion.intValue));
			}

			serializedObject.ApplyModifiedProperties();
		}

		/*
		 * FORMAT:
		 * Category,Key,Content
		 * Category,Key,"Complex, Content"
		 */
		private static void LoadFromFile(
			LocalisationTable locTable,
			LocalisationKeySchema currentSchema,
			bool emptyValuesOnly = false)
		{
			if (locTable == null) throw new ArgumentNullException(nameof(locTable));
			if (currentSchema == null) throw new ArgumentNullException(nameof(currentSchema));

			var filters = new []
			{
				"CSV files", "csv",
				"Text files", "txt",
				"All files", ""
			};

			var path = EditorUtility.OpenFilePanelWithFilters("Localisation table contents", Application.dataPath, filters);
			LoadFromFile(locTable, currentSchema, path, emptyValuesOnly);
		}

		/*
		 * FORMAT:
		 * Category,Key,Content
		 * Category,Key,"Complex, Content"
		 */
		public static void LoadFromFile(
			LocalisationTable locTable,
			LocalisationKeySchema currentSchema,
			string path,
			bool emptyValuesOnly = false)
		{
			if (locTable == null) throw new ArgumentNullException(nameof(locTable));
			if (currentSchema == null) throw new ArgumentNullException(nameof(currentSchema));
			if (path == null) throw new ArgumentNullException(nameof(path));

			try
			{
				const char separator = ',';

				var newData = new Dictionary<int, string>();

				Func<string, string> cleanInput = input => input.Replace("\t", string.Empty);

				var lines = CsvParser.Parse(File.ReadAllText(path));
				foreach (var line in lines)
				{
					if (line.Count < 3) continue;

					var category = cleanInput(line[0]);
					var key = cleanInput(line[1]);
					var value = cleanInput(line[2]);

					var lookup = currentSchema.FindKey(category, key);
					if (!lookup.IsValid)
					{
						continue;
					}

					var keyCRC = CrcUtils.GetCrc(category, key);
					if (newData.ContainsKey(keyCRC))
					{
						continue;
					}

					newData.Add(keyCRC, value);
				}

				locTable.SetData(newData, emptyValuesOnly);
				EditorUtility.SetDirty(locTable);
				AssetDatabase.SaveAssets();

				Debug.Log(string.Format("Data populated from file: {0} {1}", path,
					emptyValuesOnly ? "(empty only)" : string.Empty));
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Could not load from CSV format: {0}", e.Message));
			}
		}

		public static void SaveToFile(LocalisationTable locTable, LocalisationKeySchema currentSchema,
			bool emptyValuesOnly = false)
		{
			var path = EditorUtility.SaveFilePanel("Save localisation table", Application.dataPath,
				$"{locTable.name}{(emptyValuesOnly ? "-0" : string.Empty)}.csv", "csv");

			if (string.IsNullOrEmpty(path)) return;

			Func<string, string, string, string> cleanOutput = (csvKey1, csvKey2, csvValue) =>
				$"{csvKey1},{csvKey2},\"{csvValue}\"";

			if (locTable.CRCEncodingVersion != CrcUtils.KEY_CRC_ENCODING_VERSION)
			{
				Debug.LogError(string.Format(
					"Table encoding version ({0}) does not match current version (1) - please update the table before trying to export anything.",
					locTable.CRCEncodingVersion));
				return;
			}

			try
			{
				if (File.Exists(path))
				{
					File.Delete(path);
				}

				var streamWriter = new StreamWriter(File.OpenWrite(path));

				foreach (var category in currentSchema.categories)
				{
					foreach (var key in category.keys)
					{
						var locKeyCRC = CrcUtils.GetCrc(category.name, key);
						var locValue = string.Empty;

						try
						{
							locValue = locTable.GetString(locKeyCRC);
						}
						catch
						{
							// ignored
						}

						if (string.IsNullOrEmpty(locValue) || !emptyValuesOnly)
						{
							streamWriter.WriteLine(cleanOutput(category.name, key, locValue));
						}
					}
				}

				streamWriter.Close();
				Debug.Log(string.Format("Contents of table '{0}' written to: {1} {2}", locTable.name, path,
					emptyValuesOnly ? "(empty only)" : string.Empty));
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Could not save to CSV: {0}", e.Message));
			}
		}
	}
}