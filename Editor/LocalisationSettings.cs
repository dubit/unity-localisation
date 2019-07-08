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
	}
}