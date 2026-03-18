using Microsoft.Win32;
using System;
using System.IO;

namespace NekoPlayer.App.Utils
{
  [SupportedOSPlatform("windows")]
  public class DiscordInstallationChecker
  {
      public static bool IsDiscordInstalled()
      {
          string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
          {
              if (key != null)
              {
                  foreach (string subkeyName in key.GetSubKeyNames())
                  {
                      using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                      {
                          if (subkey != null)
                          {
                              // Check for DisplayName, which might be "Discord"
                              string displayName = subkey.GetValue("DisplayName") as string;
                              if (!string.IsNullOrEmpty(displayName) && displayName.Contains("Discord"))
                              {
                                  // Optionally, check the InstallLocation as well
                                  string installLocation = subkey.GetValue("InstallLocation") as string;
                                  if (!string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                                  {
                                      return true; // Discord is likely installed
                                  }
                              }
                          }
                      }
                  }
              }
          }
          return false; // Discord not found in HKLM\Uninstall
      }
  }
}
