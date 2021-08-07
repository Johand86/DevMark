# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2019 AS installer

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';$ProgressPreference='silentlyContinue';"]

RUN Invoke-WebRequest -OutFile nodejs.zip "https://nodejs.org/dist/v14.17.2/node-v14.17.2-win-x64.zip"; Expand-Archive nodejs.zip -DestinationPath C:\; Rename-Item 'C:\node-v14.17.2-win-x64' c:\nodejs

RUN Invoke-WebRequest -OutFile vs_buildtools.exe https://aka.ms/vs/16/release/vs_buildtools.exe

RUN Invoke-WebRequest 'https://github.com/git-for-windows/git/releases/download/v2.32.0.windows.1/MinGit-2.32.0-64-bit.zip' -OutFile MinGit.zip; Expand-Archive c:\MinGit.zip -DestinationPath c:\git

RUN Invoke-WebRequest 'https://www.python.org/ftp/python/3.9.6/python-3.9.6-amd64.exe' -OutFile PythonInstaller.exe

RUN Invoke-WebRequest -OutFile handle.zip https://download.sysinternals.com/files/Handle.zip; Expand-Archive Handle.zip -DestinationPath c:\handle

FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2019 AS dependencies

SHELL ["cmd", "/S", "/C"]

WORKDIR C:\\

COPY --from=installer vs_buildtools.exe C:

RUN start /w vs_buildtools.exe --quiet --wait --norestart --nocache modify --installPath "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools" \
        --add Microsoft.VisualStudio.Workload.AzureBuildTools \
        --add Microsoft.VisualStudio.Workload.DataBuildTools \
        --add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools \
        --add Microsoft.VisualStudio.Workload.MSBuildTools \
        --add Microsoft.VisualStudio.Workload.VCTools \
        --add Microsoft.VisualStudio.Workload.WebBuildTools \
        --add Microsoft.VisualStudio.Workload.NetCoreBuildTools \
        --add Microsoft.VisualStudio.Component.VC.Tools.x86.x64 \
        --add Microsoft.VisualStudio.Component.Windows10SDK \
        --add Microsoft.VisualStudio.Component.Windows10SDK.17763 \
        || IF "%ERRORLEVEL%"=="3010" EXIT 0

RUN del vs_buildtools.exe

COPY --from=installer C:\\nodejs\\ C:\\nodejs\\
RUN C:\nodejs\npm config set registry https://registry.npmjs.org/

COPY --from=installer C:\\git\\ C:\\git\\

COPY --from=installer C:\\PythonInstaller.exe C:\\ 
RUN PythonInstaller.exe /quiet InstallAllUsers=1 PrependPath=1 && \
    del PythonInstaller.exe

USER ContainerAdministrator
RUN SETX /M PATH "%PATH%;C:\nodejs;C:\git\cmd"

RUN npm install -g yarn

FROM dependencies AS config
        
SHELL ["cmd", "/S", "/C"]

RUN dotnet tool install --tool-path C:\\DevMark\\ --version 1.1.1 DevMark

FROM config AS final

WORKDIR C:\\DevMark\\

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';$ProgressPreference='silentlyContinue';"]

VOLUME C:\\dmshare
RUN Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\DOS Devices' -Name 'X:' -Value "\??\C:\dmshare" -Type String;

ENTRYPOINT ["devmark"]
CMD [""]