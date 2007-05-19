Release History

v1.0.705.1917:
- Fixed bug where CompactorCodeProvider was being reused and resources were experiencing collisions

v1.0.705.1608:
- Fixed bug where BuildManager fails when resources aren't prebuilt
- Added "X-JsonFx-Version" response header

v1.0.705.1501:
- Fixed bug in JsonMLTextWriter, where was outputting HTML comments
- Suppressed error where BuildManager fails when resources aren't prebuilt

v1.0.705.0801:
- Added basic History example
- Now supports full error reporting and compaction of CSS & JavaScript
- Cleaned up scripts based upon latest JSLint restrictions
- Fixed error formatting in BuildTools
- Trapping x64 exceptions in BuildTools (MSScriptControl requires x86)
- Made JsonFx strongly signed enabling placement in the GAC

v1.0.704.2321:
- Fixed bug in JsonMLTextWriter, where wouldn't write attributes of an empty element

v1.0.704.2320:
- Removed need for .browser files by adding HttpModule:
- Produces more consistent async request behavior across platforms
- Simplifies integration by reducing feature to a single line in web.config
- Moved ASP.NET .browser "fixes" into single Other.browser file
- Pruned some obsolete/dormant classes

v1.0.704.2210:
- Cleaned up display of JSON-RPC Service test
- Added ability to specify the javascript proxy namespace on a JSON-RPC service
- Made service proxy a true singleton, rather than an instance in a static field
- Cleaned up JsonService attributes a bit to more closely align with ASP.NET WebServices
- Added example WebService which uses the same class as the JsonService (to further demonstrate similarities)
- Fixed some minor bugs in edge case service errors
- Added ~/Download/LatestBuild.ashx so download URL doesn't need to change

v1.0.704.1900:
- A bare bones example, but it shows just about everything
- Added Example pages ~/Services/Default.aspx and ~/Effects/Default.aspx with some light instructions on them
- IIS Configuration (http://jsonfx.net/Download/JsonFx_Config.gif)
- You will need to register the extensions .jsonfx, .json, and .js to be handled by ASP.NET
- Eventually .css also, but that isn't quite finished
- If you end up mapping services or handlers to other extensions, then you would obviously need to register those extensions as well/instead.
- I think that is the only configuration that you need to do.
