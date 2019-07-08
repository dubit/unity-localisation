using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Editor.Window
{
	public partial class LocalisationWindow
	{
		public class CodeGenerationGui
		{
			private TextAsset configTemplate;

			public void Draw()
			{
				// Draw title
				EditorGUILayout.Space();
				GUILayout.Label("Generate localisation consts", EditorStyles.boldLabel);

				// Indent
				EditorGUI.indentLevel++;
				{
					// Show help box
					EditorGUILayout.HelpBox(
						"Click Generate to build a class file which can be referenced application-side to get localisation keys and other data in-game.",
						MessageType.Info);

					// Show paths
					EditorGUILayout.LabelField("Output filename:", LocalisationSettings.Current.CodeGenerationFilePath);
					EditorGUILayout.LabelField("Localisation data path:",
						LocalisationSettings.Current.LocalisationTableFolder + "/");

					DrawConfigTemplateAndGenerateButton();
				}
				EditorGUI.indentLevel--;
			}

			private void DrawConfigTemplateAndGenerateButton()
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Template required to generate localisation class");

				EditorGUILayout.BeginHorizontal();
				configTemplate =
					(TextAsset) EditorGUILayout.ObjectField("Config template:", configTemplate, typeof(TextAsset),
						false);

				EditorGUI.BeginDisabledGroup(configTemplate == null);
				if (GUILayout.Button("Generate Config Class"))
				{
					LocalisationConfigGenerator.GenerateLocalisationConfig(
						configTemplate, LocalisationSettings.Current.Schema);
				}

				EditorGUI.EndDisabledGroup();

				EditorGUILayout.EndHorizontal();
			}
		}
	}
}