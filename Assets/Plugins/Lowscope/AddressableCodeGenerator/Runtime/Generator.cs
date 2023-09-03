using System;
using static UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.ModificationEvent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Compilation;
using UnityEngine;

namespace Lowscope.AddressableReferenceCodeGenerator
{
	[InitializeOnLoad]
	public class Generator
	{
		static Generator()
		{
			// Only subscribe to modifications when settings exist
			if (AddressableAssetSettingsDefaultObject.SettingsExists)
				AddressableAssetSettingsDefaultObject.Settings.OnModification += OnSettingsModification;
		}

		static void OnSettingsModification(AddressableAssetSettings assetSettings,
			AddressableAssetSettings.ModificationEvent modificationEvent, object o)
		{
			if (Settings.Get().autoGeneration)
			{
				if (modificationEvent is GroupAdded or GroupRemoved or GroupRenamed)
				{
					if (o is AddressableAssetGroup group)
						if (group.entries.Count > 0)
							EditorApplication.delayCall += GenerateCode;
				}

				if (modificationEvent is EntryCreated or EntryAdded or EntryModified or EntryMoved
				    or EntryRemoved or LabelAdded or LabelRemoved)
				{
					EditorApplication.delayCall += GenerateCode;
				}
			}
		}

		[MenuItem("Tools/Addressable Code Generator/Generate")]
		static void GenerateCode()
		{
			// Get folder configuration
			var targetFolderPath = Settings.Get().targetFolderPath;
			if (string.IsNullOrEmpty(targetFolderPath))
			{
				targetFolderPath = "Source/Assets";
			}

			// Create folder, relative from assets folder.
			var folder = Path.Combine(Application.dataPath, targetFolderPath);
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			// Generate classes
			GenerateLabelsClass(folder);
			GenerateAssetsClass(folder);

			// Refresh asset database (wont refresh if nothing is changed)
			CompilationPipeline.RequestScriptCompilation();
		}

		static void GenerateAssetsClass(string folder)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("public class Assets {");

			var list = new List<AddressableAssetEntry>();
			var addedAddresses = new HashSet<string>();

			var createNestedGroupClasses = Settings.Get().createNestedGroupClasses;
			var activeGroupName = "";

			AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(list, false);
			foreach (var entry in list)
			{
				// Ignore built in data, such as resources for enum generation.
				if (entry.parentGroup.name == "Built In Data")
					continue;

				// Shorten the default local group name
				var groupName = entry.parentGroup.name;
				if (groupName == "Default Local Group")
					groupName = "Default";

				// Check if the group name has changed, if so, create a new class
				if (createNestedGroupClasses && activeGroupName != groupName)
				{
					// Close the active group class
					if (!string.IsNullOrEmpty(activeGroupName))
						stringBuilder.AppendLine("   }");

					const char bracket = '{';
					stringBuilder.AppendLine($"   public class {RemoveSpecialCharacters(groupName)} {bracket}");

					activeGroupName = groupName;
				}

				// In case the address has already been added, skip
				if (addedAddresses.Contains(entry.address))
					continue;

				var createNestedClassesSpace = createNestedGroupClasses ? "   " : "";
				var assetName = RemoveSpecialCharacters(entry.MainAsset.name);
				stringBuilder.AppendLine($"   {createNestedClassesSpace}public const string {assetName} = \"{entry.address}\"; ");
				addedAddresses.Add(entry.address);
			}

			// Close the last group class
			if (createNestedGroupClasses && !string.IsNullOrEmpty(activeGroupName))
				stringBuilder.AppendLine("   }");

			stringBuilder.AppendLine("}");

			File.WriteAllText(Path.Combine(folder, "Assets.cs"), stringBuilder.ToString());
		}

		static void GenerateLabelsClass(string folder)
		{
			List<string> labelStrings = AddressableAssetSettingsDefaultObject.Settings.GetLabels();

			// Generate script with existing asset labels
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("public class Labels {");
			for (int i = 0; i < labelStrings.Count; i++)
			{
				if (i == 0)
				{
					stringBuilder.AppendLine("   public const string Default = \"default\";");
				}
				else
				{
					stringBuilder.AppendLine($"   public const string {RemoveSpecialCharacters(labelStrings[i])} = \"{labelStrings[i]}\"; ");
				}
			}

			stringBuilder.AppendLine("}");

			File.WriteAllText(Path.Combine(folder, "Labels.cs"), stringBuilder.ToString());
		}
		
		static string RemoveSpecialCharacters(string input)
		{
			// Define a list of special characters to replace
			char[] specialCharacters = { '/', '.', ',', '-' };

			// Replace each special character with an underscore
			foreach (char specialChar in specialCharacters)
				input = input.Replace(specialChar.ToString(), "");
			
			// Remove white spaces from the field name
			input = RemoveWhiteSpaces(input);
			
			return input;
		}
		
		static string RemoveWhiteSpaces(string input)
		{
			// Replace white spaces with an empty string
			string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			return string.Join("", parts);
		}
	}
}