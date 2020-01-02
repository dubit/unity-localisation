# unity-localisation

## What is it?
A library containing classes for adding localisation support to your application.

## Requirements
* Unity 2017 or higher
* .NET 4.X or higher
* C# 6.0 or higher

## Features
* Type safe localisation key/value retrieval.
* Split translations into categories for easier organisation (via code generation)
* Easy to use out of the box components: `LocalisedText`, `LocalisedAudio`, `LocalisedImage`
* Simple API to retrieve localised values by code: `Loc.Get(Category.Key.KeyName);`

## How to use it.
Please refer to the following guides.
* [GettingStarted](./Docs/GettingStarted.md)- for info about setting up your project for localisation
* [BasicUsage](./Docs/BasicUsage.md) - covers most of the basic use cases.
* [Preferences](./Docs/Preferences.md)

## Releasing
* Use [gitflow](https://nvie.com/posts/a-successful-git-branching-model/)
* Create a release branch for the release
* On that branch, bump version number in package json file, any other business (docs/readme updates)
* Merge to master via pull request and tag the merge commit on master.
* Merge back to development.

## DUCK

This repo is part of DUCK (dubit unity component kit)
DUCK is a series of repos containing reusable component, utils, systems & tools. 

DUCK packages can be added to a project as git submodules or by using [Unity Package Manager](https://docs.unity3d.com/Manual/upm-git.html). 
