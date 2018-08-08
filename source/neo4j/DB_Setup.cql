begin

CREATE CONSTRAINT ON (g:AD_Group) ASSERT g.id IS UNIQUE;
CREATE CONSTRAINT ON (u:AD_User) ASSERT u.id IS UNIQUE;
CREATE CONSTRAINT ON (c:AD_Computer) ASSERT c.id IS UNIQUE;
CREATE CONSTRAINT ON (a:CTX_Application) ASSERT a.id IS UNIQUE;
CREATE CONSTRAINT ON (f:CTX_Farm) ASSERT f.env IS UNIQUE;
CREATE CONSTRAINT ON (f:FS_Folder) ASSERT f.path IS UNIQUE;

CREATE INDEX ON :FS_Datastore(name);
CREATE INDEX ON :AD_Group(dn);
CREATE INDEX ON :AD_User(dn);
CREATE INDEX ON :AD_Computer(dn);

commit

begin

WITH {
	groups:[
		{
			name: "Everyone",
			id: "S-1-1-0",
			description: "The generic group Everyone automatically includes everyone who uses the computer, even anonymous guests. The identifier authority value for this SID is 1 (World Authority). It has only one subauthority value, 0 (Null RID)."
		},
		{
			name: "Creator Group",
			id: "S-1-3-1",
			description: "A placeholder in an inheritable ACE. When the ACE is inherited, the system replaces this SID with the SID for the primary group of the object's creator. The primary group is used only by the POSIX subsystem."
		},
		{
			name: "Creator Group Server",
			id: "S-1-3-3",
			description: "A group that represents the current owner of the object. When an ACE that carries this SID is applied to an object, the system ignores the implicit READ_CONTROL and WRITE_DAC permissions for the object owner."
		},
		{
			name: "Local",
			id: "S-1-2-0",
			description: "A group that includes all users who have logged on locally."
		},
		{
			name: "Console Logon",
			id: "S-1-2-1",
			description: "A group that includes users who are logged on to the physical console."
		},
		{
			name: "Dialup",
			id: "S-1-5-1",
			description: "A group that includes all users who have logged on through a dial-up connection. Membership is controlled by the operating system."
		},
		{
			name: "Network",
			id: "S-1-5-2",
			description: "A group that includes all users that have logged on through a network connection. Membership is controlled by the operating system."
		},
		{
			name: "Batch",
			id: "S-1-5-3",
			description: "A group that includes all users that have logged on through a batch queue facility. Membership is controlled by the operating system."
		},
		{
			name: "Interactive",
			id: "S-1-5-4",
			description: "A group that includes all users that have logged on interactively. Membership is controlled by the operating system."
		},
		{
			name: "Logon Session",
			id: "S-1-5-5-X-Y",
			description: "A logon session. The X and Y values for these SIDs are different for each session."
		},
		{
			name: "Service",
			id: "S-1-5-6",
			description: "A group that includes all security principals that have logged on as a service. Membership is controlled by the operating system."
		},
		{
			name: "Anonymous",
			id: "S-1-5-7",
			description: "A group that includes all users that have logged on anonymously. Membership is controlled by the operating system."
		},
		{
			name: "Enterprise Domain Controllers",
			id: "S-1-5-9",
			description: "A group that includes all domain controllers in a forest that uses an Active Directory directory service. Membership is controlled by the operating system."
		},
		{
			name: "Authenticated Users",
			id: "S-1-5-11",
			description: "A group that includes all users whose identities were authenticated when they logged on. Membership is controlled by the operating system."
		},
		{
			name: "Terminal Server Users",
			id: "S-1-5-13",
			description: "A group that includes all users that have logged on to a Terminal Services server. Membership is controlled by the operating system."
		},
		{
			name: "Remote Interactive Logon",
			id: "S-1-5-14",
			description: "A group that includes all users who have logged on through a terminal services logon."
		},
		{
			name: "This Organization",
			id: "S-1-5-15",
			description: "A group that includes all users from the same organization. Only included with AD accounts and only added by a Windows Server 2003 or later domain controller."
		}
	],	
	users:[
		{
			name: "Creator Authority",
			id: "S-1-3",
			description: "An identifier authority."
		},
		{
			name: "Creator Owner",
			id: "S-1-3-0",
			description: "The generic user Creator Owner is a placeholder in an inheritable access control entry (ACE). When the ACE is inherited, the system replaces the SID for Creator Owner with the SID for the object's current owner. The identifier authority value for this SID is 3 (Creator Authority). It has only one subauthority value, 0 (Null RID)."
		},
		{
			name: "This Organization",
			id: "S-1-5-17",
			description: "An account that is used by the default Internet Information Services (IIS) user."
		},
		{
			name: "Local System",
			id: "S-1-5-18",
			description: "A service account that is used by the operating system."
		},
		{
			name: "Restricted Code",
			id: "S-1-5-12",
			description: "This SID is reserved for future use."
		},
		{
			name: "Creator Owner Server",
			id: "S-1-3-2",
			description: "This SID is not used in Windows 2000."
		},
		{
			name: "Principal Self",
			id: "S-1-5-10",
			description: "The generic user Principal Self is a placeholder in an ACE on a User, Group, or Computer object in Active Directory. When you grant permission to Principal Self, you grant it to the security principal represented by the object. During an access check, the operating system replaces the SID for Principal Self with the SID for the security principal represented by the object. The identifier authority for this SID is 5 (NT Authority). It has only one subauthority value, 10 (Self RID)."
		},
		{
			name: "Null SID(Nobody)",
			id: "S-1–0–0",
			description: "A group with no members. This is often used when a sid is not known."
		},
		{
			name: "Local Authority",
			id: "S-1-2",
			description: "An identifier authority."
		},
		{
			name: "Creator Authority",
			id: "S-1-3",
			description: "An identifier authority."
		},
		{
			name: "Non-unique Authority",
			id: "S-1-4",
			description: "An identifier authority."
		},
		{
			name: "NT Authority",
			id: "S-1-5",
			description: "An identifier authority."
		},
		{
			name: "Proxy",
			id: "S-1-5-8",
			description: "This SID is not used in Windows 2000."
		}
	]
} AS json
UNWIND json.users AS user
MERGE (u {id:user.id})
ON CREATE SET u=user,u:BuiltinUser
ON MATCH SET u:BuiltinUser,u.description=user.description
WITH json
UNWIND json.groups AS group
MERGE (g {id:group.id})
ON CREATE SET g=group,g:BuiltinGroup
ON MATCH SET g:BuiltinGroup,g.description=group.description
WITH *
MATCH (n:ORPHANED:BuiltinGroup) REMOVE n:ORPHANED
WITH *
MATCH (n:ORPHANED:BuiltinUser) REMOVE n:ORPHANED
RETURN *;

commit
