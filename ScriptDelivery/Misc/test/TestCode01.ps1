# テスト用コード

################################################################
# Prepare
################################################################

$url = "http://localhost:5000"



################################################################
# Main
################################################################

# / へのGetアクセス
$res = Invoke-RestMethod -Method Get -Uri "${url}/"
($res -ne $null) -and ($res -eq "")

# / へのPostアクセス可否
$res = Invoke-RestMethod -Method Post -Uri "${url}/"
($res -ne $null) -and ($res -eq "")

# /map へアクセスしてMapping情報を取得
$res = Invoke-RestMethod -Method Post -Uri "${url}/map"
Write-Host $res.Heaers
Write-Host "MappingList(JSON):`r`n" + $res.Body

