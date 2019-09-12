default: publish
build:
	dotnet publish -r linux-arm -c Release

publish:
	dotnet publish -r linux-arm -c Release
	scp -r bin/Release/netcoreapp2.1/linux-arm/publish/* pi@mini-display:/home/pi/dash

package: build
	tar -C ./bin/Release/netcoreapp2.1/linux-arm/publish -zcvf ./pi-dashboard.tar.gz .
