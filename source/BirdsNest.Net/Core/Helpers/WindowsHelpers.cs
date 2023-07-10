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
using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Core.Helpers
{
    public static class WindowsHelpers
    {
        public static void Subst(string drive, string path, string label)
        {
            string newpath = path;
            if (path.EndsWith(@"\") == false) { newpath = path + @"\"; }

            string existingpath;
            if (IsSubstedPath(drive, out existingpath))
            {
                if (existingpath.Equals(newpath, StringComparison.OrdinalIgnoreCase))
                {
                    Log.Debug("Subst drive already mapped to correct path: " + newpath);
                    LabelSubstDriveLetter(drive, label);
                }
                else
                {
                    Log.Error("Drive " + drive + " is already mapped to another path: " + existingpath);
                }
            }
            else
            {
                string sys32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
                Process.Start(sys32 + @"\subst.exe", drive + " " + newpath);
            }

            LabelSubstDriveLetter(drive, label);
        }

        public static void LabelSubstDriveLetter(string drive, string label)
        {
            string shortdrive = drive.Replace(@":", "");
            RegistryHelpers.SetStringValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\DriveIcons\" + shortdrive + @"\DefaultLabel", null, label);
        }


        #region Attribution: https://stackoverflow.com/a/37278193
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

        public static bool IsSubstedPath(string path, out string realPath)
        {
            StringBuilder pathInformation = new StringBuilder(250);
            string driveLetter = null;
            uint winApiResult = 0;

            realPath = null;

            try
            {
                // Get the drive letter of the path
                driveLetter = Path.GetPathRoot(path).Replace("\\", "");
            }
            catch (ArgumentException)
            {
                return false;
                //<------------------
            }

            winApiResult = QueryDosDevice(driveLetter, pathInformation, 250);

            if (winApiResult == 0)
            {
                int lastWinError = Marshal.GetLastWin32Error(); // here is the reason why it fails - not used at the moment!

                return false;
                //<-----------------
            }

            // If drive is substed, the result will be in the format of "\??\C:\RealPath\".
            if (pathInformation.ToString().StartsWith("\\??\\"))
            {
                // Strip the \??\ prefix.
                string realRoot = pathInformation.ToString().Remove(0, 4);

                // add backshlash if not present
                realRoot += pathInformation.ToString().EndsWith(@"\") ? "" : @"\";

                //Combine the paths.
                realPath = Path.Combine(realRoot, path.Replace(Path.GetPathRoot(path), ""));

                return true;
                //<--------------
            }

            realPath = path;

            return false;
        }
        #endregion

        #region Attribution: https://stackoverflow.com/a/33257128
        private static IDictionary<DriveInfo, string> GetMappedNetworkDrives()
        {
            Log.Trace("Enumerating existing network drives");
            var result = new Dictionary<DriveInfo, string>();

            try
            {
                var rawDrives = new IWshNetwork_Class().EnumNetworkDrives();
                if (rawDrives == null)
                {
                    Log.Trace("GetMappedNetworkDrives - EnumNetworkDrives returned null");
                }
                else if (rawDrives.length == 0)
                {
                    Log.Trace("GetMappedNetworkDrives - EnumNetworkDrives returned zero results");
                }
                else
                {
                    for (int i = 0; i < rawDrives.length; i += 2)
                    {
                        var info = new DriveInfo(rawDrives.Item(i));
                        Log.Trace($" Drive: {info.Name}");
                        result.Add(info, rawDrives.Item(i + 1));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error enumerating existing drives");
            }

            return result;
        }
        #endregion


        #region Attribution: https://stackoverflow.com/a/24439157
        public static bool MapDrive(string path, string letter, string description)
        {
            var existing = GetMappedNetworkDrives();
            foreach (var info in existing.Keys)
            {
                if (info.Name.StartsWith(letter, StringComparison.OrdinalIgnoreCase))
                {
                    string existingpath = existing[info];
                    if (existingpath.Equals(path, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Debug("Matching drive mapping found: " + path);
                        return true;
                    }
                    else
                    {
                        Log.Error("An existing drive is already mapped to " + letter);
                        return false;
                    }
                }
            }

            Log.Trace("Creating drive map to: " + path);

            var mapResult = WNetAddConnection2(
                new NETRESOURCE
                {
                    Scope = ResourceScope.Connected,
                    ResourceType = ResourceType.Disk,
                    DisplayType = ResourceDisplaytype.Generic,
                    Usage = 0,
                    LocalName = letter,
                    RemoteName = path,
                    Comment = description,
                    Provider = null
                }
                , null
                , null
                , 0);
            if (mapResult != 0)
            {
                Log.Error(Log.Highlight("AddConnection failed: " + mapResult));
                return false;
                // >0? check  http://msdn.microsoft.com/en-us/library/windows/desktop/ms681383(v=vs.85).aspx
            }
            else
            {
                return true;
            }
        }

        [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int WNetGetConnection([MarshalAs(UnmanagedType.LPTStr)] string localName, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName, ref int length);

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NETRESOURCE netResource, string password, string username, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        private class NETRESOURCE
        {
            public ResourceScope Scope;
            public ResourceType ResourceType;
            public ResourceDisplaytype DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        private enum ResourceScope : int
        {
            Connected = 1,
            GlobalNetwork,
            Remembered,
            Recent,
            Context
        }

        private enum ResourceType : int
        {
            Any = 0,
            Disk = 1,
            Print = 2,
            Reserved = 8,
        }

        private enum ResourceDisplaytype : int
        {
            Generic = 0x0,
            Domain = 0x01,
            Server = 0x02,
            Share = 0x03,
            File = 0x04,
            Group = 0x05,
            Network = 0x06,
            Root = 0x07,
            Shareadmin = 0x08,
            Directory = 0x09,
            Tree = 0x0a,
            Ndscontainer = 0x0b
        }
        #endregion

        #region Attribution: https://social.technet.microsoft.com/Forums/ie/en-US/bd2ced9c-9206-490f-b327-1fcae3184292/creating-network-locations?forum=ITCG
        /// <summary>
        /// Create a Windows 'Network Location'. Return the path to the network location in the file system
        /// </summary>
        /// <param name="directorypath"></param>
        /// <param name="name"></param>
        /// <param name="icon"></param>
        /// <param name="target"></param>
        /// <param name="description"></param>
        /// <param name="removeexisting"></param>
        /// <returns></returns>
        public static string CreateNetworkLocation(string directorypath, string name, string icon, string target, string description, bool removeexisting)
        {
            const string desktopINI = "[.ShellClassInfo]\r\nCLSID2={0AFACED1-E828-11D1-9187-B532F1E9575D}\r\nFlags=2";

            string path = directorypath + @"\" + name;
            Log.Trace("Creating network location to: " + path);

            if (removeexisting && Directory.Exists(path))
            {
                DeleteNetworkLocation(path);
            }
            else if (Directory.Exists(path))
            {
                ResetDirectoryAttributes(path, true);
            }

            Directory.CreateDirectory(path);
            System.IO.File.SetAttributes(path, FileAttributes.ReadOnly);

            System.IO.File.WriteAllText(path + @"\desktop.ini", desktopINI);

            System.IO.File.SetAttributes(path + @"\desktop.ini", FileAttributes.Hidden);
            System.IO.File.SetAttributes(path + @"\desktop.ini", FileAttributes.System);

            string shortcutLocation = Path.Combine(path + @"\target.lnk");

            CreateShortcut(target, shortcutLocation, description, icon);

            return path;
        }

        public static void DeleteNetworkLocation(string path)
        {
            try
            {
                //remove any attributes so the delete works
                var di = new DirectoryInfo(path);
                di.Attributes = FileAttributes.Normal;

                if (System.IO.File.Exists(path + @"\desktop.ini"))
                {
                    var fi = new FileInfo(path + @"\desktop.ini");
                    fi.Attributes = FileAttributes.Normal;
                }

                if (System.IO.File.Exists(path + @"\target.lnk"))
                {
                    var fi = new FileInfo(path + @"\target.lnk");
                    fi.Attributes = FileAttributes.Normal;
                }

                Directory.Delete(path, true);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error deleting network location");
            }
        }

        /// <summary>
        /// Reset the attributes on a directory and any files to FileAttributes.Normal. Optionally
        /// rescurse through any subfolders
        /// </summary>
        /// <param name="path"></param>
        /// <param name="recursive"></param>
        public static void ResetDirectoryAttributes(string path, bool recursive)
        {
            var di = new DirectoryInfo(path);
            di.Attributes = FileAttributes.Normal;
            foreach (var file in Directory.EnumerateFiles(path))
            {
                ResetFileAttributes(file);
            }

            if (recursive)
            {
                foreach (var subdir in Directory.EnumerateDirectories(path))
                {
                    ResetDirectoryAttributes(subdir, true);
                }
            }
        }

        /// <summary>
        /// Reset the attributes on a file to FileAttributes.Normal
        /// </summary>
        /// <param name="path"></param>
        public static void ResetFileAttributes(string path)
        {
            var fi = new FileInfo(path);
            fi.Attributes = FileAttributes.Normal;
        }

        public static void CreateWebShortcut(string target, string filepath, string description, string icon)
        {
            string urlpath = filepath.ToLower().EndsWith(".url") ? filepath : filepath + ".url";
            var iconsplit = icon.Split(',');
            string iconfile = iconsplit[0];
            string iconindex = iconsplit.Length > 1 ? iconsplit[1] : "0";
            System.IO.File.WriteAllText(urlpath, "[InternetShortcut]\r\nURL=" + target + "\r\nIconIndex=" + iconindex + "\r\nIconFile=" + iconfile);
        }
        #endregion

        public static void CreateShortcut(string target, string filepath, string description, string icon)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(filepath);

            shortcut.Description = description;
            shortcut.TargetPath = target;
            shortcut.IconLocation = icon;
            shortcut.Save();
        }

        public static string GetIconPath(string filename)
        {
            string path = null;
            string extension = Path.GetExtension(filename);
            if (string.IsNullOrWhiteSpace(extension) == false)
            {
                string typedef = RegistryHelpers.GetStringValue(@"HKEY_CLASSES_ROOT\" + extension, null, null);
                if (string.IsNullOrWhiteSpace(typedef) == false)
                {
                    path = RegistryHelpers.GetStringValue(@"HKEY_CLASSES_ROOT\" + typedef + @"\DefaultIcon", null, null);
                }
            }

            return path;
        }
    }
}
