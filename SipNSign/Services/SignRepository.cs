using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Models;

namespace com.kizwiz.sipnsign.Services
{
    /// <summary>
    /// Repository for managing sign language signs and their categories
    /// </summary>
    public class SignRepository
    {
        /// <summary>
        /// Gets the total number of signs across all categories
        /// </summary>
        public int TotalSignsCount => GetSignsByCategory().SelectMany(c => c.Signs).Count();




        public List<SignModel> GetSigns() => _categories.SelectMany(c => c.Signs).ToList();

        private readonly List<CategoryModel> _categories;

        public SignRepository()
        {
            _categories = InitializeCategories();
        }

        /// <summary>
        /// Gets signs grouped by categories
        /// </summary>
        private List<CategoryModel> InitializeCategories()
        {
            return new List<CategoryModel>
            {
                new CategoryModel
                {
                    Category = SignCategory.BasicInteractions,
                    DisplayName = "Basic Interactions",
                    Description = "Common signs for everyday communication",
                    IconName = "basic_icon",
                    Signs = new List<SignModel>
                    {
                        CreateSign("answer", "Answer", SignCategory.BasicInteractions),
                        CreateSign("help", "Help", SignCategory.BasicInteractions),
                        CreateSign("know", "Know", SignCategory.BasicInteractions),
                        CreateSign("learn", "Learn", SignCategory.BasicInteractions),
                        CreateSign("question", "Question", SignCategory.BasicInteractions),
                        CreateSign("want", "Want", SignCategory.BasicInteractions)
                    }
                },

                    new CategoryModel
                    {
                        Category = SignCategory.TimeAndCalendar,
                        DisplayName = "Time & Calendar",
                        Description = "Signs related to time, dates, and seasons",
                        IconName = "time_icon",
                        Signs = new List<SignModel>
                       {
                            CreateSign("again", "Again", SignCategory.TimeAndCalendar),
                            CreateSign("autumn", "Autumn", SignCategory.TimeAndCalendar),
                            CreateSign("spring", "Spring", SignCategory.TimeAndCalendar),
                            CreateSign("summer", "Summer", SignCategory.TimeAndCalendar),
                            CreateSign("weekend", "Weekend", SignCategory.TimeAndCalendar),
                            CreateSign("when", "When", SignCategory.TimeAndCalendar),
                            CreateSign("winter", "Winter", SignCategory.TimeAndCalendar)
                       }
                    },

                    new CategoryModel
                    {
                        Category = SignCategory.FamilyAndPeople,
                        DisplayName = "Family & People",
                        Description = "Signs for family members and people",
                        IconName = "family_icon",
                        Signs = new List<SignModel>
                       {
                            CreateSign("aunt", "Aunt", SignCategory.FamilyAndPeople),
                            CreateSign("boy", "Boy", SignCategory.FamilyAndPeople),
                            CreateSign("brother", "Brother", SignCategory.FamilyAndPeople),
                            CreateSign("child", "Child", SignCategory.FamilyAndPeople),
                            CreateSign("daughter", "Daughter", SignCategory.FamilyAndPeople),
                            CreateSign("family", "Family", SignCategory.FamilyAndPeople),
                            CreateSign("girl", "Girl", SignCategory.FamilyAndPeople),
                            CreateSign("parents", "Parents", SignCategory.FamilyAndPeople),
                            CreateSign("sister", "Sister", SignCategory.FamilyAndPeople),
                            CreateSign("son", "Son", SignCategory.FamilyAndPeople),
                            CreateSign("uncle", "Uncle", SignCategory.FamilyAndPeople)
                       }
                    },

                    new CategoryModel
                    {
                        Category = SignCategory.EmotionsAndStates,
                        DisplayName = "Emotions & States",
                        Description = "Signs for feelings and conditions",
                        IconName = "emotion_icon",
                        Signs = new List<SignModel>
                       {
                            CreateSign("confused", "Confused", SignCategory.EmotionsAndStates),
                            CreateSign("difficult", "Difficult", SignCategory.EmotionsAndStates),
                            CreateSign("easy", "Easy", SignCategory.EmotionsAndStates),
                            CreateSign("evil", "Evil", SignCategory.EmotionsAndStates),
                            CreateSign("hard", "Hard", SignCategory.EmotionsAndStates),
                            CreateSign("horrible", "Horrible", SignCategory.EmotionsAndStates),
                            CreateSign("hot", "Hot", SignCategory.EmotionsAndStates),
                            CreateSign("love", "Love", SignCategory.EmotionsAndStates),
                            CreateSign("luck", "Luck", SignCategory.EmotionsAndStates),
                            CreateSign("mental", "Mental", SignCategory.EmotionsAndStates),
                            CreateSign("miserable", "Miserable", SignCategory.EmotionsAndStates),
                            CreateSign("shy", "Shy", SignCategory.EmotionsAndStates),
                            CreateSign("smile", "Smile", SignCategory.EmotionsAndStates),
                            CreateSign("soft", "Soft", SignCategory.EmotionsAndStates),
                            CreateSign("sympathy", "Sympathy", SignCategory.EmotionsAndStates),
                            CreateSign("tired", "Tired", SignCategory.EmotionsAndStates),
                            CreateSign("ugly", "Ugly", SignCategory.EmotionsAndStates),
                            CreateSign("Weird", "Weird", SignCategory.EmotionsAndStates)
                       }
                    },

                    new CategoryModel
                    {
                        Category = SignCategory.Animals,
                        DisplayName = "Animals",
                        Description = "Signs for different animals",
                        IconName = "animal_icon",
                        Signs = new List<SignModel>
                       {
                        CreateSign("bird", "Bird", SignCategory.Animals),
                        CreateSign("dog", "Dog", SignCategory.Animals),
                        CreateSign("dolphin", "Dolphin", SignCategory.Animals),
                        CreateSign("donkey", "Donkey", SignCategory.Animals),
                        CreateSign("eagle", "Eagle", SignCategory.Animals),
                        CreateSign("elephant", "Elephant", SignCategory.Animals),
                        CreateSign("fish", "Fish", SignCategory.Animals),
                        CreateSign("giraffe", "Giraffe", SignCategory.Animals),
                        CreateSign("horse", "Horse", SignCategory.Animals),
                        CreateSign("Lion", "Lion", SignCategory.Animals),
                        CreateSign("monkey", "Monkey", SignCategory.Animals),
                        CreateSign("penguin", "Penguin", SignCategory.Animals),
                        CreateSign("pig", "Pig", SignCategory.Animals),
                        CreateSign("pigeon", "Pigeon", SignCategory.Animals),
                        CreateSign("rabbit", "Rabbit", SignCategory.Animals),
                        CreateSign("snake", "Snake", SignCategory.Animals),
                        CreateSign("tiger", "Tiger", SignCategory.Animals)
                       }
                    },

                    new CategoryModel
                    {
                        Category = SignCategory.FoodAndDrink,
                        DisplayName = "Food & Drink",
                        Description = "Signs for food and beverages",
                        IconName = "food_icon",
                        Signs = new List<SignModel>
                       {
                            CreateSign("apple", "Apple", SignCategory.FoodAndDrink),
                            CreateSign("banana", "Banana", SignCategory.FoodAndDrink),
                            CreateSign("beer", "Beer", SignCategory.FoodAndDrink),
                            CreateSign("breakfast", "Breakfast", SignCategory.FoodAndDrink),
                            CreateSign("cheese", "Cheese", SignCategory.FoodAndDrink),
                            CreateSign("chicken", "Chicken", SignCategory.FoodAndDrink),
                            CreateSign("dinner", "Dinner", SignCategory.FoodAndDrink),
                            CreateSign("drink", "Drink", SignCategory.FoodAndDrink),
                            CreateSign("food", "Food", SignCategory.FoodAndDrink),
                            CreateSign("fruit", "Fruit", SignCategory.FoodAndDrink),
                            CreateSign("juice", "Juice", SignCategory.FoodAndDrink),
                            CreateSign("lunch", "Lunch", SignCategory.FoodAndDrink),
                            CreateSign("meat", "Meat", SignCategory.FoodAndDrink),
                            CreateSign("milk", "Milk", SignCategory.FoodAndDrink),
                            CreateSign("orange", "Orange", SignCategory.FoodAndDrink),
                            CreateSign("pizza", "Pizza", SignCategory.FoodAndDrink),
                            CreateSign("sandwich", "Sandwich", SignCategory.FoodAndDrink),
                            CreateSign("soup", "Soup", SignCategory.FoodAndDrink),
                            CreateSign("vodka", "Vodka", SignCategory.FoodAndDrink),
                            CreateSign("water", "Water", SignCategory.FoodAndDrink)
                       }
                    },

                    new CategoryModel
                    {
                        Category = SignCategory.Colors,
                        DisplayName = "Colors",
                        Description = "Signs for different colors",
                        IconName = "color_icon",
                        Signs = new List<SignModel>
                       {
                            CreateSign("black", "Black", SignCategory.Colors),
                            CreateSign("blue", "Blue", SignCategory.Colors),
                            CreateSign("brown", "Brown", SignCategory.Colors),
                            CreateSign("green", "Green", SignCategory.Colors),
                            CreateSign("grey", "Grey", SignCategory.Colors),
                            CreateSign("pink", "Pink", SignCategory.Colors),
                            CreateSign("purple", "Purple", SignCategory.Colors),
                            CreateSign("red", "Red", SignCategory.Colors),
                            CreateSign("white", "White", SignCategory.Colors),
                            CreateSign("yellow", "Yellow", SignCategory.Colors)
                       }
                    },

                    new CategoryModel
                    {
                        Category = SignCategory.PlacesAndTravel,
                        DisplayName = "Places & Travel",
                        Description = "Signs for locations and countries",
                        IconName = "place_icon",
                        Signs = new List<SignModel>
                       {
                            CreateSign("America", "America", SignCategory.PlacesAndTravel),
                            CreateSign("Belgium", "Belgium", SignCategory.PlacesAndTravel),
                            CreateSign("brazil", "Brazil", SignCategory.PlacesAndTravel),
                            CreateSign("China", "China", SignCategory.PlacesAndTravel),
                            CreateSign("Croatia", "Croatia", SignCategory.PlacesAndTravel),
                            CreateSign("Egypt", "Egypt", SignCategory.PlacesAndTravel),
                            CreateSign("England", "England", SignCategory.PlacesAndTravel),
                            CreateSign("Finland", "Finland", SignCategory.PlacesAndTravel),
                            CreateSign("France", "France", SignCategory.PlacesAndTravel),
                            CreateSign("Germany", "Germany", SignCategory.PlacesAndTravel),
                            CreateSign("Holland", "Holland", SignCategory.PlacesAndTravel),
                            CreateSign("India", "India", SignCategory.PlacesAndTravel),
                            CreateSign("Ireland", "Ireland", SignCategory.PlacesAndTravel),
                            CreateSign("Japan", "Japan", SignCategory.PlacesAndTravel),
                            CreateSign("Norway", "Norway", SignCategory.PlacesAndTravel),
                            CreateSign("Poland", "Poland", SignCategory.PlacesAndTravel),
                            CreateSign("Portugal", "Portugal", SignCategory.PlacesAndTravel),
                            CreateSign("Scotland", "Scotland", SignCategory.PlacesAndTravel),
                            CreateSign("Spain", "Spain", SignCategory.PlacesAndTravel),
                            CreateSign("Sweden", "Sweden", SignCategory.PlacesAndTravel),
                            CreateSign("Switzerland", "Switzerland", SignCategory.PlacesAndTravel),
                            CreateSign("Thailand", "Thailand", SignCategory.PlacesAndTravel),
                            CreateSign("Wales", "Wales", SignCategory.PlacesAndTravel)
                       }
                    },

                    new CategoryModel
                    {
                        Category = SignCategory.HouseAndFurniture,
                        DisplayName = "House & Furniture",
                        Description = "Signs for home and furniture items",
                        IconName = "house_icon",
                        Signs = new List<SignModel>
                       {
                            CreateSign("bed", "Bed", SignCategory.HouseAndFurniture),
                            CreateSign("bedroom", "Bedroom", SignCategory.HouseAndFurniture),
                            CreateSign("chair", "Chair", SignCategory.HouseAndFurniture),
                            CreateSign("fork", "Fork", SignCategory.HouseAndFurniture),
                            CreateSign("fridge", "Fridge", SignCategory.HouseAndFurniture),
                            CreateSign("house", "House", SignCategory.HouseAndFurniture),
                            CreateSign("keyboard", "Keyboard", SignCategory.HouseAndFurniture),
                            CreateSign("kitchen", "Kitchen", SignCategory.HouseAndFurniture),
                            CreateSign("knife", "Knife", SignCategory.HouseAndFurniture),
                            CreateSign("microwave", "Microwave", SignCategory.HouseAndFurniture),
                            CreateSign("shower", "Shower", SignCategory.HouseAndFurniture),
                            CreateSign("toothbrush", "Toothbrush", SignCategory.HouseAndFurniture)
                       }
                    }
            };
        }

        /// <summary>
        /// Creates a new SignModel with randomized multiple choice options
        /// </summary>
        private SignModel CreateSign(string videoName, string correctAnswer, SignCategory category)
        {
            return new SignModel
            {
                VideoPath = $"{videoName}.mp4",
                CorrectAnswer = correctAnswer,
                Category = category,
                Choices = new List<string> { correctAnswer }  // Just add correct answer initially
            };
        }

        public void UpdateChoices()
        {
            var allSigns = GetSignsByCategory().SelectMany(c => c.Signs).ToList();
            foreach (var sign in allSigns)
            {
                var otherAnswers = allSigns
                    .Where(s => s.CorrectAnswer != sign.CorrectAnswer)
                    .OrderBy(r => Guid.NewGuid())
                    .Take(3)
                    .Select(s => s.CorrectAnswer);

                sign.Choices = new List<string> { sign.CorrectAnswer };
                sign.Choices.AddRange(otherAnswers);
                sign.Choices = sign.Choices.OrderBy(x => Guid.NewGuid()).ToList();
            }
        }


        public List<CategoryModel> GetSignsByCategory() => _categories;
    }
}