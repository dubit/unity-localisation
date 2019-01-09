﻿using System;
using DUCK.Localisation.LocalisedObjects;
using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Editor
{
	[CustomEditor(typeof(AbstractLocalisedObject), true)]
	public class LocalisedObjectEditor : UnityEditor.Editor
	{
		private SerializedProperty localisationKey;
		private SerializedProperty keyName;
		private SerializedProperty categoryName;

		private int selectedCategoryIndex;
		private int selectedKeyIndex;
		private bool initialised;

		private void Reset()
		{
			initialised = false;
			Repaint();
		}

		private void OnEnable()
		{
			localisationKey = serializedObject.FindProperty("localisationKey");
			keyName = serializedObject.FindProperty("keyName");
			categoryName = serializedObject.FindProperty("categoryName");
		}

		public override void OnInspectorGUI()
		{
			var vec2int = DrawLocalisedObject(initialised, new Vector2Int(selectedKeyIndex, selectedCategoryIndex),
				(serializedObject.targetObject as AbstractLocalisedObject).ResourceType, localisationKey, keyName, categoryName);

			serializedObject.ApplyModifiedProperties();

			selectedKeyIndex = vec2int.x;
			selectedCategoryIndex = vec2int.y;

			initialised = true;
		}

		public static Vector2Int DrawLocalisedObject(
			bool initialised,
			Vector2Int selectedKeyAndCategory,
			LocalisedResourceType resourceType,
			SerializedProperty localisationKey,
			SerializedProperty keyName,
			SerializedProperty categoryName,
			bool autosave = false)
		{
			var currentSchema = LocalisationEditor.CurrentSchema;

			if (resourceType == LocalisedResourceType.Unknown)
			{
				EditorGUILayout.HelpBox("This component doesn't specify a resource type. Please specify one by adding the [ResourceType] attribute to the class",
					MessageType.Error);
			}
			else if (currentSchema == null)
			{
				EditorGUILayout.HelpBox("Please populate Localisation Key Schema (or create a new one). Menu: DUCK/Localisation",
					MessageType.Warning);
			}
			else
			{
				var keyLookup = currentSchema.FindKey(categoryName.stringValue, keyName.stringValue);

				if (!initialised)
				{
					if (keyLookup.IsValid)
					{
						selectedKeyAndCategory = new Vector2Int
						{
							x = Math.Max(0, keyLookup.keyIndex),
							y = Math.Max(0, keyLookup.categoryIndex)
						};
					}
					else
					{
						selectedKeyAndCategory = Vector2Int.zero;
					}

					initialised = true;
				}

				var availableCategories = LocalisationEditorUtils.GetAvailableCategories(resourceType);
				var categoryNames = new string[availableCategories.Length];
				for (var i = 0; i < categoryNames.Length; i++)
				{
					if (availableCategories[i] > currentSchema.categories.Length)
					{
						categoryNames[i] = "< REMOVED? >";
					}
					else
					{
						categoryNames[i] = currentSchema.categories[availableCategories[i]].name;
					}
				}

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Category", GUILayout.MaxWidth(100f));
				var categoryIndex = selectedKeyAndCategory.y;
				if (categoryIndex < 0 || categoryIndex >= availableCategories.Length)
				{
					categoryIndex = 0;
				}
				categoryIndex = EditorGUILayout.Popup(categoryIndex, categoryNames);
				selectedKeyAndCategory.y = categoryIndex;
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Key", GUILayout.MaxWidth(100f));
				var category = currentSchema.categories[availableCategories[categoryIndex]];
				var locKeyIndex = selectedKeyAndCategory.x;
				if (locKeyIndex < 0 || locKeyIndex >= category.keys.Length)
				{
					locKeyIndex = 0;
				}
				locKeyIndex = EditorGUILayout.Popup(locKeyIndex, category.keys);
				selectedKeyAndCategory.x = locKeyIndex;
				EditorGUILayout.EndHorizontal();

				var selectedLocKey = category.keys[locKeyIndex];
				var savedLocKey = keyName.stringValue;

				if (savedLocKey != selectedLocKey)
				{
					if (autosave || GUILayout.Button("SET"))
					{
						keyName.stringValue = selectedLocKey;
						categoryName.stringValue = category.name;

						localisationKey.intValue = LocalisationEditor.GetCRC(categoryName.stringValue, keyName.stringValue);

						savedLocKey = selectedLocKey;
					}
				}

				if (string.IsNullOrEmpty(savedLocKey))
				{
					GUILayout.Label("< NONE >", EditorStyles.boldLabel);
				}
				else
				{
					var style = new GUIStyle(EditorStyles.boldLabel);

					if (!keyLookup.IsValid)
					{
						EditorGUILayout.HelpBox(
							string.Format("Key contents ({0}/{1}:{2}) not found in the schema - please set it to a new value",
								categoryName.stringValue, keyName.stringValue, localisationKey.intValue), MessageType.Error);

						style.normal.textColor = Color.red;
					}

					GUILayout.Label(string.Format("{0}/{1} ({2}) {3}",
						categoryName.stringValue,
						keyName.stringValue,
						localisationKey.intValue,
						savedLocKey != selectedLocKey ? "*" : string.Empty
					), style);
				}
			}

			return selectedKeyAndCategory;
		}
	}
}