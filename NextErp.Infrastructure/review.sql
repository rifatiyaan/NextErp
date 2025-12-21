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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [DateOfBirth] datetime2 NULL,
        [Sex] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [Photo] nvarchar(max) NULL,
        [OrgUnitId] int NOT NULL,
        [AreaId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [ParentId] int NULL,
        [Metadata] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Categories_Categories_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE TABLE [IdentityUser] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(max) NULL,
        [NormalizedUserName] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [NormalizedEmail] nvarchar(max) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_IdentityUser] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE TABLE [Modules] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(100) NOT NULL,
        [Icon] nvarchar(50) NULL,
        [Url] nvarchar(500) NULL,
        [ParentId] int NULL,
        [Type] int NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Version] nvarchar(20) NULL,
        [IsInstalled] bit NOT NULL,
        [IsEnabled] bit NOT NULL,
        [InstalledAt] datetime2 NULL,
        [Order] int NOT NULL,
        [IsActive] bit NOT NULL,
        [IsExternal] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NULL,
        [Metadata] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Modules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Modules_Modules_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Modules] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [ParentId] int NULL,
        [Price] decimal(18,2) NOT NULL,
        [Stock] int NOT NULL,
        [CategoryId] int NOT NULL,
        [ImageUrl] nvarchar(max) NULL,
        [Metadata] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Products_Products_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] int NOT NULL IDENTITY,
        [Token] nvarchar(max) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ExpiryDate] datetime2 NOT NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_IdentityUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [IdentityUser] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BranchId', N'CreatedAt', N'Description', N'IsActive', N'Metadata', N'ParentId', N'TenantId', N'Title', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Categories]'))
        SET IDENTITY_INSERT [Categories] ON;
    EXEC(N'INSERT INTO [Categories] ([Id], [BranchId], [CreatedAt], [Description], [IsActive], [Metadata], [ParentId], [TenantId], [Title], [UpdatedAt])
    VALUES (1, NULL, ''2025-12-16T12:58:21.0018516Z'', N''All electronic devices'', CAST(1 AS bit), N''{"ProductCount":"50","Department":"Tech"}'', NULL, ''00000000-0000-0000-0000-000000000000'', N''Electronics'', NULL),
    (2, NULL, ''2025-12-16T12:58:21.0018519Z'', N''Fashion and apparel'', CAST(1 AS bit), N''{"ProductCount":"120","Department":"Fashion"}'', NULL, ''00000000-0000-0000-0000-000000000000'', N''Clothing'', NULL),
    (3, NULL, ''2025-12-16T12:58:21.0018521Z'', N''All kinds of books'', CAST(1 AS bit), N''{"ProductCount":"200","Department":"Literature"}'', NULL, ''00000000-0000-0000-0000-000000000000'', N''Books'', NULL),
    (4, NULL, ''2025-12-16T12:58:21.0018523Z'', N''Appliances for home'', CAST(1 AS bit), N''{"ProductCount":"80","Department":"Home"}'', NULL, ''00000000-0000-0000-0000-000000000000'', N''Home Appliances'', NULL),
    (5, NULL, ''2025-12-16T12:58:21.0018555Z'', N''Sports equipment'', CAST(1 AS bit), N''{"ProductCount":"60","Department":"Sports"}'', NULL, ''00000000-0000-0000-0000-000000000000'', N''Sports'', NULL),
    (6, NULL, ''2025-12-16T12:58:21.0018557Z'', N''Toys for children'', CAST(1 AS bit), N''{"ProductCount":"100","Department":"Kids"}'', NULL, ''00000000-0000-0000-0000-000000000000'', N''Toys'', NULL),
    (7, NULL, ''2025-12-16T12:58:21.0018559Z'', N''Beauty and personal care'', CAST(1 AS bit), N''{"ProductCount":"70","Department":"Cosmetics"}'', NULL, ''00000000-0000-0000-0000-000000000000'', N''Beauty'', NULL),
    (8, NULL, ''2025-12-16T12:58:21.0018561Z'', N''Car and bike accessories'', CAST(1 AS bit), N''{"ProductCount":"90","Department":"Automotive"}'', NULL, ''00000000-0000-0000-0000-000000000000'', N''Automotive'', NULL),
    (9, NULL, ''2025-12-16T12:58:21.0018562Z'', N''Daily groceries'', CAST(1 AS bit), N''{"ProductCount":"150","Department":"Food"}'', NULL, ''00000000-0000-0000-0000-000000000000'', N''Grocery'', NULL),
    (10, NULL, ''2025-12-16T12:58:21.0018564Z'', N''Home and office furniture'', CAST(1 AS bit), N''{"ProductCount":"40","Department":"Home"}'', NULL, ''00000000-0000-0000-0000-000000000000'', N''Furniture'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BranchId', N'CreatedAt', N'Description', N'IsActive', N'Metadata', N'ParentId', N'TenantId', N'Title', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Categories]'))
        SET IDENTITY_INSERT [Categories] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BranchId', N'CategoryId', N'Code', N'CreatedAt', N'ImageUrl', N'IsActive', N'Metadata', N'ParentId', N'Price', N'Stock', N'TenantId', N'Title', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Products]'))
        SET IDENTITY_INSERT [Products] ON;
    EXEC(N'INSERT INTO [Products] ([Id], [BranchId], [CategoryId], [Code], [CreatedAt], [ImageUrl], [IsActive], [Metadata], [ParentId], [Price], [Stock], [TenantId], [Title], [UpdatedAt])
    VALUES (1, NULL, 1, N''IP15-001'', ''2025-12-16T12:58:21.0018720Z'', N''https://example.com/iphone15.jpg'', CAST(0 AS bit), N''{"Description":"Latest iPhone","Color":"Black","Warranty":"2 Years"}'', NULL, 1299.99, 10, ''00000000-0000-0000-0000-000000000000'', N''iPhone 15'', NULL),
    (2, NULL, 1, N''TV-002'', ''2025-12-16T12:58:21.0018725Z'', N''https://example.com/samsungtv.jpg'', CAST(0 AS bit), N''{"Description":"Smart LED TV","Color":"Silver","Warranty":"3 Years"}'', NULL, 899.5, 15, ''00000000-0000-0000-0000-000000000000'', N''Samsung TV'', NULL),
    (3, NULL, 2, N''MJKT-003'', ''2025-12-16T12:58:21.0018727Z'', N''https://example.com/jacket.jpg'', CAST(0 AS bit), N''{"Description":"Warm winter jacket","Color":"Brown","Warranty":"1 Year"}'', NULL, 199.5, 25, ''00000000-0000-0000-0000-000000000000'', N''Men''''s Jacket'', NULL),
    (4, NULL, 3, N''BK-004'', ''2025-12-16T12:58:21.0018729Z'', N''https://example.com/alchemist.jpg'', CAST(0 AS bit), N''{"Description":"Fiction book","Color":null,"Warranty":null}'', NULL, 12.99, 100, ''00000000-0000-0000-0000-000000000000'', N''Novel: The Alchemist'', NULL),
    (5, NULL, 4, N''MO-005'', ''2025-12-16T12:58:21.0018731Z'', N''https://example.com/microwave.jpg'', CAST(0 AS bit), N''{"Description":"800W Microwave","Color":"White","Warranty":"2 Years"}'', NULL, 299.0, 20, ''00000000-0000-0000-0000-000000000000'', N''Microwave Oven'', NULL),
    (6, NULL, 5, N''SP-006'', ''2025-12-16T12:58:21.0018733Z'', N''https://example.com/football.jpg'', CAST(0 AS bit), N''{"Description":"Official size 5 football","Color":"White/Black","Warranty":null}'', NULL, 29.99, 50, ''00000000-0000-0000-0000-000000000000'', N''Football'', NULL),
    (7, NULL, 6, N''TOY-007'', ''2025-12-16T12:58:21.0018736Z'', N''https://example.com/lego.jpg'', CAST(0 AS bit), N''{"Description":"Creative Lego set","Color":"Multi-color","Warranty":null}'', NULL, 59.99, 30, ''00000000-0000-0000-0000-000000000000'', N''Lego Set'', NULL),
    (8, NULL, 7, N''BTY-008'', ''2025-12-16T12:58:21.0018738Z'', N''https://example.com/lipstick.jpg'', CAST(0 AS bit), N''{"Description":"Matte lipstick","Color":"Red","Warranty":null}'', NULL, 49.99, 40, ''00000000-0000-0000-0000-000000000000'', N''Lipstick Set'', NULL),
    (9, NULL, 8, N''AUTO-009'', ''2025-12-16T12:58:21.0018740Z'', N''https://example.com/carvac.jpg'', CAST(0 AS bit), N''{"Description":"Portable vacuum for car","Color":"Black","Warranty":"6 Months"}'', NULL, 79.99, 18, ''00000000-0000-0000-0000-000000000000'', N''Car Vacuum Cleaner'', NULL),
    (10, NULL, 10, N''FUR-010'', ''2025-12-16T12:58:21.0018742Z'', N''https://example.com/chair.jpg'', CAST(0 AS bit), N''{"Description":"Ergonomic chair","Color":"Black","Warranty":"1 Year"}'', NULL, 149.99, 12, ''00000000-0000-0000-0000-000000000000'', N''Office Chair'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BranchId', N'CategoryId', N'Code', N'CreatedAt', N'ImageUrl', N'IsActive', N'Metadata', N'ParentId', N'Price', N'Stock', N'TenantId', N'Title', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Products]'))
        SET IDENTITY_INSERT [Products] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE INDEX [IX_Categories_ParentId] ON [Categories] ([ParentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE INDEX [IX_Modules_ParentId] ON [Modules] ([ParentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE INDEX [IX_Modules_TenantId_IsActive] ON [Modules] ([TenantId], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE INDEX [IX_Modules_Type] ON [Modules] ([Type]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE INDEX [IX_Products_ParentId] ON [Products] ([ParentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216125821_InitialPostgresSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251216125821_InitialPostgresSchema', N'8.0.1');
END;
GO

COMMIT;
GO

