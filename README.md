# Stamp
command line interface  
[![Build status](https://ci.appveyor.com/api/projects/status/0p5rfgk5b2kyban2?svg=true)](https://ci.appveyor.com/project/graphuk/graph-stamp) [![NuGet](https://img.shields.io/nuget/v/stamp.svg)](https://www.nuget.org/packages/Stamp)  
Stamp searches repositories on github marked with **stamp-component** topic and fetches it to current folder

## Installation
Stamp is deployed as dotnet global tool:
`dotnet tool install --global Stamp`

## Component repository requirements
**src** folder containing component content
**README.md** file with component description/installation steps
**manifest.json** file with component manifest
~~~~
{
    "name": "NavigationBlock",
    "destination": "App_Plugins",
    "addToCsproj": true
}
~~~~
![bbg](https://i.imgur.com/4fhgvsg.png)
### Available commands
`stamp --help` - prints available commands
`stamp component list`
~~~
Usage: stamp component list [options]
Options:
  -h | --help
  Show help information
  -q | --query       <TEXT>
  Query the component name
  -o | --owner       <TEXT>
  Query by the owning user
  -c | --category    <TEXT>
  Query by the component topic
~~~
`stamp component fetch`
Fetches component to the current folder, and modify *.csproj file if required
~~~
Usage: stamp component fetch [arguments] [options]
Arguments:
  Owner    <TEXT>
  name     <TEXT>

Options:
  -h | --help
  Show help information
  -r | --ref     <TEXT>
  Component alteration (branch, tag or commit hash)
~~~
