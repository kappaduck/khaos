# Publishing for Linux

Linux players use many different distributions, and a game built on one does not always start on another. This page
explains why in two minutes, and gives you the one command that makes your Linux build work on as many systems as
possible.

## The short version

Publish your Linux build inside the Steam Runtime container, like Khaos's own libraries are built:

```bash
docker run --rm -v "$PWD:/src" -w /src registry.gitlab.steamos.cloud/steamrt/steamrt4/sdk bash -c '
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 11.0 --install-dir /opt/dotnet
  apt-get update && apt-get install -y clang zlib1g-dev
  /opt/dotnet/dotnet publish MyGame.Desktop -c Release -r linux-x64'
```

Run it from your game's folder, on Linux or on Windows with Docker Desktop. The result lands in the usual publish
folder, and runs on Debian 13, Ubuntu 25.04, Fedora 42, Arch Linux and newer.

## Why it matters

Every Linux program depends on the system's C library, **glibc**, and only runs where glibc is **at least as new**
as the one it was built with. Two things in your game set that minimum:

- **Khaos's libraries**, built against glibc 2.41: the version of Debian 13, Ubuntu 25.04 and Fedora 42.
- **Your executable**, built against the glibc of the computer you publish on. If you publish on a very recent
  system, your game asks for that very recent glibc, and players on slightly older systems cannot start it, even
  though Khaos's libraries would run there.

Publishing in the same container as Khaos's libraries makes both minimums the same, so your game runs everywhere
Khaos does.

## When a player cannot start the game

On a system that is too old, the game stops at startup with a message such as:

```text
version 'GLIBC_2.41' not found
```

There is nothing the player can install to fix it, short of updating their system. Point them to the browser
version of your game, which runs on any system.
