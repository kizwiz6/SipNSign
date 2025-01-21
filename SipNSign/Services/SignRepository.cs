using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Models;
using System.Text.Json;

/// <summary>
/// Repository for managing sign data and video mappings
/// </summary>
public class SignRepository
{
    private readonly Dictionary<SignLanguage, List<SignModel>> _signsByLanguage;
    private readonly List<SignPack> _signPacks;
    private const string PURCHASED_PACKS_KEY = "purchased_packs";

    public SignRepository()
    {
        _signsByLanguage = new Dictionary<SignLanguage, List<SignModel>>
        {
            [SignLanguage.BSL] = InitializeBSLSigns()
        };

        _signPacks = InitializeSignPacks();
    }

    #region Properties
    /// <summary>
    /// Gets the total number of available signs for a specific language
    /// </summary>
    public int GetSignCount(SignLanguage language = SignLanguage.BSL) =>
           GetSigns(language).Count;

    public Dictionary<SignCategory, int> GetSignCountByCategory(SignLanguage language = SignLanguage.BSL)
    {
        var signs = GetSigns(language);
        return signs.GroupBy(s => s.Category)
                   .ToDictionary(g => g.Key, g => g.Count());
    }
    #endregion

    #region Public Methods
    public List<SignPack> GetSignPacks()
    {
        var purchasedPacks = GetPurchasedPacks();
        foreach (var pack in _signPacks)
        {
            pack.IsUnlocked = purchasedPacks.Contains(pack.Id);
        }
        return _signPacks;
    }


    /// <summary>
    /// Returns a list of all available signs with their video paths and multiple choice options
    /// for the specified language
    /// </summary>
    public List<SignModel> GetSigns(SignLanguage language = SignLanguage.BSL)
    {
        var allSigns = _signsByLanguage[language];
        var purchasedPacks = GetPurchasedPacks();
        var availableCategories = new HashSet<SignCategory>();

        // Get all categories from purchased packs
        foreach (var packId in purchasedPacks)
        {
            var pack = _signPacks.FirstOrDefault(p => p.Id == packId);
            if (pack != null)
            {
                foreach (var category in pack.Categories)
                {
                    availableCategories.Add(category);
                }
            }
        }

        // Filter signs by available categories
        return allSigns.Where(sign => availableCategories.Contains(sign.Category)).ToList();
    }

    /// <summary>
    /// Returns signs filtered by category for the specified language
    /// </summary>
    public List<SignModel> GetSignsByCategory(SignCategory category, SignLanguage language = SignLanguage.BSL)
    {
        var purchasedPacks = GetPurchasedPacks();
        var categoryPack = _signPacks.FirstOrDefault(p => p.Categories.Contains(category));

        // If the category's pack isn't purchased and it's not in the starter pack, return empty list
        if (categoryPack != null && !purchasedPacks.Contains(categoryPack.Id) && categoryPack.Id != "starter")
        {
            return new List<SignModel>();
        }

        // Return signs for the category
        return _signsByLanguage[language].Where(s => s.Category == category).ToList();
    }
    #endregion

    #region Private Methods
    private List<string> GetPurchasedPacks()
    {
        var json = Preferences.Get(PURCHASED_PACKS_KEY, "[]");
        var packs = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        packs.Add("starter"); // Starter pack is always available
        return packs;
    }
    private SignPack? GetPackForCategory(SignCategory category)
    {
        return _signPacks.FirstOrDefault(pack => pack.Categories.Contains(category));
    }
    private List<SignPack> InitializeSignPacks()
    {
        return new List<SignPack>
        {
            new SignPack
            {
                Id = "starter",
                Name = "Starter Pack",
                Description = "Basic signs including numbers, colours, and time - perfect for beginners!",
                Price = 0,
                Categories = new List<SignCategory>
                {
                    SignCategory.Colors,
                    SignCategory.Numbers,
                    SignCategory.Time,
                    SignCategory.Weather
                }
            },
            new SignPack
            {
                Id = "animals",
                Name = "Animals Pack",
                Description = "Signs for animals, nature and wildlife!",
                Price = 0.99m,
                Categories = new List<SignCategory>
                {
                    SignCategory.Animals,
                    SignCategory.Nature
                }
            },
            new SignPack
            {
                Id = "geography",
                Name = "Geography Pack",
                Description = "Travel the world with country and travel-related signs!",
                Price = 0.99m,
                Categories = new List<SignCategory>
                {
                    SignCategory.Countries,
                    SignCategory.Travel
                }
            },
            new SignPack
            {
                Id = "food_drink",
                Name = "Food & Drink Pack",
                Description = "Delicious signs for food, drinks, and dining!",
                Price = 0.99m,
                Categories = new List<SignCategory>
                {
                    SignCategory.Food,
                    SignCategory.Drinks
                }
            },
            new SignPack
            {
                Id = "emotions",
                Name = "Emotions Pack",
                Description = "Express yourself with signs for feelings and emotions!",
                Price = 0.99m,
                Categories = new List<SignCategory>
                {
                    SignCategory.Emotions,
                    SignCategory.Family,     // Include family with emotions
                    SignCategory.Occupations // And occupations as they're people-related
                }
            },
            new SignPack
            {
                Id = "hobbies",
                Name = "Sports & Hobbies Pack",
                Description = "Learn signs for sports, games, and leisure activities!",
                Price = 0.99m,
                Categories = new List<SignCategory>
                {
                    SignCategory.Sports,
                    SignCategory.Hobbies
                }
            }
        };
    }
    private List<SignModel> InitializeBSLSigns()
    {
        var signs = new List<SignModel>();

        // STARTER PACK (Free)
        #region STARTER PACK
            // Colors
            signs.AddRange(new[]
            {
            new SignModel { VideoPath = "black.mp4", Choices = new List<string> { "Black", "Blue", "Green", "Red" }, CorrectAnswer = "Black", Language = SignLanguage.BSL, Category = SignCategory.Colors },
            new SignModel { VideoPath = "blue.mp4", Choices = new List<string> { "Blue", "Black", "Yellow", "Green" }, CorrectAnswer = "Blue", Language = SignLanguage.BSL, Category = SignCategory.Colors },
            new SignModel { VideoPath = "brown.mp4", Choices = new List<string> { "Red", "Brown", "Orange", "Purple" }, CorrectAnswer = "Brown", Language = SignLanguage.BSL, Category = SignCategory.Colors },
            new SignModel { VideoPath = "green.mp4", Choices = new List<string> { "Blue", "Green", "Yellow", "Purple" }, CorrectAnswer = "Green", Language = SignLanguage.BSL, Category = SignCategory.Colors },
            new SignModel { VideoPath = "grey.mp4", Choices = new List<string> { "White", "Grey", "Black", "Silver" }, CorrectAnswer = "Grey", Language = SignLanguage.BSL, Category = SignCategory.Colors },
            new SignModel { VideoPath = "orange.mp4", Choices = new List<string> { "Orange", "Red", "Yellow", "Pink" }, CorrectAnswer = "Orange", Language = SignLanguage.BSL, Category = SignCategory.Colors },
            new SignModel { VideoPath = "pink.mp4", Choices = new List<string> { "Red", "Purple", "Pink", "Yellow" }, CorrectAnswer = "Pink", Language = SignLanguage.BSL, Category = SignCategory.Colors },
            new SignModel { VideoPath = "purple.mp4", Choices = new List<string> { "Violet", "Purple", "Blue", "Pink" }, CorrectAnswer = "Purple", Language = SignLanguage.BSL, Category = SignCategory.Colors },
            new SignModel { VideoPath = "red.mp4", Choices = new List<string> { "Red", "Pink", "Orange", "Yellow" }, CorrectAnswer = "Red", Language = SignLanguage.BSL, Category = SignCategory.Colors },
            new SignModel { VideoPath = "white.mp4", Choices = new List<string> { "Black", "White", "Grey", "Yellow" }, CorrectAnswer = "White", Language = SignLanguage.BSL, Category = SignCategory.Colors },
            new SignModel { VideoPath = "yellow.mp4", Choices = new List<string> { "Yellow", "Orange", "Gold", "Red" }, CorrectAnswer = "Yellow", Language = SignLanguage.BSL, Category = SignCategory.Colors },
            });

            // Time
            signs.AddRange(new[]
            {
            new SignModel { VideoPath = "age.mp4", Choices = new List<string> { "Young", "Old", "Age", "Time" }, CorrectAnswer = "Age", Language = SignLanguage.BSL, Category = SignCategory.Time },
            new SignModel { VideoPath = "autumn.mp4", Choices = new List<string> { "Winter", "Autumn", "Summer", "Spring" }, CorrectAnswer = "Autumn", Language = SignLanguage.BSL, Category = SignCategory.Time },
            new SignModel { VideoPath = "spring.mp4", Choices = new List<string> { "Winter", "Spring", "Autumn", "Summer" }, CorrectAnswer = "Spring", Language = SignLanguage.BSL, Category = SignCategory.Time },
            new SignModel { VideoPath = "summer.mp4", Choices = new List<string> { "Autumn", "Summer", "Winter", "Spring" }, CorrectAnswer = "Summer", Language = SignLanguage.BSL, Category = SignCategory.Time },
            new SignModel { VideoPath = "weekend.mp4", Choices = new List<string> { "Holiday", "Weekend", "Vacation", "Break" }, CorrectAnswer = "Weekend", Language = SignLanguage.BSL, Category = SignCategory.Time },
            new SignModel { VideoPath = "when.mp4", Choices = new List<string> { "When", "Where", "Why", "What" }, CorrectAnswer = "When", Language = SignLanguage.BSL, Category = SignCategory.Time },
            new SignModel { VideoPath = "winter.mp4", Choices = new List<string> { "Winter", "Summer", "Spring", "Autumn" }, CorrectAnswer = "Winter", Language = SignLanguage.BSL, Category = SignCategory.Time },
            });

            // Weather (part of starter pack)
            signs.AddRange(new[]
            {
            new SignModel { VideoPath = "hot.mp4", Choices = new List<string> { "Cool", "Cold", "Warm", "Hot" }, CorrectAnswer = "Hot", Language = SignLanguage.BSL, Category = SignCategory.Weather },
            new SignModel { VideoPath = "weather.mp4", Choices = new List<string> { "Weather", "Climate", "Rain", "Temperature" }, CorrectAnswer = "Weather", Language = SignLanguage.BSL, Category = SignCategory.Weather },
            new SignModel { VideoPath = "rain.mp4", Choices = new List<string> { "Storm", "Rain", "Drizzle", "Hail" }, CorrectAnswer = "Rain", Language = SignLanguage.BSL, Category = SignCategory.Weather },
            new SignModel { VideoPath = "storm.mp4", Choices = new List<string> { "Rain", "Storm", "Thunder", "Hurricane" }, CorrectAnswer = "Storm", Language = SignLanguage.BSL, Category = SignCategory.Weather },
            });
        #endregion

        #region Animals Pack
            // Animals
            signs.AddRange(new[]
            {
            new SignModel { VideoPath = "bird.mp4", Choices = new List<string> { "Bird", "Cat", "Dog", "Fish" }, CorrectAnswer = "Bird", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "dog.mp4", Choices = new List<string> { "Dog", "Cat", "Animal", "Bird" }, CorrectAnswer = "Dog", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "dolphin.mp4", Choices = new List<string> { "Whale", "Dolphin", "Shark", "Fish" }, CorrectAnswer = "Dolphin", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "donkey.mp4", Choices = new List<string> { "Donkey", "Horse", "Zebra", "Mule" }, CorrectAnswer = "Donkey", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "eagle.mp4", Choices = new List<string> { "Eagle", "Hawk", "Owl", "Falcon" }, CorrectAnswer = "Eagle", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "elephant.mp4", Choices = new List<string> { "Rhino", "Hippo", "Giraffe", "Elephant" }, CorrectAnswer = "Elephant", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "fish.mp4", Choices = new List<string> { "Fish", "Shark", "Dolphin", "Whale" }, CorrectAnswer = "Fish", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "giraffe.mp4", Choices = new List<string> { "Camel", "Zebra", "Elephant", "Giraffe" }, CorrectAnswer = "Giraffe", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "horse.mp4", Choices = new List<string> { "Donkey", "Horse", "Cow", "Sheep" }, CorrectAnswer = "Horse", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "Lion.mp4", Choices = new List<string> { "Tiger", "Leopard", "Lion", "Cheetah" }, CorrectAnswer = "Lion", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "monkey.mp4", Choices = new List<string> { "Monkey", "Gorilla", "Chimp", "Baboon" }, CorrectAnswer = "Monkey", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "penguin.mp4", Choices = new List<string> { "Seal", "Bird", "Penguin", "Duck" }, CorrectAnswer = "Penguin", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "pig.mp4", Choices = new List<string> { "Boar", "Sheep", "Cow", "Pig" }, CorrectAnswer = "Pig", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "pigeon.mp4", Choices = new List<string> { "Bird", "Crow", "Dove", "Pigeon" }, CorrectAnswer = "Pigeon", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "rabbit.mp4", Choices = new List<string> { "Hare", "Squirrel", "Deer", "Rabbit" }, CorrectAnswer = "Rabbit", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "snake.mp4", Choices = new List<string> { "Snake", "Lizard", "Frog", "Turtle" }, CorrectAnswer = "Snake", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            new SignModel { VideoPath = "tiger.mp4", Choices = new List<string> { "Cheetah", "Leopard", "Lion", "Tiger" }, CorrectAnswer = "Tiger", Language = SignLanguage.BSL, Category = SignCategory.Animals },
            });

            // Nature (part of Animals pack)
            signs.AddRange(new[]
            {
            new SignModel { VideoPath = "cloud.mp4", Choices = new List<string> { "Sky", "Cloud", "Rain", "Storm" }, CorrectAnswer = "Cloud", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "farm.mp4", Choices = new List<string> { "Farm", "Ranch", "Garden", "Field" }, CorrectAnswer = "Farm", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "fire.mp4", Choices = new List<string> { "Flame", "Fire", "Smoke", "Heat" }, CorrectAnswer = "Fire", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "flower.mp4", Choices = new List<string> { "Tree", "Flower", "Leaf", "Bush" }, CorrectAnswer = "Flower", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "forest.mp4", Choices = new List<string> { "Woods", "Jungle", "Forest", "Park" }, CorrectAnswer = "Forest", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "moon.mp4", Choices = new List<string> { "Moon", "Star", "Planet", "Sun" }, CorrectAnswer = "Moon", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "ocean.mp4", Choices = new List<string> { "Sea", "Ocean", "Lake", "River" }, CorrectAnswer = "Ocean", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "sea.mp4", Choices = new List<string> { "River", "Lake", "Ocean", "Sea" }, CorrectAnswer = "Sea", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "stone.mp4", Choices = new List<string> { "Rock", "Pebble", "Stone", "Boulder" }, CorrectAnswer = "Stone", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "sun.mp4", Choices = new List<string> { "Sun", "Moon", "Star", "Cloud" }, CorrectAnswer = "Sun", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "tree.mp4", Choices = new List<string> { "Plant", "Bush", "Tree", "Shrub" }, CorrectAnswer = "Tree", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            });
        #endregion

        #region Geography Pack
        // Countries
        signs.AddRange(new[]
        {
            new SignModel { VideoPath = "America.mp4", Choices = new List<string> { "America", "Canada", "Brazil", "Mexico" }, CorrectAnswer = "America", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Belgium.mp4", Choices = new List<string> { "Belgium", "Germany", "France", "Netherlands" }, CorrectAnswer = "Belgium", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "brazil.mp4", Choices = new List<string> { "Brazil", "Argentina", "Chile", "Peru" }, CorrectAnswer = "Brazil", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "China.mp4", Choices = new List<string> { "China", "Japan", "Korea", "Vietnam" }, CorrectAnswer = "China", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Croatia.mp4", Choices = new List<string> { "Croatia", "Italy", "Hungary", "Austria" }, CorrectAnswer = "Croatia", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Egypt.mp4", Choices = new List<string> { "Egypt", "Morocco", "Sudan", "Tunisia" }, CorrectAnswer = "Egypt", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "England.mp4", Choices = new List<string> { "Ireland", "Wales", "England", "Scotland" }, CorrectAnswer = "England", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Finland.mp4", Choices = new List<string> { "Norway", "Sweden", "Finland", "Denmark" }, CorrectAnswer = "Finland", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "France.mp4", Choices = new List<string> { "Spain", "Italy", "France", "Germany" }, CorrectAnswer = "France", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Germany.mp4", Choices = new List<string> { "Germany", "Austria", "Belgium", "Switzerland" }, CorrectAnswer = "Germany", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Holland.mp4", Choices = new List<string> { "Holland", "Belgium", "Denmark", "Luxembourg" }, CorrectAnswer = "Holland", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "India.mp4", Choices = new List<string> { "India", "Pakistan", "Bangladesh", "Sri Lanka" }, CorrectAnswer = "India", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Ireland.mp4", Choices = new List<string> { "Scotland", "Ireland", "Wales", "England" }, CorrectAnswer = "Ireland", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Japan.mp4", Choices = new List<string> { "Korea", "China", "Japan", "Vietnam" }, CorrectAnswer = "Japan", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Norway.mp4", Choices = new List<string> { "Sweden", "Norway", "Finland", "Denmark" }, CorrectAnswer = "Norway", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Poland.mp4", Choices = new List<string> { "Poland", "Germany", "Czech", "Hungary" }, CorrectAnswer = "Poland", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Portugal.mp4", Choices = new List<string> { "Portugal", "Spain", "Italy", "France" }, CorrectAnswer = "Portugal", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Scotland.mp4", Choices = new List<string> { "Wales", "England", "Ireland", "Scotland" }, CorrectAnswer = "Scotland", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Spain.mp4", Choices = new List<string> { "Portugal", "Spain", "Italy", "France" }, CorrectAnswer = "Spain", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Sweden.mp4", Choices = new List<string> { "Norway", "Denmark", "Sweden", "Finland" }, CorrectAnswer = "Sweden", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Switzerland.mp4", Choices = new List<string> { "Switzerland", "Germany", "Austria", "France" }, CorrectAnswer = "Switzerland", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Thailand.mp4", Choices = new List<string> { "Thailand", "Vietnam", "Cambodia", "Laos" }, CorrectAnswer = "Thailand", Language = SignLanguage.BSL, Category = SignCategory.Countries },
            new SignModel { VideoPath = "Wales.mp4", Choices = new List<string> { "Wales", "Scotland", "Ireland", "England" }, CorrectAnswer = "Wales", Language = SignLanguage.BSL, Category = SignCategory.Countries },

            // Travel
            new SignModel { VideoPath = "bicycle.mp4", Choices = new List<string> { "Car", "Bus", "Motorcycle", "Bicycle" }, CorrectAnswer = "Bicycle", Language = SignLanguage.BSL, Category = SignCategory.Travel },
            new SignModel { VideoPath = "bus.mp4", Choices = new List<string> { "Bus", "Train", "Car", "Taxi" }, CorrectAnswer = "Bus", Language = SignLanguage.BSL, Category = SignCategory.Travel },
            new SignModel { VideoPath = "car.mp4", Choices = new List<string> { "Bicycle", "Car", "Motorcycle", "Bus" }, CorrectAnswer = "Car", Language = SignLanguage.BSL, Category = SignCategory.Travel },
            new SignModel { VideoPath = "travel.mp4", Choices = new List<string> { "Journey", "Travel", "Vacation", "Tour" }, CorrectAnswer = "Travel", Language = SignLanguage.BSL, Category = SignCategory.Travel },
            new SignModel { VideoPath = "walk.mp4", Choices = new List<string> { "Walk", "Run", "Stroll", "March" }, CorrectAnswer = "Walk", Language = SignLanguage.BSL, Category = SignCategory.Travel },
        #endregion

        #region FOOD & DRINK PACK
            // Food
            new SignModel { VideoPath = "apple.mp4", Choices = new List<string> { "Orange", "Banana", "Apple", "Grapes" }, CorrectAnswer = "Apple", Language = SignLanguage.BSL, Category = SignCategory.Food },
            new SignModel { VideoPath = "banana.mp4", Choices = new List<string> { "Apple", "Grapes", "Banana", "Orange" }, CorrectAnswer = "Banana", Language = SignLanguage.BSL, Category = SignCategory.Food },
            new SignModel { VideoPath = "breakfast.mp4", Choices = new List<string> { "Lunch", "Dinner", "Breakfast", "Snack" }, CorrectAnswer = "Breakfast", Language = SignLanguage.BSL, Category = SignCategory.Food },
            new SignModel { VideoPath = "cheese.mp4", Choices = new List<string> { "Butter", "Cheese", "Milk", "Yogurt" }, CorrectAnswer = "Cheese", Language = SignLanguage.BSL, Category = SignCategory.Food },
            new SignModel { VideoPath = "chicken.mp4", Choices = new List<string> { "Chicken", "Fish", "Pork", "Beef" }, CorrectAnswer = "Chicken", Language = SignLanguage.BSL, Category = SignCategory.Food },
            new SignModel { VideoPath = "dinner.mp4", Choices = new List<string> { "Dinner", "Lunch", "Snack", "Breakfast" }, CorrectAnswer = "Dinner", Language = SignLanguage.BSL, Category = SignCategory.Food },
            new SignModel { VideoPath = "food.mp4", Choices = new List<string> { "Drink", "Meal", "Food", "Snack" }, CorrectAnswer = "Food", Language = SignLanguage.BSL, Category = SignCategory.Food },
            new SignModel { VideoPath = "fruit.mp4", Choices = new List<string> { "Vegetable", "Fruit", "Meat", "Grain" }, CorrectAnswer = "Fruit", Language = SignLanguage.BSL, Category = SignCategory.Food },
            new SignModel { VideoPath = "lunch.mp4", Choices = new List<string> { "Lunch", "Dinner", "Breakfast", "Brunch" }, CorrectAnswer = "Lunch", Language = SignLanguage.BSL, Category = SignCategory.Food },
            new SignModel { VideoPath = "meat.mp4", Choices = new List<string> { "Fish", "Vegetable", "Meat", "Grain" }, CorrectAnswer = "Meat", Language = SignLanguage.BSL, Category = SignCategory.Food },
            new SignModel { VideoPath = "pizza.mp4", Choices = new List<string> { "Sandwich", "Pizza", "Burger", "Pasta" }, CorrectAnswer = "Pizza", Language = SignLanguage.BSL, Category = SignCategory.Food },
            new SignModel { VideoPath = "sandwich.mp4", Choices = new List<string> { "Burger", "Sandwich", "Pizza", "Taco" }, CorrectAnswer = "Sandwich", Language = SignLanguage.BSL, Category = SignCategory.Food },
            new SignModel { VideoPath = "soup.mp4", Choices = new List<string> { "Stew", "Soup", "Broth", "Chowder" }, CorrectAnswer = "Soup", Language = SignLanguage.BSL, Category = SignCategory.Food },

            // Drinks
            new SignModel { VideoPath = "beer.mp4", Choices = new List<string> { "Wine", "Beer", "Juice", "Water" }, CorrectAnswer = "Beer", Language = SignLanguage.BSL, Category = SignCategory.Drinks },
            new SignModel { VideoPath = "drink.mp4", Choices = new List<string> { "Eat", "Drink", "Cook", "Pour" }, CorrectAnswer = "Drink", Language = SignLanguage.BSL, Category = SignCategory.Drinks },
            new SignModel { VideoPath = "milk.mp4", Choices = new List<string> { "Water", "Juice", "Milk", "Coffee" }, CorrectAnswer = "Milk", Language = SignLanguage.BSL, Category = SignCategory.Drinks },
            new SignModel { VideoPath = "vodka.mp4", Choices = new List<string> { "Vodka", "Whiskey", "Rum", "Tequila" }, CorrectAnswer = "Vodka", Language = SignLanguage.BSL, Category = SignCategory.Drinks },
            new SignModel { VideoPath = "water.mp4", Choices = new List<string> { "Juice", "Tea", "Water", "Milk" }, CorrectAnswer = "Water", Language = SignLanguage.BSL, Category = SignCategory.Drinks },
        #endregion

        #region EMOTIONS PACK
            // Emotions
            new SignModel { VideoPath = "confused.mp4", Choices = new List<string> { "Sad", "Happy", "Angry", "Confused" }, CorrectAnswer = "Confused", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
            new SignModel { VideoPath = "evil.mp4", Choices = new List<string> { "Good", "Kind", "Bad", "Evil" }, CorrectAnswer = "Evil", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
            new SignModel { VideoPath = "heart.mp4", Choices = new List<string> { "Heart", "Love", "Life", "Feel" }, CorrectAnswer = "Heart", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
            new SignModel { VideoPath = "horrible.mp4", Choices = new List<string> { "Great", "Terrible", "Bad", "Horrible" }, CorrectAnswer = "Horrible", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
            new SignModel { VideoPath = "love.mp4", Choices = new List<string> { "Love", "Hate", "Like", "Adore" }, CorrectAnswer = "Love", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
            new SignModel { VideoPath = "miserable.mp4", Choices = new List<string> { "Unhappy", "Miserable", "Sad", "Upset" }, CorrectAnswer = "Miserable", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
            new SignModel { VideoPath = "poor.mp4", Choices = new List<string> { "Broke", "Wealthy", "Poor", "Rich" }, CorrectAnswer = "Poor", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
            new SignModel { VideoPath = "posh.mp4", Choices = new List<string> { "Fancy", "Elegant", "Posh", "Simple" }, CorrectAnswer = "Posh", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
            new SignModel { VideoPath = "shy.mp4", Choices = new List<string> { "Shy", "Afraid", "Nervous", "Quiet" }, CorrectAnswer = "Shy", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
            new SignModel { VideoPath = "smile.mp4", Choices = new List<string> { "Grin", "Smile", "Laugh", "Happy" }, CorrectAnswer = "Smile", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
            new SignModel { VideoPath = "sympathy.mp4", Choices = new List<string> { "Sympathy", "Empathy", "Kindness", "Understanding" }, CorrectAnswer = "Sympathy", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
            new SignModel { VideoPath = "tired.mp4", Choices = new List<string> { "Sleepy", "Exhausted", "Tired", "Weary" }, CorrectAnswer = "Tired", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
            new SignModel { VideoPath = "ugly.mp4", Choices = new List<string> { "Pretty", "Unattractive", "Beautiful", "Ugly" }, CorrectAnswer = "Ugly", Language = SignLanguage.BSL, Category = SignCategory.Emotions },

            // Family (part of Emotions pack)
            new SignModel { VideoPath = "aunt.mp4", Choices = new List<string> { "Sister", "Mother", "Aunt", "Grandmother" }, CorrectAnswer = "Aunt", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "brother.mp4", Choices = new List<string> { "Father", "Brother", "Friend", "Sister" }, CorrectAnswer = "Brother", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "daughter.mp4", Choices = new List<string> { "Son", "Mother", "Daughter", "Sister" }, CorrectAnswer = "Daughter", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "family.mp4", Choices = new List<string> { "Group", "Neighbours", "Family", "Friends" }, CorrectAnswer = "Family", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "girl.mp4", Choices = new List<string> { "Boy", "Girl", "Woman", "Child" }, CorrectAnswer = "Girl", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "parents.mp4", Choices = new List<string> { "Relatives", "Siblings", "Family", "Parents" }, CorrectAnswer = "Parents", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "sister.mp4", Choices = new List<string> { "Cousin", "Brother", "Sibling", "Sister" }, CorrectAnswer = "Sister", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "son.mp4", Choices = new List<string> { "Son", "Daughter", "Child", "Boy" }, CorrectAnswer = "Son", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "uncle.mp4", Choices = new List<string> { "Uncle", "Aunt", "Cousin", "Brother" }, CorrectAnswer = "Uncle", Language = SignLanguage.BSL, Category = SignCategory.Family },
        #endregion

        #region OCCUPATIONS
            new SignModel { VideoPath = "actor.mp4", Choices = new List<string> { "Actor", "Performer", "Singer", "Dancer" }, CorrectAnswer = "Actor", Language = SignLanguage.BSL, Category = SignCategory.Occupations },
            new SignModel { VideoPath = "boss.mp4", Choices = new List<string> { "Manager", "Boss", "Worker", "Assistant" }, CorrectAnswer = "Boss", Language = SignLanguage.BSL, Category = SignCategory.Occupations },
            new SignModel { VideoPath = "chef.mp4", Choices = new List<string> { "Chef", "Waiter", "Cook", "Baker" }, CorrectAnswer = "Chef", Language = SignLanguage.BSL, Category = SignCategory.Occupations },
            new SignModel { VideoPath = "police.mp4", Choices = new List<string> { "Guard", "Officer", "Law", "Police" }, CorrectAnswer = "Police", Language = SignLanguage.BSL, Category = SignCategory.Occupations },
            new SignModel { VideoPath = "work.mp4", Choices = new List<string> { "Work", "Job", "Task", "Duty" }, CorrectAnswer = "Work", Language = SignLanguage.BSL, Category = SignCategory.Occupations },
            new SignModel { VideoPath = "bank.mp4", Choices = new List<string> { "Bank", "Store", "Office", "Shop" }, CorrectAnswer = "Bank", Language = SignLanguage.BSL, Category = SignCategory.Occupations },
            #endregion

        #region SPORTS AND HOBBIES
            // Already have videos
            new SignModel { VideoPath = "football2.mp4", Choices = new List<string> { "Football", "Soccer", "Rugby", "Sport" }, CorrectAnswer = "Football", Language = SignLanguage.BSL, Category = SignCategory.Sports },
            new SignModel { VideoPath = "dance.mp4", Choices = new List<string> { "Dance", "Move", "Jump", "Exercise" }, CorrectAnswer = "Dance", Language = SignLanguage.BSL, Category = SignCategory.Sports },
            new SignModel { VideoPath = "chess.mp4", Choices = new List<string> { "Chess", "Game", "Board", "Think" }, CorrectAnswer = "Chess", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            new SignModel { VideoPath = "guitar.mp4", Choices = new List<string> { "Guitar", "Music", "Play", "Instrument" }, CorrectAnswer = "Guitar", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            new SignModel { VideoPath = "game.mp4", Choices = new List<string> { "Game", "Play", "Fun", "Sport" }, CorrectAnswer = "Game", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },

            //// Sports - Future videos
            //new SignModel { VideoPath = "basketball.mp4", Choices = new List<string> { "Basketball", "Sport", "Ball", "Game" }, CorrectAnswer = "Basketball", Language = SignLanguage.BSL, Category = SignCategory.Sports },
            //new SignModel { VideoPath = "tennis.mp4", Choices = new List<string> { "Tennis", "Sport", "Racket", "Game" }, CorrectAnswer = "Tennis", Language = SignLanguage.BSL, Category = SignCategory.Sports },
            //new SignModel { VideoPath = "swimming.mp4", Choices = new List<string> { "Swimming", "Sport", "Water", "Pool" }, CorrectAnswer = "Swimming", Language = SignLanguage.BSL, Category = SignCategory.Sports },
            //new SignModel { VideoPath = "running.mp4", Choices = new List<string> { "Running", "Sprint", "Jog", "Exercise" }, CorrectAnswer = "Running", Language = SignLanguage.BSL, Category = SignCategory.Sports },
            //new SignModel { VideoPath = "cycling.mp4", Choices = new List<string> { "Cycling", "Bike", "Sport", "Exercise" }, CorrectAnswer = "Cycling", Language = SignLanguage.BSL, Category = SignCategory.Sports },
            //new SignModel { VideoPath = "boxing.mp4", Choices = new List<string> { "Boxing", "Fight", "Sport", "Punch" }, CorrectAnswer = "Boxing", Language = SignLanguage.BSL, Category = SignCategory.Sports },
            //new SignModel { VideoPath = "yoga.mp4", Choices = new List<string> { "Yoga", "Exercise", "Stretch", "Meditate" }, CorrectAnswer = "Yoga", Language = SignLanguage.BSL, Category = SignCategory.Sports },
            //new SignModel { VideoPath = "golf.mp4", Choices = new List<string> { "Golf", "Sport", "Club", "Ball" }, CorrectAnswer = "Golf", Language = SignLanguage.BSL, Category = SignCategory.Sports },
            //new SignModel { VideoPath = "skiing.mp4", Choices = new List<string> { "Skiing", "Snow", "Sport", "Winter" }, CorrectAnswer = "Skiing", Language = SignLanguage.BSL, Category = SignCategory.Sports },
            //new SignModel { VideoPath = "volleyball.mp4", Choices = new List<string> { "Volleyball", "Sport", "Ball", "Game" }, CorrectAnswer = "Volleyball", Language = SignLanguage.BSL, Category = SignCategory.Sports },
            //new SignModel { VideoPath = "baseball.mp4", Choices = new List<string> { "Baseball", "Sport", "Ball", "Game" }, CorrectAnswer = "Baseball", Language = SignLanguage.BSL, Category = SignCategory.Sports },
            //new SignModel { VideoPath = "skating.mp4", Choices = new List<string> { "Skating", "Ice", "Sport", "Roll" }, CorrectAnswer = "Skating", Language = SignLanguage.BSL, Category = SignCategory.Sports },

            //// Hobbies - Future videos
            //new SignModel { VideoPath = "painting.mp4", Choices = new List<string> { "Painting", "Art", "Draw", "Create" }, CorrectAnswer = "Painting", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "reading.mp4", Choices = new List<string> { "Reading", "Book", "Study", "Learn" }, CorrectAnswer = "Reading", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "writing.mp4", Choices = new List<string> { "Writing", "Pen", "Story", "Create" }, CorrectAnswer = "Writing", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "photography.mp4", Choices = new List<string> { "Photography", "Camera", "Photo", "Picture" }, CorrectAnswer = "Photography", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "gardening.mp4", Choices = new List<string> { "Gardening", "Plants", "Grow", "Garden" }, CorrectAnswer = "Gardening", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "fishing.mp4", Choices = new List<string> { "Fishing", "Fish", "Catch", "Rod" }, CorrectAnswer = "Fishing", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "camping.mp4", Choices = new List<string> { "Camping", "Tent", "Outdoors", "Nature" }, CorrectAnswer = "Camping", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "singing.mp4", Choices = new List<string> { "Singing", "Song", "Music", "Voice" }, CorrectAnswer = "Singing", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "piano.mp4", Choices = new List<string> { "Piano", "Music", "Play", "Instrument" }, CorrectAnswer = "Piano", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "drums.mp4", Choices = new List<string> { "Drums", "Music", "Play", "Instrument" }, CorrectAnswer = "Drums", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "videogames.mp4", Choices = new List<string> { "Video Games", "Gaming", "Play", "Console" }, CorrectAnswer = "Video Games", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "cards.mp4", Choices = new List<string> { "Cards", "Game", "Play", "Deck" }, CorrectAnswer = "Cards", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "movies.mp4", Choices = new List<string> { "Movies", "Film", "Watch", "Cinema" }, CorrectAnswer = "Movies", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "knitting.mp4", Choices = new List<string> { "Knitting", "Craft", "Make", "Wool" }, CorrectAnswer = "Knitting", Language = SignLanguage.BSL, Category = SignCategory.Hobbies },
            //new SignModel { VideoPath = "pottery.mp4", Choices = new List<string> { "Pottery", "Clay", "Make", "Craft" }, CorrectAnswer = "Pottery", Language = SignLanguage.BSL, Category = SignCategory.Hobbies }
        #endregion
        });

        return signs;
    }
    #endregion
}
