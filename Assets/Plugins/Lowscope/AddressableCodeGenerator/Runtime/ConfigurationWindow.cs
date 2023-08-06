using UnityEditor;
using UnityEngine;

namespace Lowscope.AddressableReferenceCodeGenerator {
  public class ConfigurationWindow : EditorWindow {
    Settings.Configuration configuration;
    bool autoGeneration;
    string targetFolderPath;

    [MenuItem ("Tools/Addressable Code Generator/Settings")]
    public static void ShowWindow () {
      var window = GetWindow<ConfigurationWindow> (false, "Generator Settings", true);
      window.maxSize = new Vector2 (350f, 50f);
      window.minSize = window.maxSize;
    }

    void OnEnable () {
      configuration = Settings.Get ();
      autoGeneration = configuration.autoGeneration;
      targetFolderPath = configuration.targetFolderPath;
      if (targetFolderPath == "") {
        targetFolderPath = "Source/Assets";
      }
    }

    void OnGUI () {
      EditorGUI.BeginChangeCheck ();

      autoGeneration = EditorGUILayout.Toggle ("Automatic Generation", autoGeneration);

      if (EditorGUI.EndChangeCheck ()) {
        configuration = new Settings.Configuration {
          autoGeneration = autoGeneration,
          targetFolderPath = configuration.targetFolderPath
        };
        Settings.Set (configuration);
      }

      EditorGUI.BeginChangeCheck ();

      targetFolderPath = EditorGUILayout.TextField ("Folder path", targetFolderPath);

      if (EditorGUI.EndChangeCheck ()) {
        configuration = new Settings.Configuration {
          autoGeneration = configuration.autoGeneration,
          targetFolderPath = targetFolderPath
        };
        Settings.Set (configuration);
      }
    }
  }
}