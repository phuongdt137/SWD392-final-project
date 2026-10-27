$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:5089'
foreach ($account in @(@{Name='staff'; Path='/Timetable/Index'}, @{Name='teacher'; Path='/Timetable/Teacher'}, @{Name='student'; Path='/Timetable/Student'})) {
    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $login = Invoke-WebRequest "$base/Auth/Login" -WebSession $session
    $token = [regex]::Match($login.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
    if (!$token) { throw 'Missing login antiforgery token' }
    $response = Invoke-WebRequest "$base/Auth/Login" -Method Post -WebSession $session -Body @{
        UsernameOrEmail=$account.Name; Password='Password@123'; __RequestVerificationToken=$token
    }
    if ($response.BaseResponse.RequestMessage.RequestUri.AbsolutePath -eq '/Auth/Login') { throw "Login failed: $($account.Name)" }
    $page = Invoke-WebRequest ($base + $account.Path) -WebSession $session
    if ($page.StatusCode -ne 200 -or !$page.Content.Contains('name="week"')) { throw "Timetable failed: $($account.Name)" }
    Write-Output "PASS HTTP: $($account.Name) login and $($account.Path)"
    if ($account.Name -eq 'staff') {
        $create = Invoke-WebRequest "$base/Timetable/Create" -WebSession $session
        if (!$create.Content.Contains('id="employeeId"')) { throw 'Create view not rendered' }
        Write-Output 'PASS HTTP: staff create form and dropdown queries'
    } else {
        $denied = Invoke-WebRequest "$base/api/teaching-assignment" -Method Post -ContentType 'application/json' -Body '{}' -WebSession $session -SkipHttpErrorCheck
        if ($denied.StatusCode -ne 403) { throw "Expected 403 for $($account.Name), got $($denied.StatusCode)" }
        Write-Output "PASS HTTP: $($account.Name) cannot create timetable"
    }
}
