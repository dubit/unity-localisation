using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DUCK.Localisation
{
	/// <summary>
	/// Localisation manager class which maintains access to the localisation tables, loads them on-demand when the language is changed,
	/// and returns localised content in the correct language.
	/// </summary>
	public static class Localiser
	{
		/// <summary>
		/// List of CultureInfos representing the languages supported by the localiser
		/// </summary>
		public static readonly CultureInfo[] SupportedLocales =
		{
			new CultureInfo("en-GB"),
			new CultureInfo("en-US"),
			new CultureInfo("fr"),
			new CultureInfo("it"),
			new CultureInfo("de"),
			new CultureInfo("es"),
			new CultureInfo("pt-BR")
		};

		/// <summary>
		/// The current locale, i.e. the language the application is currently localised to
		/// </summary>
		public static CultureInfo CurrentLocale { get; private set; }

		/// <summary>
		/// True if the localiser has initialised successfully and loaded a localisation table
		/// </summary>
		public static bool Initialised { get { return currentLocalisationTable != null; } }

		private static readonly Dictionary<string, string> validTablePaths = new Dictionary<string, string>();
		private static readonly Dictionary<string, string> allTablePaths = new Dictionary<string, string>();

		private static string defaultCulture = "en-US";
		private static LocalisationTable currentLocalisationTable;

		public static event Action OnLocaleChanged;

		/// <summary>
		/// Initialises the localisation system in a folder in Resources/ where the localisation table assets are found.
		/// </summary>
		/// <param name="path">Filepath to the folder of localisation tables</param>
		/// <param name="cultureName">Optional culture name to initialise with. Falls back to default culture if not passed in or invalid</param>
		public static bool Initialise(string path, string cultureName = null)
		{
			CurrentLocale = NameToCultureInfo(cultureName);

			validTablePaths.Clear();
			allTablePaths.Clear();

			var localisationTables = Resources.LoadAll<LocalisationTable>(path);

			if (localisationTables.Length == 0)
			{
				throw new ArgumentException(string.Format("No localisation tables found at path: {0}", path));
			}

			foreach (var locTable in localisationTables)
			{
				var fullPath = Path.Combine(path, locTable.name);

				allTablePaths.Add(locTable.name, fullPath);

				if (locTable.SupportedLocales.Count == 0)
				{
					Debug.LogWarning(string.Format(
						"Did not initialise localisation table '{0}' as it has no supported locales so would never be used.",
						locTable.name));
					continue;
				}

				foreach (var localeName in locTable.SupportedLocales)
				{
					if (validTablePaths.ContainsKey(localeName))
					{
						Debug.LogWarning(string.Format("Localisation table contains locale '{0}' which has already been added!",
							localeName));
					}
					else
					{
						validTablePaths.Add(localeName, fullPath);

						if (localeName == CurrentLocale.Name && Application.isPlaying)
						{
							currentLocalisationTable = locTable;
						}
						else if (!locTable.SupportedLocales.Contains(CurrentLocale.Name))
						{
							Resources.UnloadAsset(locTable);
						}
					}
				}
			}

			if (currentLocalisationTable == null && Application.isPlaying)
			{
				Debug.LogWarning(string.Format("Error: unsupported locale: {0}, switching to {1}", CurrentLocale.Name, defaultCulture));
				SwitchCulture(defaultCulture);
			}

			return Initialised;
		}

		/// <summary>
		/// Sets up the default culture, i.e. the language the application will fall back to if it can't load the one requested
		/// <param name="culture">The culture to make default</param>
		/// <returns>bool - if the culture was successfuly set as the default</returns>
		/// </summary>
		public static bool OverrideDefaultCulture(string culture)
		{
			if (SupportedLocales.All(c => c.Name != culture) || NameToCultureInfo(culture).Name != culture)
			{
				Debug.LogWarning("Invalid default culture name: " + culture);
				return false;
			}

			defaultCulture = culture;
			return true;
		}

		/// <summary>
		/// Switches the application to the specified culture
		/// <param name="culture">The culture to switch to</param>
		/// <returns>bool - if the culture was switched to successfully</returns>
		/// </summary>
		public static bool SwitchCulture(string culture)
		{
			var loadedTable = LoadLocalisationTable(culture);
			if (loadedTable == null)
			{
				Debug.LogWarning(string.Format("Error loading localisation table for locale: {0}", culture));
				return false;
			}

			CurrentLocale = new CultureInfo(culture);
			currentLocalisationTable = loadedTable;

			if (OnLocaleChanged != null)
			{
				OnLocaleChanged.Invoke();
			}

			return true;
		}

		/// <summary>
		/// Reverts to select the default Culture
		/// <returns>bool - if the default culture was switched to successfully</returns>
		/// </summary>
		public static bool RevertToDefaultCulture()
		{
			return SwitchCulture(defaultCulture);
		}

		/// <summary>
		/// Returns the localised string content associated with the specified key, in the currently selected culture
		/// </summary>
		/// <param name="key">The CRC key requested</param>
		/// <param name="target">The string reference into which the localised text will be copied</param>
		/// <returns>bool - whether retrieving the localised string was successful or not</returns>
		public static bool GetLocalisedString(int key, out string target)
		{
			if (!Initialised)
			{
				throw new Exception("Localiser not initialised");
			}

			try
			{
				target = currentLocalisationTable.GetString(key);
			}
			catch (KeyNotFoundException)
			{
				target = string.Empty;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Dictionary of all localisation tables (name --> path) found in the Resources target folder (whether valid or not)
		/// </summary>
		public static Dictionary<string, string> GetTablePaths()
		{
			return allTablePaths;
		}

		private static CultureInfo NameToCultureInfo(string cultureName)
		{
			CultureInfo cultureInfo = null;

			try
			{
				cultureInfo = new CultureInfo(cultureName);
			}
			catch (ArgumentException)
			{
				Debug.LogError("Invalid culture name: " + cultureName);
				cultureInfo = new CultureInfo(defaultCulture);
			}

			return cultureInfo;
		}

		private static LocalisationTable LoadLocalisationTable(string localeName)
		{
			if (!validTablePaths.ContainsKey(localeName)) return null;

			var loadPath = validTablePaths[localeName];
			return Resources.Load<LocalisationTable>(loadPath);
		}
	}
}