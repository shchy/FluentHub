nuget setApiKey %1 -Source https://www.nuget.org/api/v2/package

nuget push FluentHub.%2.nupkg -Source https://www.nuget.org/api/v2/package
nuget push FluentHub.TCP.%2.nupkg -Source https://www.nuget.org/api/v2/package
nuget push FluentHub.UDP.%2.nupkg -Source https://www.nuget.org/api/v2/package
nuget push FluentHub.Serial.%2.nupkg -Source https://www.nuget.org/api/v2/package
nuget push FluentHub.Unity.%2.nupkg -Source https://www.nuget.org/api/v2/package
