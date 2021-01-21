#!/opt/microsoft/powershell/7/pwsh

param(
    [Parameter(Mandatory)][string] $repository,
    [Parameter(Mandatory)][int] $issueNumber,
    [Parameter(Mandatory)][string] $apiToken
)

$userAgent = "YamlDotNet syntax checked bot (yamldotnet@aaubry.net)"

Invoke-WebRequest https://api.github.com/repos/$repository/issues/$issueNumber/comments?per_page=100 -UserAgent $userAgent | `
    % { $_.Content } | `
    ConvertFrom-Json | `
    ? { $_.user.login -eq "yamldotnet" } | `
    % {
        $r = Invoke-WebRequest https://api.github.com/repos/$repository/issues/comments/$($_.id) `
            -UserAgent $userAgent `
            -Headers @{ Authorization = "Bearer $apiToken" } `
            -Method Delete

        if ($r.StatusCode -ne 204) {
            Write-Error $r
        }
    }

Invoke-WebRequest https://api.github.com/repos/$repository/issues/$issueNumber -UserAgent $userAgent | `
    % { $_.Content } | `
    ConvertFrom-Json | `
    % { $_.body } | `
    rundoc list-blocks - "-t#yaml" | `
    ConvertFrom-Json | `
    % { $_.code_blocks } | `
    % {
        $errors = @()
        $previousLine = ''
        $_.code | /opt/yamlreference/dist/build/yaml2yeast/yaml2yeast | % {
            if ($_ -match "^!Unexpected") {
                $previousLine -match "L: (\d+), c: (\d+)" | Out-Null
                $errors += "Line $($Matches[1]), char $([int]$Matches[2] + 1): $($_.TrimStart('!'))"
            }
            $previousLine = $_
        }

        if ($errors.Count -ne 0)
        {
            $quotedCode = $_.code.TrimEnd("`n").TrimEnd("`r") -replace '(^|\n)','$1> '
            $comment = "> ``````yaml`n$quotedCode`n> ```````n**This YAML snippet appears to be invalid.**  `nThe following errors were identifier by the [reference parser](http://ben-kiki.org/ypaste/):`n - " + [string]::Join("`n - ", $errors)

            $r = Invoke-WebRequest https://api.github.com/repos/$repository/issues/$issueNumber/comments `
                 -UserAgent $userAgent `
                 -Headers @{ Authorization = "Bearer $apiToken"; "Content-Type" = "application/json" } `
                 -Method Post `
                 -Body (@{ body = $comment } | ConvertTo-Json)

            if ($r.StatusCode -ne 201) {
                Write-Error $r
            }
        }
    }
