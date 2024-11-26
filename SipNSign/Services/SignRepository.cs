using com.kizwiz.sipnsign.Models;
using Microsoft.Maui.Devices;

public class SignRepository
{
    public List<SignModel> GetSigns()
    {
        var basePath = DeviceInfo.Platform == DevicePlatform.WinUI ? "ms-appx:///Raw/" : "";

        return new List<SignModel>
        {
            new SignModel { VideoPath = $"{basePath}actor.mp4", Choices = new List<string> { "Actor", "Performer", "Singer", "Dancer" }, CorrectAnswer = "Actor" },
            new SignModel { VideoPath = $"{basePath}again.mp4", Choices = new List<string> { "Once", "Again", "Repeat", "Return" }, CorrectAnswer = "Again" },
            new SignModel { VideoPath = $"{basePath}age.mp4", Choices = new List<string> { "Young", "Old", "Age", "Time" }, CorrectAnswer = "Age" },
            new SignModel { VideoPath = $"{basePath}angel.mp4", Choices = new List<string> { "Devil", "Angel", "Spirit", "Saint" }, CorrectAnswer = "Angel" },
            new SignModel { VideoPath = $"{basePath}answer.mp4", Choices = new List<string> { "Answer", "Question", "Reply", "Ask" }, CorrectAnswer = "Answer" },
            new SignModel { VideoPath = $"{basePath}argue.mp4", Choices = new List<string> { "Fight", "Argue", "Discuss", "Debate" }, CorrectAnswer = "Argue" },
            new SignModel { VideoPath = $"{basePath}autumn.mp4", Choices = new List<string> { "Winter", "Autumn", "Summer", "Spring" }, CorrectAnswer = "Autumn" },
            new SignModel { VideoPath = $"{basePath}bedroom.mp4", Choices = new List<string> { "Kitchen", "Bedroom", "Bathroom", "Living Room" }, CorrectAnswer = "Bedroom" },
            new SignModel { VideoPath = $"{basePath}beer.mp4", Choices = new List<string> { "Wine", "Beer", "Juice", "Water" }, CorrectAnswer = "Beer" },
            new SignModel { VideoPath = $"{basePath}birthday.mp4", Choices = new List<string> { "Anniversary", "Birthday", "Celebration", "Party" }, CorrectAnswer = "Birthday" },
            new SignModel { VideoPath = $"{basePath}breakfast.mp4", Choices = new List<string> { "Lunch", "Dinner", "Breakfast", "Snack" }, CorrectAnswer = "Breakfast" },
            new SignModel { VideoPath = $"{basePath}bus.mp4", Choices = new List<string> { "Bus", "Train", "Car", "Taxi" }, CorrectAnswer = "Bus" },
            new SignModel { VideoPath = $"{basePath}car.mp4", Choices = new List<string> { "Bicycle", "Car", "Motorcycle", "Bus" }, CorrectAnswer = "Car" },
            new SignModel { VideoPath = $"{basePath}chicken.mp4", Choices = new List<string> { "Chicken", "Fish", "Pork", "Beef" }, CorrectAnswer = "Chicken" },
            new SignModel { VideoPath = $"{basePath}dance.mp4", Choices = new List<string> { "Jump", "Dance", "Move", "Run" }, CorrectAnswer = "Dance" },
            new SignModel { VideoPath = $"{basePath}dolphin.mp4", Choices = new List<string> { "Whale", "Dolphin", "Shark", "Fish" }, CorrectAnswer = "Dolphin" },
            new SignModel { VideoPath = $"{basePath}dog.mp4", Choices = new List<string> { "Dog", "Cat", "Animal", "Bird" }, CorrectAnswer = "Dog" },
            new SignModel { VideoPath = $"{basePath}donkey.mp4", Choices = new List<string> { "Donkey", "Horse", "Zebra", "Mule" }, CorrectAnswer = "Donkey" },
            new SignModel { VideoPath = $"{basePath}forget.mp4", Choices = new List<string> { "Remember", "Forget", "Recall", "Learn" }, CorrectAnswer = "Forget" },
            new SignModel { VideoPath = $"{basePath}help.mp4", Choices = new List<string> { "Assist", "Help", "Support", "Aid" }, CorrectAnswer = "Help" },
            new SignModel { VideoPath = $"{basePath}read.mp4", Choices = new List<string> { "Write", "Read", "Speak", "Listen" }, CorrectAnswer = "Read" },
            new SignModel { VideoPath = $"{basePath}sun.mp4", Choices = new List<string> { "Sun", "Moon", "Star", "Cloud" }, CorrectAnswer = "Sun" },
        };
    }
}
