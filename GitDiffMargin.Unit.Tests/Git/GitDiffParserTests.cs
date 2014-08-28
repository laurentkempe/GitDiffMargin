using System;
using System.Collections.Generic;
using System.Linq;
using GitDiffMargin.Git;
using NUnit.Framework;
using Shouldly;

namespace GitDiffMargin.Unit.Tests.Git
{
    // ReSharper disable InconsistentNaming 

    [TestFixture]
    public class GitDiffParserTests
    {
        /*
        git diff --unified=0 Sources/skyeEditor/Core/Model/Dependency/IModelDependency.cs                                                  

         * 
diff --git a/skye-editor/Sources/skyeEditor/Core/Model/Dependency/IModelDependency.cs b/skye-editor/Sources/skyeEditor/Core/Model/Dependency/IModelDependency.cs            
index b8a4c69..e73b080 100644                                                                                                                                               
--- a/skye-editor/Sources/skyeEditor/Core/Model/Dependency/IModelDependency.cs                                                                                              
+++ b/skye-editor/Sources/skyeEditor/Core/Model/Dependency/IModelDependency.cs                                                                                              
@@ -41,0 +42,20 @@ namespace skyeEditor.Core.Model.Dependency                                                                                                               
+                                                                                                                                                                           
+        /// <summary>                                                                                                                                                      
+        /// Gets the supplier reference path.                                                                                                                              
+        /// <example>for ref[iBIZPartner.partnerSearchRef].iBIZPartnerSearch.firstName, returns iBIZPartner.partnerSearchRef</example>                                     
+        /// </summary>                                                                                                                                                     
+        string SupplierReferencePath { get; }                                                                                                                              
+                                                                                                                                                                           
+        /// <summary>                                                                                                                                                      
+        /// Gets the supplier reference path.                                                                                                                              
+        /// <example>for ref[iBIZPartner.partnerSearchRef].iBIZPartnerSearch.firstName, returns iBIZPartnerSearch.firstName</example>                                      
+        /// </summary>                                                                                                                                                     
+        string SupplierShortPath { get; }                                                                                                                                  
+                                                                                                                                                                           
+        /// <summary>                                                                                                                                                      
+        /// Gets a value indicating whether this instance is a dependency through reference.                                                                               
+        /// </summary>                                                                                                                                                     
+        /// <value>                                                                                                                                                        
+        ///    <c>true</c> if this instance is dependency through reference; otherwise, <c>false</c>.                                                                      
+        /// </value>                                                                                                                                                       
+        bool IsDependencyThroughReference { get; }                                                                                             
         */

        private const string FirstGitDiff = 
@"diff --git a/skye-editor/Sources/skyeEditor/Core/Model/Dependency/IModelDependency.cs b/skye-editor/Sources/skyeEditor/Core/Model/Dependency/IModelDependency.cs            
index b8a4c69..e73b080 100644                                                                                                                                               
--- a/skye-editor/Sources/skyeEditor/Core/Model/Dependency/IModelDependency.cs                                                                                              
+++ b/skye-editor/Sources/skyeEditor/Core/Model/Dependency/IModelDependency.cs                                                                                              
@@ -41,0 +42,20 @@ namespace skyeEditor.Core.Model.Dependency                                                                                                               
+                                                                                                                                                                           
+        /// <summary>                                                                                                                                                      
+        /// Gets the supplier reference path.                                                                                                                              
+        /// <example>for ref[iBIZPartner.partnerSearchRef].iBIZPartnerSearch.firstName, returns iBIZPartner.partnerSearchRef</example>                                     
+        /// </summary>                                                                                                                                                     
+        string SupplierReferencePath { get; }                                                                                                                              
+                                                                                                                                                                           
+        /// <summary>                                                                                                                                                      
+        /// Gets the supplier reference path.                                                                                                                              
+        /// <example>for ref[iBIZPartner.partnerSearchRef].iBIZPartnerSearch.firstName, returns iBIZPartnerSearch.firstName</example>                                      
+        /// </summary>                                                                                                                                                     
+        string SupplierShortPath { get; }                                                                                                                                  
+                                                                                                                                                                           
+        /// <summary>                                                                                                                                                      
+        /// Gets a value indicating whether this instance is a dependency through reference.                                                                               
+        /// </summary>                                                                                                                                                     
+        /// <value>                                                                                                                                                        
+        ///    <c>true</c> if this instance is dependency through reference; otherwise, <c>false</c>.                                                                      
+        /// </value>                                                                                                                                                       
+        bool IsDependencyThroughReference { get; }                                                                                              * 
";

        private const string SecondGitDiff = 
@"diff --git a/skye-editor/Sources/skyeEditor/Core/Model/Dependency/ModelDependency.cs b/skye-editor/Sources/skyeEditor/Core/Model/Dependency/ModelDependency.cs              
index 157e930..571aa23 100644                                                                                                                                               
--- a/skye-editor/Sources/skyeEditor/Core/Model/Dependency/ModelDependency.cs                                                                                               
+++ b/skye-editor/Sources/skyeEditor/Core/Model/Dependency/ModelDependency.cs                                                                                               
@@ -68,2 +67,0 @@ namespace skyeEditor.Core.Model.Dependency                                                                                                                
-        #region IModelDependency Members                                                                                                                                   
-                                                                                                                                                                           
@@ -170,0 +169,27 @@ namespace skyeEditor.Core.Model.Dependency                                                                                                             
+        public string SupplierShortPath                                                                                                                                    
+        {                                                                                                                                                                  
+            get                                                                                                                                                            
+            {                                                                                                                                                              
+                if (IsDependencyThroughReference)                                                                                                                          
+                {                                                                                                                                                          
+                    return SupplierReference.Split(new[] { ""ref["", ""]."" }, StringSplitOptions.RemoveEmptyEntries)[1];                                                      
+                }                                                                                                                                                          
+                                                                                                                                                                           
+                var splits = SupplierPath.Split('.').Reverse().ToArray();                                                                                                  
+                return splits[1] + ""."" + splits[0];                                                                                                                        
+            }                                                                                                                                                              
+        }                                                                                                                                                                  
+                                                                                                                                                                           
+        public bool IsDependencyThroughReference                                                                                                                           
+        {                                                                                                                                                                  
+            get { return Supplier == null && SupplierReference.StartsWith(""ref[""); }                                                                                       
+        }                                                                                                                                                                  
+                                                                                                                                                                           
+        public string SupplierReferencePath                                                                                                                                
+        {                                                                                                                                                                  
+            get                                                                                                                                                            
+            {                                                                                                                                                              
+                return _wrappedDependency.SupplierReference.Split(new[] {""ref["", ""].""}, StringSplitOptions.RemoveEmptyEntries)[0];                                         
+            }                                                                                                                                                              
+        }                                                                                                                                                                  
+                                                                                                                                                                           
@@ -185,2 +209,0 @@ namespace skyeEditor.Core.Model.Dependency                                                                                                              
-        #endregion                                                                                                                                                         
-                                                                                                                             
";

        private const string EmptyGitDiff = "";

        private const string ThirdGitDiff = 
@"diff --git a/README.md b/README.md
index 8bb01f5..51495f9 100644
--- a/README.md
+++ b/README.md
@@ -1 +1 @@
-# Hubot
+# Hubot 2
";

        private const string DiffOfADeleteOfThreeLines = 
@"diff --git a/note.txt b/note.txt
index e91ba58..e2dbef0 100644
--- a/note.txt
+++ b/note.txt
@@ -7,3 +6,0 @@ using GitDiffMargin.Git;
-using Microsoft.VisualStudio.Shell;
-using Microsoft.VisualStudio.Text;
-using Microsoft.VisualStudio.Text.Editor;";

        private const string DiffFromLibGit = 
@"diff --git a/ConsoleApplication1/Class1.cs b/ConsoleApplication1/Class1.cs
index 6f4c525..a42139b 100644
--- a/ConsoleApplication1/Class1.cs
+++ b/ConsoleApplication1/Class1.cs
@@ -5,2 +4,0 @@
-
-
@@ -9 +7 @@
-    class Class1
+    class Class2
@@ -11,0 +10,2 @@
+        {
+            int i = 100;
@@ -14,2 +13,0 @@
-        {
-            int i = 0;
@@ -17,0 +16 @@
+        //Hello
";

        [Test]
        public void Parse_DiffFromLibGit_Expect5HunkRangeInfos()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(DiffFromLibGit, 0);
            
            //Act
            var hunkRangeInfos = gitDiffParser.Parse().ToList();

            //Assert
            hunkRangeInfos.Count.ShouldBe(5);
        }

        [Test]
        public void Parse_DiffFromLibGit_ExpectFirstHunkRangeToBeDeletion()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(DiffFromLibGit, 0);
            
            //Act
            var hunkRangeInfos = gitDiffParser.Parse().ToList();

            //Assert
            hunkRangeInfos[0].IsDeletion.ShouldBe(true);
            hunkRangeInfos[0].IsAddition.ShouldBe(false);
            hunkRangeInfos[0].IsModification.ShouldBe(false);
        }

        [Test]
        public void Parse_DiffFromLibGit_ExpectSecondHunkRangeToBeModification()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(DiffFromLibGit, 0);
            
            //Act
            var hunkRangeInfos = gitDiffParser.Parse().ToList();

            //Assert
            hunkRangeInfos[1].IsDeletion.ShouldBe(false);
            hunkRangeInfos[1].IsAddition.ShouldBe(false);
            hunkRangeInfos[1].IsModification.ShouldBe(true);
        }

        [Test]
        public void Parse_DiffFromLibGit_ExpectSecondHunkRangeOriginalText()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(DiffFromLibGit, 0);
            
            //Act
            var hunkRangeInfos = gitDiffParser.Parse().ToList();

            //Assert
            hunkRangeInfos[1].OriginalText.ShouldBe(new List<string> {"    class Class1"});
        }

        [Test]
        public void Parse_DiffFromLibGit_ExpectThirdHunkRangeToBeAddition()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(DiffFromLibGit, 0);
            
            //Act
            var hunkRangeInfos = gitDiffParser.Parse().ToList();

            //Assert
            hunkRangeInfos[2].IsDeletion.ShouldBe(false);
            hunkRangeInfos[2].IsAddition.ShouldBe(true);
            hunkRangeInfos[2].IsModification.ShouldBe(false);
        }

        [Test]
        public void Parse_DiffFromLibGit_ExpectFourthHunkRangeToBeDeletion()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(DiffFromLibGit, 0);
            
            //Act
            var hunkRangeInfos = gitDiffParser.Parse().ToList();

            //Assert
            hunkRangeInfos[3].IsDeletion.ShouldBe(true);
            hunkRangeInfos[3].IsAddition.ShouldBe(false);
            hunkRangeInfos[3].IsModification.ShouldBe(false);
        }

        [Test]
        public void Parse_DiffFromLibGit_ExpectFifthHunkRangeToBeAddition()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(DiffFromLibGit, 0);
            
            //Act
            var hunkRangeInfos = gitDiffParser.Parse().ToList();

            //Assert
            hunkRangeInfos[4].IsDeletion.ShouldBe(false);
            hunkRangeInfos[4].IsAddition.ShouldBe(true);
            hunkRangeInfos[4].IsModification.ShouldBe(false);
        }

        [Test]
        public void Parse_EmptyGitDiff_Expect0HunkRangeInfos()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(EmptyGitDiff, 0);
            
            //Act
            var hunkRangeInfos = gitDiffParser.Parse().ToList();

            //Assert
            hunkRangeInfos.Count.ShouldBe(0);
        }

        [Test]
        public void Parse_WithOneHunk_ExpectHunkRanges()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(FirstGitDiff, 0);
            
            //Act
            var hunkRanges = gitDiffParser.Parse().ToList();

            //Assert
            hunkRanges[0].OriginalHunkRange.StartingLineNumber.ShouldBe(40);
            hunkRanges[0].OriginalHunkRange.NumberOfLines.ShouldBe(0);
            hunkRanges[0].NewHunkRange.StartingLineNumber.ShouldBe(41);
            hunkRanges[0].NewHunkRange.NumberOfLines.ShouldBe(20);
        }
        
        [Test]
        public void Parse_WithOneHunkWithoutLineCount_ExpectHunkRanges()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(ThirdGitDiff, 0);
            
            //Act
            var hunkRanges = gitDiffParser.Parse().ToList();

            //Assert
            hunkRanges[0].OriginalHunkRange.StartingLineNumber.ShouldBe(0);
            hunkRanges[0].OriginalHunkRange.NumberOfLines.ShouldBe(1);
            hunkRanges[0].NewHunkRange.StartingLineNumber.ShouldBe(0);
            hunkRanges[0].NewHunkRange.NumberOfLines.ShouldBe(1);
        }

        [Test]
        public void Parse_WithThreeHunk_ExpectHunkRanges()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(SecondGitDiff, 0);
            
            //Act
            var hunkRanges = gitDiffParser.Parse().ToList();

            //Assert
            hunkRanges[0].OriginalHunkRange.StartingLineNumber.ShouldBe(67);
            hunkRanges[0].OriginalHunkRange.NumberOfLines.ShouldBe(2);
            hunkRanges[0].NewHunkRange.StartingLineNumber.ShouldBe(66);
            hunkRanges[0].NewHunkRange.NumberOfLines.ShouldBe(0);

            hunkRanges[1].OriginalHunkRange.StartingLineNumber.ShouldBe(169);
            hunkRanges[1].OriginalHunkRange.NumberOfLines.ShouldBe(0);
            hunkRanges[1].NewHunkRange.StartingLineNumber.ShouldBe(168);
            hunkRanges[1].NewHunkRange.NumberOfLines.ShouldBe(27);

            hunkRanges[2].OriginalHunkRange.StartingLineNumber.ShouldBe(184);
            hunkRanges[2].OriginalHunkRange.NumberOfLines.ShouldBe(2);
            hunkRanges[2].NewHunkRange.StartingLineNumber.ShouldBe(208);
            hunkRanges[2].NewHunkRange.NumberOfLines.ShouldBe(0);
        }

        [Test]
        public void GetUnifiedFormatHunkLine_WithOneHunk_ExpectHunkLine()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(FirstGitDiff, 0);
            
            //Act
            var unifiedFormatHunk = gitDiffParser.GetUnifiedFormatHunkLines().ToList();

            //Assert
            unifiedFormatHunk[0].Item1.ShouldBe("@@ -41,0 +42,20 @@ namespace skyeEditor.Core.Model.Dependency");
        }

        [Test]
        public void GetUnifiedFormatHunkLine_DeleteDiff_ExpectedHunkLine()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(DiffOfADeleteOfThreeLines, 0);

            //Act
            var unifiedFormatHunk = gitDiffParser.GetUnifiedFormatHunkLines().ToList();

            //Assert
            unifiedFormatHunk[0].Item1.ShouldBe("@@ -7,3 +6,0 @@ using GitDiffMargin.Git;");
        }

        [Test]
        public void GetUnifiedFormatHunkLine_WithTwoHunk_ExpectHunkLine()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(SecondGitDiff, 0);
            
            //Act
            List<Tuple<string, IEnumerable<string>>> unifiedFormatHunk = gitDiffParser.GetUnifiedFormatHunkLines().ToList();

            //Assert
            unifiedFormatHunk[0].Item1.ShouldBe("@@ -68,2 +67,0 @@ namespace skyeEditor.Core.Model.Dependency");
            unifiedFormatHunk[1].Item1.ShouldBe("@@ -170,0 +169,27 @@ namespace skyeEditor.Core.Model.Dependency");
            unifiedFormatHunk[2].Item1.ShouldBe("@@ -185,2 +209,0 @@ namespace skyeEditor.Core.Model.Dependency");
        }

        [Test]
        public void GetHunkOriginalFile_WithOneHunk_ExpectHunkOriginalFile()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(FirstGitDiff, 0);
            
            //Act
            string hunkOriginalFile = gitDiffParser.GetHunkNewFile(gitDiffParser.GetUnifiedFormatHunkLines().First().Item1);

            //Assert
            hunkOriginalFile.ShouldBe("42,20");
        }

        [Test]
        public void GetHunkNewFile_WithOneHunk_ExpectHunkNewFile()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(FirstGitDiff, 0);
            
            //Act
            string hunkOriginalFile = gitDiffParser.GetHunkOriginalFile(gitDiffParser.GetUnifiedFormatHunkLines().First().Item1);

            //Assert
            hunkOriginalFile.ShouldBe("41,0");
        }

        [Test]
        public void GetHunkOriginalFile_DeleteDiff_ExpectHunkOriginalFile()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(DiffOfADeleteOfThreeLines, 0);

            //Act
            var hunkOriginalFile = gitDiffParser.GetHunkNewFile(gitDiffParser.GetUnifiedFormatHunkLines().First().Item1);

            //Assert
            hunkOriginalFile.ShouldBe("6,0");
        }

        [Test]
        public void GetHunkNewFile_DeleteDiff_ExpectHunkNewFile()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(DiffOfADeleteOfThreeLines, 0);

            //Act
            var hunkOriginalFile = gitDiffParser.GetHunkOriginalFile(gitDiffParser.GetUnifiedFormatHunkLines().First().Item1);

            //Assert
            hunkOriginalFile.ShouldBe("7,3");
        }
    }

    // ReSharper restore InconsistentNaming 
}
