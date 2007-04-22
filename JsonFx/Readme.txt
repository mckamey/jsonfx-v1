Release History

2007-04-22-1014_JsonFx.zip:

- Cleaned up display of JSON-RPC Service test
- Added ability to specify the javascript proxy namespace on a JSON-RPC service
- Made service proxy a true singleton, rather than an instance in a static field
- Cleaned up JsonService attributes a bit to more closely align with ASP.NET WebServices
- Added example WebService which uses the same class as the JsonService (to further demonstrate similarities)
- Fixed some minor bugs in edge case service errors
- Added ~/Download/LatestBuild.ashx so download URL doesn't need to change

2007-04-19-0022_JsonFx.zip:

- A bare bones example, but it shows just about everything
- Added Example pages ~/Services/Default.aspx and ~/Effects/Default.aspx with some light instructions on them
- IIS Configuration (http://jsonfx.net/Download/JsonFx_Config.gif)
- You will need to register the extensions .jsonfx, .json, and .js to be handled by ASP.NET
- Eventually .css also, but that isn't quite finished
- If you end up mapping services or handlers to other extensions, then you would obviously need to register those extensions as well/instead.
- I think that is the only configuration that you need to do.
