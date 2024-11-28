using IsWiX2022AddIn.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace IsWiX2022AddIn.Config
{
  /// <summary>Represents settings for the multi-project template wizard.</summary>
  internal class SettingsMPT
  {
    #region Member Variables

    /// <summary>The list of project mappings.</summary>
    private List<MappedTemplate> _projectMappings = new List<MappedTemplate>();

    /// <summary>Indicates if the default directory should be deleted.</summary>
    private bool _deleteDefaultDirectory = true;

    #endregion Member Variables

    #region Constructors

    /// <summary>Creates a new instance of <see cref="SettingsMPT"/> from the specified <paramref name="xml"/>.</summary>
    /// <param name="xml">The xml representation of the settings.</param>
    internal SettingsMPT(XElement xml)
    {
      if (xml != null && xml.Name != null && xml.Name.LocalName.Equals(Constants.XML_ELEM_SETTINGS))
      {
        XAttribute attrib = xml.Attribute(Constants.XML_ATTR_DELETE_DEFAULT_DIR);
        bool delDefaultDir = _deleteDefaultDirectory;
        if (attrib != null && bool.TryParse(attrib.Value, out delDefaultDir))
        {
          _deleteDefaultDirectory = delDefaultDir;
        }

        string name = string.Empty, path = string.Empty, template = string.Empty;
        foreach (XElement mapElem in xml.Elements().Where(x => x.Name.LocalName.Equals(Constants.XML_ELEM_MAPPED_PROJECT_TEMPLATE)))
        {
          if (mapElem != null)
          {
            // Get and resolve the value of the "name" attribute
            attrib = mapElem.Attribute(Constants.XML_ATTR_NAME);
            if (attrib != null) { name = Parameters.Resolve(attrib.Value); }

            // Get and resolve the value of the "path" attribute
            attrib = mapElem.Attribute(Constants.XML_ATTR_PATH);
            if (attrib != null) { path = Parameters.Resolve(attrib.Value); }

            // Get and resolve the value of the "template" attribute
            attrib = mapElem.Attribute(Constants.XML_ATTR_TEMPLATE);
            if (attrib != null) { template = Parameters.Resolve(attrib.Value); }

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(template))
            {
              if (string.IsNullOrEmpty(path)) { path = name; }

              _projectMappings.Add(new MappedTemplate() { Name = name, Path = path, Template = template, });
            }
          }
        }
      }
    }
    #endregion Constructors

    #region Properties

    #region DeleteDefaultDirectory
    /// <summary>Indicates if the default directory should be deleted.</summary>
    public bool DeleteDefaultDirectory { get { return _deleteDefaultDirectory; } set { _deleteDefaultDirectory = value; } }
    #endregion DeleteDefaultDirectory

    #region ProjectMappings
    /// <summary>The list of project mappings.</summary>
    public List<MappedTemplate> ProjectMappings { get { return _projectMappings; } set { _projectMappings = value; } }
    #endregion ProjectMappings

    #endregion Properties
  }
}
