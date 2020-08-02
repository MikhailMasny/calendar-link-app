# Calendar Link

![.NET Core](https://github.com/MikhailMasny/calendar-link-app/workflows/.NET%20Core/badge.svg)

A web-application developed on the .NET 3.1 (LTS). The main idea of a web application is to develop a quickly generate a google calendar link. This repository can also serve as a template for creating the application with the account and some basic functionality.

## Getting Started

The developed web application is WebAPI. When you start, you need to follow the link: `/swagger` in order to start using it. By clicking on the link: `/api/Calendar`, you can enter the necessary data and get the generated link for google calendar.

## Application settings

For the correct functioning of web application, it is necessary to update the [appsettnigs.json](https://github.com/MikhailMasny/calendar-link-app/blob/master/src/Masny.WebApi/appsettings.json) file in the root directory of the web project, filled in according to the template below.

```
{
  "AppSettings": {
    "Secret": "your-secret",
    "EmailFrom": "",
    "SmtpHost": "your.smtp.host",
    "SmtpPort": 123,
    "SmtpUser": "your@e.mail",
    "SmtpPass": "your-pass"
  },
  "ConnectionStrings": {
    "WebApiDatabase": "your-database"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

## Built with
- [ASP.NET Core 3.1](https://docs.microsoft.com/en-us/aspnet/core/);
- [Monolithic architecture](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures);
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/);

## Author
[Mikhail M.](https://mikhailmasny.github.io/) - Software Engineer;

## License
This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/MikhailMasny/calendar-link-app/blob/master/LICENSE) file for details.
