$connString = "Data Source=VPNSERVER1\SQLEXPRESS;Initial Catalog=Training_DB_Harsh_Patil;User ID=Training_DB_Harsh_Patil;Password=Training_DB_Harsh_Patil"

function Get-SPDefinition($spName) {
    $query = "SELECT OBJECT_DEFINITION(OBJECT_ID('$spName')) AS Definition"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connString)
    $command = New-Object System.Data.SqlClient.SqlCommand($query, $connection)
    try {
        $connection.Open()
        $reader = $command.ExecuteReader()
        if ($reader.Read()) {
            $def = $reader["Definition"]
            if ($def -ne [DBNull]::Value -and $def -ne $null) {
                Write-Output "--- Definition for $spName ---"
                Write-Output $def
            } else {
                Write-Output "Procedure $spName not found or has no definition."
            }
        }
    } catch {
        Write-Error $_.Exception.Message
    } finally {
        $connection.Close()
    }
}

Get-SPDefinition "EnrollmentDetails"
Get-SPDefinition "GetDistinctStatus"
Get-SPDefinition "sp_GetStudents"
Get-SPDefinition "sp_GetCurrentYearCourseOfferings"
Get-SPDefinition "sp_GetEnrollmentById"
Get-SPDefinition "sp_SaveEnrollment"
