param([string]$file)
(Get-FileHash $file -Algorithm SHA256).Hash