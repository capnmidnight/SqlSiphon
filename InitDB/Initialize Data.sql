if not exists(select * from RoleTypes where RoleTypeName = 'Internal')
	insert into RoleTypes(RoleTypeName) values('Internal');
go

if not exists(select * from RoleTypes where RoleTypeName = 'State DOT')
	insert into RoleTypes(RoleTypeName) values('State DOT');
go

if not exists(select * from aspnet_Roles where RoleName = 'Admin')
	exec aspnet_Roles_CreateRole @ApplicationName = 'BBICARS', @RoleName = 'Admin';
go

if not exists(select * from aspnet_Roles where RoleName = 'User')
	exec aspnet_Roles_CreateRole @ApplicationName = 'BBICARS', @RoleName = 'User';
go

if not exists(select * from aspnet_Roles where RoleName = 'Alabama')
	exec aspnet_Roles_CreateRole @ApplicationName = 'BBICARS', @RoleName = 'Alabama';
go

if not exists(select * from aspnet_Roles where RoleName = 'Texas')
	exec aspnet_Roles_CreateRole @ApplicationName = 'BBICARS', @RoleName = 'Texas';
go

if not exists(select * from aspnet_Roles where RoleName = 'Wisconsin')
	exec aspnet_Roles_CreateRole @ApplicationName = 'BBICARS', @RoleName = 'Wisconsin';
go

if not exists(select * from aspnet_Roles where RoleName = 'Rieker')
	exec aspnet_Roles_CreateRole @ApplicationName = 'BBICARS', @RoleName = 'Rieker';
go

delete from RoleTypesForRoles;
insert into RoleTypesForRoles(RoleTypeID, RoleID)
select RoleTypeID, RoleID
from RoleTypes, aspnet_Roles
where RoleTypeName = 'Internal' and RoleName in ('Admin', 'User');

insert into RoleTypesForRoles(RoleTypeID, RoleID)
select RoleTypeID, RoleID
from RoleTypes, aspnet_Roles
where RoleTypeName = 'State DOT' and RoleName not in ('Admin', 'User');
go