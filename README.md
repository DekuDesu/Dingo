# Dingo Chat
A ASP.NET Core Blazor-Server single-page encrypted messaging website. 

## How to build
Docker:
Pull repo, and run the following command in powershell.
```docker
    docker-compose up -d --build
```
This will build/start both the app and the required MSSQL Databases automatically.

## Production Use of Application
This project has not been setup for Continuous Integration or a verbose build process that would allow easy production deployment without modification. Please don't pull this repo and throw it up as a production website you will receive development errors, and those aren't fun for either the user or you.

## Development Logging
This app uses Serilog for logging, this is not included in the docker file. You will need to create your own Serilog docker container for logging for this app. This is not required as most errors/messages are also logged to the linux container's output.

## Rights for use
This is a fun little project I wanted to do to practice ASP.NET/Blazor. Obvious Attribution required. No other restrictions and/or limitations.