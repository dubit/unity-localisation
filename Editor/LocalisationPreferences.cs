using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Editor
{
	public static class LocalisationPreferences
	{
		public static string CodeGenerationFilePath { get; private set; } = "Scripts/Localisation/LocalisationConfig.cs";
		public static string LocalisationTableFolder { get; private set; } = "Resources/Localisation";

		static LocalisationPreferences()
		{
			CodeGenerationFilePath = LoadSetting(nameof(CodeGenerationFilePath), CodeGenerationFilePath);
			LocalisationTableFolder = LoadSetting(nameof(LocalisationTableFolder), LocalisationTableFolder);
		}

#if UNITY_2018_3_OR_NEWER
		[PreferenceItem("Duck/Localisation")]
#else
		[PreferenceItem("Localisation")]
#endif
		private static void CustomPreferencesGUI()
		{
			CodeGenerationFilePath = DrawSetting(nameof(CodeGenerationFilePath), CodeGenerationFilePath);
			LocalisationTableFolder = DrawSetting(nameof(LocalisationTableFolder), LocalisationTableFolder);
		}

		private static string LoadSetting(string key, string value)
		{
			var editorPrefKey = $"localisation.{key}";
			if (EditorPrefs.HasKey(editorPrefKey))
			{
				return EditorPrefs.GetString(editorPrefKey);
			}

			return value;
		}

		private static string DrawSetting(string key, string value)
		{
			var editorPrefKey = $"localisation.{key}";

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(key));

			var newValue = EditorGUILayout.TextField(value);
			if (value != newValue)
			{
				EditorPrefs.SetString(editorPrefKey, newValue);
			}

			EditorGUILayout.EndHorizontal();

			return newValue;
		}
	}
}