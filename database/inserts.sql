-- ============================================================
-- Script: Criação das tabelas e dados iniciais
-- Projeto: TL.Exemplo-CQRS
-- ============================================================

-- 1. Criar tabela Users
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Username NVARCHAR(50) NOT NULL,
        Email NVARCHAR(200) NOT NULL,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'User',
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    PRINT 'Tabela Users criada.';
END
GO

-- 2. Criar tabela Products
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(150) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        StockQuantity INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    PRINT 'Tabela Products criada.';
END
GO

-- 3. Criar índices
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Name')
    CREATE INDEX IX_Products_Name ON Products (Name);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_IsDeleted')
    CREATE INDEX IX_Products_IsDeleted ON Products (IsDeleted);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
    CREATE UNIQUE INDEX IX_Users_Email ON Users (Email);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username')
    CREATE UNIQUE INDEX IX_Users_Username ON Users (Username);
GO

-- ============================================================
-- INSERTS
-- ============================================================

-- 4. Inserir usuários de teste
-- Senha: Admin@123 (hash BCrypt)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Id, Username, Email, PasswordHash, Role, IsDeleted, CreatedAt)
    VALUES (NEWID(), 'admin', 'admin@tl.com', '$2a$11$K8GpFYGjCPzQWkMCV.qZGOZY1ZFiGXqIECFvS5qI7cG3vPlL5lO9q', 'Admin', 0, GETUTCDATE());
END
GO

-- Senha: User@123 (hash BCrypt)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'user')
BEGIN
    INSERT INTO Users (Id, Username, Email, PasswordHash, Role, IsDeleted, CreatedAt)
    VALUES (NEWID(), 'user', 'user@tl.com', '$2a$11$K8HEpFYGjCPzQWkMCV.qZGOZY1ZFiGXqIECFvS5qI7cG3vPlL5lO9u', 'User', 0, GETUTCDATE());
END
GO

-- 5. Inserir produtos de exemplo
IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN
    INSERT INTO Products (Id, Name, Description, Price, StockQuantity, IsActive, IsDeleted, CreatedAt)
    VALUES
        (NEWID(), 'Notebook Dell XPS 15', 'Notebook premium com tela OLED 15.6", Intel Core i9, 32GB RAM, 1TB SSD.', 12999.99, 10, 1, 0, GETUTCDATE()),
        (NEWID(), 'Mouse Logitech MX Master 3', 'Mouse sem fio ergonômico com scroll MagSpeed e conexão Bluetooth.', 549.90, 50, 1, 0, GETUTCDATE()),
        (NEWID(), 'Teclado Mecânico Keychron K2', 'Teclado mecânico compacto 75%, switches Red, compatível com Windows e Mac.', 699.00, 30, 1, 0, GETUTCDATE()),
        (NEWID(), 'Monitor LG UltraWide 34"', 'Monitor ultrawide 34 polegadas, resolução 3440x1440, 144Hz, IPS.', 3299.00, 8, 1, 0, GETUTCDATE()),
        (NEWID(), 'Headset Sony WH-1000XM5', 'Fone over-ear com cancelamento de ruído líder do setor, 30h de bateria.', 1899.90, 20, 0, 0, GETUTCDATE());
    PRINT 'Dados inseridos com sucesso.';
END
GO

-- 6. Verificar dados
SELECT 'Users' AS Tabela, COUNT(*) AS Total FROM Users
UNION ALL
SELECT 'Products' AS Tabela, COUNT(*) AS Total FROM Products;
GO