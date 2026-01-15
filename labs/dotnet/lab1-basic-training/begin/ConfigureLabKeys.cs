using System.Security.Cryptography;
using LabKeyEncrypter.Library;

public sealed class ConfigureLabKeys
{
   private readonly bool _verbose;

   public string Password { get; }

   public ConfigureLabKeys(string password, bool verbose = false)
   {
      Password = password;
      _verbose = verbose;
   }

   /// <summary>
   /// One-time lab configuration:
   /// - Locate the keys directory
   /// - If appsettings.Local.json already exists in keys, do nothing (unless overwriteExisting is true)
   /// - Randomly pick an encrypted appsettings file, decrypt it, and copy it to appsettings.Local.json
   /// - If a sibling labs folder exists, distribute appsettings.Local.json to lab*/src/{start,end}
   /// </summary>
   public void RandomizeDecryptDistribute(string startPath = ".", bool overwriteExisting = false)
   {
      var keysPath = FindKeysDirectoryPath(startPath);
      var keysDir = new DirectoryInfo(keysPath);

      var localSettingsFileName = "appsettings.Local.json";
      var localSettingsPath = Path.Combine(keysPath, localSettingsFileName);

      if (File.Exists(localSettingsPath) && !overwriteExisting)
      {
         if (_verbose)
         {
            Console.WriteLine("Skipping lab configuration: appsettings.Local.json already present.");
            Console.WriteLine($"{localSettingsPath}");
         }
         return;
      }

      if (File.Exists(localSettingsPath) && overwriteExisting && _verbose)
      {
         Console.WriteLine("Overwriting lab configuration: appsettings.Local.json already present.");
         Console.WriteLine($"{localSettingsPath}");
      }

      Console.WriteLine("One-time lab configuration started.");

      var encryptedSettingsPath = RandomlySelectEncryptedSettings(keysPath);
      if (_verbose)
      {
         Console.WriteLine($"Selected encrypted settings: {encryptedSettingsPath}");
      }

      try
      {
         LabKeyJsonFileValueEncrypter.DecryptJsonValues(encryptedSettingsPath, Password);
      }
      catch (CryptographicException ex)
      {
         Console.WriteLine($"Error decrypting {encryptedSettingsPath}: {ex.Message}");
         Console.WriteLine($"Double-check that password '{Password}' is correct.");
         Environment.Exit(1);
         return;
      }

      var decryptedSettingsPath = encryptedSettingsPath.Replace("_encrypted.json", ".json", StringComparison.OrdinalIgnoreCase);
      if (!File.Exists(decryptedSettingsPath))
      {
         throw new FileNotFoundException("Decryption did not produce expected output file.", decryptedSettingsPath);
      }

      File.Copy(decryptedSettingsPath, localSettingsPath, overwrite: true);

      // Optional distribution step: if there are lab folders nearby, copy appsettings.Local.json into them.
      var labsDir = FindLabsDirectory(keysDir);
      var labsPath = labsDir?.FullName;
      if (!string.IsNullOrWhiteSpace(labsPath))
      {
         var labTargets = GetAllLabPaths(labsPath);
         foreach (var labTarget in labTargets)
         {
            var labSettingsPath = Path.Combine(labTarget, localSettingsFileName);
            Directory.CreateDirectory(labTarget);
            File.Copy(localSettingsPath, labSettingsPath, overwrite: true);
         }

         if (_verbose && labTargets.Length > 0)
         {
            Console.WriteLine($"Distributed settings to {labTargets.Length} lab folders under {labsPath}.");
         }
      }
      else if (_verbose)
      {
         Console.WriteLine("No 'labs' directory found above keys; skipping distribution.");
      }

      Console.WriteLine("One-time lab configuration completed.");
   }

   /// <summary>
   /// Finds a keys directory by walking up the directory tree.
   /// Accepts either:
   /// - a directory named 'keys'
   /// - a directory that contains a 'keys' subdirectory
   /// </summary>
   public string FindKeysDirectoryPath(string startPath)
   {
      var currentDir = new DirectoryInfo(Path.GetFullPath(startPath));

      while (currentDir != null)
      {
         // Case 1: current directory is keys
         if (string.Equals(currentDir.Name, "keys", StringComparison.OrdinalIgnoreCase))
         {
            return currentDir.FullName;
         }

         // Case 2: current directory contains a keys subdirectory
         var candidate = Path.Combine(currentDir.FullName, "keys");
         if (Directory.Exists(candidate) && ContainsEncryptedSettings(candidate))
         {
            return candidate;
         }

         if (_verbose)
         {
            Console.WriteLine($"Searching: {currentDir.FullName}");
         }

         currentDir = currentDir.Parent;
      }

      throw new InvalidOperationException("Unable to locate a 'keys' directory (containing encrypted appsettings files).");
   }

   public string RandomlySelectEncryptedSettings(string keysPath)
   {
      // Can support more than one pattern
      // Example in Jan 2026: 3.appsettings.Local_encrypted.json => 3.appsettings.Local.json
      var patterns = new[]
      {
            "*.appsettings.Local_encrypted.json",
        };

      var files = patterns
          .SelectMany(p => Directory.GetFiles(keysPath, p))
          .Distinct(StringComparer.OrdinalIgnoreCase)
          .ToArray();

      if (files.Length == 0)
      {
         throw new FileNotFoundException($"No encrypted appsettings files found in {keysPath}.");
      }

      return files[Random.Shared.Next(files.Length)];
   }

   public string[] GetAllLabPaths(string labsPath)
   {
      // in 2026 the only "lab path" that matters is labs/dotnet (for .NET labs)
      // and labs/python (for Python labs)
      // the directory tree structure is /labs/keys, /labs/dotnet/ and /labs/python/

      // assuming labsPath is .../labs
      var result = new List<string> { "dotnet", "python" }
         .Select(labName => Path.Combine(labsPath, labName))
         .Where(Directory.Exists)
         .ToList();

      if (_verbose)
      {
         foreach (var path in result)
         {
            Console.WriteLine($"Lab target: {path}");
         }
      }

      return result.ToArray();
   }

   private static DirectoryInfo? FindLabsDirectory(DirectoryInfo keysDir)
   {
      var current = keysDir;
      while (current != null)
      {
         if (string.Equals(current.Name, "labs", StringComparison.OrdinalIgnoreCase))
         {
            return current;
         }

         current = current.Parent;
      }

      return null;
   }

   private static bool ContainsEncryptedSettings(string keysPath)
   {
      return Directory.GetFiles(keysPath, "*.appsettings.Local_encrypted.json").Length > 0;
   }
}
