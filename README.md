# unity-localisation

## What is it?
A library containing classes for adding localisation support to your application.

## Requirements
* Unity 2017 or higher
* .NET 4.X or higher

## Features
* Supports localisation for different media types (Text, Audio, Images).
* Type safe localisation key/value retrieval.
* Split translations into categories for easier organisation.
* Validation to auto detect missing values for different locales.
* Easy to use out of the box components: `LocalisedText`, `LocalisedAudio`, `LocalisedImage`
* Simple API to retrieve localised values by code: `Loc.Get(Category.Key.KeyName);`

## How to use it.
Please refer to the following guides.
* [GettingStarted](./Docs/GettingStarted.md)- for info about setting up your project for localisation
* [BasicUsage](./Docs/BasicUsage.md) - covers most of the basic use cases.
* [Preferences](./Docs/Preferences.md)

The localiser works by ingesting a set of localisation keys (the ID values for each string in your application, e.g. MainMenuTitle, MainMenuBodyText, MainMenuStartButtonText etc). These can then be used to generate a localisation class in code which declares enums containing all these keys, which the application can use to retrieve them in a type-safe way. The enum values are converted to CRC/hash values so that retrieval is optimal in terms of performance, too.

For this reason, the setup includes two parts:
* Creating a key schema, which is only used at editor-time to generate the generate the Localisation class and the key enums
* Creating localisation tables which each represent the localised strings and language(s) they support

When the application is built, the editor-side bits will not be included, leaving only the data (localisation tables) and the generated lookup class which now contains the enums of all the loc keys needed.


### Retrieving a localised string in code

Loc.Get(Category.Key.KeyName);

e.g. Loc.Get(Loc.MainMenu.MainMenuTitle);

### Changing language (e.g. due to a UI option)

Localiser.SwitchLocale(newLocaleName);

e.g. Localiser.SwitchLocale("fr"); // switch to French

This will automatically update any localisation-sensitive components.

### Localisation-sensitive components

LocalisedText.cs : attach this to a gameObject which also contains a UI.Text component to make it automatically update the Text to the correct localised string on enable, or when the locale is changed.

LocalisedImage.cs and LocalisedAudio.cs : attach these next to a UI.Image or an AudioSource component to make them automatically update the image (e.g. country flag) or audio (e.g. welcome message) to the current locale. The text values in this case will need to be the path in Resources/ where the asset is to be loaded from. You will need to include your default language assets too.

In most cases, just putting a LocalisedText component on any gameObject in your UI which has a Text component on it should make everything work.