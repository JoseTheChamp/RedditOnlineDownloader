USE [master]
GO
/****** Object:  Database [WebTest,Trusted_Connection=True]    Script Date: 01.05.2023 14:36:00 ******/
CREATE DATABASE [WebTest,Trusted_Connection=True]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'WebTest,Trusted_Connection=True', FILENAME = N'C:\Users\Apepk\WebTest,Trusted_Connection=True.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'WebTest,Trusted_Connection=True_log', FILENAME = N'C:\Users\Apepk\WebTest,Trusted_Connection=True_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [WebTest,Trusted_Connection=True].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET ARITHABORT OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET  ENABLE_BROKER 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET  MULTI_USER 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET DB_CHAINING OFF 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET QUERY_STORE = OFF
GO
USE [WebTest,Trusted_Connection=True]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 01.05.2023 14:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[downloadHistories]    Script Date: 01.05.2023 14:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[downloadHistories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](10) NOT NULL,
	[DownloadedPosts] [nvarchar](max) NOT NULL,
	[DownloadTime] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_downloadHistories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Downloads]    Script Date: 01.05.2023 14:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Downloads](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProgressAbs] [int] NOT NULL,
	[ProgressAbsMax] [int] NOT NULL,
	[ProgressRel] [float] NOT NULL,
	[DownloadStart] [datetime2](7) NOT NULL,
	[DownloadFinished] [datetime2](7) NULL,
	[IsFinished] [bit] NOT NULL,
	[UserId] [int] NOT NULL,
	[IsDownloadable] [bit] NOT NULL,
 CONSTRAINT [PK_Downloads] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Statistics]    Script Date: 01.05.2023 14:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Statistics](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](10) NOT NULL,
	[Downloads] [int] NOT NULL,
	[DownloadedPosts] [int] NOT NULL,
	[DomainsJson] [nvarchar](max) NOT NULL,
	[SubredditsJson] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Statistics] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Templates]    Script Date: 01.05.2023 14:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Templates](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](10) NOT NULL,
	[ShowDownloaded] [bit] NOT NULL,
	[GroupBySubreddit] [bit] NOT NULL,
	[Nsfw] [nvarchar](4) NOT NULL,
	[DomainsForm] [nvarchar](max) NOT NULL,
	[Numbering] [nvarchar](18) NOT NULL,
	[SubredditName] [bit] NOT NULL,
	[DomainName] [bit] NOT NULL,
	[PriorityName] [bit] NOT NULL,
	[Title] [int] NOT NULL,
	[SubredditFolder] [bit] NOT NULL,
	[DomainFolder] [bit] NOT NULL,
	[PriorityFolder] [bit] NOT NULL,
	[Empty] [bit] NOT NULL,
	[Split] [bit] NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_Templates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 01.05.2023 14:36:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](max) NOT NULL,
	[AccessToken] [nvarchar](max) NOT NULL,
	[Registered] [datetime2](7) NOT NULL,
	[LastLogin] [datetime2](7) NOT NULL,
	[RedditId] [nvarchar](10) NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Index [IX_Downloads_UserId]    Script Date: 01.05.2023 14:36:00 ******/
CREATE NONCLUSTERED INDEX [IX_Downloads_UserId] ON [dbo].[Downloads]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Downloads] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDownloadable]
GO
ALTER TABLE [dbo].[Downloads]  WITH CHECK ADD  CONSTRAINT [FK_Downloads_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Downloads] CHECK CONSTRAINT [FK_Downloads_Users_UserId]
GO
USE [master]
GO
ALTER DATABASE [WebTest,Trusted_Connection=True] SET  READ_WRITE 
GO
INSERT INTO [dbo].[Statistics] ([UserId], [Downloads], [DownloadedPosts], [DomainsJson], [SubredditsJson])
VALUES ('SYSTEM', 0, 0, '', '')
GO
