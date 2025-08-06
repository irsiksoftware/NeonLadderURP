using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// ScriptableObject database for dialogue choices with dictionary lookup
    /// Create via Assets -> Create -> NeonLadder -> Dialogue Choice Database
    /// </summary>
    [CreateAssetMenu(fileName = "New Dialogue Choice Database", menuName = "NeonLadder/Dialogue Choice Database", order = 2)]
    public class DialogueChoiceDatabase : ScriptableObject
    {
        [Header("Database Configuration")]
        [Tooltip("Name of this choice database")]
        public string databaseName = "Dialogue Choices";
        
        [Tooltip("Version for tracking updates")]
        public string version = "1.0";

        [Header("Choice Categories")]
        [Tooltip("All dialogue choice sets organized by category")]
        public List<DialogueChoiceCategory> choiceCategories = new List<DialogueChoiceCategory>();

        [Header("Localization")]
        [Tooltip("Supported languages")]
        public string[] supportedLanguages = { "en", "zh-Hans", "ur" };

        // Runtime dictionary for fast lookups
        private Dictionary<string, DialogueChoiceSet> _choiceLookup;
        private Dictionary<string, List<DialogueChoiceSet>> _categoryLookup;

        public void OnEnable()
        {
            BuildLookupDictionaries();
        }

        public void OnValidate()
        {
            BuildLookupDictionaries();
        }

        /// <summary>
        /// Build lookup dictionaries for runtime performance
        /// </summary>
        private void BuildLookupDictionaries()
        {
            _choiceLookup = new Dictionary<string, DialogueChoiceSet>();
            _categoryLookup = new Dictionary<string, List<DialogueChoiceSet>>();

            foreach (var category in choiceCategories)
            {
                if (!_categoryLookup.ContainsKey(category.categoryName))
                {
                    _categoryLookup[category.categoryName] = new List<DialogueChoiceSet>();
                }

                foreach (var choiceSet in category.choiceSets)
                {
                    // Add to main lookup
                    string key = $"{category.categoryName}.{choiceSet.choiceSetName}";
                    _choiceLookup[key] = choiceSet;
                    
                    // Add to category lookup
                    _categoryLookup[category.categoryName].Add(choiceSet);
                }
            }
        }

        /// <summary>
        /// Get dialogue choices by key (category.choiceSetName)
        /// </summary>
        public DialogueChoiceSet GetChoiceSet(string key)
        {
            if (_choiceLookup == null) BuildLookupDictionaries();
            
            return _choiceLookup.TryGetValue(key, out var choiceSet) ? choiceSet : null;
        }

        /// <summary>
        /// Get all choice sets in a category
        /// </summary>
        public List<DialogueChoiceSet> GetChoicesByCategory(string categoryName)
        {
            if (_categoryLookup == null) BuildLookupDictionaries();
            
            return _categoryLookup.TryGetValue(categoryName, out var choices) ? choices : new List<DialogueChoiceSet>();
        }

        /// <summary>
        /// Get localized choices for a specific language
        /// </summary>
        public LocalizedDialogueChoice[] GetLocalizedChoices(string key, string languageCode = "en")
        {
            var choiceSet = GetChoiceSet(key);
            if (choiceSet == null) return new LocalizedDialogueChoice[0];

            return choiceSet.choices.Select(choice => new LocalizedDialogueChoice
            {
                text = choice.GetLocalizedText(languageCode),
                consequence = choice.consequence,
                requiredSkill = choice.requiredSkill,
                requiredLevel = choice.requiredLevel,
                isEnabled = choice.isEnabled
            }).ToArray();
        }

        /// <summary>
        /// Search for choice sets by name or tags
        /// </summary>
        public List<DialogueChoiceSet> SearchChoices(string searchTerm)
        {
            if (_choiceLookup == null) BuildLookupDictionaries();

            var results = new List<DialogueChoiceSet>();
            searchTerm = searchTerm.ToLower();

            foreach (var choiceSet in _choiceLookup.Values)
            {
                // Search in name
                if (choiceSet.choiceSetName.ToLower().Contains(searchTerm))
                {
                    results.Add(choiceSet);
                    continue;
                }

                // Search in tags
                if (choiceSet.tags.Any(tag => tag.ToLower().Contains(searchTerm)))
                {
                    results.Add(choiceSet);
                    continue;
                }

                // Search in choice text
                if (choiceSet.choices.Any(choice => 
                    choice.english.ToLower().Contains(searchTerm) ||
                    choice.chineseSimplified.ToLower().Contains(searchTerm) ||
                    choice.urdu.ToLower().Contains(searchTerm)))
                {
                    results.Add(choiceSet);
                }
            }

            return results;
        }

        /// <summary>
        /// Get all available keys for debugging
        /// </summary>
        public string[] GetAllKeys()
        {
            if (_choiceLookup == null) BuildLookupDictionaries();
            return _choiceLookup.Keys.ToArray();
        }

        /// <summary>
        /// Validate the database integrity
        /// </summary>
        [ContextMenu("Validate Database")]
        public void ValidateDatabase()
        {
            var issues = new List<string>();

            foreach (var category in choiceCategories)
            {
                if (string.IsNullOrEmpty(category.categoryName))
                {
                    issues.Add("Category with empty name found");
                    continue;
                }

                foreach (var choiceSet in category.choiceSets)
                {
                    if (string.IsNullOrEmpty(choiceSet.choiceSetName))
                    {
                        issues.Add($"Choice set with empty name in category '{category.categoryName}'");
                        continue;
                    }

                    foreach (var choice in choiceSet.choices)
                    {
                        if (string.IsNullOrEmpty(choice.english))
                        {
                            issues.Add($"Empty English text in '{category.categoryName}.{choiceSet.choiceSetName}'");
                        }
                        if (string.IsNullOrEmpty(choice.chineseSimplified))
                        {
                            issues.Add($"Empty Chinese text in '{category.categoryName}.{choiceSet.choiceSetName}'");
                        }
                        if (string.IsNullOrEmpty(choice.urdu))
                        {
                            issues.Add($"Empty Urdu text in '{category.categoryName}.{choiceSet.choiceSetName}'");
                        }
                    }
                }
            }

            if (issues.Count == 0)
            {
                Debug.Log("✅ Database validation passed - no issues found!");
            }
            else
            {
                Debug.LogWarning($"⚠️ Database validation found {issues.Count} issues:\n" + string.Join("\n", issues));
            }
        }
    }

    [System.Serializable]
    public class DialogueChoiceCategory
    {
        [Header("Category Info")]
        public string categoryName = "New Category";
        
        [TextArea(2, 3)]
        public string description = "";
        
        [Header("Choice Sets")]
        public List<DialogueChoiceSet> choiceSets = new List<DialogueChoiceSet>();
    }

    [System.Serializable]
    public class DialogueChoiceSet
    {
        [Header("Choice Set Info")]
        public string choiceSetName = "New Choice Set";
        
        [TextArea(2, 3)]
        public string description = "";
        
        [Header("Metadata")]
        public string[] tags = new string[0];
        
        [Header("Choices")]
        public List<MultiLanguageDialogueChoice> choices = new List<MultiLanguageDialogueChoice>();
    }

    [System.Serializable]
    public class MultiLanguageDialogueChoice
    {
        [Header("Localized Text")]
        [TextArea(2, 3)]
        public string english = "";
        
        [TextArea(2, 3)]
        public string chineseSimplified = "";
        
        [TextArea(2, 3)]
        public string urdu = "";

        [Header("Choice Properties")]
        public string consequence = "";
        public SkillType requiredSkill = SkillType.Intuition;
        public int requiredLevel = 0;
        public bool isEnabled = true;

        [Header("Rewards")]
        public DialogReward reward;

        public string GetLocalizedText(string languageCode)
        {
            return languageCode switch
            {
                "en" => english,
                "zh-Hans" => chineseSimplified,
                "ur" => urdu,
                _ => english
            };
        }
    }

    [System.Serializable]
    public class LocalizedDialogueChoice
    {
        public string text;
        public string consequence;
        public SkillType requiredSkill;
        public int requiredLevel;
        public bool isEnabled;
    }
}