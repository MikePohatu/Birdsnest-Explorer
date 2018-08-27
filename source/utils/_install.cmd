SET FILECALLPATH=%~dp0

if not exist c:\birdsnest mkdir c:\birdsnest

REM Install .net core runtime
"%FILECALLPATH%dotnet-hosting-2.1.2-win.exe" /quiet /install

REM Install java
"%FILECALLPATH%jre-8u181-windows-x64.exe" AUTO_UPDATE=Disable WEB_ANALYTICS=Disable INSTALL_SILENT=Enable

REM Copy and setup neo4j
SET PathToSetupScript=%FILECALLPATH%DB_Setup.cql
SET PathToNeo4j=C:\birdsnest\neo4j
SET PathToBirdsNestDb=%PathToNeo4j%\data\databases\graph.db

xcopy /i /e /h /y "%FILECALLPATH%neo4j-community-3.4.5" "%PathToNeo4j%"
call "%PathToNeo4j%\bin\neo4j.bat" install-service
net start neo4j
net stop neo4j
call "%PathToNeo4j%\bin\neo4j-shell.bat" -file "%PathToSetupScript%" -path "%PathToBirdsNestDb%"
net start neo4j 

REM Copy and setup the scanners
xcopy /i /e /h /y "%FILECALLPATH%Scanners" c:\birdsnest\Scanners

REM Copy and setup the console
xcopy /i /e /h /y "%FILECALLPATH%Console" c:\birdsnest\Console
REM powershell -executionpolicy bypass Install-WindowsFeature Web-Server -IncludeManagementTools –IncludeAllSubFeature
REM Appcmd add site /name:neoproxy /id:10 /physcialPath: c:\birdsnest\neoproxy /bindings:http:/*80
