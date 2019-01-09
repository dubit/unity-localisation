namespace DUCK.Localisation.LocalisedObjects
{
	public interface ILocalisedObject
	{
#if UNITY_EDITOR
		AbsrtactLocalisedObject.LocalisedResourceType ResourceType { get; }
#endif
	}
}
