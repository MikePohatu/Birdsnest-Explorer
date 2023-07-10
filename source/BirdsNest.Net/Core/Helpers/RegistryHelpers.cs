#region license
// Copyright (c) 2019-2023 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
using Core.Logging;
using Microsoft.Win32;
using System;

namespace Core.Helpers
{
    public static class RegistryHelpers
    {
        public static bool ValueExists(string key, string value)
        {
            object o = null;
            try
            {
                o = Registry.GetValue(key, value, null);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error getting registry value");
            }

            return o == null ? false : true;
        }

        public static bool KeyExists(string key)
        {
            object o = null;
            try
            {
                o = Registry.GetValue(key, null, -1);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error getting registry key");
            }

            return o == null ? false : true;
        }

        public static int GetIntValue(string key, string value, int defaultvalue)
        {
            int reg = defaultvalue;
            try
            {
                object o = Registry.GetValue(key, value, defaultvalue);
                if (o != null) { reg = (int)o; }
                else { reg = defaultvalue; }
            }
            catch (Exception e)
            {
                Log.Debug(e, ("Error getting registry value " + key + "\\" + value));
            }
            return reg;
        }

        /// <summary>
        /// Get a string value from registry. Return the default value if not found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static string GetStringValue(string key, string value, string defaultvalue)
        {
            string reg = defaultvalue;
            try
            {
                object o = Registry.GetValue(key, value, defaultvalue);
                if (o != null) { reg = (string)o; }
                else { reg = defaultvalue; }
            }
            catch (Exception e)
            {
                Log.Debug(e, "Error getting registry value " + key + "\\" + value);
            }
            return reg;
        }

        public static void SetStringValue(string key, string valueName, string value)
        {
            try
            {
                Registry.SetValue(key, valueName, value, RegistryValueKind.String);
            }
            catch (Exception e)
            {
                Log.Debug(e, "Error setting registry SZ value " + key + "\\" + valueName + "to " + value);
            }
        }

        public static void SetDWordValue(string key, string valueName, int value)
        {
            try
            {
                Registry.SetValue(key, valueName, value, RegistryValueKind.DWord);
            }
            catch (Exception e)
            {
                Log.Debug(e, "Error setting registry DWORD value " + key + "\\" + valueName + "to " + value);
            }
        }
    }
}
