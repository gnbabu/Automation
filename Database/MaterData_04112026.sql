
GO
INSERT [aut].[PriorityStatus] ([PriorityID], [PriorityName]) VALUES (1, N'Tier 1')
GO
INSERT [aut].[PriorityStatus] ([PriorityID], [PriorityName]) VALUES (2, N'Tier 2')
GO
INSERT [aut].[PriorityStatus] ([PriorityID], [PriorityName]) VALUES (3, N'Tier 3')
GO
SET IDENTITY_INSERT [aut].[TimeZone] ON 
GO
INSERT [aut].[TimeZone] ([TimeZoneID], [TimeZoneName], [UTCOffsetMinutes], [Description]) VALUES (1, N'UTC', 0, N'Coordinated Universal Time')
GO
INSERT [aut].[TimeZone] ([TimeZoneID], [TimeZoneName], [UTCOffsetMinutes], [Description]) VALUES (2, N'Eastern Standard Time', -300, N'UTC-5')
GO
INSERT [aut].[TimeZone] ([TimeZoneID], [TimeZoneName], [UTCOffsetMinutes], [Description]) VALUES (3, N'Central Standard Time', -360, N'UTC-6')
GO
INSERT [aut].[TimeZone] ([TimeZoneID], [TimeZoneName], [UTCOffsetMinutes], [Description]) VALUES (4, N'India Standard Time', 330, N'UTC+5:30')
GO
INSERT [aut].[TimeZone] ([TimeZoneID], [TimeZoneName], [UTCOffsetMinutes], [Description]) VALUES (5, N'Pacific Standard Time', -480, N'UTC-8')
GO
SET IDENTITY_INSERT [aut].[TimeZone] OFF
GO
SET IDENTITY_INSERT [aut].[UserRole] ON 
GO
INSERT [aut].[UserRole] ([RoleID], [RoleName]) VALUES (1, N'Admin')
GO
INSERT [aut].[UserRole] ([RoleID], [RoleName]) VALUES (2, N'Tester')
GO
INSERT [aut].[UserRole] ([RoleID], [RoleName]) VALUES (3, N'Manager')
GO
INSERT [aut].[UserRole] ([RoleID], [RoleName]) VALUES (4, N'Viewer')
GO
SET IDENTITY_INSERT [aut].[UserRole] OFF
GO
INSERT [aut].[UserStatus] ([StatusID], [StatusName]) VALUES (1, N'Active')
GO
INSERT [aut].[UserStatus] ([StatusID], [StatusName]) VALUES (2, N'Suspended')
GO
INSERT [aut].[UserStatus] ([StatusID], [StatusName]) VALUES (3, N'Pending')
GO
