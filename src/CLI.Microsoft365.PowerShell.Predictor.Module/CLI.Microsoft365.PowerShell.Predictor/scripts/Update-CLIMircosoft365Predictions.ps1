function Update-CLIMircosoft365Predictions {

    Param($installedCLIMicrosoft365Version)
    
    $windowTitle = $Host.UI.RawUI.WindowTitle;

    try {
        if($null -eq $installedCLIMicrosoft365Version) {
            $installedCLIMicrosoft365Version = "0.0.0";
        }

        if ($(Test-Path (Join-Path -Path $PSScriptRoot -ChildPath "..\Data\CLI.Microsoft365.PowerShell.Suggestions.$($installedCLIMicrosoft365Version).json")) -eq $false) {

            Write-Host "Updating CLI.Microsoft365.PowerShell.Predictor suggestions for version $($installedCLIMicrosoft365Version)..." -ForegroundColor Yellow;
            
            $json = @();
    
            # get all cli for Microsoft 365 docs from the m365 cli docs folder
            $m365cliDocsPath = Join-Path $(npm root -g) -ChildPath "@pnp\cli-microsoft365\docs\docs\cmd";
            $files = Get-ChildItem -Path $m365cliDocsPath -Filter "*.md" -Recurse;

            # create a regex pattern to match the example code
            $pattern = "(?<=``````sh)(.*?)(?=``````)"
            
            # set id to 1
            $id = 1;
    
            # loop through each file
            $files | ForEach-Object {
        
                # get the file data
                $fileData = Get-Content $_.FullName;
                
                $result = [regex]::Matches($fileData, $pattern);
    
                $i = 1;
                foreach ($item in $result) {
    
                    $value = $item.Value.Trim();
    
                    # if the item value contains [options] then don't add it to the json
                    if ($value -match "\[options\]") {
                        continue;
                    }

                    # extract everything before --
                    $commandName = $value.Split("--")[0].Trim();
                    
                    $json += @{
                        "CommandName" = $commandName
                        "Command" = $value
                        "Rank"    = $i
                        "Id"      = $id
                    }
                    $i++;
                    $id++;
                }
            }

            # Create Data folder if it doesn't exist
            if ($(Test-Path (Join-Path -Path $PSScriptRoot -ChildPath "..\Data")) -eq $false) {
                New-Item -ItemType Directory -Path (Join-Path -Path $PSScriptRoot -ChildPath "..\Data") | Out-Null;
            }
    
            $jsonPath = Join-Path $PSScriptRoot -ChildPath "..\Data\CLI.Microsoft365.PowerShell.Suggestions.$($installedCLIMicrosoft365Version).json";
            # write the json to a file
            $json | ConvertTo-Json -Depth 10 | Out-File -FilePath $jsonPath -Encoding UTF8 -Force;
        }
    }
    catch {
        throw "Failed to load CLI.Microsoft365.PowerShell.Predictor module. Error in getting commands from docs"   
    }
    finally {
        # Reset the title
        $Host.UI.RawUI.WindowTitle = $windowTitle
    }
}