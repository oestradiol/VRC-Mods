using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using MelonLoader;
using DesktopCamera;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(BuildInfo.Company)]
[assembly: AssemblyProduct(BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + BuildInfo.Author)]
[assembly: AssemblyTrademark(BuildInfo.Company)]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a02961d7-45a2-42c2-937a-8323fcba5ed5")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(BuildInfo.Version)]
[assembly: AssemblyFileVersion(BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonModInfo(typeof(DesktopCamera.Main), BuildInfo.Name, BuildInfo.Version, BuildInfo.Author, BuildInfo.DownloadLink)]
[assembly: MelonModGame(BuildInfo.GameDeveloper, BuildInfo.Game)]
