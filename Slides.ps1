[CmdletBinding(PositionalBinding=$false)]
Param(
    [Parameter(ValueFromRemainingArguments)]
    [string[]]$Remaining
)

# Run the slides
Push-Location
Set-Location slides/Spectre.Presentation
dotnet run -- $Remaining
Pop-Location