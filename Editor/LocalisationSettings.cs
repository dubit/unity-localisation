using UnityEngine;

namespace DUCK.Localisation.Editor
{
	public partial class LocalisationSettings : ScriptableObject
	{
		[SerializeField]
		private string codeGenerationFilePath;
		public string CodeGenerationFilePath => codeGenerationFilePath;

		[SerializeField]
		private string localisationTableFolder;
		public string LocalisationTableFolder => localisationTableFolder;

		[SerializeField]
		private LocalisationKeySchema schema = new LocalisationKeySchema();
		public LocalisationKeySchema Schema => schema;
	}
}