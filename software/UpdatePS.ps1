dotnet publish mClara.sln -c Release
Remove-Item -Recurse .\ClaraMgmt\bin
Copy-Item -Recurse .\src\ClaraMgmt\bin\release\net5.0\publish .\ClaraMgmt\bin
