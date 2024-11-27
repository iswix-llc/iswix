
namespace IsWiXGlobalParams
{
  /// <summary>Defines constant values used by the Multi-Project Template Wizard.</summary>
  internal static class Constants
  {
    #region Member Variables

    /// <summary>The literal value '_'.</summary>
    internal const char VALID_SAFE_PROJECT_SPACE = '_';

    /// <summary>The literal value ' '.</summary>
    internal const char INVALID_SAFE_PROJECT_SPACE = ' ';

    /// <summary>The literal value '$'.</summary>
    internal const char INDICATOR_PARAM = '$';

    /// <summary>The literal value "$safeprojectname$".</summary>
    internal const string SAFE_PROJECT_KEY = "$safeprojectname$";

    /// <summary>The literal value "$destinationdirectory$".</summary>
    internal const string DEST_DIR_KEY = "$destinationdirectory$";

    /// <summary>The literal value "$wizarddata$".</summary>
    internal const string WIZARD_DATA_KEY = "$wizarddata$";

    /// <summary>The literal value "$global{0}$".</summary>
    internal const string FORMAT_GLOBAL_KEY = "$global{0}$";

    /// <summary>The literal value "guid{0}".</summary>
    internal const string FORMAT_GUID_KEY = "guid{0}";

    /// <summary>The literal value "D".</summary>
    internal const string GUID_FORMATTER = "D";

    /// <summary>The literal value "(\$.*?\$)".</summary>
    internal const string PATTERN_RESOLVE = @"(\$.*?\$)";

    /// <summary>The literal value "^\$global.*\$$".</summary>
    internal const string PATTERN_GLOBALIZED = @"^\$global.*\$$";

    /// <summary>The literal value "Settings".</summary>
    internal const string XML_ELEM_SETTINGS = "Settings";

    /// <summary>The literal value "MappedProjectTemplate".</summary>
    internal const string XML_ELEM_MAPPED_PROJECT_TEMPLATE = "MappedProjectTemplate";

    /// <summary>The literal value "name".</summary>
    internal const string XML_ATTR_NAME = "name";

    /// <summary>The literal value "path".</summary>
    internal const string XML_ATTR_PATH = "path";

    /// <summary>The literal value "template".</summary>
    internal const string XML_ATTR_TEMPLATE = "template";

    /// <summary>The literal value "deleteDefaultDirectory".</summary>
    internal const string XML_ATTR_DELETE_DEFAULT_DIR = "deleteDefaultDirectory";

    /// <summary>The literal value "namespace".</summary>
    internal const string XML_ATTR_NAMESPACE = "namespace";

    #endregion Member Variables
  }
}
