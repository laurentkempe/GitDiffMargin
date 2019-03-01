$ErrorActionPreference = 'Stop';

$toolsPath = Split-Path $MyInvocation.MyCommand.Definition
$filePath = "$toolsPath\GitDiffMargin.vsix"

$package = 'GitDiffMargin'
$vsixUrl = "file://" + $filePath.Replace("\", "/")

$params = @{
    PackageName = $package;
    VsixUrl     = $vsixUrl;
}

Install-VisualStudioVsixExtension @params
