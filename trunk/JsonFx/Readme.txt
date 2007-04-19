2007-04-19-0022_JsonFx.zip

So this is pretty bare bones, but it shows just about everything.  There are two Example pages ~/Services/Default.aspx and ~/Effects/Default.aspx with some light instructions on them.

IIS Configuration:

You will need to register the extensions .jsonfx, .json, and .js to be handled by ASP.NET.  Eventually .css also, but that isn't quite finished.  Also if you end up mapping services or handlers to other extensions, then you would obviously need to register those extensions as well/instead.

I think that is the only configuration that you need to do.

Good luck and let me know if it works or not!

smm