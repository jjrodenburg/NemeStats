if($PSVersionTable.PSVersion.Major -lt 4){
    Write-Error "PowerShell version 4 or higher is required to run this script. It's easy to Google and download -- go do that first."
    return
}

if(Test-Path ./Source/UI/PrivateAppSettings.config)
{
    Write-Output "It looks like PrivateAppSettings.config is already in place. Moving on."
}else
{
    Write-Output "Copying ./Miscellaneous/PrivateAppSettings.config to ./Source/UI/PrivateAppSettings.config"
    Copy-Item ./Miscellaneous/PrivateAppSettings.config ./Source/UI/PrivateAppSettings.config
}

$connectionString = "Data Source=.\SQLEXPRESS;Initial Catalog=NerdScorekeeper;Integrated Security=True"
$answer = Read-Host -Prompt "The default connection string will be `"Data Source=.\SQLEXPRESS;Initial Catalog=NerdScorekeeper;Integrated Security=True`". Are you OK with this? (You can change it later in Source/UI/PrivateAppSettings.config). Y/N?"
if($answer -ne "Y")
{
    $connectionString = Read-Host -Prompt "What connection string would you like to use instead?"
    (Get-Content ./Source/UI/PrivateAppSettings.config -Raw) `
    -replace '(.*?Data Source=).*?[\n](.*?)', "`$1$connectionString`n`$2" |
Out-File ./Source/UI/PrivateAppSettings.config
    Write-Output "Connection string updated."
}

Write-Output "Now go to Package Manager Console in Visual Studio and run `"update-database -ProjectName BusinessLogic`" to run Entity Framework migrations. This should create your database. `
    If you have trouble running the migrations then please check your connection string in /Source/UI/PrivateAppSettings.config and make sure your account has dbo access to the database. `
    You may also want to try creating the `"NerdScorekeeper`" database manually and then try running the migrations again."

