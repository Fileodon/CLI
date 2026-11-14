using Mono.Unix;

namespace cli;

internal static class SetupConnection
{
    public static async Task Setup(string key)
    {
        await CreateUser();
        var paths = CreateDirectories(key);
        SetUserPermissions(paths);
    }

    private static string[] CreateDirectories(string key)
    {
        var authorizedKeysPath = Path.Combine("/home/fileodon", ".ssh", "authorized_keys");
        var sshPath = Path.Combine("/home/fileodon", ".ssh");
        Directory.CreateDirectory(sshPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        File.SetUnixFileMode(sshPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        File.AppendAllText(authorizedKeysPath, key + Environment.NewLine, new System.Text.UTF8Encoding(false));
        File.SetUnixFileMode(authorizedKeysPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        var uploadPath = Path.Combine("/home/fileodon", "upload");
        Directory.CreateDirectory(uploadPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        File.SetUnixFileMode(uploadPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        return [sshPath, authorizedKeysPath, uploadPath];
    }

    private static async Task CreateUser()
    {
        if (UnixUserInfo.GetLocalUsers()
            .Any(x => x.UserName.Equals("fileodon", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "useradd",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("-U");
        startInfo.ArgumentList.Add("-m");
        startInfo.ArgumentList.Add("-s");
        startInfo.ArgumentList.Add("/bin/bash");
        startInfo.ArgumentList.Add("-p");
        startInfo.ArgumentList.Add("*");
        startInfo.ArgumentList.Add("fileodon");

        using var process = System.Diagnostics.Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start useradd process.");
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        await standardOutput;
        var error = await standardError;
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
            throw new ArgumentException("No paths provided for setting user permissions.", nameof(paths));
        }

        var user = UnixUserInfo.GetLocalUsers()
            .FirstOrDefault(x => x.UserName.Equals("fileodon", StringComparison.OrdinalIgnoreCase));
        var group = UnixGroupInfo.GetLocalGroups()
            .FirstOrDefault(x => x.GroupName.Equals("fileodon", StringComparison.OrdinalIgnoreCase));
        if (user is null || group is null)
        {
            throw new InvalidOperationException("Could not find the fileodon user and group after creating the account.");
        }

        foreach (string path in paths)
        {
            if (Directory.Exists(path))
            {
                new UnixDirectoryInfo(path).SetOwner(user, group);
            }
            else if (File.Exists(path))
            {
                new UnixFileInfo(path).SetOwner(user, group);
            }
            else
            {
                throw new FileNotFoundException($"Path does not exist: {path}", path);
            }
        }
    }
}
