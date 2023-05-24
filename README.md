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

#### Functions of the App
- Login - Redirect to Reddit site for login and permissions
- Fetching Posts - Start's right after succesfull login in backgroud, using Reddit API.
- Post filtering - Based on Subreddit, Domain, 18+
- Post naming - Based on Subreddit, Domain, title, id/numbering
- Post structure - How will the posts be structured
- Post downloading - Images, imagce galleries, videos, text, comments
- History of downloads - App keeps track of what ids the user already downloaded
- Templates - User can save his favorite settings for future use
- Stats - App's and user's stats (total downloads, total downloaded posts, distribution of subreddits)


#### Running the program
In order to run this project you will need Microsoft SQL Server. Use the file called "DDL.sql" to create the databse. You will need to modify the "DefaultConnection" in file called "appsettings.json" to your connection info. Aditionaly you will need to create your own Reddit App (https://www.reddit.com/prefs/apps). With your new Reddit app you will need to replace [ID:SECRET] in "LoggedIn.cshtml.cs" and also [ID] in "_layout.cshtml" and "Index.cshtml". This project is from Visual Studio 2022 17.4.1. In the file called KnotekJ_WebovaAplikace_JM_2023.pdf you can see the Bachelor thesis (in czech) including user's manual starting on page 48.
