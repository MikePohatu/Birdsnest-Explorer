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
#endregion

using System;

namespace ADScanner.ActiveDirectory
{
    [Flags]
    public enum UserAccountControlDefinitions
    {
        SCRIPT = 1,
        ACCOUNTDISABLE = 2,
        HOMEDIR_REQUIRED = 8,
        LOCKOUT = 16,
        PASSWD_NOTREQD = 32,
        PASSWD_CANT_CHANGE = 64,
        ENCRYPTED_TEXT_PWD_ALLOWED = 128,
        TEMP_DUPLICATE_ACCOUNT = 256,
        NORMAL_ACCOUNT = 512,
        INTERDOMAIN_TRUST_ACCOUNT = 2048,
        WORKSTATION_TRUST_ACCOUNT = 4096,
        SERVER_TRUST_ACCOUNT = 8192,
        DONT_EXPIRE_PASSWORD = 65536,
        MNS_LOGON_ACCOUNT = 131072,
        SMARTCARD_REQUIRED = 262144,
        TRUSTED_FOR_DELEGATION = 524288,
        NOT_DELEGATED = 1048576,
        USE_DES_KEY_ONLY = 2097152,
        DONT_REQ_PREAUTH = 4194304,
        PASSWORD_EXPIRED = 8388608,
        TRUSTED_TO_AUTH_FOR_DELEGATION = 16777216,
        PARTIAL_SECRETS_ACCOUNT = 67108864
    }
}
