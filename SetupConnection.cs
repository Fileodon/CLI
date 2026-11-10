using System.Security.Cryptography;
using Mono.Unix;

namespace cli;

internal static class SetupConnection
{


    public static void Setup(string key)
    {
        try
        {
            CreateUser();
            var paths = CreateDirectories(key);
            SetUserPermissions(paths);
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
        Directory.CreateDirectory(sshPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        File.AppendAllLines(authorizedKeysPath, [key], System.Text.Encoding.UTF8);
        var uploadPath = Path.Combine("/home/fileodon", "upload");
        // 0o600 permissions for the upload directory
        Directory.CreateDirectory(uploadPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        return [sshPath, authorizedKeysPath, uploadPath];
    }

    private static byte[] GenerateKey(int size)
    {
        var key = RSA.Create(size);
        if (key is null)
        {
            Initialize.Log("Failed to generate RSA key pair.");
            return null;
        }

        var privateKey = key.ExportRSAPrivateKey();
        if (privateKey is null)
        {
            Initialize.Log("Failed to export RSA private key.");
            return null;
        }
        return privateKey;
    }

    private static void CreateUser()
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "sudo",
            Arguments = "useradd -m -s /bin/bash -p * fileodon",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        System.Diagnostics.Process.Start(psi);

        Task.WaitAll(
            Task.Run(() =>
            {
                var process = System.Diagnostics.Process.Start(psi);
                if (process is null)
                {
                    Initialize.Log("Failed to start useradd process.");
                    return;
                }
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    Initialize.Log($"useradd process exited with code {process.ExitCode}.");
                    return;
                }
            })
        );
    }

    private static void SetUserPermissions(string[] paths)
    {
        if (paths is null || paths.Length == 0)
        {
            Initialize.Log("No paths provided for setting user permissions.");
            return;
        }

        foreach (string path in paths)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Initialize.Log($"Path does not exist: {path}");
                continue;
            }

            var user = UnixUserInfo.GetLocalUsers().FirstOrDefault(x => x.UserName.Equals("fileodon", StringComparison.OrdinalIgnoreCase));
            var group = UnixGroupInfo.GetLocalGroups().FirstOrDefault(x => x.GroupName.Equals("fileodon", StringComparison.OrdinalIgnoreCase));
            new UnixDirectoryInfo(path).SetOwner(user, group);

        }
    }
}
