using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Auth
{
    public static class Types
    {
        public const string BirdsNestAdminsPolicy = "IsBirdsNestAdmin";
        public const string BirdsNestUsersPolicy ="IsBirdsNestUser";

        public const string BirdsNestAdminsClaim = "BirdsNestAdmins";
        public const string BirdsNestUsersClaim = "BirdsNestUsers";
    }
}
