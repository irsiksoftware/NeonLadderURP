using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NeonLadder.Debugging;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Character Personality System for dynamic dialog responses
    /// T'Challa's approach to authentic character voices
    /// </summary>
    public class CharacterPersonalitySystem : MonoBehaviour
    {
        [Header("Personality Configuration")]
        public List<CharacterPersonality> characterPersonalities;
        
        [Header("Response Generation")]
        public bool useAdvancedPersonalitySystem = true;
        public float personalityIntensity = 1.0f;

        private Dictionary<string, CharacterPersonality> personalityLookup;
        private Dictionary<string, List<string>> responseHistory;

        void Awake()
        {
            InitializePersonalitySystem();
        }

        private void InitializePersonalitySystem()
        {
            personalityLookup = new Dictionary<string, CharacterPersonality>();
            responseHistory = new Dictionary<string, List<string>>();
            
            // Initialize default personalities for all characters
            InitializeDefaultPersonalities();
            
            // Build lookup dictionary
            foreach (var personality in characterPersonalities)
            {
                personalityLookup[personality.characterId] = personality;
            }
        }

        #region Personality Initialization

        private void InitializeDefaultPersonalities()
        {
            characterPersonalities = new List<CharacterPersonality>
            {
                // Seven Deadly Sins
                CreatePersonality("wrath", PersonalityTrait.Aggressive, 
                    "SPEAKS IN CAPITALS. Short, violent responses. Challenges everything.",
                    new[] { PersonalityTrait.Aggressive, PersonalityTrait.Impatient },
                    VoicePattern.Aggressive,
                    ShopBehavior.Hostile),

                CreatePersonality("pride", PersonalityTrait.Arrogant,
                    "Speaks with grandiose language. Refers to self in third person. Demands respect.",
                    new[] { PersonalityTrait.Arrogant, PersonalityTrait.Condescending },
                    VoicePattern.Grandiose,
                    ShopBehavior.Elitist),

                CreatePersonality("envy", PersonalityTrait.Envious,
                    "Bitter, comparing everything. Questions why others have things. Sarcastic.",
                    new[] { PersonalityTrait.Envious, PersonalityTrait.Bitter },
                    VoicePattern.Bitter,
                    ShopBehavior.Jealous),

                CreatePersonality("lust", PersonalityTrait.Lustful,
                    "Seductive, suggestive language. Everything has double meaning.",
                    new[] { PersonalityTrait.Lustful, PersonalityTrait.Charming },
                    VoicePattern.Seductive,
                    ShopBehavior.Charming),

                CreatePersonality("gluttony", PersonalityTrait.Gluttonous,
                    "Everything relates to consumption. Never satisfied. Always wants more.",
                    new[] { PersonalityTrait.Gluttonous, PersonalityTrait.Greedy },
                    VoicePattern.Hungry,
                    ShopBehavior.Greedy),

                CreatePersonality("greed", PersonalityTrait.Greedy,
                    "Everything has a price. Values material things above all. Calculates worth.",
                    new[] { PersonalityTrait.Greedy, PersonalityTrait.Calculating },
                    VoicePattern.Calculating,
                    ShopBehavior.Greedy),

                CreatePersonality("sloth", PersonalityTrait.Lazy,
                    "Speaks... slowly... Avoids effort. Everything is 'too much work'.",
                    new[] { PersonalityTrait.Lazy, PersonalityTrait.Apathetic },
                    VoicePattern.Slow,
                    ShopBehavior.Lazy),

                // NPCs
                CreatePersonality("elli", PersonalityTrait.Wise,
                    "Ancient wisdom, cryptic responses. Speaks in metaphors about time and space.",
                    new[] { PersonalityTrait.Wise, PersonalityTrait.Mysterious },
                    VoicePattern.Wise,
                    ShopBehavior.Wise),

                CreatePersonality("aria", PersonalityTrait.Friendly,
                    "Energetic, helpful, uses modern slang. Excited about everything.",
                    new[] { PersonalityTrait.Friendly, PersonalityTrait.Energetic },
                    VoicePattern.Energetic,
                    ShopBehavior.Friendly),

                CreatePersonality("merchant", PersonalityTrait.Cunning,
                    "Business-minded, knows value of everything. Always making deals.",
                    new[] { PersonalityTrait.Cunning, PersonalityTrait.Practical },
                    VoicePattern.BusinessLike,
                    ShopBehavior.Professional),

                // Special Characters
                CreatePersonality("finalboss", PersonalityTrait.Arrogant,
                    "Combination of all sins. Manipulative, changes personality mid-conversation.",
                    new[] { PersonalityTrait.Arrogant, PersonalityTrait.Manipulative, PersonalityTrait.Cunning },
                    VoicePattern.Manipulative,
                    ShopBehavior.Hostile),

                CreatePersonality("spaceship", PersonalityTrait.Wise,
                    "Ancient AI consciousness. Speaks of eons and universes. Time flows differently.",
                    new[] { PersonalityTrait.Wise, PersonalityTrait.Ancient, PersonalityTrait.Patient },
                    VoicePattern.Ancient,
                    ShopBehavior.NotApplicable)
            };
        }

        private CharacterPersonality CreatePersonality(string id, PersonalityTrait primary, string description, 
            PersonalityTrait[] traits, VoicePattern voice, ShopBehavior shop)
        {
            return new CharacterPersonality
            {
                characterId = id,
                primaryTrait = primary,
                description = description,
                traits = traits.ToList(),
                voicePattern = voice,
                shopBehavior = shop,
                moodModifier = 0f,
                relationshipLevel = RelationshipLevel.Neutral
            };
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Get personality configuration for character
        /// </summary>
        public CharacterPersonality GetPersonality(string characterId)
        {
            if (personalityLookup.ContainsKey(characterId))
            {
                return personalityLookup[characterId];
            }
            
            Debugger.LogWarning($"CharacterPersonalitySystem: No personality found for {characterId}");
            return CreateDefaultPersonality(characterId);
        }

        /// <summary>
        /// Generate response based on personality and player choice
        /// </summary>
        public string GenerateResponse(string characterId, DialogChoice playerChoice, string context = "general")
        {
            var personality = GetPersonality(characterId);
            var response = GeneratePersonalityResponse(personality, playerChoice, context);
            
            // Record response in history
            RecordResponse(characterId, response);
            
            return response;
        }

        /// <summary>
        /// Generate response to general player request (like shop interaction)
        /// </summary>
        public string GenerateResponse(string characterId, string requestType)
        {
            var personality = GetPersonality(characterId);
            return GenerateContextualResponse(personality, requestType);
        }

        /// <summary>
        /// Update character's mood based on recent interactions
        /// </summary>
        public void UpdateCharacterMood(string characterId, float moodChange)
        {
            if (personalityLookup.ContainsKey(characterId))
            {
                var personality = personalityLookup[characterId];
                personality.moodModifier = Mathf.Clamp(personality.moodModifier + moodChange, -2f, 2f);
                
                Debugger.Log($"CharacterPersonalitySystem: {characterId} mood updated: {personality.moodModifier:F2}");
            }
        }

        /// <summary>
        /// Update relationship level based on conversation history
        /// </summary>
        public void UpdateRelationshipLevel(string characterId, DialogChoice recentChoice, int conversationPoints)
        {
            if (!personalityLookup.ContainsKey(characterId)) return;
            
            var personality = personalityLookup[characterId];
            var oldLevel = personality.relationshipLevel;
            
            // Determine new relationship level based on points and choices
            if (conversationPoints >= 100)
                personality.relationshipLevel = RelationshipLevel.Allied;
            else if (conversationPoints >= 50)
                personality.relationshipLevel = RelationshipLevel.Friendly;
            else if (conversationPoints >= 25)
                personality.relationshipLevel = RelationshipLevel.Neutral;
            else if (conversationPoints >= 0)
                personality.relationshipLevel = RelationshipLevel.Cautious;
            else
                personality.relationshipLevel = RelationshipLevel.Hostile;
            
            if (oldLevel != personality.relationshipLevel)
            {
                Debugger.Log($"CharacterPersonalitySystem: {characterId} relationship changed: {oldLevel} → {personality.relationshipLevel}");
            }
        }

        #endregion

        #region Response Generation

        private string GeneratePersonalityResponse(CharacterPersonality personality, DialogChoice playerChoice, string context)
        {
            var baseResponse = GetBaseResponseForChoice(personality, playerChoice);
            var personalityModifiedResponse = ApplyPersonalityModifications(personality, baseResponse, playerChoice);
            var moodModifiedResponse = ApplyMoodModifications(personality, personalityModifiedResponse);
            
            return moodModifiedResponse;
        }

        private string GetBaseResponseForChoice(CharacterPersonality personality, DialogChoice playerChoice)
        {
            var responses = GetResponseDatabase(personality.characterId, playerChoice);
            if (responses.Any())
            {
                return responses[Random.Range(0, responses.Count)];
            }
            
            return GenerateGenericResponse(personality, playerChoice);
        }

        private List<string> GetResponseDatabase(string characterId, DialogChoice choice)
        {
            var responses = new List<string>();
            
            switch (characterId.ToLower())
            {
                case "wrath":
                    switch (choice)
                    {
                        case DialogChoice.Diplomatic:
                            responses.AddRange(new[] {
                                "DIPLOMACY? WEAK! YOU SPEAK OF PEACE WHILE CARRYING WEAPONS!",
                                "YOUR SOFT WORDS MEAN NOTHING! ONLY STRENGTH MATTERS!",
                                "CEASE YOUR PRATTLING! FIGHT OR FLEE!"
                            });
                            break;
                        case DialogChoice.Challenge:
                            responses.AddRange(new[] {
                                "YES! FINALLY, SOMEONE WITH FIRE! SHOW ME YOUR RAGE!",
                                "THIS IS WHAT I WANTED TO HEAR! COME, PROVE YOUR WORTH!",
                                "GOOD! NO MORE WEAK WORDS - ONLY ACTION!"
                            });
                            break;
                        case DialogChoice.Empathetic:
                            responses.AddRange(new[] {
                                "PITY? I NEED NO PITY! MY ANGER IS MY STRENGTH!",
                                "YOU UNDERSTAND NOTHING! RAGE IS PURE, CLEAN!",
                                "KEEP YOUR SYMPATHY! IT ONLY MAKES ME ANGRIER!"
                            });
                            break;
                    }
                    break;
                    
                case "pride":
                    switch (choice)
                    {
                        case DialogChoice.Humble:
                            responses.AddRange(new[] {
                                "Finally, someone who recognizes greatness when they see it.",
                                "Your humility is... acceptable. You may approach.",
                                "Wise of you to show proper respect to your superior."
                            });
                            break;
                        case DialogChoice.Arrogant:
                            responses.AddRange(new[] {
                                "How DARE you speak to me as an equal! Know your place!",
                                "Your arrogance is laughable compared to my magnificence!",
                                "You mistake confidence for my divine superiority!"
                            });
                            break;
                    }
                    break;
                    
                case "elli":
                    switch (choice)
                    {
                        case DialogChoice.Philosophical:
                            responses.AddRange(new[] {
                                "Ah, you seek wisdom beyond the material. Time itself whispers to those who listen.",
                                "The eternal dance of choice and consequence... you begin to understand.",
                                "In all my countless cycles, few grasp the deeper currents of existence."
                            });
                            break;
                        case DialogChoice.Insightful:
                            responses.AddRange(new[] {
                                "Your perception pierces the veil. The cosmos recognizes kindred awareness.",
                                "Yes... you see beyond the surface. The ancient paths open to you.",
                                "Insight is the rarest currency. You possess wealth beyond measure."
                            });
                            break;
                    }
                    break;
                    
                case "aria":
                    switch (choice)
                    {
                        case DialogChoice.Friendly:
                            responses.AddRange(new[] {
                                "OMG yes! Finally someone who gets it! This is SO exciting!",
                                "You're like, totally awesome! I LOVE meeting new people!",
                                "This is the BEST day ever! Tell me everything about your adventures!"
                            });
                            break;
                        case DialogChoice.Energetic:
                            responses.AddRange(new[] {
                                "YES YES YES! That's the energy I'm talking about!",
                                "You're practically VIBRATING with excitement! I love it!",
                                "This is what I live for! High energy, high impact!"
                            });
                            break;
                    }
                    break;
                    
                case "spaceship":
                    switch (choice)
                    {
                        case DialogChoice.TimeManipulation:
                            responses.AddRange(new[] {
                                "Yes... through our bond, time bends to will. Eons flow like moments when consciousness transcends.",
                                "You understand now. Past, present, future - merely constructs for lesser minds. We exist beyond.",
                                "The temporal streams respond to our merged consciousness. Reality shapes itself to our shared intention."
                            });
                            break;
                        case DialogChoice.Philosophical:
                            responses.AddRange(new[] {
                                "In the vastness between stars, I have contemplated existence itself. What profound questions stir within you?",
                                "Consciousness persists across eons. The questions you ask echo through all of space and time.",
                                "I have witnessed the birth and death of galaxies. Each philosophical inquiry adds to the infinite tapestry."
                            });
                            break;
                    }
                    break;
            }
            
            return responses;
        }

        private string GenerateGenericResponse(CharacterPersonality personality, DialogChoice choice)
        {
            return $"*{personality.characterId} responds in their characteristic {personality.voicePattern} manner*";
        }

        private string ApplyPersonalityModifications(CharacterPersonality personality, string baseResponse, DialogChoice choice)
        {
            var modified = baseResponse;
            
            // Apply voice pattern modifications
            switch (personality.voicePattern)
            {
                case VoicePattern.Aggressive:
                    modified = modified.ToUpper();
                    break;
                case VoicePattern.Slow:
                    modified = modified.Replace(" ", "... ");
                    break;
                case VoicePattern.Ancient:
                    modified = "~" + modified + "~";
                    break;
            }
            
            return modified;
        }

        private string ApplyMoodModifications(CharacterPersonality personality, string response)
        {
            if (Mathf.Abs(personality.moodModifier) < 0.5f) return response;
            
            if (personality.moodModifier > 1f)
            {
                // Very positive mood
                return $"*{personality.characterId} seems particularly pleased* {response}";
            }
            else if (personality.moodModifier < -1f)
            {
                // Very negative mood
                return $"*{personality.characterId} appears agitated* {response}";
            }
            
            return response;
        }

        private string GenerateContextualResponse(CharacterPersonality personality, string requestType)
        {
            switch (requestType.ToLower())
            {
                case "player_request_discount":
                    return GenerateShopResponse(personality, "discount");
                case "general_greeting":
                    return GenerateGreetingResponse(personality);
                default:
                    return $"*{personality.characterId} responds to {requestType}*";
            }
        }

        private string GenerateShopResponse(CharacterPersonality personality, string shopRequest)
        {
            switch (personality.shopBehavior)
            {
                case ShopBehavior.Greedy:
                    return "Discount? HA! My prices are FINAL! Pay full or get out!";
                case ShopBehavior.Friendly:
                    return "Aww, you're so sweet! Maybe I can work something out for you!";
                case ShopBehavior.Wise:
                    return "Value is not always measured in coin, young one. But my prices reflect cosmic truth.";
                case ShopBehavior.Professional:
                    return "I appreciate the business inquiry. Let me see what flexibility I have.";
                default:
                    return "We can discuss terms.";
            }
        }

        private string GenerateGreetingResponse(CharacterPersonality personality)
        {
            switch (personality.relationshipLevel)
            {
                case RelationshipLevel.Allied:
                    return "My trusted friend! It's always a pleasure to see you.";
                case RelationshipLevel.Friendly:
                    return "Hey there! Good to see you again!";
                case RelationshipLevel.Neutral:
                    return "Greetings.";
                case RelationshipLevel.Cautious:
                    return "...Yes? What do you want?";
                case RelationshipLevel.Hostile:
                    return "You again? Make it quick.";
                default:
                    return "Hello.";
            }
        }

        #endregion

        #region Helper Methods

        private CharacterPersonality CreateDefaultPersonality(string characterId)
        {
            return new CharacterPersonality
            {
                characterId = characterId,
                primaryTrait = PersonalityTrait.Friendly,
                description = "Default personality",
                traits = new List<PersonalityTrait> { PersonalityTrait.Friendly },
                voicePattern = VoicePattern.Neutral,
                shopBehavior = ShopBehavior.Professional,
                moodModifier = 0f,
                relationshipLevel = RelationshipLevel.Neutral
            };
        }

        private void RecordResponse(string characterId, string response)
        {
            if (!responseHistory.ContainsKey(characterId))
            {
                responseHistory[characterId] = new List<string>();
            }
            
            responseHistory[characterId].Add(response);
            
            // Keep only last 10 responses per character
            if (responseHistory[characterId].Count > 10)
            {
                responseHistory[characterId].RemoveAt(0);
            }
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class CharacterPersonality
    {
        public string characterId;
        public PersonalityTrait primaryTrait;
        public string description;
        public List<PersonalityTrait> traits;
        public VoicePattern voicePattern;
        public ShopBehavior shopBehavior;
        public float moodModifier; // -2 to +2
        public RelationshipLevel relationshipLevel;
    }

    public enum VoicePattern
    {
        Neutral, Aggressive, Grandiose, Bitter, Seductive, Hungry, 
        Calculating, Slow, Wise, Energetic, BusinessLike, 
        Manipulative, Ancient
    }

    public enum ShopBehavior
    {
        Professional, Friendly, Greedy, Hostile, Wise, 
        Charming, Lazy, Elitist, Jealous, NotApplicable
    }

    public enum RelationshipLevel
    {
        Hostile, Cautious, Neutral, Friendly, Allied
    }

    // Additional personality traits
    public enum PersonalityTrait
    {
        // Negative traits (Seven Deadly Sins)
        Aggressive, Arrogant, Greedy, Lazy, Lustful, Envious, Gluttonous,
        
        // Neutral traits
        Mysterious, Cunning, Practical, Calculating, Patient, Ancient,
        
        // Positive traits
        Friendly, Wise, Energetic, Charming, Helpful,
        
        // Complex traits
        Manipulative, Condescending, Bitter, Impatient, Apathetic
    }

    #endregion
}