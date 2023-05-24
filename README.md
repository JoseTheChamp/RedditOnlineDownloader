## Reddit Online Downloader

This repository only contains project from Visual Studio 2022. In the File called KnotekJ_WebovaAplikace_JM_2023.pdf you can see the Bachelor thesis (in czech). This repository does not include

#### Technologies used:
- ASP.NET Core
- Razor Pages
- C#
- Microsoft SQL Server
- Microsoft Entity Framework
- Reddit API
- HTML, CSS, JS, AJAX

#### Bachelor thesis annotation: 
The goal of this work is creation of web application, which will allow users to download posts in bulk from social site called Reddit. Users will be able to specify preferences of the download (location, folder structure, naming of files). Application will use a database for saving user preferences. Application will fetch the posts from Reddit with Reddit API. The web page will be created with the help of a library called ASP.NET and language C#.

#### Running the program
In order to run this project you will need Microsoft SQL Server. Use the file called "DDL.sql" to create the databse. You will need to modify the "DefaultConnection" in file called "appsettings.json" to your connection info. Aditionaly you will need to create your own Reddit App (https://www.reddit.com/prefs/apps). With you new Reddit app you will need to replace the id and secret in "LoggedIn.cshtml.cs" and also id in "_layout.cs" and "Index.cshtml". This project is from Visual Studio 2022 17.4.1. In the File called KnotekJ_WebovaAplikace_JM_2023.pdf you can see the Bachelor thesis (in czech) including user's manual starting on page 48.
