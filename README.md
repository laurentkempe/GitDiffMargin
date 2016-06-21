# Git Diff Margin [![Build status](https://ci.appveyor.com/api/projects/status/42u2e3db3exo546p)](https://ci.appveyor.com/project/laurentkempe/gitdiffmargin)


Git Diff Margin displays live Git changes of the currently edited file on Visual Studio margin and scroll bar.

* Supports Visual Studio 2010 through Visual Studio 2015 and Visual Studio 15 preview
* Quickly view all current file changes on
    * Left margin
    * Scroll Bars in map and bar mode with and without source overview
        * blue rectangle for modifications
        * green rectangles for new lines
        * red triangles for deletions
        * all colors configurable through Visual Studio Fonts and Colors options
* Undo the change
* Copy the old code into the clipboard
* Copy a part of the old code by selecting it in the popup
* Show the diff in Visual Studio Diff window except for Visual Studio 2010 which still use configured Git external diff tool
* Navigate to previous/next change on the file using user defined keyboard shortcuts or the popup icons
* Support Visual Studio 2013 Dark, Light and Blue Theme
* Support zoom

![Screenshot](http://i1.visualstudiogallery.msdn.s-msft.com/cf49cf30-2ca6-4ea0-b7cc-6a8e0dadc1a8/image/file/142621/1/gitdiffmargin-preview.png)

Perfect companion of [Visual Studio Tools for Git](http://visualstudiogallery.msdn.microsoft.com/abafc7d6-dcaa-40f4-8a5e-d6724bdb980c)

## Installation

Grab it from inside of Visual Studio's Extension Manager, or via the [Extension Gallery link](http://visualstudiogallery.msdn.microsoft.com/cf49cf30-2ca6-4ea0-b7cc-6a8e0dadc1a8)

Or use the [Chocolatey installation](https://chocolatey.org/packages/GitDiffMargin)

## Video

You might see a little video on the [following page](https://www.flickr.com/photos/laurentkempe/14879945429/).

## Credits

Thanks to Sam Harwell [@sharwell](https://github.com/sharwell) for all the improvements

Thanks to Rick Sladkey [@ricksladkey](https://github.com/ricksladkey) for the fixes

Thanks to [@Iristyle](https://github.com/Iristyle) for the chocolatey package
 
Thanks to [@heinzbeinz](https://github.com/heinzbeinz) for the support of Visual Studio 15 preview
