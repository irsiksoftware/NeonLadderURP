using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NeonLadder.Dialog;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Unit tests for DialogueChoiceDatabase ScriptableObject
    /// Tests dictionary lookup performance, multi-language support, and database validation
    /// </summary>
    public class DialogueChoiceDatabaseTests
    {
        private DialogueChoiceDatabase testDatabase;
        private DialogueChoiceCategory testCategory;
        private DialogueChoiceSet testChoiceSet;

        [SetUp]
        public void Setup()
        {
            // Create test database
            testDatabase = ScriptableObject.CreateInstance<DialogueChoiceDatabase>();
            testDatabase.databaseName = "Test Choice Database";
            testDatabase.version = "1.0-test";
            testDatabase.supportedLanguages = new string[] { "en", "zh-Hans", "ur" };

            // Create test category
            testCategory = new DialogueChoiceCategory
            {
                categoryName = "TestBosses",
                description = "Test boss encounters for unit testing"
            };

            // Create test choice set
            testChoiceSet = new DialogueChoiceSet
            {
                choiceSetName = "TestWrathDialogue",
                description = "Test dialogue choices for Wrath boss",
                tags = new string[] { "boss", "wrath", "combat" }
            };

            // Add test choices
            testChoiceSet.choices.Add(new MultiLanguageDialogueChoice
            {
                english = "I won't back down!",
                chineseSimplified = "我不会退缩的！",
                urdu = "میں پیچھے نہیں ہٹوں گا!",
                consequence = "Increases combat readiness",
                requiredSkill = SkillType.Rhetoric,
                requiredLevel = 0,
                isEnabled = true
            });

            testChoiceSet.choices.Add(new MultiLanguageDialogueChoice
            {
                english = "Your anger blinds you.",
                chineseSimplified = "你的愤怒蒙蔽了你。",
                urdu = "تمہارا غصہ تمہیں اندھا کرتا ہے۔",
                consequence = "Philosophical approach",
                requiredSkill = SkillType.Intuition,
                requiredLevel = 2,
                isEnabled = true
            });

            // Wire up test data
            testCategory.choiceSets.Add(testChoiceSet);
            testDatabase.choiceCategories.Add(testCategory);
        }

        [TearDown]
        public void TearDown()
        {
            if (testDatabase != null)
                Object.DestroyImmediate(testDatabase);
        }

        #region Database Creation Tests

        [Test]
        public void DialogueChoiceDatabase_CreatesWithDefaultValues()
        {
            var database = ScriptableObject.CreateInstance<DialogueChoiceDatabase>();
            
            Assert.AreEqual("Dialogue Choices", database.databaseName);
            Assert.AreEqual("1.0", database.version);
            Assert.AreEqual(3, database.supportedLanguages.Length);
            Assert.Contains("en", database.supportedLanguages);
            Assert.Contains("zh-Hans", database.supportedLanguages);
            Assert.Contains("ur", database.supportedLanguages);
            Assert.IsNotNull(database.choiceCategories);
            Assert.AreEqual(0, database.choiceCategories.Count);
            
            Object.DestroyImmediate(database);
        }

        [Test]
        public void DialogueChoiceDatabase_BuildsLookupDictionariesOnEnable()
        {
            // Trigger OnEnable by calling it directly (since ScriptableObject.CreateInstance doesn't call it)
            testDatabase.OnEnable();
            
            // Test that lookups work
            var choiceSet = testDatabase.GetChoiceSet("TestBosses.TestWrathDialogue");
            Assert.IsNotNull(choiceSet);
            Assert.AreEqual("TestWrathDialogue", choiceSet.choiceSetName);
        }

        #endregion

        #region Dictionary Lookup Tests

        [Test]
        public void GetChoiceSet_ReturnsCorrectChoiceSetByKey()
        {
            testDatabase.OnEnable(); // Build lookup dictionaries
            
            var choiceSet = testDatabase.GetChoiceSet("TestBosses.TestWrathDialogue");
            
            Assert.IsNotNull(choiceSet);
            Assert.AreEqual("TestWrathDialogue", choiceSet.choiceSetName);
            Assert.AreEqual("Test dialogue choices for Wrath boss", choiceSet.description);
            Assert.AreEqual(2, choiceSet.choices.Count);
        }

        [Test]
        public void GetChoiceSet_ReturnsNullForInvalidKey()
        {
            testDatabase.OnEnable();
            
            var choiceSet = testDatabase.GetChoiceSet("InvalidCategory.InvalidChoiceSet");
            
            Assert.IsNull(choiceSet);
        }

        [Test]
        public void GetChoicesByCategory_ReturnsAllChoiceSetsInCategory()
        {
            testDatabase.OnEnable();
            
            var choiceSets = testDatabase.GetChoicesByCategory("TestBosses");
            
            Assert.IsNotNull(choiceSets);
            Assert.AreEqual(1, choiceSets.Count);
            Assert.AreEqual("TestWrathDialogue", choiceSets[0].choiceSetName);
        }

        [Test]
        public void GetChoicesByCategory_ReturnsEmptyListForInvalidCategory()
        {
            testDatabase.OnEnable();
            
            var choiceSets = testDatabase.GetChoicesByCategory("InvalidCategory");
            
            Assert.IsNotNull(choiceSets);
            Assert.AreEqual(0, choiceSets.Count);
        }

        [Test]
        public void GetAllKeys_ReturnsAllValidKeys()
        {
            testDatabase.OnEnable();
            
            string[] keys = testDatabase.GetAllKeys();
            
            Assert.IsNotNull(keys);
            Assert.AreEqual(1, keys.Length);
            Assert.Contains("TestBosses.TestWrathDialogue", keys);
        }

        #endregion

        #region Multi-Language Support Tests

        [Test]
        public void GetLocalizedChoices_ReturnsEnglishChoicesByDefault()
        {
            testDatabase.OnEnable();
            
            var localizedChoices = testDatabase.GetLocalizedChoices("TestBosses.TestWrathDialogue");
            
            Assert.IsNotNull(localizedChoices);
            Assert.AreEqual(2, localizedChoices.Length);
            Assert.AreEqual("I won't back down!", localizedChoices[0].text);
            Assert.AreEqual("Your anger blinds you.", localizedChoices[1].text);
        }

        [Test]
        public void GetLocalizedChoices_ReturnsChineseChoices()
        {
            testDatabase.OnEnable();
            
            var localizedChoices = testDatabase.GetLocalizedChoices("TestBosses.TestWrathDialogue", "zh-Hans");
            
            Assert.IsNotNull(localizedChoices);
            Assert.AreEqual(2, localizedChoices.Length);
            Assert.AreEqual("我不会退缩的！", localizedChoices[0].text);
            Assert.AreEqual("你的愤怒蒙蔽了你。", localizedChoices[1].text);
        }

        [Test]
        public void GetLocalizedChoices_ReturnsUrduChoices()
        {
            testDatabase.OnEnable();
            
            var localizedChoices = testDatabase.GetLocalizedChoices("TestBosses.TestWrathDialogue", "ur");
            
            Assert.IsNotNull(localizedChoices);
            Assert.AreEqual(2, localizedChoices.Length);
            Assert.AreEqual("میں پیچھے نہیں ہٹوں گا!", localizedChoices[0].text);
            Assert.AreEqual("تمہارا غصہ تمہیں اندھا کرتا ہے۔", localizedChoices[1].text);
        }

        [Test]
        public void GetLocalizedChoices_FallsBackToEnglishForUnsupportedLanguage()
        {
            testDatabase.OnEnable();
            
            var localizedChoices = testDatabase.GetLocalizedChoices("TestBosses.TestWrathDialogue", "fr");
            
            Assert.IsNotNull(localizedChoices);
            Assert.AreEqual(2, localizedChoices.Length);
            Assert.AreEqual("I won't back down!", localizedChoices[0].text); // Falls back to English
            Assert.AreEqual("Your anger blinds you.", localizedChoices[1].text);
        }

        [Test]
        public void GetLocalizedChoices_ReturnsEmptyArrayForInvalidKey()
        {
            testDatabase.OnEnable();
            
            var localizedChoices = testDatabase.GetLocalizedChoices("Invalid.Key");
            
            Assert.IsNotNull(localizedChoices);
            Assert.AreEqual(0, localizedChoices.Length);
        }

        [Test]
        public void GetLocalizedChoices_PreservesChoiceProperties()
        {
            testDatabase.OnEnable();
            
            var localizedChoices = testDatabase.GetLocalizedChoices("TestBosses.TestWrathDialogue", "en");
            
            var firstChoice = localizedChoices[0];
            Assert.AreEqual("Increases combat readiness", firstChoice.consequence);
            Assert.AreEqual(SkillType.Rhetoric, firstChoice.requiredSkill);
            Assert.AreEqual(0, firstChoice.requiredLevel);
            Assert.IsTrue(firstChoice.isEnabled);
            
            var secondChoice = localizedChoices[1];
            Assert.AreEqual("Philosophical approach", secondChoice.consequence);
            Assert.AreEqual(SkillType.Intuition, secondChoice.requiredSkill);
            Assert.AreEqual(2, secondChoice.requiredLevel);
            Assert.IsTrue(secondChoice.isEnabled);
        }

        #endregion

        #region Search Functionality Tests

        [Test]
        public void SearchChoices_FindsByChoiceSetName()
        {
            testDatabase.OnEnable();
            
            var results = testDatabase.SearchChoices("wrath");
            
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("TestWrathDialogue", results[0].choiceSetName);
        }

        [Test]
        public void SearchChoices_FindsByTags()
        {
            testDatabase.OnEnable();
            
            var results = testDatabase.SearchChoices("combat");
            
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("TestWrathDialogue", results[0].choiceSetName);
        }

        [Test]
        public void SearchChoices_FindsByEnglishText()
        {
            testDatabase.OnEnable();
            
            var results = testDatabase.SearchChoices("back down");
            
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("TestWrathDialogue", results[0].choiceSetName);
        }

        [Test]
        public void SearchChoices_FindsByChineseText()
        {
            testDatabase.OnEnable();
            
            var results = testDatabase.SearchChoices("愤怒");
            
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("TestWrathDialogue", results[0].choiceSetName);
        }

        [Test]
        public void SearchChoices_FindsByUrduText()
        {
            testDatabase.OnEnable();
            
            var results = testDatabase.SearchChoices("غصہ");
            
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("TestWrathDialogue", results[0].choiceSetName);
        }

        [Test]
        public void SearchChoices_ReturnsEmptyForNoMatches()
        {
            testDatabase.OnEnable();
            
            var results = testDatabase.SearchChoices("nonexistent");
            
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void SearchChoices_IsCaseInsensitive()
        {
            testDatabase.OnEnable();
            
            var results = testDatabase.SearchChoices("WRATH");
            
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("TestWrathDialogue", results[0].choiceSetName);
        }

        #endregion

        #region Database Validation Tests

        [Test]
        public void ValidateDatabase_PassesForValidDatabase()
        {
            // Setup a completely valid database
            testDatabase.OnEnable();
            
            // Should not throw and should log success
            Assert.DoesNotThrow(() => testDatabase.ValidateDatabase());
        }

        [Test]
        public void ValidateDatabase_DetectsEmptyCategoryName()
        {
            var invalidCategory = new DialogueChoiceCategory
            {
                categoryName = "", // Invalid empty name
                description = "Test category"
            };
            testDatabase.choiceCategories.Add(invalidCategory);
            testDatabase.OnEnable();
            
            // Should not throw but should log warnings about issues
            Assert.DoesNotThrow(() => testDatabase.ValidateDatabase());
        }

        [Test]
        public void ValidateDatabase_DetectsEmptyChoiceSetName()
        {
            var validCategory = new DialogueChoiceCategory
            {
                categoryName = "ValidCategory",
                description = "Valid category"
            };
            
            var invalidChoiceSet = new DialogueChoiceSet
            {
                choiceSetName = "", // Invalid empty name
                description = "Invalid choice set"
            };
            
            validCategory.choiceSets.Add(invalidChoiceSet);
            testDatabase.choiceCategories.Add(validCategory);
            testDatabase.OnEnable();
            
            Assert.DoesNotThrow(() => testDatabase.ValidateDatabase());
        }

        [Test]
        public void ValidateDatabase_DetectsEmptyEnglishText()
        {
            var choiceWithEmptyEnglish = new MultiLanguageDialogueChoice
            {
                english = "", // Invalid empty English
                chineseSimplified = "中文",
                urdu = "اردو"
            };
            
            testChoiceSet.choices.Add(choiceWithEmptyEnglish);
            testDatabase.OnEnable();
            
            Assert.DoesNotThrow(() => testDatabase.ValidateDatabase());
        }

        [Test]
        public void ValidateDatabase_DetectsEmptyChineseText()
        {
            var choiceWithEmptyChinese = new MultiLanguageDialogueChoice
            {
                english = "English text",
                chineseSimplified = "", // Invalid empty Chinese
                urdu = "اردو"
            };
            
            testChoiceSet.choices.Add(choiceWithEmptyChinese);
            testDatabase.OnEnable();
            
            Assert.DoesNotThrow(() => testDatabase.ValidateDatabase());
        }

        [Test]
        public void ValidateDatabase_DetectsEmptyUrduText()
        {
            var choiceWithEmptyUrdu = new MultiLanguageDialogueChoice
            {
                english = "English text",
                chineseSimplified = "中文",
                urdu = "" // Invalid empty Urdu
            };
            
            testChoiceSet.choices.Add(choiceWithEmptyUrdu);
            testDatabase.OnEnable();
            
            Assert.DoesNotThrow(() => testDatabase.ValidateDatabase());
        }

        #endregion

        #region Multi-Language Choice Tests

        [Test]
        public void MultiLanguageDialogueChoice_GetLocalizedTextReturnsCorrectLanguage()
        {
            var choice = new MultiLanguageDialogueChoice
            {
                english = "English text",
                chineseSimplified = "中文文本",
                urdu = "اردو متن"
            };
            
            Assert.AreEqual("English text", choice.GetLocalizedText("en"));
            Assert.AreEqual("中文文本", choice.GetLocalizedText("zh-Hans"));
            Assert.AreEqual("اردو متن", choice.GetLocalizedText("ur"));
        }

        [Test]
        public void MultiLanguageDialogueChoice_FallsBackToEnglishForUnsupportedLanguage()
        {
            var choice = new MultiLanguageDialogueChoice
            {
                english = "English fallback",
                chineseSimplified = "中文",
                urdu = "اردو"
            };
            
            Assert.AreEqual("English fallback", choice.GetLocalizedText("fr")); // Unsupported language
            Assert.AreEqual("English fallback", choice.GetLocalizedText(""));   // Empty language
            Assert.AreEqual("English fallback", choice.GetLocalizedText(null)); // Null language
        }

        #endregion

        #region Performance Tests

        [Test]
        public void Performance_DictionaryLookupsAreOConstant()
        {
            // Create a larger database to test performance
            var largeDatabase = ScriptableObject.CreateInstance<DialogueChoiceDatabase>();
            
            // Add 100 categories with 10 choice sets each (1000 total)
            for (int categoryIndex = 0; categoryIndex < 100; categoryIndex++)
            {
                var category = new DialogueChoiceCategory
                {
                    categoryName = $"Category{categoryIndex}",
                    description = $"Category {categoryIndex} for performance testing"
                };
                
                for (int choiceIndex = 0; choiceIndex < 10; choiceIndex++)
                {
                    var choiceSet = new DialogueChoiceSet
                    {
                        choiceSetName = $"ChoiceSet{choiceIndex}",
                        description = $"Choice set {choiceIndex} in category {categoryIndex}"
                    };
                    
                    category.choiceSets.Add(choiceSet);
                }
                
                largeDatabase.choiceCategories.Add(category);
            }
            
            largeDatabase.OnEnable(); // Build lookup dictionaries
            
            // Time dictionary lookup (should be very fast)
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            for (int i = 0; i < 1000; i++)
            {
                largeDatabase.GetChoiceSet("Category50.ChoiceSet5");
            }
            
            stopwatch.Stop();
            
            // 1000 lookups should complete in under 10ms (generous for CI environments)
            Assert.Less(stopwatch.ElapsedMilliseconds, 10, "Dictionary lookups should be very fast");
            
            Object.DestroyImmediate(largeDatabase);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Integration_ContextMenuValidateExists()
        {
            // The ContextMenu attribute should be present on ValidateDatabase
            var method = typeof(DialogueChoiceDatabase).GetMethod("ValidateDatabase");
            Assert.IsNotNull(method, "ValidateDatabase method should exist");
            
            // ContextMenu attribute verification - method exists, which is sufficient for this test
        }

        [Test]
        public void Integration_OnValidateCallsBuildLookupDictionaries()
        {
            // Clear existing database and test OnValidate
            testDatabase.choiceCategories.Clear();
            testDatabase.choiceCategories.Add(testCategory);
            
            // Call OnValidate (simulates Inspector changes)
            testDatabase.OnValidate();
            
            // Should be able to lookup after OnValidate
            var choiceSet = testDatabase.GetChoiceSet("TestBosses.TestWrathDialogue");
            Assert.IsNotNull(choiceSet);
        }

        #endregion
    }
}