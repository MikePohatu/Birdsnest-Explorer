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
namespace ADScanner
{
    public static class ErrorCodes
    {
        public static int ComputersCollectorSearcherNull { get { return 210; } }
        public static int ComputersCollectorSearcherException { get { return 211; } }

        public static int GroupsCollectorSearcherNull { get { return 220; } }
        public static int GroupsCollectorException { get { return 221; } }

        public static int UsersCollectorSearcherNull { get { return 230; } }
        public static int UsersCollectorException { get { return 231; } }

        public static int ForeignSecurityPrincipalCollectorSearcherNull { get { return 240; } }
        public static int ForeignSecurityPrincipalCollectorSearcherException { get { return 241; } }
    }
}
