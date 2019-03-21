using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Editor
{
	[CustomEditor(typeof(LocalisationKeySchema))]
	public class LocalisationKeySchemaEditor : UnityEditor.Editor
	{
		private SerializedProperty categories;

		private bool[] contentToggles = new bool[10];

		private readonly Dictionary<string, int> duplicateCategories = new Dictionary<string, int>();
		private readonly Dictionary<string, int> duplicateKeys = new Dictionary<string, int>();

		private void OnEnable()
		{
			categories = serializedObject.FindProperty("categories");
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox(
				"Define your localisation keys by category. Each category can be for a different type of content (e.g. Text, Image or Audio).",
				MessageType.Info);

			var arraySize = categories.arraySize;
			if (contentToggles.Length < arraySize)
			{
				contentToggles = new bool[arraySize];
			}

			duplicateCategories.Clear();
			var blankNames = false;

			var schema = target as LocalisationKeySchema;
			for (var i = 0; i < arraySize; i++)
			{
				if (schema != null && schema.categories[i] == null)
				{
					schema.categories[i] = new LocalisationKeySchema.LocalisationKeyCategory();
					serializedObject.Update();
				}

				var categoryProperty = categories.GetArrayElementAtIndex(i);

				var nameProperty = categoryProperty.FindPropertyRelative("name");
				var keysProperty = categoryProperty.FindPropertyRelative("keys");

				var labelContent = string.Format("{0} ({1})", nameProperty.stringValue, keysProperty.arraySize.ToString());

				if (!duplicateCategories.ContainsKey(nameProperty.stringValue))
				{
					duplicateCategories[nameProperty.stringValue] = 0;
				}
				duplicateCategories[nameProperty.stringValue]++;

				if (string.IsNullOrEmpty(nameProperty.stringValue))
				{
					blankNames = true;
				}

				EditorGUI.indentLevel++;
				contentToggles[i] = EditorGUILayout.Foldout(contentToggles[i], labelContent);
				if (contentToggles[i])
				{
					if (DrawCategory(nameProperty, keysProperty, i))
					{
						EditorGUI.indentLevel--;
						break;
					}

					EditorGUI.indentLevel--;
				}

				EditorGUI.indentLevel--;
			}

			var enumerator = duplicateCategories.GetEnumerator();
			for (var e = enumerator; enumerator.MoveNext();)
			{
				if (e.Current.Value > 1)
				{
					EditorGUILayout.HelpBox(
						string.Format("The name '{0}' belongs to {1} categories - they must be unique!", e.Current.Key, e.Current.Value),
						MessageType.Error);
				}
			}
			enumerator.Dispose();

			if (blankNames)
			{
				EditorGUILayout.HelpBox("Categories without a name exist - please populate them.", MessageType.Error);
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(string.Format("Count: {0}", categories.arraySize));
			if (GUILayout.Button("Add Category"))
			{
				categories.InsertArrayElementAtIndex(categories.arraySize);

				var newCategory = categories.GetArrayElementAtIndex(categories.arraySize - 1);
				newCategory.FindPropertyRelative("name").stringValue = "NewCategory";
				newCategory.FindPropertyRelative("keys").arraySize = 0;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			if (GUILayout.Button("Import Schema"))
			{
				ImportSchema(schema);
			}

			serializedObject.ApplyModifiedProperties();
		}

		private bool DrawCategory(SerializedProperty nameProperty, SerializedProperty keysProperty, int index)
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.BeginHorizontal();
			var newName = EditorGUILayout.TextField("Name", nameProperty.stringValue);
			newName = newName.Trim();
			newName = Regex.Replace(newName, @"[^a-zA-Z0-9\-]", string.Empty);
			newName = Regex.Replace(newName, @"^[\d-]*\s*", string.Empty);
			if (nameProperty.stringValue != newName)
			{
				nameProperty.stringValue = newName;
			}

			var buttonStyle = new GUIStyle(GUI.skin.button) {normal = {textColor = Color.red}};
			if (GUILayout.Button("DELETE", buttonStyle))
			{
				categories.DeleteArrayElementAtIndex(index);
				return true;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel++;

			duplicateKeys.Clear();
			var blankKeys = false;

			for (var j = 0; j < keysProperty.arraySize; j++)
			{
				var keyProperty = keysProperty.GetArrayElementAtIndex(j);

				GUILayoutOption[] options = {GUILayout.MinWidth(180.0f)};
				EditorGUILayout.BeginHorizontal();

				var newKey = EditorGUILayout.TextField(string.Empty, keyProperty.stringValue, options);
				newKey = newKey.Trim();
				newKey = Regex.Replace(newKey, @"[^a-zA-Z0-9\-]", string.Empty);
				newKey = Regex.Replace(newKey, @"^[\d-]*\s*", string.Empty);
				if (keyProperty.stringValue != newKey)
				{
					keyProperty.stringValue = newKey;
				}

				if (!duplicateKeys.ContainsKey(keyProperty.stringValue))
				{
					duplicateKeys[keyProperty.stringValue] = 0;
					EditorGUILayout.LabelField(string.Empty, GUILayout.MaxWidth(80f));
				}
				else
				{
					var labelStyle = new GUIStyle(EditorStyles.boldLabel) {normal = {textColor = Color.red}};
					EditorGUILayout.LabelField("✕", labelStyle, GUILayout.MaxWidth(80f));
				}
				duplicateKeys[keyProperty.stringValue]++;

				if (string.IsNullOrEmpty(keyProperty.stringValue))
				{
					blankKeys = true;
				}

				if (GUILayout.Button("  X  ", GUILayout.MaxWidth(40f)))
				{
					keysProperty.DeleteArrayElementAtIndex(j);
					EditorGUILayout.EndHorizontal();
					break;
				}

				EditorGUILayout.EndHorizontal();

				var enumerator = duplicateKeys.GetEnumerator();
				for (var e = enumerator; enumerator.MoveNext();)
				{
					if (e.Current.Value > 1)
					{
						EditorGUILayout.HelpBox(
							string.Format("The key '{0}' exists {1} times in this category - they must be unique!", e.Current.Key,
								e.Current.Value), MessageType.Error);
					}
				}
				enumerator.Dispose();

				if (blankKeys)
				{
					EditorGUILayout.HelpBox("Blank keys exist - please populate them.", MessageType.Error);
				}
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(string.Format("Count: {0}", keysProperty.arraySize));
			if (GUILayout.Button("  +  ", GUILayout.MaxWidth(40f)))
			{
				keysProperty.arraySize++;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			return false;
		}

		private void ImportSchema(LocalisationKeySchema schema)
		{
			var filters = new []
			{
				"CSV files", "csv",
				"Text files", "txt",
				"All files", ""
			};

			var path = EditorUtility.OpenFilePanelWithFilters("Localisation Schema", Application.dataPath, filters);

			if (string.IsNullOrEmpty(path)) return;

			var newCategories = new List<string>();
			var newKeys = new Dictionary<string, List<string>>();

			try
			{
				var lines = CsvParser.Parse(File.ReadAllText(path));
				foreach (var line in lines)
				{
					var categoryName = line[0];
					var keyName = line[1];

					if (!newCategories.Contains(categoryName))
					{
						newCategories.Add(categoryName);
					}

					if (!newKeys.ContainsKey(categoryName))
					{
						newKeys.Add(categoryName, new List<string>());
					}

					var keys = newKeys[categoryName];

					if (!keys.Contains(keyName))
					{
						keys.Add(keyName);
					}
				}

				foreach (var newCategoryName in newCategories)
				{
					SerializedProperty category = null;
					for (var i = 0; i < categories.arraySize; i++)
					{
						var existingCategory = categories.GetArrayElementAtIndex(i);
						if (existingCategory.FindPropertyRelative("name").stringValue == newCategoryName)
						{
							category = existingCategory;
							Debug.Log("Category: " + newCategoryName + " - already exists");
							break;
						}
					}

					if (category == null)
					{
						categories.InsertArrayElementAtIndex(categories.arraySize);
						category = categories.GetArrayElementAtIndex(categories.arraySize - 1);
						category.FindPropertyRelative("name").stringValue = newCategoryName;
						category.FindPropertyRelative("keys").ClearArray();

						Debug.Log("Add category: " + newCategoryName);
					}

					var keys = category.FindPropertyRelative("keys");
					foreach (var newKeyName in newKeys[newCategoryName])
					{
						var keyAlreadyExists = false;
						for (var i = 0; i < keys.arraySize; i++)
						{
							var key = keys.GetArrayElementAtIndex(i);
							if (key.stringValue == newKeyName)
							{
								Debug.Log("Key: " +newCategoryName + " : " + newKeyName + " - already exists");
								keyAlreadyExists = true;
								break;
							}
						}

						if (!keyAlreadyExists)
						{
							keys.InsertArrayElementAtIndex(keys.arraySize);
							var newKey = keys.GetArrayElementAtIndex(keys.arraySize - 1);
							newKey.stringValue = newKeyName;

							Debug.Log("Add Key: " + newCategoryName + " : " + newKeyName);
						}
					}
				}

				// Adding category

			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Could not load from CSV format: {0}", e.Message));
			}
		}
	}
}