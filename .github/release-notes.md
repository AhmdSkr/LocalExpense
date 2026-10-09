## Download and run

1. Download the `LocalExpense-…-win-x64.zip` below. It runs on Windows 10 and 11 (64-bit) and needs no .NET installed.
2. Unzip it anywhere and run `LocalExpense.exe`. There is no installer; to remove the app, delete the folder.
3. To look around before entering your own data, click **Sample data**. A second window opens on three years of a fictional family's money; nothing you do there touches your own data.

**Windows SmartScreen.** This build is not code-signed yet, so on first run Windows may say "Windows protected your PC". Click **More info**, then **Run anyway**.

**Your data** is stored in `%LocalAppData%\LocalExpense\localexpense.db`. Back up that file to back up everything.

## Verify the download (optional)

Compare the zip's hash with `SHA256SUMS.txt`:

```powershell
Get-FileHash .\LocalExpense-*-win-x64.zip -Algorithm SHA256
```

Check that the zip was built by this repository's release workflow, using the [GitHub CLI](https://cli.github.com/):

```powershell
gh attestation verify .\LocalExpense-<version>-win-x64.zip --repo AhmdSkr/LocalExpense
```
