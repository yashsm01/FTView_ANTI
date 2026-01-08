-- Create TagMasters table in ALARM HISTORY database
USE [ALARM HISTORY];
GO

CREATE TABLE dbo.TagMasters
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TagName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    HighLimit FLOAT NOT NULL DEFAULT 0,
    LowLimit FLOAT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Create index on TagName for faster lookups
CREATE INDEX IX_TagMasters_TagName ON dbo.TagMasters(TagName);
GO
