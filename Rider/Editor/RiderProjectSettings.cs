using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace Packages.Rider.Editor
{
  static class RiderProjectSettings
  {
    private static readonly Regex invalidCharactersRegex = new("[^a-z0-9_-]", RegexOptions.IgnoreCase);
    private static readonly Regex consecutiveHyphens = new("[-]{2,}");
    
    [Serializable]
    private class StorageModel
    {
      public int version = 1;
      public string solutionName;
    }

    private static string SettingsPath{
      get {
        string rootDirectory = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(rootDirectory, "ProjectSettings", "Rider.json");
      }
    }
    
    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider() {
      return new SettingsProvider("Project/JetBrains Rider", SettingsScope.Project) {
        guiHandler = (_searchContext) => {
          string currentSolutionName = GetSolutionName();
          EditorGUI.BeginChangeCheck();
          string newSolutionName = EditorGUILayout.TextField("Solution Name", currentSolutionName);
          EditorGUILayout.HelpBox(
            @"By default, the C# solution name is set to the name of the project's parent folder.

You can change this behavior by filling the field above.
Only letters, digits, underscores and hyphens are allowed.

Leave empty to use the default behaviour.", MessageType.Info, wide: false);
          if (EditorGUI.EndChangeCheck() && currentSolutionName != newSolutionName) {
            newSolutionName = SanitizeSolutionName(newSolutionName);
            SetSolutionName(newSolutionName);
          }
        }
      };
    }

    private static string SanitizeSolutionName(string _unsanitizedSolutionName) {
      string sanitizedSolutionName = _unsanitizedSolutionName.Trim();
      sanitizedSolutionName = invalidCharactersRegex.Replace(sanitizedSolutionName, "-");
      sanitizedSolutionName = consecutiveHyphens.Replace(sanitizedSolutionName, "-");
      sanitizedSolutionName = sanitizedSolutionName.Trim('-');
      return sanitizedSolutionName;
    }
    
    public static string GetSolutionName() {
      string path = SettingsPath;
      string solutionName = "";
      if (File.Exists(path)) {
        string json = File.ReadAllText(path);
        StorageModel model = JsonUtility.FromJson<StorageModel>(json);
        solutionName = model.solutionName;
      }
      return solutionName;
    }

    private static void SetSolutionName(string _solutionName) {
      StorageModel model = new() { solutionName = _solutionName };
      string json = JsonUtility.ToJson(model);
      File.WriteAllText(SettingsPath, json);
    }
  }
}