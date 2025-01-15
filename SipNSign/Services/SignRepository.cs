using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Models;

/// <summary>
/// Repository for managing sign data and video mappings
/// </summary>
public class SignRepository
{
    private readonly Dictionary<SignLanguage, List<SignModel>> _signsByLanguage;

    public SignRepository()
    {
        _signsByLanguage = new Dictionary<SignLanguage, List<SignModel>>
        {
            [SignLanguage.BSL] = InitializeBSLSigns()
        };
    }

    #region Properties
    /// <summary>
    /// Gets the total number of available signs for a specific language
    /// </summary>
    public int GetSignCount(SignLanguage language = SignLanguage.BSL) =>
        _signsByLanguage.TryGetValue(language, out var signs) ? signs.Count : 0;
    #endregion

    #region Public Methods
    /// <summary>
    /// Returns a list of all available signs with their video paths and multiple choice options
    /// for the specified language
    /// </summary>
    public List<SignModel> GetSigns(SignLanguage language = SignLanguage.BSL)
    {
        return _signsByLanguage.TryGetValue(language, out var signs) ? signs : new List<SignModel>();
    }

    /// <summary>
    /// Returns signs filtered by category for the specified language
    /// </summary>
    public List<SignModel> GetSignsByCategory(SignCategory category, SignLanguage language = SignLanguage.BSL)
    {
        return GetSigns(language).Where(s => s.Category == category).ToList();
    }
    #endregion

    #region Private Methods
    private List<SignModel> InitializeBSLSigns()
    {
        return new List<SignModel>
        {
            // Animals
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

            // Colors
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

            // Countries
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
            // Drinks
            new SignModel { VideoPath = "beer.mp4", Choices = new List<string> { "Wine", "Beer", "Juice", "Water" }, CorrectAnswer = "Beer", Language = SignLanguage.BSL, Category = SignCategory.Drinks },
            new SignModel { VideoPath = "drink.mp4", Choices = new List<string> { "Eat", "Drink", "Cook", "Pour" }, CorrectAnswer = "Drink", Language = SignLanguage.BSL, Category = SignCategory.Drinks },
new SignModel { VideoPath = "milk.mp4", Choices = new List<string> { "Water", "Juice", "Milk", "Coffee" }, CorrectAnswer = "Milk", Language = SignLanguage.BSL, Category = SignCategory.Drinks },
            new SignModel { VideoPath = "vodka.mp4", Choices = new List<string> { "Vodka", "Whiskey", "Rum", "Tequila" }, CorrectAnswer = "Vodka", Language = SignLanguage.BSL, Category = SignCategory.Drinks },
            new SignModel { VideoPath = "water.mp4", Choices = new List<string> { "Juice", "Tea", "Water", "Milk" }, CorrectAnswer = "Water", Language = SignLanguage.BSL, Category = SignCategory.Drinks },

            // Emotions/Feelings
            new SignModel { VideoPath = "confused.mp4", Choices = new List<string> { "Sad", "Happy", "Angry", "Confused" }, CorrectAnswer = "Confused", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
            new SignModel { VideoPath = "evil.mp4", Choices = new List<string> { "Good", "Kind", "Bad", "Evil" }, CorrectAnswer = "Evil", Language = SignLanguage.BSL, Category = SignCategory.Emotions },
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

            // Family
            new SignModel { VideoPath = "aunt.mp4", Choices = new List<string> { "Sister", "Mother", "Aunt", "Grandmother" }, CorrectAnswer = "Aunt", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "brother.mp4", Choices = new List<string> { "Father", "Brother", "Friend", "Sister" }, CorrectAnswer = "Brother", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "daughter.mp4", Choices = new List<string> { "Son", "Mother", "Daughter", "Sister" }, CorrectAnswer = "Daughter", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "family.mp4", Choices = new List<string> { "Group", "Neighbours", "Family", "Friends" }, CorrectAnswer = "Family", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "girl.mp4", Choices = new List<string> { "Boy", "Girl", "Woman", "Child" }, CorrectAnswer = "Girl", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "parents.mp4", Choices = new List<string> { "Relatives", "Siblings", "Family", "Parents" }, CorrectAnswer = "Parents", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "sister.mp4", Choices = new List<string> { "Cousin", "Brother", "Sibling", "Sister" }, CorrectAnswer = "Sister", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "son.mp4", Choices = new List<string> { "Son", "Daughter", "Child", "Boy" }, CorrectAnswer = "Son", Language = SignLanguage.BSL, Category = SignCategory.Family },
            new SignModel { VideoPath = "uncle.mp4", Choices = new List<string> { "Uncle", "Aunt", "Cousin", "Brother" }, CorrectAnswer = "Uncle", Language = SignLanguage.BSL, Category = SignCategory.Family },

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

            // Nature
            new SignModel { VideoPath = "cloud.mp4", Choices = new List<string> { "Sky", "Cloud", "Rain", "Storm" }, CorrectAnswer = "Cloud", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "farm.mp4", Choices = new List<string> { "Farm", "Ranch", "Garden", "Field" }, CorrectAnswer = "Farm", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "fire.mp4", Choices = new List<string> { "Flame", "Fire", "Smoke", "Heat" }, CorrectAnswer = "Fire", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "flower.mp4", Choices = new List<string> { "Tree", "Flower", "Leaf", "Bush" }, CorrectAnswer = "Flower", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "forest.mp4", Choices = new List<string> { "Woods", "Jungle", "Forest", "Park" }, CorrectAnswer = "Forest", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "moon.mp4", Choices = new List<string> { "Moon", "Star", "Planet", "Sun" }, CorrectAnswer = "Moon", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "ocean.mp4", Choices = new List<string> { "Sea", "Ocean", "Lake", "River" }, CorrectAnswer = "Ocean", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "rain.mp4", Choices = new List<string> { "Storm", "Rain", "Drizzle", "Hail" }, CorrectAnswer = "Rain", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "sea.mp4", Choices = new List<string> { "River", "Lake", "Ocean", "Sea" }, CorrectAnswer = "Sea", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "stone.mp4", Choices = new List<string> { "Rock", "Pebble", "Stone", "Boulder" }, CorrectAnswer = "Stone", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "storm.mp4", Choices = new List<string> { "Rain", "Storm", "Thunder", "Hurricane" }, CorrectAnswer = "Storm", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "sun.mp4", Choices = new List<string> { "Sun", "Moon", "Star", "Cloud" }, CorrectAnswer = "Sun", Language = SignLanguage.BSL, Category = SignCategory.Nature },
            new SignModel { VideoPath = "tree.mp4", Choices = new List<string> { "Plant", "Bush", "Tree", "Shrub" }, CorrectAnswer = "Tree", Language = SignLanguage.BSL, Category = SignCategory.Nature },

            // Occupations
            new SignModel { VideoPath = "actor.mp4", Choices = new List<string> { "Actor", "Performer", "Singer", "Dancer" }, CorrectAnswer = "Actor", Language = SignLanguage.BSL, Category = SignCategory.Occupations },
            new SignModel { VideoPath = "boss.mp4", Choices = new List<string> { "Manager", "Boss", "Worker", "Assistant" }, CorrectAnswer = "Boss", Language = SignLanguage.BSL, Category = SignCategory.Occupations },
            new SignModel { VideoPath = "chef.mp4", Choices = new List<string> { "Chef", "Waiter", "Cook", "Baker" }, CorrectAnswer = "Chef", Language = SignLanguage.BSL, Category = SignCategory.Occupations },
            new SignModel { VideoPath = "police.mp4", Choices = new List<string> { "Guard", "Officer", "Law", "Police" }, CorrectAnswer = "Police", Language = SignLanguage.BSL, Category = SignCategory.Occupations },
            new SignModel { VideoPath = "work.mp4", Choices = new List<string> { "Work", "Job", "Task", "Duty" }, CorrectAnswer = "Work", Language = SignLanguage.BSL, Category = SignCategory.Occupations },

            // Time Related
            new SignModel { VideoPath = "age.mp4", Choices = new List<string> { "Young", "Old", "Age", "Time" }, CorrectAnswer = "Age", Language = SignLanguage.BSL, Category = SignCategory.Time },
            new SignModel { VideoPath = "autumn.mp4", Choices = new List<string> { "Winter", "Autumn", "Summer", "Spring" }, CorrectAnswer = "Autumn", Language = SignLanguage.BSL, Category = SignCategory.Time },
            new SignModel { VideoPath = "spring.mp4", Choices = new List<string> { "Winter", "Spring", "Autumn", "Summer" }, CorrectAnswer = "Spring", Language = SignLanguage.BSL, Category = SignCategory.Time },
            new SignModel { VideoPath = "summer.mp4", Choices = new List<string> { "Autumn", "Summer", "Winter", "Spring" }, CorrectAnswer = "Summer", Language = SignLanguage.BSL, Category = SignCategory.Time },
            new SignModel { VideoPath = "weekend.mp4", Choices = new List<string> { "Holiday", "Weekend", "Vacation", "Break" }, CorrectAnswer = "Weekend", Language = SignLanguage.BSL, Category = SignCategory.Time },
            new SignModel { VideoPath = "when.mp4", Choices = new List<string> { "When", "Where", "Why", "What" }, CorrectAnswer = "When", Language = SignLanguage.BSL, Category = SignCategory.Time },
            new SignModel { VideoPath = "winter.mp4", Choices = new List<string> { "Winter", "Summer", "Spring", "Autumn" }, CorrectAnswer = "Winter", Language = SignLanguage.BSL, Category = SignCategory.Time },

            // Travel/Transportation
            new SignModel { VideoPath = "bicycle.mp4", Choices = new List<string> { "Car", "Bus", "Motorcycle", "Bicycle" }, CorrectAnswer = "Bicycle", Language = SignLanguage.BSL, Category = SignCategory.Travel },
            new SignModel { VideoPath = "bus.mp4", Choices = new List<string> { "Bus", "Train", "Car", "Taxi" }, CorrectAnswer = "Bus", Language = SignLanguage.BSL, Category = SignCategory.Travel },
            new SignModel { VideoPath = "car.mp4", Choices = new List<string> { "Bicycle", "Car", "Motorcycle", "Bus" }, CorrectAnswer = "Car", Language = SignLanguage.BSL, Category = SignCategory.Travel },
            new SignModel { VideoPath = "travel.mp4", Choices = new List<string> { "Journey", "Travel", "Vacation", "Tour" }, CorrectAnswer = "Travel", Language = SignLanguage.BSL, Category = SignCategory.Travel },
            new SignModel { VideoPath = "walk.mp4", Choices = new List<string> { "Walk", "Run", "Stroll", "March" }, CorrectAnswer = "Walk", Language = SignLanguage.BSL, Category = SignCategory.Travel },

            // Weather
            new SignModel { VideoPath = "hard.mp4", Choices = new List<string> { "Soft", "Tough", "Hard", "Firm" }, CorrectAnswer = "Hard", Language = SignLanguage.BSL, Category = SignCategory.Weather },
            new SignModel { VideoPath = "hot.mp4", Choices = new List<string> { "Cool", "Cold", "Warm", "Hot" }, CorrectAnswer = "Hot", Language = SignLanguage.BSL, Category = SignCategory.Weather },
            new SignModel { VideoPath = "soft.mp4", Choices = new List<string> { "Hard", "Soft", "Smooth", "Gentle" }, CorrectAnswer = "Soft", Language = SignLanguage.BSL, Category = SignCategory.Weather },
            new SignModel { VideoPath = "weather.mp4", Choices = new List<string> { "Weather", "Climate", "Rain", "Temperature" }, CorrectAnswer = "Weather", Language = SignLanguage.BSL, Category = SignCategory.Weather }
        };
    }
    #endregion
}
