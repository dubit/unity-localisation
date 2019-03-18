using System.Reflection;
using UnityEngine;

namespace DUCK.Localisation.LocalisedObjects
{
	public abstract class AbstractLocalisedObject : MonoBehaviour
	{
		public LocalisedResourceType ResourceType
		{
			get
			{
				var resourceTypeAttribute = GetType().GetCustomAttribute<ResourceTypeAttribute>();
				return resourceTypeAttribute?.ResourceType ?? LocalisedResourceType.Unknown;
			}
		}

		[SerializeField]
		protected LocalisedValue localisedValue;
	}

	/// <summary>
	/// LocalisedObject family of components: attach one of these to a Text, Image or AudioSource to make them localisation-aware.
	/// When the language is changed, they will automatically try to update their content to the localised version.
	/// </summary>
	public abstract class AbstractLocalisedObject<TComponent> : AbstractLocalisedObject
		where TComponent : Component
	{
		protected TComponent Component { get; private set; }

		protected abstract void HandleLocaleChanged(bool translationFound, string localisedString);

		protected void Awake()
		{
			Component = GetComponent<TComponent>();

			Localiser.OnLocaleChanged += OnLocaleChanged;

			OnLocaleChanged();
		}

		protected void OnDestroy()
		{
			Localiser.OnLocaleChanged -= OnLocaleChanged;
		}

		private void OnLocaleChanged()
		{
			if (Localiser.Initialised)
			{
				var localisedText = "";
				var foundtranslation = Localiser.GetLocalisedString(localisedValue.LocalisationKey, out localisedText);
				HandleLocaleChanged(foundtranslation, localisedText);
			}
			else
			{
				Debug.LogWarning("Cannot localise, the localiser is not initialised!");
			}
		}
	}
}
