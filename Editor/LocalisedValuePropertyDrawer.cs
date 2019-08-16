using System;
using System.Linq;
using DUCK.Localisation.Editor.Window;
using DUCK.Localisation.LocalisedObjects;
using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Editor
{
	[CustomPropertyDrawer(typeof(LocalisedValue))]
	public class LocalisedValuePropertyDrawer: PropertyDrawer
	{
		private const float LINE_HEIGHT = 18;
		private const float INDENTATION = 10;

		private bool isInitialized;
		private Vector2Int selectedKeyAndCategory;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return LINE_HEIGHT * 3f;
		}

		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(rect, label, property);

			// Draw label
			EditorGUI.LabelField(rect, label);

			// Prepare data
			var settings = LocalisationSettings.Current;
			if (settings == null)
			{
				EditorGUILayout.HelpBox("Please create a LocalisationSettings object (or create a new one). Menu: DUCK/Localisation",
					MessageType.Error);
			}
			else
			{
				var error = DrawProperty(rect, property, settings);
				if (!string.IsNullOrEmpty(error))
				{
					EditorGUILayout.HelpBox(error, MessageType.Error);
				}
			}

			EditorGUI.EndProperty();
		}

		private string DrawProperty(Rect rect, SerializedProperty property, LocalisationSettings settings)
		{
			var currentSchema = settings.Schema;
			var keyName = property.FindPropertyRelative("keyName");
			var categoryName = property.FindPropertyRelative("categoryName");
			var localisationKey = property.FindPropertyRelative("localisationKey");

			var keyLookup = currentSchema.FindKey(
				categoryName.stringValue,
				keyName.stringValue);

			if (!isInitialized)
			{
				selectedKeyAndCategory = keyLookup.IsValid ?
					new Vector2Int
					{
						x = Math.Max(0, keyLookup.keyIndex),
						y = Math.Max(0, keyLookup.categoryIndex),
					}
					: Vector2Int.zero;

				isInitialized = true;
			}

			// CATEGORIES
			var categories = currentSchema.categories;
			if (categories.Length == 0) return "No categories found. Add some to the schema";

			var categoryNames = categories.Select(c => c.name).ToArray();
			var categoryIndex = selectedKeyAndCategory.y;
			categoryIndex = categoryIndex < 0 || categoryIndex >= categoryNames.Length ?
				0 : selectedKeyAndCategory.y;

			var categoryRect = new Rect(rect)
			{
				x = rect.x + INDENTATION,
				y = rect.y + LINE_HEIGHT,
				height = LINE_HEIGHT,
				width = rect.width - INDENTATION,
			};
			categoryIndex = EditorGUI.Popup(categoryRect, "Category", categoryIndex, categoryNames);
			selectedKeyAndCategory.y = categoryIndex;

			var category = currentSchema.categories[categoryIndex];

			if (category.keys.Length == 0) return "This category is empty";

			// KEYS
			var locKeyIndex = selectedKeyAndCategory.x;
			if (locKeyIndex < 0 || locKeyIndex >= category.keys.Length)
			{
				locKeyIndex = 0;
			}
			var keysRect = new Rect(rect)
			{
				x = rect.x + INDENTATION,
				y = rect.y + LINE_HEIGHT * 2,
				height = LINE_HEIGHT,
				width = rect.width - INDENTATION
			};
			locKeyIndex = EditorGUI.Popup(keysRect, "Key", locKeyIndex,  category.keys);
			selectedKeyAndCategory.x = locKeyIndex;

			var selectedLocKey = category.keys[locKeyIndex];
			var savedLocKey = keyName.stringValue;

			var crc = CrcUtils.GetCrc(
				category.name,
				selectedLocKey);

			if (localisationKey.intValue != crc)
			{
				keyName.stringValue = selectedLocKey;
				categoryName.stringValue = category.name;

				localisationKey.intValue = CrcUtils.GetCrc(
					categoryName.stringValue,
					keyName.stringValue);

				savedLocKey = selectedLocKey;
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
					return string.Format("Key contents ({0}/{1}:{2}) not found in the schema - please set it to a new value",
							categoryName.stringValue, keyName.stringValue, localisationKey.intValue);

					style.normal.textColor = Color.red;
				}

				GUILayout.Label(string.Format("{0}/{1} ({2}) {3}",
					categoryName.stringValue,
					keyName.stringValue,
					localisationKey.intValue,
					savedLocKey != selectedLocKey ? "*" : string.Empty
				), style);
			}

			return null;
		}
	}
}