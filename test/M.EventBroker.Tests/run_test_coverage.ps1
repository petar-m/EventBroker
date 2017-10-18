$dotnet='"C:/Program Files/dotnet/dotnet.exe"'  
$userFolder=[System.Environment]::GetFolderPath('UserProfile')
$opencover= Join-Path -Path $userFolder -ChildPath  ".nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe"  
$reportgenerator= Join-Path -Path $userFolder -ChildPath ".nuget\packages\ReportGenerator\3.0.2\tools\ReportGenerator.exe"

$targetargs='"test -f netcoreapp2.0"'
$filter='"+[*]M.EventBroker.* -[*.Tests]* -[xunit.*]* -[FakeItEasy*]*"'
$coveragefile="test_coverage\coverage.xml"
$coveragedir="test_coverage\coverage"

# Run code coverage analysis  
Start-Process $opencover "-oldStyle -register:user -target:$($dotnet) -output:$($coveragefile) -targetargs:$($targetargs) -filter:$($filter) -skipautoprops -hideskipped:All" -NoNewWindow -Wait

# Generate the report  
Start-Process $reportgenerator "-targetdir:$($coveragedir) -reporttypes:Html;Badges -reports:$($coveragefile) -verbosity:Error" -NoNewWindow -Wait

# Open the report  
Start-Process "$($coveragedir)\index.htm"  