IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918064521_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260918064521_InitialCreate', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918065023_CreateMtsCoreTables'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] int NOT NULL IDENTITY,
        [FullName] nvarchar(150) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Role] nvarchar(30) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918065023_CreateMtsCoreTables'
)
BEGIN
    CREATE TABLE [Clients] (
        [ClientId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [CompanyName] nvarchar(200) NOT NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [City] nvarchar(max) NULL,
        [Country] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Clients] PRIMARY KEY ([ClientId]),
        CONSTRAINT [FK_Clients_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918065023_CreateMtsCoreTables'
)
BEGIN
    CREATE TABLE [Projects] (
        [ProjectId] int NOT NULL IDENTITY,
        [ClientId] int NOT NULL,
        [ProjectName] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Status] nvarchar(50) NOT NULL,
        [ProjectAmount] decimal(18,2) NULL,
        [StartDate] datetime2 NULL,
        [ExpectedEndDate] datetime2 NULL,
        [CompletedDate] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Projects] PRIMARY KEY ([ProjectId]),
        CONSTRAINT [FK_Projects_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918065023_CreateMtsCoreTables'
)
BEGIN
    CREATE TABLE [ProjectMilestones] (
        [ProjectMilestoneId] int NOT NULL IDENTITY,
        [ProjectId] int NOT NULL,
        [MilestoneName] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [ProgressPercentage] int NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [PlannedDate] datetime2 NULL,
        [CompletedDate] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProjectMilestones] PRIMARY KEY ([ProjectMilestoneId]),
        CONSTRAINT [FK_ProjectMilestones_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([ProjectId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918065023_CreateMtsCoreTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clients_UserId] ON [Clients] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918065023_CreateMtsCoreTables'
)
BEGIN
    CREATE INDEX [IX_ProjectMilestones_ProjectId] ON [ProjectMilestones] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918065023_CreateMtsCoreTables'
)
BEGIN
    CREATE INDEX [IX_Projects_ClientId] ON [Projects] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918065023_CreateMtsCoreTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918065023_CreateMtsCoreTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260918065023_CreateMtsCoreTables', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919172649_CreateQuotesTable'
)
BEGIN
    CREATE TABLE [Quotes] (
        [QuoteId] int NOT NULL IDENTITY,
        [ClientId] int NOT NULL,
        [ProjectId] int NOT NULL,
        [QuoteNumber] nvarchar(50) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [ValidUntil] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Quotes] PRIMARY KEY ([QuoteId]),
        CONSTRAINT [FK_Quotes_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Quotes_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([ProjectId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919172649_CreateQuotesTable'
)
BEGIN
    CREATE INDEX [IX_Quotes_ClientId] ON [Quotes] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919172649_CreateQuotesTable'
)
BEGIN
    CREATE INDEX [IX_Quotes_ProjectId] ON [Quotes] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919172649_CreateQuotesTable'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Quotes_QuoteNumber] ON [Quotes] ([QuoteNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919172649_CreateQuotesTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260919172649_CreateQuotesTable', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919185832_CreateMessagesTable'
)
BEGIN
    CREATE TABLE [Messages] (
        [MessageId] int NOT NULL IDENTITY,
        [SenderUserId] int NOT NULL,
        [ReceiverUserId] int NOT NULL,
        [ProjectId] int NULL,
        [Subject] nvarchar(200) NULL,
        [MessageText] nvarchar(max) NOT NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ReadAt] datetime2 NULL,
        CONSTRAINT [PK_Messages] PRIMARY KEY ([MessageId]),
        CONSTRAINT [FK_Messages_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([ProjectId]) ON DELETE SET NULL,
        CONSTRAINT [FK_Messages_Users_ReceiverUserId] FOREIGN KEY ([ReceiverUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Messages_Users_SenderUserId] FOREIGN KEY ([SenderUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919185832_CreateMessagesTable'
)
BEGIN
    CREATE INDEX [IX_Messages_ProjectId] ON [Messages] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919185832_CreateMessagesTable'
)
BEGIN
    CREATE INDEX [IX_Messages_ReceiverUserId] ON [Messages] ([ReceiverUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919185832_CreateMessagesTable'
)
BEGIN
    CREATE INDEX [IX_Messages_SenderUserId] ON [Messages] ([SenderUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919185832_CreateMessagesTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260919185832_CreateMessagesTable', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919193958_CreateDocumentsTable'
)
BEGIN
    CREATE TABLE [Documents] (
        [DocumentId] int NOT NULL IDENTITY,
        [ClientId] int NOT NULL,
        [ProjectId] int NULL,
        [UploadedByUserId] int NOT NULL,
        [FileName] nvarchar(255) NOT NULL,
        [StoredFileName] nvarchar(255) NOT NULL,
        [FilePath] nvarchar(500) NOT NULL,
        [ContentType] nvarchar(150) NOT NULL,
        [FileSize] bigint NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([DocumentId]),
        CONSTRAINT [FK_Documents_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]),
        CONSTRAINT [FK_Documents_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([ProjectId]) ON DELETE SET NULL,
        CONSTRAINT [FK_Documents_Users_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919193958_CreateDocumentsTable'
)
BEGIN
    CREATE INDEX [IX_Documents_ClientId] ON [Documents] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919193958_CreateDocumentsTable'
)
BEGIN
    CREATE INDEX [IX_Documents_ProjectId] ON [Documents] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919193958_CreateDocumentsTable'
)
BEGIN
    CREATE INDEX [IX_Documents_UploadedByUserId] ON [Documents] ([UploadedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260919193958_CreateDocumentsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260919193958_CreateDocumentsTable', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920062716_AddProjectInquiries'
)
BEGIN
    CREATE TABLE [ProjectInquiries] (
        [ProjectInquiryId] int NOT NULL IDENTITY,
        [ClientId] int NOT NULL,
        [ProjectName] nvarchar(200) NOT NULL,
        [ProjectType] nvarchar(100) NOT NULL,
        [Budget] nvarchar(100) NULL,
        [StartDate] datetime2 NULL,
        [DeliveryDate] datetime2 NULL,
        [Description] nvarchar(max) NOT NULL,
        [Requirements] nvarchar(max) NOT NULL,
        [Technologies] nvarchar(1000) NULL,
        [AdditionalRequirements] nvarchar(3000) NULL,
        [Status] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ProjectInquiries] PRIMARY KEY ([ProjectInquiryId]),
        CONSTRAINT [FK_ProjectInquiries_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920062716_AddProjectInquiries'
)
BEGIN
    CREATE INDEX [IX_ProjectInquiries_ClientId] ON [ProjectInquiries] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920062716_AddProjectInquiries'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920062716_AddProjectInquiries', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923072847_AddPublicProjectInquirySupport'
)
BEGIN
    ALTER TABLE [ProjectInquiries] DROP CONSTRAINT [FK_ProjectInquiries_Clients_ClientId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923072847_AddPublicProjectInquirySupport'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectInquiries]') AND [c].[name] = N'ClientId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [ProjectInquiries] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [ProjectInquiries] ALTER COLUMN [ClientId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923072847_AddPublicProjectInquirySupport'
)
BEGIN
    ALTER TABLE [ProjectInquiries] ADD [CompanyName] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923072847_AddPublicProjectInquirySupport'
)
BEGIN
    ALTER TABLE [ProjectInquiries] ADD [ContactEmail] nvarchar(150) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923072847_AddPublicProjectInquirySupport'
)
BEGIN
    ALTER TABLE [ProjectInquiries] ADD [ContactName] nvarchar(150) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923072847_AddPublicProjectInquirySupport'
)
BEGIN
    ALTER TABLE [ProjectInquiries] ADD [ContactPhone] nvarchar(30) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923072847_AddPublicProjectInquirySupport'
)
BEGIN
    ALTER TABLE [ProjectInquiries] ADD [ServiceRequired] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923072847_AddPublicProjectInquirySupport'
)
BEGIN
    ALTER TABLE [ProjectInquiries] ADD CONSTRAINT [FK_ProjectInquiries_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923072847_AddPublicProjectInquirySupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260923072847_AddPublicProjectInquirySupport', N'10.0.12');
END;

COMMIT;
GO

