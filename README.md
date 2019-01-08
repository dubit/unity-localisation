# unity-localisation

## What is it?
A library containing classes for adding localisation support to your application.

## Requirements
* Unity 2017 or higher
* .NET 4.X or higher

## How to use it.
The localiser works by ingesting a set of localisation keys (the ID values for each string in your application, e.g. MainMenuTitle, MainMenuBodyText, MainMenuStartButtonText etc). These can then be used to generate a localisation class in code which declares enums containing all these keys, which the application can use to retrieve them in a type-safe way. The enum values are converted to CRC/hash values so that retrieval is optimal in terms of performance, too.

For this reason, the setup includes two parts:
* Creating a key schema, which is only used at editor-time to generate the generate the Localisation class and the key enums
* Creating localisation tables which each represent the localised strings and language(s) they support

When the application is built, the editor-side bits will not be included, leaving only the data (localisation tables) and the generated lookup class which now contains the enums of all the loc keys needed.

## Editor-side

### Open the Localisation editor window

In Unity: Toolbar -> DUCK -> Localisation

### Attach, or create, a key schema

In the Localisation window, you will see a slot for assigning a key schema if you have one, and a button for generating a new one. Create or assign one - it doesn't need to be in Resources/ as it's editor-side only.

Once your key schema exists, start declaring localisation keys. Sets of keys are grouped into a category for making them easier to find (e.g. UI, CharacterNames, ErrorMessages, etc). Additionally, each category may also have a Type: Text, Audio or Image. They are all strings internally, and Text will be the type you use most often - the other types are covered later in this Readme.

Populate the schema with some meainingful data and keys. And make sure it's assigned in the Localisation window, as this is used to drive a lot of the rest of the localisation Editor UI. You only need one key schema per project, as it declares the keys the application knows about, not their values (yet).

### Generate a Loc class

This file is the 'bridge' the application will use to retrieve strings from the Localisation system. It declares all the enums the code will need to reference to do this, so for that reason, it needs to be automatically (re)generated every time you add or remove keys (or categories, or change category types, or anything else you edit in the schema). Once you're happy with your keys, assign the LocalisationConfigTemplate file in the 'Config template:' slot, and click 'Generate Loc Class'. This will autogenerate the class your application will need to reference to get strings - including the Keys enums and the Get(Enum key) function.

### Create and/or populate localisation tables

First, hit 'Find/Refresh' to automatically find any localisation tables which already exist in the Localisation folder (by default, resources/Localisation - and it does need to be in Resources as these will be loaded at runtime). If there isn't one for the language you want (or it's a new project), click 'Create new' to create a new table asset. Rename the asset to something meaningful (e.g. "English", "Francais", language-name etc), and select it in the inspector. You will see all the keys you created in the schema, each with a text box input. This is where you actually enter the localised text.

Also, at the top of the table, you will see an array of 'Supported locales' - make sure this has at least one entry in (e.g. usually the default 'english' one has both en-GB and en-US) or it won't be used for any language.

If you change anything in the schema, you will need to regenerate the Loc class (as above), but do NOT regenerate or overwrite the existing tables, or you will lose all the data you've already entered. Any keys you've added will appear in your table in the inspector ready to be populated with text, and any removed keys will disappear.

## Application-side

### Initialisation

Refer to the API of DUCK.Localisation.Localiser.cs - but you will probably only need to do the following in most cases:

* Localiser.Initialise("Localisation"); // default: looks in Resources/Localisation and starts in the user's device language

or...

* Localiser.Initialise("OtherFolderNameInResources"); // to look for tables in a folder other than Resources/Localisation
* Localiser.Initialise("Localisation", "en-GB"); // to set a specific starting language

... in most cases, your default table should have both en-GB and en-US supported, and the localiser will retrieve all compatible tables from this folder.

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