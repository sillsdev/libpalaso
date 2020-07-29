# WinForms designer

Starting with .NET Core 3.1 it's now possible to use WinForms in sdk-style
csproj files. However, support for opening files in Designer is still not
fully implemented.

JetBrains Rider is able to open files in design mode on Windows.
Current versions of Visual Studio 2019 are also able to open files in
designer if an experimental option is enabled. However, in my testing
it had problems with some projects whereas with others it worked.

To allow Visual Studio 2019 to open files in designer the top of the
.csproj file has to look like this:

```xml
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
  <PropertyGroup>
    <UseWindowsForms>true</UseWindowsForms>
    ...
```

(Strangely enough JetBrains Rider is able to open files in designer
even without those settings...)

Currently on Linux (`dotnet-sdk-3.1` version 3.1.302) there's still a bug in
the .NET sdk that looks for a file in the wrong case. To work around
this bug one can create symlinks from `Microsoft.WinFx.*` to
`Microsoft.WinFX.*` in `/usr/share/dotnet/sdk/3.1.302/Sdks/Microsoft.NET.Sdk.WindowsDesktop/targets`.
This can be done with the following command:

```bash
cd /usr/share/dotnet/sdk/3.1.302/Sdks/Microsoft.NET.Sdk.WindowsDesktop/targets
for f in Microsoft.WinFx.*; do
	sudo ln -s $f ${f/x/X}
done
```
