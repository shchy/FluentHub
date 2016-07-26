nuget pack .\FluentHub\FluentHub.csproj -Prop Configuration=Release
nuget pack .\FluentHub.TCP\FluentHub.TCP.csproj -Prop Configuration=Release
nuget pack .\FluentHub.UDP\FluentHub.UDP.csproj -Prop Configuration=Release
nuget pack .\FluentHub.Serial\FluentHub.Serial.csproj -Prop Configuration=Release

nuget psetApiKey %1 -Source https://www.nuget.org/api/v2/package

nuget push FluentHub.1.0.0.nupkg -Source https://www.nuget.org/api/v2/package
nuget push FluentHub.TCP.1.0.0.nupkg -Source https://www.nuget.org/api/v2/package
nuget push FluentHub.UDP.1.0.0.nupkg -Source https://www.nuget.org/api/v2/package
nuget push FluentHub.Serial.1.0.0.nupkg -Source https://www.nuget.org/api/v2/package
