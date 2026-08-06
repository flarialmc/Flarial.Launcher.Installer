# Flarial Launcher Installer
An installer for [Flarial Client's MSIX Launcher.](https://github.com/flarialmc/Flarial.Launcher)

## Resources
- The launcher's MSIX package requires a public certificate to be installed so it is trusted.
  - You may download it here: https://cdn.flarial.xyz/launcher/Flarial.Launcher.cer
 
- The installer verifies the public certificate's thumbprint using SHA256:
  - `080862035B63C6B01A1F7F5E2A286939808F502ADCA100BDCB6F805FB0DD4171` 

## Notes
- This repository contains source code for the installer used for the MSIX launcher.
- Starting 3rd August 2026, all 'portable' launchers will be auto-migrated to MSIX.
