#region license
// Copyright (c) 2019-2020 "20Road"
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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Helpers
{
    public static class RegistryHelpers
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static bool ValueExists(string key, string value)
        {
            object o = null;
            try
            {
                o = Registry.GetValue(key, value, null);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error getting registry value");
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
                _logger.Error(e, "Error getting registry key");
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
                _logger.Debug(e, "Error getting registry value {0}\\{1}", key, value);
            }
            return reg;
        }

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
                _logger.Debug(e, "Error getting registry value {0}\\{1}", key, value);
            }
            return reg;
        }

        /// <summary>
        /// Return a dictionary of values with their names as keys. Valid hive values:
        /// HKLM, HKCU, HKCR
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="keypath"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetValues(string hive, string keypath)
        {
            if (hive == null) { throw new ArgumentException("hive cannot be null"); }

            RegistryHive root;
            string hiveupper = hive.ToUpper();
            switch (hiveupper)
            {
                case "HKLM":
                    root = RegistryHive.LocalMachine;
                    break;
                case "HKCU":
                    root = RegistryHive.CurrentUser;
                    break;
                case "HKCR":
                    root = RegistryHive.ClassesRoot;
                    break;
                default:
                    throw new ArgumentException($"Invalid hive value: {hive}. Must be HKLM, HKCU, or HKCR");
            }

            Dictionary<string, object> results = new Dictionary<string, object>();
            try
            {
                RegistryKey rootkey = RegistryKey.OpenBaseKey(root, RegistryView.Default);
                RegistryKey subkey = rootkey.OpenSubKey(keypath);
                if (subkey == null) { _logger.Warn($"Registry sub key not found: {hive}\\{keypath}"); }
                else
                {
                    foreach (string valname in subkey.GetValueNames())
                    {
                        object val = subkey.GetValue(valname);
                        results.Add(valname, val);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error reading registry values from {hive}\\{keypath}");
            }
            return results;
        }

        public static void SetStringValue(string key, string valueName, string value)
        {
            try
            {
                Registry.SetValue(key, valueName, value, RegistryValueKind.String);
            }
            catch (Exception e)
            {
                _logger.Debug(e, "Error setting registry SZ value {0}\\{1} to {2}", key, valueName, value);
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
                _logger.Debug(e, "Error setting registry DWORD value {0}\\{1} to {2}", key, valueName, value);
            }
        }

        /// <summary>
        /// Get the ImagePath for a given service. return string.Empty if not found
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static string GetServiceInstallPath(string serviceName)
        {
            RegistryKey regkey;
            regkey = Registry.LocalMachine.OpenSubKey(string.Format(@"SYSTEM\CurrentControlSet\services\{0}", serviceName));

            if (regkey == null || regkey.GetValue("ImagePath") == null)
                return string.Empty;
            else
                return regkey.GetValue("ImagePath").ToString();
        }
    }
}
