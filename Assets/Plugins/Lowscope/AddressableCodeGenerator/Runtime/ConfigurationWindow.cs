using UnityEditor;
using UnityEngine;

namespace Lowscope.AddressableReferenceCodeGenerator {
  public class ConfigurationWindow : EditorWindow {
    Settings.Configuration configuration;
    
    bool createNestedGroupClasses;
    bool autoGeneration;
    string targetFolderPath;

    [MenuItem ("Tools/Addressable Code Generator/Settings")]
    public static void ShowWindow () {
      var window = GetWindow<ConfigurationWindow> (false, "Generator Settings", true);
      window.maxSize = new Vector2 (350f, 70f);
      window.minSize = window.maxSize;
    }

    void UpdateConfiguration()
    {
      configuration = new Settings.Configuration {
        autoGeneration = autoGeneration,
        targetFolderPath = configuration.targetFolderPath,
        createNestedGroupClasses = createNestedGroupClasses
      };
      Settings.Set (configuration);
    }

    void OnEnable () {
      configuration = Settings.Get ();
      autoGeneration = configuration.autoGeneration;
      targetFolderPath = configuration.targetFolderPath;
      createNestedGroupClasses = configuration.createNestedGroupClasses;
      
      if (targetFolderPath != "") 
        return;
      
      targetFolderPath = "Source/Assets";
      autoGeneration = true;
      targetFolderPath = configuration.targetFolderPath;
      createNestedGroupClasses = true;
    }

    void OnGUI () {
      // Check for changes to automatic generation
      EditorGUI.BeginChangeCheck ();
      autoGeneration = EditorGUILayout.Toggle ("Automatic Generation", autoGeneration);
      if (EditorGUI.EndChangeCheck ()){
        UpdateConfiguration();
      }

      // Check for changes to nested group classes
      EditorGUI.BeginChangeCheck ();
      createNestedGroupClasses = EditorGUILayout.Toggle ("Create Nested Group Classes", createNestedGroupClasses);
      if (EditorGUI.EndChangeCheck ()) {
        UpdateConfiguration();
      }

      // Check for changes to target folder path
      targetFolderPath = EditorGUILayout.TextField ("Folder path", targetFolderPath);
      if (EditorGUI.EndChangeCheck ()) {
        UpdateConfiguration();
      }
    }
  }
}