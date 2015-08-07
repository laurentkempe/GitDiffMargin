try {
  $package = 'GitDiffMargin'

  $params = @{
    PackageName = $package;
    VsixUrl = 'https://visualstudiogallery.msdn.microsoft.com/cf49cf30-2ca6-4ea0-b7cc-6a8e0dadc1a8/file/101267/12/GitDiffMargin.vsix';
  }

  Install-ChocolateyVsixPackage @params

  Write-ChocolateySuccess $package
} catch {
  Write-ChocolateyFailure $package "$($_.Exception.Message)"
  throw
}
