# Publishing for Linux

A Khaos game published with NativeAOT is a native executable, and Linux executables run only where the
system's C library (glibc) is at least the one they were built against.

Two things set the oldest Linux a player can have:

- **The SDL libraries** in `KappaDuck.Khaos.Native`, built against glibc 2.41 (Steam Runtime 4,
  Debian 13): Debian 13, Ubuntu 25.04, Fedora 42, Arch and newer.
- **Your executable**, linked against the glibc of the machine you publish on. Published on a newer
  system than your players', it will not start, even though the libraries would.

To publish for the same systems as the libraries, publish inside the same image:

```bash
docker run --rm -v "$PWD:/src" -w /src registry.gitlab.steamos.cloud/steamrt/steamrt4/sdk bash -c '
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --jsonfile global.json --install-dir /opt/dotnet
  /opt/dotnet/dotnet publish MyGame.Desktop -c Release -r linux-x64'
```

On a system that is too old, the game stops at startup with `version 'GLIBC_2.xx' not found`.
