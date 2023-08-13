using System;
using UnityEditor;
using UnityEngine;

namespace Lowscope.AddressableReferenceCodeGenerator {
  public static class Settings {
    [Serializable]
    public struct Configuration {
      public bool autoGeneration;
      public bool createNestedGroupClasses;
      public string targetFolderPath;
    }

    public static Configuration Get () => GetObjectFromEditorSession<Configuration> ("ARCG_Configuration");

    public static void Set (Configuration configuration) => StoreObjectToEditorSession (configuration, "ARCG_Configuration");

    static void StoreObjectToEditorSession<T> (T sessionObject, string key) {
      var configurationString = JsonUtility.ToJson (sessionObject);
      SessionState.SetString (key, configurationString);
      EditorPrefs.SetString (key, configurationString);
    }

    static T GetObjectFromEditorSession<T> (string key) where T : struct {
      var getSessionString = SessionState.GetString (key, "");

      // Try to obtain settings from editor prefs if not present in session
      if (string.IsNullOrEmpty (getSessionString)) {
        getSessionString = EditorPrefs.GetString (key, "");
      }

      // Generate default configuration or deserialize the configuration string into an object
      return string.IsNullOrEmpty (getSessionString) ? default : JsonUtility.FromJson<T> (getSessionString);
    }
  }
}