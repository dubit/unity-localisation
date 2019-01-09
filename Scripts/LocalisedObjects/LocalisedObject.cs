using UnityEngine;

namespace DUCK.Localisation.LocalisedObjects
{
	/// <summary>
	/// LocalisedObject family of components: attach one of these to a Text, Image or AudioSource to make them localisation-aware.
	/// When the language is changed, they will automatically try to update their content to the localised version.
	/// </summary>
	public abstract class LocalisedObject : MonoBehaviour, ILocalisedObject
	{
		// Only used by Editor for human-readable display options
		// When built, all categories and keys are compiled away to CRC int values
#if UNITY_EDITOR
		public enum LocalisedResourceType
		{
			Text,
			Image,
			Audio
		}

		public abstract LocalisedResourceType ResourceType { get; }

		[SerializeField]
		protected string keyName;
		[SerializeField]
		protected string categoryName;
#endif

		[SerializeField]
		protected int localisationKey;

		protected abstract void OnLocaleChanged();

		protected virtual void Awake()
		{
			Localiser.OnLocaleChanged += OnLocaleChanged;

			OnLocaleChanged();
		}

		protected void OnDestroy()
		{
			Localiser.OnLocaleChanged -= OnLocaleChanged;
		}
	}
}
