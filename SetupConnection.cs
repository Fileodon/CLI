using Mono.Unix;

namespace cli;

internal static class SetupConnection
{
    public static async Task<int> Setup(string key)
    {
        try
        {
            Initialize.Log("Creationg user");
            await CreateUser();
            Initialize.Log("User created");
            Initialize.Log("Creating directories");
            var paths = CreateDirectories(key);
            Initialize.Log("Directories created");
            Initialize.Log("Setting user permissions");
            SetUserPermissions(paths);
            Initialize.Log("Done setting user permissions");
            return 0;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static string[] CreateDirectories(string key)
    {
        var authorizedKeysPath = Path.Combine("/home/fileodon", ".ssh", "authorized_keys");
        var sshPath = Path.Combine("/home/fileodon", ".ssh");
        Initialize.Log($"Creating .ssh directory at {sshPath} and authorized_keys file at {authorizedKeysPath}.");
        Directory.CreateDirectory(sshPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        Initialize.Log($"Creating authorized_keys file at {authorizedKeysPath}.");
        File.SetUnixFileMode(sshPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        Initialize.Log($"Appending SSH key to authorized_keys file at {authorizedKeysPath}.");
        File.AppendAllText(authorizedKeysPath, key + Environment.NewLine, new System.Text.UTF8Encoding(false));
        Initialize.Log($"Setting permissions for authorized_keys file at {authorizedKeysPath}.");
        File.SetUnixFileMode(authorizedKeysPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        Initialize.Log($"Creating upload directory at /home/fileodon/upload.");
        var uploadPath = Path.Combine("/home/fileodon", "upload");
        Directory.CreateDirectory(uploadPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        Initialize.Log($"Setting permissions for upload directory at {uploadPath}.");
        File.SetUnixFileMode(uploadPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        Initialize.Log("Setup completed successfully.");
        return [sshPath, authorizedKeysPath, uploadPath];
    }

    private static async Task CreateUser()
    {
        Initialize.Log("Checking if the fileodon user exists.");
        if (UnixUserInfo.GetLocalUsers()
            .Any(x => x.UserName.Equals("fileodon", StringComparison.OrdinalIgnoreCase)))
        {
            Initialize.Log("The fileodon user already exists. Skipping user creation.");
            return;
        }

        Initialize.Log("Creating the fileodon user.");
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "useradd",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        Initialize.Log("Running useradd command to create the fileodon user.");
        startInfo.ArgumentList.Add("-U");
        startInfo.ArgumentList.Add("-m");
        startInfo.ArgumentList.Add("-s");
        startInfo.ArgumentList.Add("/bin/bash");
        startInfo.ArgumentList.Add("-p");
        startInfo.ArgumentList.Add("*");
        startInfo.ArgumentList.Add("fileodon");
        Initialize.Log($"Executing command: {startInfo.FileName} {string.Join(" ", startInfo.ArgumentList)}");

        using var process = System.Diagnostics.Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start useradd process.");
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        await standardOutput;
        var error = await standardError;
        Initialize.Log($"useradd process exited with code {process.ExitCode}: {error.Trim()}");
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"useradd process exited with code {process.ExitCode}: {error.Trim()}");
        }
    }

    private static void SetUserPermissions(string[] paths)
    {
        if (paths is null || paths.Length == 0)
        {
            Initialize.Log("No paths provided for setting user permissions. Skipping permission setting.");
            throw new ArgumentException("No paths provided for setting user permissions.", nameof(paths));
        }
        Initialize.Log("Setting ownership of created directories and files to the fileodon user and group.");
        var user = UnixUserInfo.GetLocalUsers()
            .FirstOrDefault(x => x.UserName.Equals("fileodon", StringComparison.OrdinalIgnoreCase));
        Initialize.Log($"Found user: {user?.UserName ?? "null"}");
        var group = UnixGroupInfo.GetLocalGroups()
            .FirstOrDefault(x => x.GroupName.Equals("fileodon", StringComparison.OrdinalIgnoreCase));
        Initialize.Log($"Found group: {group?.GroupName ?? "null"}");
        if (user is null || group is null)
        {
            Initialize.Log("Could not find the fileodon user and group after creating the account.");
            throw new InvalidOperationException("Could not find the fileodon user and group after creating the account.");
        }
        Initialize.Log($"Setting ownership for paths: {string.Join(", ", paths)}");
        foreach (string path in paths)
        {
            if (Directory.Exists(path))
            {
                Initialize.Log($"Setting ownership for directory: {path}");
                new UnixDirectoryInfo(path).SetOwner(user, group);
                Initialize.Log("Ownership set!");
            }
            else if (File.Exists(path))
            {
                Initialize.Log($"Setting ownership for file: {path}");
                new UnixFileInfo(path).SetOwner(user, group);
                Initialize.Log("Ownership set!");
            }
            else
            {
                Initialize.Log($"Path does not exist: {path}");
                throw new FileNotFoundException($"Path does not exist: {path}", path);
            }
        }
    }
}
