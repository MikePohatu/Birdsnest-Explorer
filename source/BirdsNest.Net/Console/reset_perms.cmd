icacls "%~dp0logs" /grant "IIS AppPool\birdsnest-console":(OI)(CI)M
icacls "%~dp0wwwroot\dynamic" /grant "IIS AppPool\birdsnest-console":(OI)(CI)M