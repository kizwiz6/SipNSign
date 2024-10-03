using com.kizwiz.sipnsign.Models;

/// <summary>
/// Repository class for retrieving a list of sign models.
/// </summary>
public class SignRepository
{
    /// <summary>
    /// Retrieves a list of signs with their associated video paths, choices, and correct answers.
    /// </summary>
    /// <returns>
    /// A list of <see cref="SignModel"/> objects, each containing the video path, answer choices, and the correct answer.
    /// </returns>
    public List<SignModel> GetSigns()
    {
        return new List<SignModel>
        {
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/actor", Choices = new List<string> { "Actor", "Performer", "Singer" }, CorrectAnswer = "Actor" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/again", Choices = new List<string> { "Once", "Again", "Repeat" }, CorrectAnswer = "Again" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/age", Choices = new List<string> { "Young", "Old", "Age" }, CorrectAnswer = "Age" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/angel", Choices = new List<string> { "Devil", "Angel", "Spirit" }, CorrectAnswer = "Angel" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/answer", Choices = new List<string> { "Answer", "Question", "Reply" }, CorrectAnswer = "Answer" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/argue", Choices = new List<string> { "Fight", "Argue", "Discuss" }, CorrectAnswer = "Argue" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/autumn", Choices = new List<string> { "Winter", "Autumn", "Summer" }, CorrectAnswer = "Autumn" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/bedroom", Choices = new List<string> { "Kitchen", "Bedroom", "Bathroom" }, CorrectAnswer = "Bedroom" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/beer", Choices = new List<string> { "Wine", "Beer", "Juice" }, CorrectAnswer = "Beer" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/birthday", Choices = new List<string> { "Anniversary", "Birthday", "Celebration" }, CorrectAnswer = "Birthday" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/breakfast", Choices = new List<string> { "Lunch", "Dinner", "Breakfast" }, CorrectAnswer = "Breakfast" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/bus", Choices = new List<string> { "Bus", "Train", "Car" }, CorrectAnswer = "Bus" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/car", Choices = new List<string> { "Bicycle", "Car", "Motorcycle" }, CorrectAnswer = "Car" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/chicken", Choices = new List<string> { "Chicken", "Fish", "Pork" }, CorrectAnswer = "Chicken" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/dance", Choices = new List<string> { "Jump", "Dance", "Move" }, CorrectAnswer = "Dance" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/dolphin", Choices = new List<string> { "Whale", "Dolphin", "Shark" }, CorrectAnswer = "Dolphin" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/dog", Choices = new List<string> { "Dog", "Cat", "Animal" }, CorrectAnswer = "Dog" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/donkey", Choices = new List<string> { "Donkey", "Horse", "Zebra" }, CorrectAnswer = "Donkey" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/forget", Choices = new List<string> { "Remember", "Forget", "Recall" }, CorrectAnswer = "Forget" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/help", Choices = new List<string> { "Assist", "Help", "Support" }, CorrectAnswer = "Help" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/read", Choices = new List<string> { "Write", "Read", "Speak" }, CorrectAnswer = "Read" },
            new SignModel { VideoPath = "android.resource://com.kizwiz.sipnsign/raw/sun", Choices = new List<string> { "Sun", "Moon", "Star" }, CorrectAnswer = "Sun" }
        };
    }
}
