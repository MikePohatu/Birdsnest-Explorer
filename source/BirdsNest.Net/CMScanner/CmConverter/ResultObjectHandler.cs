#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
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
//
#endregion
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace CMScanner.CmConverter
{
    public static class ResultObjectHandler
    {
        /// <summary>
        /// Attempts to return the value of a property, returns null on error
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string GetString(IResultObject resource, string property)
        {
            try
            {
                string s = resource[property].StringValue;
                return s;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to return the value of a propery, returns -1 on error
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static int GetInt(IResultObject resource, string property)
        {
            try
            {
                int i = resource[property].IntegerValue;
                return i;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Attempts to return the value of a propery, returns false on error
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool GetBool(IResultObject resource, string property)
        {
            try
            {
                bool b = resource[property].BooleanValue;
                return b;
            }
            catch
            {
                return false;
            }
        }
    }
}
