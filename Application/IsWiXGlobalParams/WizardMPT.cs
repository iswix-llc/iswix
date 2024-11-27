using EnvDTE;
using EnvDTE80;
using IsWiXGlobalParams.Config;
using IsWiXGlobalParams.Objects;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Linq;

namespace IsWiXGlobalParams
{
  /// <summary>The wizard used to expose replacement parameters across multiple projects.</summary>
  public class WizardMPT : IWizard
  {
    #region Member Variables

    /// <summary>The development environment.</summary>
    private DTE2 _dte = null;

    /// <summary>The destination directory for sub projects.</summary>
    private string _destDir = string.Empty;

    /// <summary>The root directory of template.</summary>
    private string _templateDir = string.Empty;

    /// <summary>The settings for the wizard.</summary>
    private SettingsMPT _settings = null;

    /// <summary>Indicates if the current wizard is the top level template.</summary>
    private bool _isTopLevel = false;

    #endregion Member Variables

    #region Methods

    #region IsValid
    /// <summary>Determines if the specified <paramref name="template"/> is valid.</summary>
    /// <param name="template">The template to evaluate.</param>
    /// <returns>True if the template is valid, otherwise false.</returns>
    private bool IsValid(MappedTemplate template)
    {
      return template != null && !string.IsNullOrEmpty(template.Name) && template.Path != null && !string.IsNullOrEmpty(template.Template);
    }
    #endregion IsValid

    #region BeforeOpeningFile
    /// <summary>This method is called before opening any item that has the OpenInEditor attribute.</summary>
    /// <param name="projectItem">The item to be opened.</param>
    public void BeforeOpeningFile(ProjectItem projectItem) { OnBeforeOpeningFile(projectItem); }
    #endregion BeforeOpeningFile

    #region OnBeforeOpeningFile
    /// <summary>This method is called before opening any item that has the OpenInEditor attribute.</summary>
    /// <param name="projectItem">The item to be opened.</param>
    protected virtual void OnBeforeOpeningFile(ProjectItem projectItem) { }
    #endregion OnBeforeOpeningFile

    #region OnProjectFinishedGenerating
    /// <summary>Called after a project has finished generating.</summary>
    /// <param name="project">The project that was generated.</param>
    protected virtual void OnProjectFinishedGenerating(Project project) { }
    #endregion OnProjectFinishedGenerating

    #region OnProjectItemFinishedGenerating
    /// <summary>This method is only called for item templates, not for project templates.</summary>
    /// <param name="projectItem">The item that was generated.</param>
    protected virtual void OnProjectItemFinishedGenerating(ProjectItem projectItem) { }
    #endregion OnProjectItemFinishedGenerating

    #region OnRunFinished
    /// <summary>This method is called when the wizard is complete.</summary>
    protected virtual void OnRunFinished() { }
    #endregion OnRunFinished

    #region OnBeforeRunStarted
    /// <summary>Called when the wizard first begins and before the global parameters are create.</summary>
    /// <remarks>Override and return false to stop default global parameters.</remarks>
    protected virtual bool OnBeforeRunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
    {
      return true;
    }
    #endregion OnBeforeRunStarted

    #region OnAfterRunStarted
    /// <summary>Called when the wizard first begins and before the global parameters are create.</summary>
    protected virtual void OnAfterRunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) { }
    #endregion OnAfterRunStarted

    #region OnShouldAddProjectItem
    /// <summary>This method is only called for item templates, not for project templates.</summary>
    /// <param name="filePath">The full path to the item.</param>
    /// <returns>True if the item should be added, otherwise false.</returns>
    protected virtual bool OnShouldAddProjectItem(string filePath) { return true; }
    #endregion OnShouldAddProjectItem

    #region ProjectFinishedGenerating
    /// <summary>Called after a project has finished generating.</summary>
    /// <param name="project">The project that was generated.</param>
    public void ProjectFinishedGenerating(Project project)
    {
      if (_isTopLevel && _settings != null)
      {
        if (_settings.ProjectMappings != null)
        {
          string templatePath = null, projectPath = null, solutionDir = Path.GetDirectoryName(_destDir);
          foreach (MappedTemplate template in _settings.ProjectMappings)
          {
            if (IsValid(template))
            {
              try
              {
                templatePath = Path.Combine(_templateDir, template.Template);
                projectPath = Path.Combine(solutionDir, template.Path);
                _dte.Solution.AddFromTemplate(templatePath, projectPath, template.Name);
              }
              catch { /* GULP */ }
            }
          }
        }

        if (_settings.DeleteDefaultDirectory && !string.IsNullOrEmpty(_destDir))
        {
          System.Threading.Thread t = new System.Threading.Thread(() =>
          {
            try
            {
              System.Threading.Thread.Sleep(1000);
              if (Directory.Exists(_destDir) && Directory.GetFileSystemEntries(_destDir).Length == 0)
              {
                Directory.Delete(_destDir);
              }
            }
            catch { /* GULP */ }
          });
          t.Start();
        }
      }

      OnProjectFinishedGenerating(project);
    }
    #endregion ProjectFinishedGenerating

    #region ProjectItemFinishedGenerating
    /// <summary>This method is only called for item templates, not for project templates.</summary>
    /// <param name="projectItem">The item that was generated.</param>
    public void ProjectItemFinishedGenerating(ProjectItem projectItem) { OnProjectItemFinishedGenerating(projectItem); }
    #endregion ProjectItemFinishedGenerating

    #region RunFinished
    /// <summary>This method is called when the wizard is complete.</summary>
    public void RunFinished() { OnRunFinished(); }
    #endregion RunFinished

    #region RunStarted
    /// <summary>This method is called when the wizard first begins.</summary>
    public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
    {
      _isTopLevel = WizardRunKind.AsMultiProject.Equals(runKind);

      if (OnBeforeRunStarted(automationObject, replacementsDictionary, runKind, customParams))
      {
        // Check if we are running as the top level template
        if (_isTopLevel)
        {
          // Clear any leftover items from the static instance.
          Parameters.Clear();

          // We are running as the top level template so record the replacements dictionary
          foreach (string key in replacementsDictionary.Keys)
          {
            Parameters.Set(key, replacementsDictionary[key]);
          }

          // Extend the number of guids from 10 to 100
          for (int i = 1; i <= 100; i++)
          {
            Parameters.Set(string.Format(Constants.FORMAT_GUID_KEY, i), Guid.NewGuid().ToString(Constants.GUID_FORMATTER));
          }
        }

        // Make sure each template that runs us has access to the global parameters
        foreach (var parameter in Parameters.All)
        {
          if (replacementsDictionary.ContainsKey(parameter.Key))
          {
            replacementsDictionary[parameter.Key] = Parameters.Get(parameter.Key).ToString();
          }
          else
          {
            replacementsDictionary.Add(parameter.Key, parameter.Value.ToString());
          }
        }
      }
      OnAfterRunStarted(automationObject, replacementsDictionary, runKind, customParams);

      if (_isTopLevel && replacementsDictionary != null)
      {
        try { _dte = (DTE2)automationObject; }
        catch { /* GULP */ }

        try { _settings = new SettingsMPT(XElement.Parse(replacementsDictionary[Constants.WIZARD_DATA_KEY])); }
        catch { /* GULP */ }

        try { _destDir = replacementsDictionary[Constants.DEST_DIR_KEY]; }
        catch { /* GULP */ }

        try { _templateDir = Path.GetDirectoryName(customParams[0] as string); }
        catch { /* GULP */ }
      }
    }
    #endregion RunStarted

    #region ShouldAddProjectItem
    /// <summary>This method is only called for item templates, not for project templates.</summary>
    /// <param name="filePath">The full path to the item.</param>
    /// <returns>True if the item should be added, otherwise false.</returns>
    public bool ShouldAddProjectItem(string filePath) { return OnShouldAddProjectItem(filePath); }
    #endregion ShouldAddProjectItem

    #endregion Methods
  }
}