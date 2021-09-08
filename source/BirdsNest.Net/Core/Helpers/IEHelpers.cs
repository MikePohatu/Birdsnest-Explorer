#region license
// Copyright (c) 2021 20Road Limited
//
// This file is part of Folders
//
// Folders is free software: with the exception of attributed regions of code,
// you can redistribute it and/or modify it under the terms of the GNU
// General Public License as published by the Free Software Foundation, 
// version 3 of the License.
//
// Where attributed code is incompatible with the terms of the GPLv3, the 
// license assigned to that code takes precedence.
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
using System;

namespace Core.Helpers
{
    public static class IEHelpers
    {
        public enum BrowserEmulationMode : int
        {
            IE7Standards = 7000,
            IE8 = 8000,
            IE9 = 9000,
            IE10 = 10000,
            IE11 = 11000,
            IE11Forced = 11001
        }

        public enum BrowserBitDepth
        {
            X86, Amd64, Both
        }

        #region Attribution: https://www.tallan.com/blog/2014/04/15/setting-compatibility-mode-of-a-wpf-web-browser/
        private static void SetBrowserFeatureControlKey(string feature, string appName, int value, bool wow64)
        {
            string keypath = string.Concat(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature);

            //if windows on windows64 is set to true, we want to set the 32bit keys. If on a 64bit OS, add the Wow6432Node
            if (wow64)
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    keypath = string.Concat(@"HKEY_CURRENT_USER\Software\Wow6432Node\Microsoft\Internet Explorer\Main\FeatureControl\", feature);
                }
            }

            RegistryHelpers.SetDWordValue(keypath, appName, value);

        }
        public static void SetBrowserEmulationMode(BrowserEmulationMode emulationmode, BrowserBitDepth bitdepth)
        {
            var fileName = AppDomain.CurrentDomain.FriendlyName;

            if (string.Compare(fileName, "devenv.exe", true) == 0 || string.Compare(fileName, "XDesProc.exe", true) == 0) { return; }
            var mode = (int)emulationmode;
            if (bitdepth == BrowserBitDepth.Both || bitdepth == BrowserBitDepth.X86)
            {
                SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, mode, false);
            }

            if (bitdepth == BrowserBitDepth.Both || bitdepth == BrowserBitDepth.Amd64)
            {
                SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, mode, true);
            }
        }
        #endregion
    }
}
