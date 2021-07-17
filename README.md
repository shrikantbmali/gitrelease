# Introduction

![image](docs/back.png)\
I am not sure if ```git-release``` is a good name for this project, but that's what I came up with at the time.

## What is it?

`git-release` is a dotnet command used in xamarin applications for versioning, changelog management and auto build-number increment.

## Why?

I wanted a way where in I can track the versions updates and changelogs using [conventional-changelog](https://www.conventionalcommits.org/en/v1.0.0/) but for C# projects.

## How does it work?


### Installing

``` powershell
dotnet tool install -g git-release
```

Once the tool is installed you can set it up for xamarin project like:

``` powershell
gitrelease init
```

This command will create required files for this to work and set the project version to 1.0-beta