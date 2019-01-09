using UnityEngine;
using UnityEngine.UI;

namespace DUCK.Localisation.LocalisedObjects
{
	/// <summary>
	/// Automatically updates an Image's sprite to a different file path specified in the localisation table for the current locale
	/// </summary>
	[RequireComponent(typeof(Image))]
	public class LocalisedImage : AbsrtactLocalisedObject
	{
		private Image image;

#if UNITY_EDITOR
		public override LocalisedResourceType ResourceType { get { return LocalisedResourceType.Image; } }
#endif

		protected override void Awake()
		{
			image = GetComponent<Image>();

			base.Awake();
		}

		protected override void OnLocaleChanged()
		{
			if (image == null) return;

			string newPath;

			if (Localiser.GetLocalisedString(localisationKey, out newPath))
			{
				var newSprite = Resources.Load<Sprite>(newPath);

				if (newSprite != null)
				{
					image.sprite = newSprite;
				}
			}
			else
			{
				image.sprite = null;
			}
		}
	}
}