using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IsWiX2022AddIn
{
  /// <summary>Storage for global parameters.</summary>
  public static class Parameters
  {
    #region Member Variables

    /// <summary>Private storage for the global parameters.</summary>
    private static Dictionary<string, object> _parameters = new Dictionary<string, object>();

    #endregion Member Variables

    #region Properties

    #region All
    /// <summary>Return the underlying parameter collection.</summary>
    internal static Dictionary<string, object> All { get { return _parameters; } }
    #endregion All

    #endregion Properties

    #region Methods

    #region Get
    /// <summary>Gets the parameter by the specified <paramref name="key"/>.</summary>
    /// <param name="key">The key.</param>
    /// <returns>The parameter value or an empty string.</returns>
    public static object Get(string key)
    {
      object retVal = string.Empty;
      string keyInternal = key.Globalize();

      retVal = HasPrivate(keyInternal) ? _parameters[keyInternal] : string.Empty;

      return retVal;
    }
    #endregion Get

    #region Set
    /// <summary>Sets the parameter with the specified <paramref name="key"/> to the specified <paramref name="value"/>. See remarks for more info.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <remarks>If a parameter with the specified <paramref name="key"/> doesn not exist it will be created.</remarks>
    public static void Set(string key, object value)
    {
      string keyInternal = key.Globalize();

      if (Constants.SAFE_PROJECT_KEY.Equals(key) && value != null)
      {
        value = value.ToString().Replace(Constants.INVALID_SAFE_PROJECT_SPACE, Constants.VALID_SAFE_PROJECT_SPACE);
      }

      if (!string.IsNullOrEmpty(keyInternal))
      {
        if (HasPrivate(keyInternal)) { _parameters[keyInternal] = value; }
        else { _parameters.Add(keyInternal, value); }
      }
    }
    #endregion Set

    #region Has
    /// <summary>Determines if a parameter is defined with the specified <paramref name="key"/> converted to a global parameter name.</summary>
    /// <param name="key">The key.</param>
    /// <returns>True if the an entry was found, otherwise false.</returns>
    public static bool Has(string key)
    {
      return HasPrivate(key.Globalize());
    }
    #endregion Has

    #region HasPrivate
    /// <summary>Determines if there is an entry with the specified <paramref name="key"/> in the parameter collection.</summary>
    /// <param name="key">The key.</param>
    /// <returns>True if the an entry was found, otherwise false.</returns>
    private static bool HasPrivate(string key)
    {
      return (_parameters != null && !string.IsNullOrEmpty(key) && _parameters.ContainsKey(key));
    }
    #endregion HasPrivate

    #region Globalize
    /// <summary>Creates a global parameter key from the specified <paramref name="key"/>.</summary>
    /// <param name="key">The key to globalize.</param>
    /// <returns>The parameter key converted to a global parameter key, or the original value.</returns>
    public static string Globalize(this string key)
    {
      string retVal = key;

      if (!string.IsNullOrEmpty(key) && !Regex.IsMatch(key, Constants.PATTERN_GLOBALIZED))
      {
        retVal = string.Format(Constants.FORMAT_GLOBAL_KEY, key.TrimStart(Constants.INDICATOR_PARAM).TrimEnd(Constants.INDICATOR_PARAM));
      }

      return retVal;
    }
    #endregion Globalize

    #region Clear
    /// <summary>Clears the underlying parameter collection.</summary>
    internal static void Clear()
    {
      if (_parameters != null)
      {
        _parameters.Clear();
      }
    }
    #endregion Clear

    #region Resolve
    /// <summary>Replaces all occurrences of parameters names found in the specified <paramref name="value"/> with the parameter value.</summary>
    /// <param name="value">The value to resolves tokens in.</param>
    public static string Resolve(string value)
    {
      string retVal = value;

      if (!string.IsNullOrEmpty(retVal))
      {
        Match m = Regex.Match(retVal, Constants.PATTERN_RESOLVE);
        while (m != null && m.Success)
        {
          if (!string.IsNullOrEmpty(m.Value) && Has(m.Value))
          {
            retVal = retVal.Replace(m.Value, (Get(m.Value) as string) ?? string.Empty);
            m = Regex.Match(retVal, Constants.PATTERN_RESOLVE);
          }
          else { m = null; }
        }
      }

      return retVal;
    }
    #endregion Resolve

    #endregion Methods
  }
}