﻿#region license
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
namespace Console
{
    public class InstallInfo
    {
        public static InstallInfo Instance { get; } = new InstallInfo();
        private InstallInfo() { }
        public string RootPath { get; set; }
        public string DbPath { get; set; }
        public string DbSvcAccount { get; set; }
        public ServerVersionInfo Versions { get { return ServerVersionInfo.Instance; } }
    }
}
