using System;
using System.IO;
using System.Runtime.InteropServices;

static class CreateAliasesHelper
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

    // List of DLL aliases
    private static readonly string[][] Aliases = new string[][]
    {
        new string[]{ "gtk-4-1.dll", "libgtk-4-1.dll" },
        new string[]{ "adwaita-1-0.dll", "libadwaita-1-0.dll" },
        new string[]{ "glib-2.0-0.dll", "libglib-2.0-0.dll" },
        new string[]{ "gobject-2.0-0.dll", "libgobject-2.0-0.dll" },
        new string[]{ "gio-2.0-0.dll", "libgio-2.0-0.dll" },
        new string[]{ "gmodule-2.0-0.dll", "libgmodule-2.0-0.dll" },
        new string[]{ "gthread-2.0-0.dll", "libgthread-2.0-0.dll" },
        new string[]{ "graphene-1.0-0.dll", "libgraphene-1.0-0.dll" },
        new string[]{ "pango-1.0-0.dll", "libpango-1.0-0.dll" },
        new string[]{ "pangocairo-1.0-0.dll", "libpangocairo-1.0-0.dll" },
        new string[]{ "cairo-2.dll", "libcairo-2.dll" },
        new string[]{ "cairo-gobject-2.dll", "libcairo-gobject-2.dll" },
        new string[]{ "gdk_pixbuf-2.0-0.dll", "libgdk_pixbuf-2.0-0.dll" }
    };

    public static void EnsureAliasesExist(string folder)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        foreach (var pair in Aliases)
        {
            string original = Path.Combine(folder, pair[0]);
            string alias = Path.Combine(folder, pair[1]);

            if (!File.Exists(original))
                throw new FileNotFoundException($"Original DLL not found: {original}");

            if (File.Exists(alias))
                continue;

            bool success = CreateHardLink(alias, original, IntPtr.Zero);
            if (!success)
            {
                int err = Marshal.GetLastWin32Error();
                throw new IOException($"Failed to create hardlink {alias} -> {original}, error code: {err}");
            }
        }
    }
}
