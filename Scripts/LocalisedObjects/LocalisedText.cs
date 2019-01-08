using UnityEngine;
using UnityEngine.UI;

namespace DUCK.Localisation.LocalisedObjects
{
	/// <summary>
	/// Automatically updates a Text component's text to the current locale
	/// </summary>
	[RequireComponent(typeof(Text))]
	public class LocalisedText : LocalisedObject
	{
		private Text text;

#if UNITY_EDITOR
		public override LocalisedResourceType ResourceType { get { return LocalisedResourceType.Text; } }
#endif

		protected override void Awake()
		{
			text = GetComponent<Text>();

			base.Awake();
		}

		protected override void OnLocaleChanged()
		{
			if (text == null) return;

			string newText;

			text.text = Localiser.GetLocalisedString(localisationKey, out newText)
				? newText
				: string.Format("<color=red>{0}</color>", localisationKey);
		}
	}
}