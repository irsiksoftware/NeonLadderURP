# PowerShell script to parse Unity test results XML to JSON
# Created by @bruce-banner for detailed test analysis

param(
    [string]$PlayModeXml = "TestOutput\playmode_results.xml",
    [string]$EditModeXml = "TestOutput\editmode_results.xml",
    [string]$OutputJson = "TestOutput\detailed_test_results.json"
)

function Parse-TestXml {
    param(
        [string]$XmlPath,
        [string]$Mode
    )
    
    if (-not (Test-Path $XmlPath)) {
        Write-Warning "XML file not found: $XmlPath"
        return $null
    }
    
    try {
        [xml]$xmlContent = Get-Content $XmlPath
        $testRun = $xmlContent.SelectSingleNode("//test-run")
        
        if ($testRun -eq $null) {
            Write-Warning "No test-run node found in $XmlPath"
            return $null
        }
        
        # Parse main statistics
        $results = @{
            Mode = $Mode
            TotalTests = [int]$testRun.testcasecount
            PassedTests = [int]$testRun.passed
            FailedTests = [int]$testRun.failed
            SkippedTests = [int]$testRun.skipped
            InconclusiveTests = [int]$testRun.inconclusive
            Duration = $testRun.duration
            Result = $testRun.result
            StartTime = $testRun.'start-time'
            EndTime = $testRun.'end-time'
            TestCases = @()
            TestSuites = @{}
        }
        
        # Parse individual test cases
        $testCases = $xmlContent.SelectNodes("//test-case")
        foreach ($testCase in $testCases) {
            $caseResult = @{
                Name = $testCase.name
                FullName = $testCase.fullname
                ClassName = $testCase.classname
                Result = $testCase.result
                Duration = $testCase.duration
                Seed = $testCase.seed
            }
            
            # Add failure info if exists
            $failure = $testCase.SelectSingleNode(".//failure")
            if ($failure) {
                $caseResult.FailureMessage = $failure.message.'#cdata-section'
                $caseResult.StackTrace = $failure.'stack-trace'.'#cdata-section'
            }
            
            # Add output if exists
            $output = $testCase.SelectSingleNode(".//output")
            if ($output) {
                $caseResult.Output = $output.'#cdata-section'
            }
            
            $results.TestCases += $caseResult
            
            # Group by test suite
            $suiteName = $testCase.classname
            if (-not $results.TestSuites.ContainsKey($suiteName)) {
                $results.TestSuites[$suiteName] = @{
                    SuiteName = $suiteName
                    TotalTests = 0
                    PassedTests = 0
                    FailedTests = 0
                    SkippedTests = 0
                    InconclusiveTests = 0
                    Tests = @()
                }
            }
            
            $suite = $results.TestSuites[$suiteName]
            $suite.TotalTests++
            $suite.Tests += $caseResult
            
            switch ($testCase.result) {
                "Passed" { $suite.PassedTests++ }
                "Failed" { $suite.FailedTests++ }
                "Skipped" { $suite.SkippedTests++ }
                "Ignored" { $suite.SkippedTests++ }
                "Inconclusive" { $suite.InconclusiveTests++ }
            }
        }
        
        return $results
    }
    catch {
        Write-Error "Error parsing ${XmlPath}: $($_.Exception.Message)"
        return @{
            Mode = $Mode
            Error = $_.Exception.Message
        }
    }
}

# Main execution
Write-Host "🧪 Parsing Unity test results..." -ForegroundColor Cyan

$summary = @{
    GenerationTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    ProjectPath = (Get-Location).Path
    UnityVersion = "6000.0.26f1"
    PlayModeResults = $null
    EditModeResults = $null
}

# Parse PlayMode results
if (Test-Path $PlayModeXml) {
    Write-Host "📊 Parsing PlayMode results from $PlayModeXml..." -ForegroundColor Green
    $summary.PlayModeResults = Parse-TestXml -XmlPath $PlayModeXml -Mode "PlayMode"
} else {
    Write-Warning "PlayMode XML not found: $PlayModeXml"
}

# Parse EditMode results
if (Test-Path $EditModeXml) {
    Write-Host "📊 Parsing EditMode results from $EditModeXml..." -ForegroundColor Green
    $summary.EditModeResults = Parse-TestXml -XmlPath $EditModeXml -Mode "EditMode"
} else {
    Write-Warning "EditMode XML not found: $EditModeXml"
}

# Convert to JSON and save
$jsonOutput = $summary | ConvertTo-Json -Depth 10
$jsonOutput | Out-File -FilePath $OutputJson -Encoding UTF8

Write-Host "✅ Test results parsed successfully!" -ForegroundColor Green
Write-Host "📄 Output saved to: $OutputJson" -ForegroundColor Yellow

# Display summary
if ($summary.PlayModeResults) {
    $pm = $summary.PlayModeResults
    Write-Host "PlayMode: $($pm.TotalTests) total, $($pm.PassedTests) passed, $($pm.FailedTests) failed, $($pm.SkippedTests) skipped" -ForegroundColor Cyan
    
    if ($pm.TestSuites) {
        Write-Host "PlayMode Test Suites:" -ForegroundColor Yellow
        foreach ($suite in $pm.TestSuites.Values) {
            $suiteName = $suite.SuiteName
            $totalTests = $suite.TotalTests
            $passedTests = $suite.PassedTests
            $skippedTests = $suite.SkippedTests
            Write-Host "  - ${suiteName}: ${totalTests} tests (${passedTests} passed, ${skippedTests} skipped)" -ForegroundColor White
        }
    }
}

if ($summary.EditModeResults) {
    $em = $summary.EditModeResults
    Write-Host "EditMode: $($em.TotalTests) total, $($em.PassedTests) passed, $($em.FailedTests) failed, $($em.SkippedTests) skipped" -ForegroundColor Cyan
    
    if ($em.TestSuites) {
        Write-Host "EditMode Test Suites:" -ForegroundColor Yellow
        foreach ($suite in $em.TestSuites.Values) {
            $suiteName = $suite.SuiteName
            $totalTests = $suite.TotalTests
            $passedTests = $suite.PassedTests
            $skippedTests = $suite.SkippedTests
            Write-Host "  - ${suiteName}: ${totalTests} tests (${passedTests} passed, ${skippedTests} skipped)" -ForegroundColor White
        }
    }
}

Write-Host ""
Write-Host "For detailed analysis, check the JSON file: $OutputJson" -ForegroundColor Magenta