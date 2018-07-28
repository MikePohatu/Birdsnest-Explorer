SET PathToSetupScript=C:\Source\Repos\BirdsNest\source\neo4j\DB_Setup.cql
SET PathToNeo4j=C:\neo4j-community-3.4.5
SET PathToBirdsNestDb=%PathToNeo4j%\data\databases\graph.db

"%PathToNeo4j%\bin\neo4j-shell.bat" -file "%PathToSetupScript%" -path "%PathToBirdsNestDb%"