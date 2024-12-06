using com.kizwiz.sipnsign.Models;

public class SignRepository
{
    public List<SignModel> GetSigns()
    {
        return new List<SignModel>
        {
            new SignModel { VideoPath = "actor.mp4", Choices = new List<string> { "Actor", "Performer", "Singer", "Dancer" }, CorrectAnswer = "Actor" },
            new SignModel { VideoPath = "again.mp4", Choices = new List<string> { "Once", "Again", "Repeat", "Return" }, CorrectAnswer = "Again" },
            new SignModel { VideoPath = "age.mp4", Choices = new List<string> { "Young", "Old", "Age", "Time" }, CorrectAnswer = "Age" },
            new SignModel { VideoPath = "angel.mp4", Choices = new List<string> { "Devil", "Angel", "Spirit", "Saint" }, CorrectAnswer = "Angel" },
            new SignModel { VideoPath = "answer.mp4", Choices = new List<string> { "Answer", "Question", "Reply", "Ask" }, CorrectAnswer = "Answer" },
            new SignModel { VideoPath = "argue.mp4", Choices = new List<string> { "Fight", "Argue", "Discuss", "Debate" }, CorrectAnswer = "Argue" },
            new SignModel { VideoPath = "autumn.mp4", Choices = new List<string> { "Winter", "Autumn", "Summer", "Spring" }, CorrectAnswer = "Autumn" },
            new SignModel { VideoPath = "bedroom.mp4", Choices = new List<string> { "Kitchen", "Bedroom", "Bathroom", "Living Room" }, CorrectAnswer = "Bedroom" },
            new SignModel { VideoPath = "beer.mp4", Choices = new List<string> { "Wine", "Beer", "Juice", "Water" }, CorrectAnswer = "Beer" },
            new SignModel { VideoPath = "birthday.mp4", Choices = new List<string> { "Anniversary", "Birthday", "Celebration", "Party" }, CorrectAnswer = "Birthday" },
            new SignModel { VideoPath = "breakfast.mp4", Choices = new List<string> { "Lunch", "Dinner", "Breakfast", "Snack" }, CorrectAnswer = "Breakfast" },
            new SignModel { VideoPath = "bus.mp4", Choices = new List<string> { "Bus", "Train", "Car", "Taxi" }, CorrectAnswer = "Bus" },
            new SignModel { VideoPath = "car.mp4", Choices = new List<string> { "Bicycle", "Car", "Motorcycle", "Bus" }, CorrectAnswer = "Car" },
            new SignModel { VideoPath = "chicken.mp4", Choices = new List<string> { "Chicken", "Fish", "Pork", "Beef" }, CorrectAnswer = "Chicken" },
            new SignModel { VideoPath = "dance.mp4", Choices = new List<string> { "Jump", "Dance", "Move", "Run" }, CorrectAnswer = "Dance" },
            new SignModel { VideoPath = "dolphin.mp4", Choices = new List<string> { "Whale", "Dolphin", "Shark", "Fish" }, CorrectAnswer = "Dolphin" },
            new SignModel { VideoPath = "dog.mp4", Choices = new List<string> { "Dog", "Cat", "Animal", "Bird" }, CorrectAnswer = "Dog" },
            new SignModel { VideoPath = "donkey.mp4", Choices = new List<string> { "Donkey", "Horse", "Zebra", "Mule" }, CorrectAnswer = "Donkey" },
            new SignModel { VideoPath = "forget.mp4", Choices = new List<string> { "Remember", "Forget", "Recall", "Learn" }, CorrectAnswer = "Forget" },
            new SignModel { VideoPath = "help.mp4", Choices = new List<string> { "Assist", "Help", "Support", "Aid" }, CorrectAnswer = "Help" },
            new SignModel { VideoPath = "read.mp4", Choices = new List<string> { "Write", "Read", "Speak", "Listen" }, CorrectAnswer = "Read" },
            new SignModel { VideoPath = "sun.mp4", Choices = new List<string> { "Sun", "Moon", "Star", "Cloud" }, CorrectAnswer = "Sun" },
        };
    }
}
