if(![System.IO.File]::Exists("tools/warp-packer.exe")) {
	if(![System.IO.Directory]::Exists("tools")) {
		New-Item "tools" -itemtype directory
	}

	[Net.ServicePointManager]::SecurityProtocol = "tls12, tls11, tls" ; Invoke-WebRequest https://github.com/dgiagio/warp/releases/download/v0.3.0/windows-x64.warp-packer.exe -OutFile tools/warp-packer.exe
}

pushd ./src/CLI/
	dotnet publish --self-contained --configuration  Release -r win10-x64
popd

	if(![System.IO.Directory]::Exists("_")) {
		New-Item "_" -itemtype directory
	}

.\tools\warp-packer --arch windows-x64 --input_dir src/CLI/bin/netcoreapp2.2/win10-x64/publish --exec stamp.exe --output _\stamp.exe
