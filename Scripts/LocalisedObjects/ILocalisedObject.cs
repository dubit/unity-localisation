namespace DUCK.Localisation.LocalisedObjects
{
	public interface ILocalisedObject
	{
#if UNITY_EDITOR
		LocalisedObject.LocalisedResourceType ResourceType { get; }
#endif
	}
}
