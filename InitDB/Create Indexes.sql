if not exists(select * from sys.indexes where name = 'IDX_Curve1')
CREATE NONCLUSTERED INDEX IDX_Curve1 ON dbo.Curve
(
	CurveId ASC,
	CurveName ASC,
	PassNumber ASC,
	CreatedDateTime ASC
) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY  = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
if not exists(select * from sys.indexes where name = 'IDX_FilesForRoles1')
CREATE NONCLUSTERED INDEX IDX_FilesForRoles1 ON dbo.FilesForRoles
(
	FileID ASC,
	RoleID ASC
) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY  = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
if not exists(select * from sys.indexes where name = 'IDX_GPSSamples_All')
CREATE NONCLUSTERED INDEX IDX_GPSSamples_All ON dbo.GPSSamples
(
	FileID ASC,
	SampleID ASC,
	SampleTimestamp ASC,
	Latitude ASC,
	Longitude ASC,
	Velocity ASC,
	Heading ASC
) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY  = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
if not exists(select * from sys.indexes where name = 'IDX_MileMarkers_LatLong')
CREATE NONCLUSTERED INDEX IDX_MileMarkers_LatLong ON dbo.MileMarkers
(
	MileMarkerID ASC,
	RoadName ASC,
	Mile ASC,
	Latitude ASC,
	Longitude ASC
) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY  = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
if not exists(select * from sys.indexes where name = 'IDX_CachedDataSessionTiles')
CREATE NONCLUSTERED INDEX IDX_CachedDataSessionTiles ON dbo.CachedDataSessionTiles
(
	FileID ASC,
	MinLatitude ASC,
	MinLongitude ASC,
	MaxLatitude ASC,
	MaxLongitude ASC,
	Zoom ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

if not exists(select * from sys.indexes where name = 'IDX_Signs')
CREATE NONCLUSTERED INDEX IDX_Signs ON dbo.Signs
(
	SignID ASC,
	CreatedOn ASC,
	InstalledOn ASC,
	RemovedOn ASC,
	DeletedOn ASC,
	Latitude ASC,
	Longitude ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO