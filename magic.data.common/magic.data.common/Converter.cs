/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;

namespace magic.data.common
{
    /// <summary>
    /// Helper class to convert values from database to lambda values.
    /// </summary>
    public static class Converter
    {
        /// <summary>
        /// Converts the given database value to the relevant native .Net type.
        /// for instance, if given DBNull as type, it will return simply "null" value, etc.
        /// </summary>
        /// <param name="value">Database value.</param>
        /// <returns>The value in the equivalent .Net type.</returns>
        public static object GetValue(object value)
        {
            if (value == null || value.GetType() == typeof(DBNull))
                return null;
            return value;
        }
    }
}
