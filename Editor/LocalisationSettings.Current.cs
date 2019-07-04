using UnityEditor;
using UnityEngine;

namespace DUCK.Localisation.Editor
{
	public partial class LocalisationSettings
	{
		private const string GUID_EDITOR_PREF_KEY = "LocalisationSettingsGuid";

		private static LocalisationSettings current;
		public static LocalisationSettings Current
		{
			get
			{
				if (current != null) return current;

				if (EditorPrefs.HasKey(GUID_EDITOR_PREF_KEY))
				{
					var guid = EditorPrefs.GetString(GUID_EDITOR_PREF_KEY);
					var assetPath = AssetDatabase.GUIDToAssetPath(guid);
					current = AssetDatabase.LoadAssetAtPath<LocalisationSettings>(assetPath);
				}
				else
				{
					// create or load it...
					var assets = AssetDatabase.FindAssets($"t:{nameof(LocalisationSettings)}");
					if (assets.Length > 0)
					{
						current = AssetDatabase.LoadAssetAtPath<LocalisationSettings>(
							AssetDatabase.GUIDToAssetPath(assets[0]));
					}
					else
					{
						current = CreateInstance<LocalisationSettings>();
						AssetDatabase.CreateAsset(current, "Assets/LocalisationSettings.asset");

						var path = AssetDatabase.GetAssetPath(current);
						var guid = AssetDatabase.AssetPathToGUID(path);
						EditorPrefs.SetString(GUID_EDITOR_PREF_KEY, guid);
					}
				}

				return current;
			}
		}
	}
}