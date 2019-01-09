# Basic Usage

## Contents
* [Localised objects/components](#localised-objectscomponents)
* [Retrieving a localised string in code](#retrieving-a-localised-string-in-code)
* [Change language at runtime](#change-language-at-runtime)
* [Extending LocalisedObject](#extending-localisedobject)

## Localised objects/components
With localised components you may need to ensure that the schema is set and that the tables were refreshed before you will be able to configure it. [See also](https://github.com/dubit/unity-localisation/issues/5)

There are 3 LocalisedObjects that come ready to use, They are `LocalisedText`, `LocalisedImage`, and `LocalisedAudio`.
They work in conjunction with `Text`, `Image` and `AudioSource` respectively.

To configure it add the base component (`Text`, `Image` or `AudioSource`), then add the localised component counterpart

Set the category and the key. and press the set button. (The UI is the same for all 3)

![LocalisedText](./localised-text.png)

## Retrieving a localised string in code
`Localiser.GetLocalisedString(Loc.MainMenu.MainMenuTitle);`

## Change language at runtime

`Localiser.SwitchLocale("fr"); // switch to French`

This will automatically update any localisation-sensitive components.

## Extending LocalisedObject
In some situations you may want to set the text, sprite, or audio on a different component other than `Text`, `Image` or `AudioSource`.

For example you may want to set the text on a `TextMeshPro` object. To accomplish this just extend `LocalisedObject` and make the relevant changes.
#endif
eg:

```c#
[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalisedTextMeshProUGUI : LocalisedObject
{
    private TextMeshProUGUI text;

#if UNITY_EDITOR
    public override LocalisedResourceType ResourceType { get { return LocalisedResourceType.Text; } }
#endif

    protected override void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();

        base.Awake();
    }

    protected override void OnLocaleChanged()
    {
        if (text == null) return;

        string newText;

        text.text = Localiser.GetLocalisedString(localisationKey, out newText)
            ? newText
            : string.Format("<color=red>{0}</color>", localisationKey);
    }
```

Note: we cannot provide this as part of the package without introducing a dependency on the TextMeshPro assembly, otherwise we would ship this library with this component.
