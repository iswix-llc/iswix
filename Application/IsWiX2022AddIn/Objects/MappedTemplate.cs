using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsWiX2022AddIn.Objects
{
  /// <summary>Represents a mapped template.</summary>
  internal class MappedTemplate
  {
    #region Properties

    #region Name
    /// <summary>The name of the project.</summary>
    public string Name { get; set; }
    #endregion Name

    #region Path
    /// <summary>The path to the project.</summary>
    public string Path { get; set; }
    #endregion Path

    #region Template
    /// <summary>The path to the template.</summary>
    public string Template { get; set; }
    #endregion Template

    #endregion
  }
}
