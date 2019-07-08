using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Editor.Window
{
	public partial class LocalisationWindow
	{
		public class TablesGui
		{
			private List<TableDrawer> tableDrawers;

			public void Refresh()
			{
				var localisationTablePaths = AssetDatabase.FindAssets($"t:{nameof(LocalisationTable)}")
					.Select(AssetDatabase.GUIDToAssetPath)
					.Where(p => !p.Contains("Tests/Data/Test"))
					.ToList();

				tableDrawers = new List<TableDrawer>(localisationTablePaths.Count);

				foreach (var tablePath in localisationTablePaths)
				{
					var table = AssetDatabase.LoadAssetAtPath<LocalisationTable>(tablePath);
					tableDrawers.Add(new TableDrawer(table));
					Resources.UnloadAsset(table);
				}
			}

			public void Draw()
			{
				// Draw title
				EditorGUILayout.Space();
				GUILayout.Label("Localisation tables", EditorStyles.boldLabel);

				// Draw refresh/create buttons
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Refresh")) Refresh();
				if (GUILayout.Button("Create new")) CreateNewTable();
				EditorGUILayout.EndHorizontal();

				if (tableDrawers == null)
				{
					Refresh();
					return;
				}

				foreach (var tableDrawer in tableDrawers)
				{
					tableDrawer.Draw();
				}
			}

			private void CreateNewTable()
			{
			}
		}
	}
}