Dont mind this file. I'ts just a list of commands I sometime use :).

# Publishing
```
dotnet publish Source\DevMark\DevMark.csproj -o C:\temp\DevMark16 -c Release
```

# Update devsecret
```
dotnet user-secrets set "ServiceApiKey" "secret" --project Source\DevMark\DevMark.csproj
```

# Package CLI tool
```
dotnet pack Source\DevMark\DevMark.csproj -c Release
```

# Install/uninstall pacakge from local
```
dotnet tool install --global --add-source .\Source\DevMark\nupkg DevMark
dotnet tool uninstall --global DevMark
```