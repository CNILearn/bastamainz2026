$template = Get-Content .\Codebreaker.template.json -Raw
$template = $template.Replace('"dummy1a"', '"eu-central-1a"')
$template = $template.Replace('"dummy1b"', '"eu-central-1b"')
Set-Content .\Codebreaker.template.json $template -Encoding utf8

$manifest = Get-Content .\manifest.json -Raw
$manifest = $manifest.Replace('"dummy1a"', '"eu-central-1a"')
$manifest = $manifest.Replace('"dummy1b"', '"eu-central-1b"')
Set-Content .\manifest.json $manifest -Encoding utf8

Write-Host "Checking for remaining dummy availability zones..."
findstr /S /I "dummy1a dummy1b" *.json
