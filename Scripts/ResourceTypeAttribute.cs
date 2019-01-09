using System;
using DUCK.Localisation.LocalisedObjects;

namespace DUCK.Localisation
{
	public class ResourceTypeAttribute : Attribute
	{
		public LocalisedResourceType ResourceType { get; }

		public ResourceTypeAttribute(LocalisedResourceType resourceType)
		{
			ResourceType = resourceType;
		}
	}
}