﻿#region license
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
using System.Collections.Generic;

namespace Console.Auth.Local
{
    public class LocalAuthConfiguration : IAuthConfiguration
    {
        public string Name { get; set; }
        public List<string> AdminUsers { get; set; } = new List<string>();
        public List<string> Users { get; set; } = new List<string>();
        public int TimeoutSeconds { get; set; } = 900;

        public ILogin GetLogin(string username, string password)
        {
            return new LocalLogin(this, username, password);
        }
    }
}
