# Git Diff Margin [![Build status](https://ci.appveyor.com/api/projects/status/n2j1hcqpdel0xj0c/branch/master?svg=true)](https://ci.appveyor.com/project/laurentkempe/gitdiffmargin/branch/master) [![Gitter](https://img.shields.io/gitter/room/inferred/freebuilder.svg?style=flat-square)](https://gitter.im/GitDiffMargin/Lobby)


Git Diff Margin displays live Git changes of the currently edited file on Visual Studio margin and scroll bar.

* Supports Visual Studio 2012 through Visual Studio 2015 and Visual Studio 2017 RC
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
* Show the diff in Visual Studio Diff window
* Navigate to previous/next change on the file using user defined keyboard shortcuts or the popup icons
* Support Visual Studio 2013 Dark, Light and Blue Theme
* Support zoom

![Screenshot](http://i1.visualstudiogallery.msdn.s-msft.com/cf49cf30-2ca6-4ea0-b7cc-6a8e0dadc1a8/image/file/142621/1/gitdiffmargin-preview.png)

Git Diff Margin version 3.2.2 is the latest release supporting Visual Studio 2010 it uses LibGit2Sharp v0.23.1 and is not being maintained. You can [download it here](https://github.com/laurentkempe/GitDiffMargin/releases/tag/v3.2.2).

## Installation

Grab it from inside of Visual Studio's Extension Manager, or via the [Extension Gallery link](https://marketplace.visualstudio.com/items?itemName=LaurentKempe.GitDiffMargin)

Or use the [Chocolatey installation](https://chocolatey.org/packages/GitDiffMargin)

## Video

You might see a little video on the [following page](https://www.flickr.com/photos/laurentkempe/14879945429/).

## Credits

Thanks to Sam Harwell [@sharwell](https://github.com/sharwell) for all the improvements

Thanks to Rick Sladkey [@ricksladkey](https://github.com/ricksladkey) for the fixes

Thanks to [@Iristyle](https://github.com/Iristyle) for the chocolatey package
 
Thanks to [@heinzbeinz](https://github.com/heinzbeinz) for the support of Visual Studio 15 preview

Thanks to [@jcansdale](https://github.com/jcansdale) for bugfix
