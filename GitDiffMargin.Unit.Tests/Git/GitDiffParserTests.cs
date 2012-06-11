﻿using System;
using System.Collections.Generic;
using System.Linq;
using GitDiffMargin.Git;
using NUnit.Framework;
using Shouldly;

namespace GitDiffMargin.Unit.Tests.Git
{
    // ReSharper disable InconsistentNaming 

    [TestFixture]
    public class Tests1
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

        private string FirstGitDiff = @"
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
+        bool IsDependencyThroughReference { get; }                                                                                              * 
";

        private string SecondGitDiff = @"
diff --git a/skye-editor/Sources/skyeEditor/Core/Model/Dependency/ModelDependency.cs b/skye-editor/Sources/skyeEditor/Core/Model/Dependency/ModelDependency.cs              
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
        
        private string EmptyGitDiff = "";

        private string ThirdGitDiff = @"
diff --git a/README.md b/README.md
index 8bb01f5..51495f9 100644
--- a/README.md
+++ b/README.md
@@ -1 +1 @@
-# Hubot
+# Hubot 2
";

        [Test]
        public void Parse_EmptyGitDiff_Expect0HunkRangeInfos()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(EmptyGitDiff);
            
            //Act
            var hunkRangeInfos = gitDiffParser.Parse().ToList();

            //Assert
            hunkRangeInfos.Count.ShouldBe(0);
        }

        [Test]
        public void Parse_WithOneHunk_ExpectHunkRanges()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(FirstGitDiff);
            
            //Act
            var hunkRanges = gitDiffParser.Parse().ToList();

            //Assert
            hunkRanges[0].OriginaleHunkRange.StartingLineNumber.ShouldBe(40);
            hunkRanges[0].OriginaleHunkRange.NumberOfLines.ShouldBe(0);
            hunkRanges[0].NewHunkRange.StartingLineNumber.ShouldBe(41);
            hunkRanges[0].NewHunkRange.NumberOfLines.ShouldBe(20);
        }
        
        [Test]
        public void Parse_WithOneHunkWithoutLineCount_ExpectHunkRanges()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(ThirdGitDiff);
            
            //Act
            var hunkRanges = gitDiffParser.Parse().ToList();

            //Assert
            hunkRanges[0].OriginaleHunkRange.StartingLineNumber.ShouldBe(0);
            hunkRanges[0].OriginaleHunkRange.NumberOfLines.ShouldBe(1);
            hunkRanges[0].NewHunkRange.StartingLineNumber.ShouldBe(0);
            hunkRanges[0].NewHunkRange.NumberOfLines.ShouldBe(1);
        }

        [Test]
        public void Parse_WithThreeHunk_ExpectHunkRanges()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(SecondGitDiff);
            
            //Act
            var hunkRanges = gitDiffParser.Parse().ToList();

            //Assert
            hunkRanges[0].OriginaleHunkRange.StartingLineNumber.ShouldBe(67);
            hunkRanges[0].OriginaleHunkRange.NumberOfLines.ShouldBe(2);
            hunkRanges[0].NewHunkRange.StartingLineNumber.ShouldBe(66);
            hunkRanges[0].NewHunkRange.NumberOfLines.ShouldBe(0);

            hunkRanges[1].OriginaleHunkRange.StartingLineNumber.ShouldBe(169);
            hunkRanges[1].OriginaleHunkRange.NumberOfLines.ShouldBe(0);
            hunkRanges[1].NewHunkRange.StartingLineNumber.ShouldBe(168);
            hunkRanges[1].NewHunkRange.NumberOfLines.ShouldBe(27);

            hunkRanges[2].OriginaleHunkRange.StartingLineNumber.ShouldBe(184);
            hunkRanges[2].OriginaleHunkRange.NumberOfLines.ShouldBe(2);
            hunkRanges[2].NewHunkRange.StartingLineNumber.ShouldBe(208);
            hunkRanges[2].NewHunkRange.NumberOfLines.ShouldBe(0);
        }

        [Test]
        public void GetUnifiedFormatHunkLine_WithOneHunk_ExpectHunkLine()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(FirstGitDiff);
            
            //Act
            var unifiedFormatHunk = gitDiffParser.GetUnifiedFormatHunkLines().ToList();

            //Assert
            unifiedFormatHunk[0].Item1.ShouldBe("@@ -41,0 +42,20 @@ namespace skyeEditor.Core.Model.Dependency");
        }

        [Test]
        public void GetUnifiedFormatHunkLine_WithTwoHunk_ExpectHunkLine()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(SecondGitDiff);
            
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
            var gitDiffParser = new GitDiffParser(FirstGitDiff);
            
            //Act
            string hunkOriginalFile = gitDiffParser.GetHunkNewFile(gitDiffParser.GetUnifiedFormatHunkLines().First().Item1);

            //Assert
            hunkOriginalFile.ShouldBe("42,20");
        }

        [Test]
        public void GetHunkNewFile_WithOneHunk_ExpectHunkNewFile()
        {
            //Arrange
            var gitDiffParser = new GitDiffParser(FirstGitDiff);
            
            //Act
            string hunkOriginalFile = gitDiffParser.GetHunkOriginalFile(gitDiffParser.GetUnifiedFormatHunkLines().First().Item1);

            //Assert
            hunkOriginalFile.ShouldBe("41,0");
        }
    }

    // ReSharper restore InconsistentNaming 
}
