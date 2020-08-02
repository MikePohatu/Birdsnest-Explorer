:begin

CREATE INDEX ON :BuiltinObject(id);
CREATE INDEX ON :BuiltinUser(id);
CREATE INDEX ON :BuiltinGroup(id);

CREATE CONSTRAINT ON (u:AD_User) ASSERT u.id IS UNIQUE;
CREATE CONSTRAINT ON (c:AD_Computer) ASSERT c.id IS UNIQUE;

CREATE INDEX ON :AD_ForeignSecurityPrincipal(dn);
CREATE INDEX ON :AD_ForeignSecurityPrincipal(id);
CREATE INDEX ON :AD_ForeignSecurityPrincipal(domainid);
CREATE INDEX ON :AD_Group(dn);
CREATE INDEX ON :AD_Group(id);
CREATE INDEX ON :AD_Object(id);
CREATE INDEX ON :AD_Object(dn);
CREATE INDEX ON :AD_Object(domainid);
CREATE INDEX ON :AD_User(dn);
CREATE INDEX ON :AD_Computer(dn);

CREATE CONSTRAINT ON (f:FS_Folder) ASSERT f.path IS UNIQUE;
CREATE INDEX ON :FS_Datastore(name);
CREATE INDEX ON :FS_Folder(scannerid);

CREATE CONSTRAINT ON (app:CTX_Application) ASSERT app.id IS UNIQUE;
CREATE CONSTRAINT ON (farm:CTX_Farm) ASSERT farm.name IS UNIQUE;

CREATE INDEX ON :CM_Collection(id);
CREATE INDEX ON :CM_Device(id);
CREATE INDEX ON :CM_User(id);
CREATE INDEX ON :CM_ConfigurationItem(id);

CREATE INDEX ON :WU_Update(KB);
CREATE INDEX ON :WU_Update(id);


:commit

:begin

WITH {
  groups: [
    {
      name: "EVERYONE",
      id: "S-1-1-0",
      description: "A group that includes all users."
    },
    {
      name: "LOCAL",
      id: "S-1-2-0",
      description: "A group that includes all users who have logged on locally."
    },
    {
      name: "CONSOLE_LOGON",
      id: "S-1-2-1",
      description: "A group that includes users who are logged on to the physical console. This SID can be used to implement security policies that grant different rights based on whether a user has been granted physical access to the console."
    },
    {
      name: "OWNER_RIGHTS",
      id: "S-1-3-4",
      description: "A group that represents the current owner of the object. When an ACE that carries this SID is applied to an object, the system ignores the implicit READ_CONTROL and WRITE_DAC permissions for the object owner."
    },
    {
      name: "DIALUP",
      id: "S-1-5-1",
      description: "A group that includes all users who have logged on through a dial-up connection."
    },
    {
      name: "NETWORK",
      id: "S-1-5-2",
      description: "A group that includes all users who have logged on through a network connection."
    },
    {
      name: "BATCH",
      id: "S-1-5-3",
      description: "A group that includes all users who have logged on through a batch queue facility."
    },
    {
      name: "INTERACTIVE",
      id: "S-1-5-4",
      description: "A group that includes all users who have logged on interactively."
    },
    {
      name: "SERVICE",
      id: "S-1-5-6",
      description: "A group that includes all security principals that have logged on as a service."
    },
    {
      name: "ANONYMOUS",
      id: "S-1-5-7",
      description: "A group that represents an anonymous logon."
    },
    {
      name: "ENTERPRISE_DOMAIN_CONTROLLERS",
      id: "S-1-5-9",
      description: "A group that includes all domain controllers in a forest that uses an Active Directory directory service."
    },
    {
      name: "AUTHENTICATED_USERS",
      id: "S-1-5-11",
      description: "A group that includes all users whose identities were authenticated when they logged on."
    },
    {
      name: "TERMINAL_SERVER_USER",
      id: "S-1-5-13",
      description: "A group that includes all users who have logged on to a Terminal Services server."
    },
    {
      name: "REMOTE_INTERACTIVE_LOGON",
      id: "S-1-5-14",
      description: "A group that includes all users who have logged on through a terminal services logon."
    },
    {
      name: "THIS_ORGANIZATION",
      id: "S-1-5-15",
      description: "A group that includes all users from the same organization. If this SID is present, the OTHER_ORGANIZATION SID MUST NOT be present."
    },
    {
      name: "TERMINAL_SERVER_USER",
      id: "S-1-5-13",
      description: "A group that includes all users who have logged on to a Terminal Services server."
    },
    {
      name: "REMOTE_INTERACTIVE_LOGON",
      id: "S-1-5-14",
      description: "A group that includes all users who have logged on through a terminal services logon."
    },
    {
      name: "THIS_ORGANIZATION",
      id: "S-1-5-15",
      description: "A group that includes all users from the same organization. If this SID is present, the OTHER_ORGANIZATION SID MUST NOT be present."
    },
    {
      name: "BUILTIN_ADMINISTRATORS",
      id: "S-1-5-32-544",
      description: "A built-in group. After the initial installation of the operating system, the only member of the group is the Administrator account. When a computer joins a domain, the Domain Administrators group is added to the Administrators group. When a server becomes a domain controller, the Enterprise Administrators group also is added to the Administrators group."
    },
    {
      name: "BUILTIN_USERS",
      id: "S-1-5-32-545",
      description: "A built-in group. After the initial installation of the operating system, the only member is the Authenticated Users group. When a computer joins a domain, the Domain Users group is added to the Users group on the computer."
    },
    {
      name: "BUILTIN_GUESTS",
      id: "S-1-5-32-546",
      description: "A built-in group. The Guests group allows users to log on with limited privileges to a computer's built-in Guest account."
    },
    {
      name: "POWER_USERS",
      id: "S-1-5-32-547",
      description: "A built-in group. Power users can perform the following actions: Create local users and groups, modify and delete accounts that they have created, remove users from the Power Users, Users, and Guests groups, install programs, create, manage, and delete local printers, create and delete file shares."
    },
    {
      name: "ACCOUNT_OPERATORS",
      id: "S-1-5-32-548",
      description: "A built-in group that exists only on domain controllers. Account Operators have permission to create, modify, and delete accounts for users, groups, and computers in all containers and organizational units of Active Directory except the Built-in container and the Domain Controllers OU. Account Operators do not have permission to modify the Administrators and Domain Administrators groups, nor do they have permission to modify the accounts for members of those groups."
    },
    {
      name: "SERVER_OPERATORS",
      id: "S-1-5-32-549",
      description: "A built-in group that exists only on domain controllers. Server Operators can perform the following actions: Log on to a server interactively, create and delete network shares, start and stop services, back up and restore files, format the hard disk of a computer, shut down the computer."
    },
    {
      name: "PRINTER_OPERATORS",
      id: "S-1-5-32-550",
      description: "A built-in group that exists only on domain controllers. Print Operators can manage printers and document queues."
    },
    {
      name: "BACKUP_OPERATORS",
      id: "S-1-5-32-551",
      description: "A built-in group. Backup Operators can back up and restore all files on a computer, regardless of the permissions that protect those files."
    },
    {
      name: "REPLICATOR",
      id: "S-1-5-32-552",
      description: "A built-in group that is used by the File Replication Service (FRS) on domain controllers."
    },
    {
      name: "ALIAS_PREW2KCOMPACC",
      id: "S-1-5-32-554",
      description: "A backward compatibility group that allows read access on all users and groups in the domain."
    },
    {
      name: "REMOTE_DESKTOP",
      id: "S-1-5-32-555",
      description: "An alias. Members of this group are granted the right to log on remotely."
    },
    {
      name: "NETWORK_CONFIGURATION_OPS",
      id: "S-1-5-32-556",
      description: "An alias. Members of this group can have some administrative privileges to manage configuration of networking features."
    },
    {
      name: "INCOMING_FOREST_TRUST_BUILDERS",
      id: "S-1-5-32-557",
      description: "An alias. Members of this group can create incoming, one-way trusts to this forest."
    },
    {
      name: "PERFMON_USERS",
      id: "S-1-5-32-558",
      description: "An alias. Members of this group have remote access to monitor this computer."
    },
    {
      name: "PERFLOG_USERS",
      id: "S-1-5-32-559",
      description: "An alias. Members of this group have remote access to schedule the logging of performance counters on this computer."
    },
    {
      name: "WINDOWS_AUTHORIZATION_ACCESS_GROUP",
      id: "S-1-5-32-560",
      description: "An alias. Members of this group have access to the computed tokenGroupsGlobalAndUniversal attribute on User objects."
    },
    {
      name: "TERMINAL_SERVER_LICENSE_SERVERS",
      id: "S-1-5-32-561",
      description: "An alias. A group for Terminal Server License Servers."
    },
    {
      name: "DISTRIBUTED_COM_USERS",
      id: "S-1-5-32-562",
      description: "An alias. A group for COM to provide computer-wide access controls that govern access to all call, activation, or launch requests on the computer."
    },
    {
      name: "IIS_IUSRS",
      id: "S-1-5-32-568",
      description: "A built-in group account for IIS users."
    },
    {
      name: "CRYPTOGRAPHIC_OPERATORS",
      id: "S-1-5-32-569",
      description: "A built-in group account for cryptographic operators."
    },
    {
      name: "EVENT_LOG_READERS",
      id: "S-1-5-32-573",
      description: "A built-in local group.  Members of this group can read event logs from the local machine."
    },
    {
      name: "CERTIFICATE_SERVICE_DCOM_ACCESS",
      id: "S-1-5-32-574",
      description: "A built-in local group. Members of this group are allowed to connect to Certification Authorities in the enterprise."
    },
    {
      name: "RDS_REMOTE_ACCESS_SERVERS",
      id: "S-1-5-32-575",
      description: "Servers in this group enable users of RemoteApp programs and personal virtual desktops access to these resources. This group needs to be populated on servers running RD Connection Broker. RD Gateway servers and RD Web Access servers used in the deployment need to be in this group."
    },
    {
      name: "RDS_ENDPOINT_SERVERS",
      id: "S-1-5-32-576",
      description: "A group that enables member servers to run virtual machines and host sessions."
    },
    {
      name: "RDS_MANAGEMENT_SERVERS",
      id: "S-1-5-32-577",
      description: "A group that allows members to access WMI resources over management protocols (such as WS-Management via the Windows Remote Management service)."
    },
    {
      name: "HYPER_V_ADMINS",
      id: "S-1-5-32-578",
      description: "A group that gives members access to all administrative features of Hyper-V."
    },
    {
      name: "ACCESS_CONTROL_ASSISTANCE_OPS",
      id: "S-1-5-32-579",
      description: "A local group that allows members to remotely query authorization attributes and permissions for resources on the local computer."
    },
    {
      name: "REMOTE_MANAGEMENT_USERS",
      id: "S-1-5-32-580",
      description: "Members of this group can access Windows Management Instrumentation (WMI) resources over management protocols (such as WS-Management [DMTF-DSP0226]). This applies only to WMI namespaces that grant access to the user."
    },
    {
      name: "LOCAL_ACCOUNT",
      id: "S-1-5-113",
      description: "A group that includes all users who are local accounts."
    },
    {
      name: "LOCAL_ACCOUNT_AND_MEMBER_OF_ADMINISTRATORS_GROUP",
      id: "S-1-5-114",
      description: "A group that includes all users who are local accounts and members of the administrators group."
    },
    {
      name: "OTHER_ORGANIZATION",
      id: "S-1-5-1000",
      description: "A group that includes all users and computers from another organization. If this SID is present, THIS_ORGANIZATION SID MUST NOT be present."
    },
    {
      name: "ALL_APP_PACKAGES",
      id: "S-1-15-2-1",
      description: "All applications running in an app package context."
    }
  ],
  users: [
    {
      name: "NULL",
      id: "S-1-0-0",
      description: "No Security principal."
    },
    {
      name: "CREATOR_OWNER",
      id: "S-1-3-0",
      description: "A placeholder in an inheritable access control entry (ACE). When the ACE is inherited, the system replaces this SID with the SID for the object's creator."
    },
    {
      name: "CREATOR_GROUP",
      id: "S-1-3-1",
      description: "A placeholder in an inheritable ACE. When the ACE is inherited, the system replaces this SID with the SID for the primary group of the object's creator."
    },
    {
      name: "OWNER_SERVER",
      id: "S-1-3-2",
      description: "A placeholder in an inheritable ACE. When the ACE is inherited, the system replaces this SID with the SID for the object's owner server."
    },
    {
      name: "GROUP_SERVER",
      id: "S-1-3-3",
      description: "A placeholder in an inheritable ACE. When the ACE is inherited, the system replaces this SID with the SID for the object's group server."
    },
    {
      name: "NT_AUTHORITY",
      id: "S-1-5",
      description: "A SID containing only the SECURITY_NT_AUTHORITY identifier authority."
    },
    {
      name: "PROXY",
      id: "S-1-5-8",
      description: "Identifies a SECURITY_NT_AUTHORITY Proxy."
    },
    {
      name: "PRINCIPAL_SELF",
      id: "S-1-5-10",
      description: "A placeholder in an inheritable ACE on an account object or group object in Active Directory. When the ACE is inherited, the system replaces this SID with the SID for the security principal that holds the account."
    },
    {
      name: "RESTRICTED_CODE",
      id: "S-1-5-12",
      description: "This SID is used to control access by untrusted code. ACL validation against tokens with RC consists of two checks, one against the token's normal list of SIDs and one against a second list (typically containing RC - the \"RESTRICTED_CODE\" token - and a subset of the original token SIDs). Access is granted only if a token passes both tests. Any ACL that specifies RC must also specify WD - the \"EVERYONE\" token. When RC is paired with WD in an ACL, a superset of \"EVERYONE\", including untrusted code, is described."
    },
    {
      name: "IUSR",
      id: "S-1-5-17",
      description: "An account that is used by the default Internet Information Services (IIS) user."
    },
    {
      name: "LOCAL_SYSTEM",
      id: "S-1-5-18",
      description: "An account that is used by the operating system."
    },
    {
      name: "LOCAL_SERVICE",
      id: "S-1-5-19",
      description: "A local service account."
    },
    {
      name: "NETWORK_SERVICE",
      id: "S-1-5-20",
      description: "A network service account."
    },
    {
      name: "COMPOUNDED_AUTHENTICATION",
      id: "S-1-5-21-0-0-0-496",
      description: "Device identity is included in the Kerberos service ticket. If a forest boundary was crossed, then claims transformation occurred."
    },
    {
      name: "CLAIMS_VALID",
      id: "S-1-5-21-0-0-0-497",
      description: "Claims were queried for in the account's domain, and if a forest boundary was crossed, then claims transformation occurred."
    },
    {
      name: "WRITE_RESTRICTED_CODE",
      id: "S-1-5-33",
      description: "A SID that allows objects to have an ACL that lets any service process with a write-restricted token to write to the object."
    },
    {
      name: "NTLM_AUTHENTICATION",
      id: "S-1-5-64-10",
      description: "A SID that is used when the NTLM authentication package authenticated the client."
    },
    {
      name: "SCHANNEL_AUTHENTICATION",
      id: "S-1-5-64-14",
      description: "A SID that is used when the SChannel authentication package authenticated the client."
    },
    {
      name: "DIGEST_AUTHENTICATION",
      id: "S-1-5-64-21",
      description: "A SID that is used when the Digest authentication package authenticated the client."
    },
    {
      name: "THIS_ORGANIZATION_CERTIFICATE",
      id: "S-1-5-65-1",
      description: "A SID that indicates that the client's Kerberos service ticket's PAC contained a NTLM_SUPPLEMENTAL_CREDENTIAL structure (as specified in [MS-PAC] section 2.6.4). If the OTHER_ORGANIZATION SID is present, then this SID MUST NOT be present. "
    },
    {
      name: "NT_SERVICE",
      id: "S-1-5-80",
      description: "An NT Service account prefix."
    },
    {
      name: "USER_MODE_DRIVERS",
      id: "S-1-5-84-0-0-0-0-0",
      description: "Identifies a user-mode driver process."
    },
    {
      name: "ML_UNTRUSTED",
      id: "S-1-16-0",
      description: "An untrusted integrity level."
    },
    {
      name: "ML_LOW",
      id: "S-1-16-4096",
      description: "A low integrity level."
    },
    {
      name: "ML_MEDIUM",
      id: "S-1-16-8192",
      description: "A medium integrity level."
    },
    {
      name: "ML_MEDIUM_PLUS",
      id: "S-1-16-8448",
      description: "A medium-plus integrity level."
    },
    {
      name: "ML_HIGH",
      id: "S-1-16-12288",
      description: "A high integrity level."
    },
    {
      name: "ML_SYSTEM",
      id: "S-1-16-16384",
      description: "A system integrity level."
    },
    {
      name: "ML_PROTECTED_PROCESS",
      id: "S-1-16-20480",
      description: "A protected-process integrity level."
    },
    {
      name: "ML_SECURE_PROCESS",
      id: "S-1-16-28672",
      description: "A secure process integrity level."
    },
    {
      name: "AUTHENTICATION_AUTHORITY_ASSERTED_IDENTITY",
      id: "S-1-18-1",
      description: "A SID that means the client's identity is asserted by an authentication authority based on proof of possession of client credentials."
    },
    {
      name: "SERVICE_ASSERTED_IDENTITY",
      id: "S-1-18-2",
      description: "A SID that means the client's identity is asserted by a service."
    },
    {
      name: "FRESH_PUBLIC_KEY_IDENTITY",
      id: "S-1-18-3",
      description: "A SID that means the client's identity is asserted by an authentication authority based on proof of current possession of client public key credentials."
    },
    {
      name: "KEY_TRUST_IDENTITY",
      id: "S-1-18-4",
      description: "A SID that means the client's identity is based on proof of possession of public key credentials using the key trust object."
    },
    {
      name: "KEY_PROPERTY_MFA",
      id: "S-1-18-5",
      description: "A SID that means the key trust object had the multifactor authentication (MFA) property."
    },
    {
      name: "KEY_PROPERTY_ATTESTATION",
      id: "S-1-18-6",
      description: "A SID that means the key trust object had the attestation property."
    }
  ]
} AS json
UNWIND json.users AS user
MERGE (u {id:user.id})
SET u.name=user.name, u.description=user.description, u:BuiltinUser, u:BuiltinObject
WITH json
UNWIND json.groups AS group
MERGE (g {id:group.id})
SET g.name=group.name, g.description=group.description, g:BuiltinGroup, g:BuiltinObject
WITH *
MATCH (n:Orphaned:BuiltinGroup) REMOVE n:Orphaned
WITH *
MATCH (n:Orphaned:BuiltinUser) REMOVE n:Orphaned
RETURN *;

:commit
