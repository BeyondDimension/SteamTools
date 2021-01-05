$Here = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

$pfx = new-object System.Security.Cryptography.X509Certificates.X509Certificate2 
$certPath = "$Here\lib\rootCert.pfx"
$pfxPass =  ""
$pfx.import($certPath,$pfxPass,"Exportable,PersistKeySet") 
$store = new-object System.Security.Cryptography.X509Certificates.X509Store([System.Security.Cryptography.X509Certificates.StoreName]::Root, "localmachine")
$store.open("MaxAllowed") 
$store.add($pfx) 
$store.close()