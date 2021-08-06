Dont mind this file. I'ts just a list of commands I sometime use :).

# Publishing
```
dotnet publish Source\DevMark\DevMark.csproj -o C:\temp\DevMark -c Release
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


# Docker

## Build Windows
```
docker build -t "devmarkwin:latest" -m=2GB . -f DevMarkWin.Dockerfile
```

## Build Linux
```
docker build -t "devmarklinux:latest" -m=2GB . -f DevMarkLinux.Dockerfile
```

## Run Linux
```
docker run --cpus=24 --memory=30GB -v X:\docker\linux:/dmshare "devmarklinux:latest" run Echo
docker run --cpus=24 --memory=30GB -v X:\docker\linux:/dmshare "devmarklinuxdevelopment:latest" run Echo
```

## Run Windows
```
docker run --cpus=24 --memory=30GB -v X:\docker\win:C:\dmshare "devmarkwin:latest" run Echo
docker run --cpus=24 --memory=30GB -v X:\docker\win:C:\dmshare "devmarkwindevelopment:latest" run Echo
```

## Build and run through DM - Linux
```
DevMark run Echo --docker --dockerfile DevMarkLinuxDevelopment.Dockerfile --docker-dev-container --verbose
```

## Build and run through DM - Windows
```
DevMark run Echo --docker --dockerfile DevMarkWinDevelopment.Dockerfile --docker-dev-container --verbose
```

## Build and run DevContainer - Windows
DevMark run EfCore --docker --dockerfile DevMarkWinDevelopment.Dockerfile --docker-dev-container --docker-dev-source-path "C:\Repos\DevMark" -w X:\DM2

## Execute CMD, Windows
```
docker run --cpus=24 --memory=30GB -v X:\docker:C:\work "devmarkwin:latest" check --all --docker-wait
docker exec -it NAME powershell
```

## Execute CMD, Linux
```
docker run -dit "devmarklinux:latest"
docker exec -it NAME pwsh
```

# Mount (not currently used)
```
docker run --cpus=24 --memory=32GB --mount source=devmarkwin,target=c:\DMVol "devmarkwin:latest" 
```

# Upload to dev env
```
devmark upload --api-key devsecret --url https://localhost:44324 --result x.json
```