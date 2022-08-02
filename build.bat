cd GmodAddonCompressor

dotnet publish --configuration Release -r win-x86 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained false
dotnet publish --configuration Release -r win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained false
