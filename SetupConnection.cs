using System.Security.Cryptography;
using System.Buffers.Binary;
using Mono.Unix;

namespace cli;

internal static class SetupConnection
{


    public static void Setup(string key, string publicKeyPath)
    {
        try
        {
            CreateUser();
            var paths = CreateDirectories(key, publicKeyPath);
            SetUserPermissions(paths);
        }
        catch (Exception)
        {

            throw;
        }
    }

    private static string[] CreateDirectories(string key, string publicKeyPath)
    {
        var authorizedKeysPath = Path.Combine("/home/fileodon", ".ssh", "authorized_keys");
        var sshPath = Path.Combine("/home/fileodon", ".ssh");
        Directory.CreateDirectory(sshPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        File.AppendAllLines(authorizedKeysPath, [key], System.Text.Encoding.UTF8);
        var publicKeyDirectory = Path.GetDirectoryName(publicKeyPath);
        if (string.IsNullOrEmpty(publicKeyDirectory))
        {
            throw new ArgumentException("The public key path must include a directory.", nameof(publicKeyPath));
        }

        Directory.CreateDirectory(publicKeyDirectory);
        File.WriteAllText(publicKeyPath, ConvertSshPublicKeyToPem(key), System.Text.Encoding.ASCII);
        var uploadPath = Path.Combine("/home/fileodon", "upload");
        // 0o600 permissions for the upload directory
        Directory.CreateDirectory(uploadPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        return [sshPath, authorizedKeysPath, uploadPath, publicKeyDirectory, publicKeyPath];
    }

    private static string ConvertSshPublicKeyToPem(string key)
    {
        string[] parts = key.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || parts[0] != "ssh-rsa")
        {
            throw new FormatException("The received public key is not a supported ssh-rsa key.");
        }

        byte[] keyBlob;
        try
        {
            keyBlob = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException exception)
        {
            throw new FormatException("The received public key contains invalid base64 data.", exception);
        }

        var offset = 0;
        byte[] keyType = ReadSshField(keyBlob, ref offset);
        if (!keyType.AsSpan().SequenceEqual("ssh-rsa"u8))
        {
            throw new FormatException("The received public key does not contain an ssh-rsa key.");
        }

        byte[] exponent = ReadSshField(keyBlob, ref offset);
        byte[] modulus = ReadSshField(keyBlob, ref offset);
        if (offset != keyBlob.Length)
        {
            throw new FormatException("The received public key contains unexpected data.");
        }

        using RSA rsa = RSA.Create();
        rsa.ImportParameters(new RSAParameters
        {
            Exponent = RemoveLeadingZeroes(exponent),
            Modulus = RemoveLeadingZeroes(modulus)
        });
        return rsa.ExportRSAPublicKeyPem();
    }

    private static byte[] ReadSshField(byte[] data, ref int offset)
    {
        if (offset > data.Length - sizeof(uint))
        {
            throw new FormatException("The received public key is truncated.");
        }

        uint length = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset, sizeof(uint)));
        offset += sizeof(uint);
        if (length > data.Length - offset)
        {
            throw new FormatException("The received public key is truncated.");
        }

        byte[] value = data.AsSpan(offset, checked((int)length)).ToArray();
        offset += (int)length;
        return value;
    }

    private static byte[] RemoveLeadingZeroes(byte[] value)
    {
        int firstNonZero = 0;
        while (firstNonZero < value.Length - 1 && value[firstNonZero] == 0)
        {
            firstNonZero++;
        }

        return value[firstNonZero..];
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
