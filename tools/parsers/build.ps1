
dir -Attributes Directory | % {
    docker build $_.Name -t "aaubry/yaml-$($_.Name)"
}

dir -Attributes Directory | % {
    Write-Host "echo a:b | docker run --rm -i aaubry/yaml-$($_.Name)"
}
