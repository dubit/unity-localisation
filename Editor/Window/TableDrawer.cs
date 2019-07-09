using System.IO;
using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Editor.Window
{
	public class TableDrawer
	{
		private const float MAX_BUTTON_SIZE = 60f;

		private readonly string tableName;
		private readonly string tablePath;

		public TableDrawer(LocalisationTable table)
		{
			tablePath = AssetDatabase.GetAssetPath(table);
			tableName = Path.GetFileName(tablePath);
		}

		public void Draw()
		{
			EditorGUILayout.BeginHorizontal("Box");

			// Table
			EditorGUILayout.LabelField(tableName, EditorStyles.label, GUILayout.MaxWidth(200f), GUILayout.MinWidth(50f));

			EditorGUILayout.Space();

			// Import
			if (GUILayout.Button("Import", GUILayout.MaxWidth(MAX_BUTTON_SIZE))) Import();
			if (GUILayout.Button("Export", GUILayout.MaxWidth(MAX_BUTTON_SIZE))) Export();
			if (GUILayout.Button("Ping", GUILayout.MaxWidth(MAX_BUTTON_SIZE))) PingTable();

			EditorGUILayout.EndHorizontal();
		}

		private void Import()
		{
			var filters = new []
			{
				"CSV files", "csv",
				"Text files", "txt",
				"All files", ""
			};

			var csvPath = EditorUtility.OpenFilePanelWithFilters(
				"Localisation table contents", Application.dataPath, filters);

			if (string.IsNullOrEmpty(csvPath)) return;

			var table = AssetDatabase.LoadAssetAtPath<LocalisationTable>(tablePath);
			LocalisationTableEditor.LoadFromFile(table, LocalisationSettings.Current.Schema, csvPath);
			Resources.UnloadAsset(table);
		}

		private void Export()
		{
			var table = AssetDatabase.LoadAssetAtPath<LocalisationTable>(tablePath);
			LocalisationTableEditor.SaveToFile(table, LocalisationSettings.Current.Schema);
			Resources.UnloadAsset(table);
		}

		private void PingTable()
		{
			var table = AssetDatabase.LoadAssetAtPath<LocalisationTable>(tablePath);
			EditorGUIUtility.PingObject(table);
			Resources.UnloadAsset(table);
		}
	}
}