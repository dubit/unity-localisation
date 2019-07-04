using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Editor.Window
{
	public partial class LocalisationWindow
	{
		public class TablesGui
		{
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

			public void Draw()
			{
				EditorGUILayout.Space();
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
						metaData[i].missingValues =
							LocalisationTableEditor.FindEmptyValues(locTable, CurrentSchema, true);
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
				var fineLabelStyle = new GUIStyle(EditorStyles.label)
					{normal = {textColor = Color.Lerp(Color.green, Color.black, 0.5f)}};
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
									Debug.LogError(string.Format("Could not load table: Resources/{0}.asset",
										tablePath.Value));
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
}