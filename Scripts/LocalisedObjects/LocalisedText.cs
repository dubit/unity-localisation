﻿using UnityEngine;
using UnityEngine.UI;

namespace DUCK.Localisation.LocalisedObjects
{
	/// <summary>
	/// Automatically updates a Text component's text to the current locale
	/// </summary>
	[RequireComponent(typeof(Text))]
	[ResourceType(LocalisedResourceType.Text)]
	public class LocalisedText : AbstractLocalisedObject<Text>
	{
		protected override void HandleLocaleChanged(bool translationFound, string localisedString)
		{
			Component.text = translationFound ? localisedString : $"<color=red>{localisationKey}</color>";
		}
	}
}