using static UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.ModificationEvent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Lowscope.AddressableReferenceCodeGenerator {
  [InitializeOnLoad]
  public class Generator {
    static Generator () {
      // Only subscribe to modifications when settings exist
      if (AddressableAssetSettingsDefaultObject.SettingsExists)
        AddressableAssetSettingsDefaultObject.Settings.OnModification += OnSettingsModification;
    }

    static void OnSettingsModification (AddressableAssetSettings assetSettings, AddressableAssetSettings.ModificationEvent modificationEvent, object o) {
      if (Settings.Get ().autoGeneration && modificationEvent is EntryCreated or EntryAdded or EntryModified or EntryMoved or EntryRemoved) {
        GenerateCode ();
      }
    }

    [MenuItem ("Tools/Addressable Code Generator/Generate")]
    static void GenerateCode () {
      // Get folder configuration
      var targetFolderPath = Settings.Get ().targetFolderPath;
      if (string.IsNullOrEmpty (targetFolderPath)) {
        targetFolderPath = "Source/Assets";
      }

      // Create folder, relative from assets folder.
      var folder = Path.Combine (Application.dataPath, targetFolderPath);
      if (!Directory.Exists (folder)) {
        Directory.CreateDirectory (folder);
      }

      // Generate classes
      GenerateLabelsClass (folder);
      GenerateAssetsClass (folder);

      // Refresh asset database (wont refresh if nothing is changed)
      AssetDatabase.Refresh (ImportAssetOptions.Default);
    }

    static void GenerateAssetsClass (string folder) {
      var stringBuilder = new StringBuilder ();
      stringBuilder.AppendLine ("public class Assets {");

      var list = new List<AddressableAssetEntry> ();
      var addedAddresses = new HashSet<string> ();

      AddressableAssetSettingsDefaultObject.Settings.GetAllAssets (list, false);
      foreach (var entry in list) {

        // Ignore built in data, such as resources for enum generation.
        if (entry.parentGroup.name == "Built In Data")
          continue;

        // In case the address has already been added, skip
        if (addedAddresses.Contains (entry.address))
          continue;

        stringBuilder.AppendLine ($"   public const string {entry.MainAsset.name.Replace (' ', '_')} = \"{entry.address}\"; ");
        addedAddresses.Add (entry.address);
      }

      stringBuilder.AppendLine ("}");

      File.WriteAllText (Path.Combine (folder, "Assets.cs"), stringBuilder.ToString ());
    }

    static void GenerateLabelsClass (string folder) {
      List<string> labelStrings = AddressableAssetSettingsDefaultObject.Settings.GetLabels ();

      // Generate script with existing asset labels
      StringBuilder stringBuilder = new StringBuilder ();
      stringBuilder.AppendLine ("public class Labels {");
      for (int i = 0; i < labelStrings.Count; i++) {
        if (i == 0) {
          stringBuilder.AppendLine ("   public const string Default = \"default\";");
        } else {
          stringBuilder.AppendLine ($"   public const string {labelStrings[i].Replace (' ', '_')} = \"{labelStrings[i]}\"; ");
        }
      }
      stringBuilder.AppendLine ("}");

      File.WriteAllText (Path.Combine (folder, "Labels.cs"), stringBuilder.ToString ());
    }
  }
}