Stop-Service 'MobaXterm'
Get-CimInstance -ClassName Win32_service -Filter "Name='MobaXterm'" | Remove-CimInstance