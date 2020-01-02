using UnityEngine;

namespace DUCK.Localisation.Editor
{
	public partial class LocalisationSettings : ScriptableObject
	{
		[SerializeField]
		private string codeGenerationFilePath = "Scripts/Localisation/LocalisationConsts.cs";
		public string CodeGenerationFilePath => codeGenerationFilePath;

		[SerializeField]
		private string localisationTableFolder = "Resources/Localisation";
		public string LocalisationTableFolder => localisationTableFolder;

		[SerializeField]
		private LocalisationKeySchema schema = new LocalisationKeySchema();
		public LocalisationKeySchema Schema => schema;
	}
}