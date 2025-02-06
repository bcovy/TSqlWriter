$connString = "Server=localhost, 1434; Database=master; User ID=sa; Password=pass!w0rld1;"
$server = "localhost, 1434"
$ErrorActionPreference = "Stop"

Write-Host "************************************************************************************" -f Magenta
Write-Host "Hello.  This script will create a test database for the SqlWriter project." -f Magenta
Write-Host "************************************************************************************" -f Magenta
# Verify the docker db container is running.
Write-Host "Testing connectivity for docker test server: " -NoNewLine; Write-Host "$server" -f Green

try {
	$connection = New-Object System.Data.OleDb.OleDbConnection "Provider=sqloledb; $connString"
    $connection.Open()
    # This will run if the Open() method does not throw an exception.
    Write-Host "Connection verified."
}
catch {
    Write-Error "Unable to connect to server: $server."
}
finally {
    $connection.Close()
}

try {
    Write-Host "Creating SqlMakerDb database."
    Invoke-Sqlcmd -InputFile "./BuildDatabaseObjects.sql" -ConnectionString $connString

    Write-Host "Creating SqlMakerDb database data."
    Invoke-Sqlcmd -InputFile "./CreateDatabaseData.sql" -ConnectionString $connString

    Write-Host "Finished creating test database."
    Write-Host "Test database creation script is complete.  Database connection string is:"
    Write-Host "- Server=localhost, 1434; Database=SqlMakerDb; User ID=sa; Password=pass!w0rld1;" -f Green
    Write-Host "The test database name is: "  -NoNewLine; Write-Host "SqlMakerDb" -f Green
}
catch {
    Write-Error $Error[0]
}
finally {
    [System.Data.SqlClient.SqlConnection]::ClearAllPools()
}