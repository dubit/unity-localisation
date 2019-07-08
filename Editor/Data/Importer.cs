using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Editor.Data
{
	public static class Importer
	{
		/// <summary>
		/// Import from csv file, in the format (Category,Key,Content)
		/// </summary>
		/// <param name="path">The path to the csv file to import from</param>
		/// <param name="locTable">The localisation table to import to</param>
		/// <param name="currentSchema">The current schema to import against</param>
		/// <param name="emptyValuesOnly"> If set to true, the import will only import values that are currently empty,
		/// and will not override existing values, defaults to false.
		/// </param>
		/// <param name="saveAssets">If set to true the LocalisationTable object will be saved to disk.
		/// Otherwise it will import and the changes will be in memory (and therefore require saving). defaults to true
		/// </param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void ImportFromCsv(
			string path,
			LocalisationTable locTable,
			LocalisationKeySchema currentSchema,
			bool emptyValuesOnly = false,
			bool saveAssets = true)
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

				Debug.Log($"Data populated from file: {path} {(emptyValuesOnly ? "(empty only)" : string.Empty)}");
			}
			catch (Exception e)
			{
				Debug.LogError($"Could not load from CSV format: {e.Message}");
			}
		}
	}
}