icacls "%~dp0logs" /grant "IIS AppPool\birdsnest-console-pool":(OI)(CI)M
icacls "%~dp0wwwroot\dynamic" /grant "IIS AppPool\birdsnest-console-pool":(OI)(CI)M