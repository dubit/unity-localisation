namespace DUCK.Localisation
{
	public interface ILocalisedObject
	{
#if UNITY_EDITOR
		LocalisedObject.LocalisedResourceType ResourceType { get; }
#endif
	}
}
