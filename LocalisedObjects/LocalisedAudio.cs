using UnityEngine;

namespace DUCK.Localisation.LocalisedObjects
{
	/// <summary>
	/// Automatically updates an AudioSource's clip to a different file path specified in the localisation table for the current locale
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	public class LocalisedAudio : LocalisedObject
	{
		private AudioSource source;

		public override LocalisedResourceType ResourceType { get { return LocalisedResourceType.Audio; } }

		protected override void Awake()
		{
			source = GetComponent<AudioSource>();

			base.Awake();
		}

		protected override void OnLocaleChanged()
		{
			if (source == null) return;

			string newPath;

			if (Localiser.GetLocalisedString(localisationKey, out newPath))
			{
				var newAudio = Resources.Load<AudioClip>(newPath);

				if (newAudio != null)
				{
					source.clip = newAudio;
				}
			}
			else
			{
				source.clip = null;
			}
		}
	}
}